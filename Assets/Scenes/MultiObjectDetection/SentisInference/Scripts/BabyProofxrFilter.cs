using System;
using System.Collections.Generic;
using UnityEngine;

namespace PassthroughCameraSamples.MultiObjectDetection
{
    /// <summary>
    /// Handles filtering of detected objects based on various criteria for BabyProofxr.
    /// </summary>
    public class BabyProofxrFilter
    {
        public bool IgnoreDangerZoneFilter {get; private set;}

        private readonly float chockingHazardMaxSize;
        private readonly Dictionary<int, string> dangerousLabelDict;
        private readonly Dictionary<int, string> ignoreLabelDict;
        private readonly BoundingZoneManager boundingDangerZoneManager;

        private PassthroughCameraEye cameraEye;

        [Header("Debug purposes")]
        private readonly TestImageManager testImageManager;
        private readonly Camera debugCamera;

        // to warn of potential code changes
        public static event Action<bool> IsDangerZoneFilterOn;

        public BabyProofxrFilter(
            float chockingHazardMaxSize, 
            Dictionary<int, string> dangerousLabelDict,
            Dictionary<int, string> ignoreLabelDict,
            BoundingZoneManager boundingDangerZoneManager,
            PassthroughCameraEye cameraEye,
            TestImageManager testImageManager,
            Camera debugCamera
        )
        {
            this.chockingHazardMaxSize = chockingHazardMaxSize;
            this.dangerousLabelDict = dangerousLabelDict;
            this.ignoreLabelDict = ignoreLabelDict;
            this.boundingDangerZoneManager = boundingDangerZoneManager;
            this.testImageManager = testImageManager;
            this.debugCamera = debugCamera;
            this.cameraEye = cameraEye;
            XRDebugLogViewer.Log($"[{nameof(BabyProofxrFilter)}] - Constructor: Invoking IsDangerZoneFilterOn with value {IgnoreDangerZoneFilter}. Subscriber count: {IsDangerZoneFilterOn?.GetInvocationList().Length ?? 0}");
            IsDangerZoneFilterOn?.Invoke(IgnoreDangerZoneFilter);
        }

        public void ToggleIgnoreDangerZoneFilter()
        {
            IgnoreDangerZoneFilter = !IgnoreDangerZoneFilter;
            XRDebugLogViewer.Log($"[{nameof(BabyProofxrFilter)}] - ToggleIgnoreDangerZoneFilter: Invoking IsDangerZoneFilterOn with value {IgnoreDangerZoneFilter}. Subscriber count: {IsDangerZoneFilterOn?.GetInvocationList().Length ?? 0}");
            IsDangerZoneFilterOn?.Invoke(IgnoreDangerZoneFilter);
        }

        /// <summary>
        /// Filters the detected objects based on dangerous labels and chocking hazard criteria.
        /// </summary>
        /// <param name="output">Tensor containing bounding box data</param>
        /// <param name="labelIDs">Tensor containing label IDs</param>
        /// <param name="labels">Array of label names</param>
        /// <param name="displayWidth">Width of the display area</param>
        /// <param name="displayHeight">Height of the display area</param>
        /// <param name="imageWidth">Width of the input image</param>
        /// <param name="imageHeight">Height of the input image</param>
        /// <param name="camRes">Camera resolution</param>
        /// <param name="environmentRaycast">Raycast utility for world position calculation</param>
        /// <returns>List of filtered bounding boxes</returns>
        public List<BabyProofxrInferenceUiManager.BabyProofBoundingBox> FilterResults(
            Unity.Sentis.Tensor<float> output,
            Unity.Sentis.Tensor<int> labelIDs,
            string[] labels,
            float displayWidth,
            float displayHeight,
            float imageWidth,
            float imageHeight,
            Vector2Int camRes,
            EnvironmentRayCastSampleManager environmentRaycast)
        {
            List<BabyProofxrInferenceUiManager.BabyProofBoundingBox> filteredBoxes = new();

            var boxesFound = output.shape[0];
            if (boxesFound <= 0 || !boundingDangerZoneManager.IsInitialized)
            {
                return filteredBoxes;
            }

            var scaleX = displayWidth / imageWidth;
            var scaleY = displayHeight / imageHeight;
            var halfWidth = displayWidth / 2;
            var halfHeight = displayHeight / 2;

            for (var n = 0; n < boxesFound; n++)
            {
                // Skip if the label is in the ignore dictionary
                if (ignoreLabelDict.ContainsKey(labelIDs[n]))
                {
                    continue;
                }

                // Get bounding box center coordinates
                var centerX = output[n, 0] * scaleX - halfWidth;
                var centerY = output[n, 1] * scaleY - halfHeight;
                var boxWidth = output[n, 2] * scaleX;
                var boxHeight = output[n, 3] * scaleY;

                var centerPerX = (centerX + halfWidth) / displayWidth;
                var centerPerY = (centerY + halfHeight) / displayHeight;
                Vector3? centerWorldPos = CalculateWorldPosition(centerPerX, centerPerY, camRes, environmentRaycast);

                if (centerWorldPos == null) continue;

                // Calculate surrounding box size in the real world
                float[] surroundBoxWorldDistance = CalculateSurroundingBoxDistances(
                    centerX, centerY, boxWidth, boxHeight,
                    displayWidth, displayHeight,
                    camRes, environmentRaycast,
                    (Vector3)centerWorldPos);

                bool isObjectInDangerZone = boundingDangerZoneManager.TryGetZone((Vector3)centerWorldPos, out var matchingZone);
                // Skip if object is neither dangerous nor a chocking hazard
                if (!isObjectInDangerZone && !IgnoreDangerZoneFilter)
                {
                    continue;
                }

                string label = labels[labelIDs[n]].Trim().Replace(" ", "_").Replace("\n", "_").Replace("\r", "_").Replace("\t", "_");

                // Check if object is a chocking hazard
                bool isChockingHazard = IsChockingHazard(surroundBoxWorldDistance);
                // Check if object is in dangerous objects list
                bool isDangerousObject = dangerousLabelDict.ContainsKey(labelIDs[n]);

                XRDebugLogViewer.Log($"Object Found: {label}, Chocking {isChockingHazard}, Dangerous {isDangerousObject}");
                // Create bounding box
                var box = new BabyProofxrInferenceUiManager.BabyProofBoundingBox
                {
                    BaseBox = new SentisInferenceUiManager.BoundingBox
                    {
                        CenterX = centerX,
                        CenterY = centerY,
                        Width = boxWidth,
                        Height = boxHeight,
                        Label = label,
                        WorldPos = centerWorldPos,
                        ClassName = label
                    },
                    Id = n,
                    IsDangerous = isDangerousObject,
                    IsChockingHazard = isChockingHazard
                };

                filteredBoxes.Add(box);
            }

            return filteredBoxes;
        }

        private bool IsChockingHazard(float[] surroundBoxWorldDistance)
        {
            return surroundBoxWorldDistance[0] + surroundBoxWorldDistance[1] < chockingHazardMaxSize
                && surroundBoxWorldDistance[2] + surroundBoxWorldDistance[3] < chockingHazardMaxSize;
        }

        private float[] CalculateSurroundingBoxDistances(
            float centerX, float centerY, float boxWidth, float boxHeight,
            float displayWidth, float displayHeight,
            Vector2Int camRes, EnvironmentRayCastSampleManager environmentRaycast,
            Vector3 centerWorldPos)
        {
            Vector2[] vector2s = {
                new Vector2(-boxWidth / 2, 0),
                new Vector2(boxWidth / 2, 0),
                new Vector2(0, -boxHeight / 2),
                new Vector2(0, boxHeight / 2)
            };

            float[] surroundBoxWorldDistance = new float[4];
            for (int i = 0; i < vector2s.Length; i++)
            {
                Vector2 v2 = vector2s[i];
                float perX = (centerX + displayWidth/2 + v2.x) / displayWidth;
                float perY = (centerY + displayHeight/2 + v2.y) / displayHeight;
                Vector3? worldPos = CalculateWorldPosition(perX, perY, camRes, environmentRaycast);

                surroundBoxWorldDistance[i] = worldPos != null ? 
                    Vector3.Distance(centerWorldPos, (Vector3)worldPos) : 
                    Mathf.Infinity;
            }

            return surroundBoxWorldDistance;
        }

        private Vector3? CalculateWorldPosition(float perX, float perY, Vector2Int camRes, EnvironmentRayCastSampleManager environmentRaycast)
        {
            // Get the 3D marker world position using Depth Raycast
            var centerPixel = new Vector2Int(Mathf.RoundToInt(perX * camRes.x), Mathf.RoundToInt((1.0f - perY) * camRes.y));
#if !UNITY_EDITOR
            var ray = PassthroughCameraUtils.ScreenPointToRayInWorld(cameraEye, centerPixel);
#else
            if (testImageManager == null)
            {
                Debug.LogWarning("TestImageManager reference is missing. Cannot calculate world position in Editor mode.");
                return null;
            }

            // Get the raw image's transform
            var rawImageTransform = testImageManager.transform;
            var rawImagePosition = rawImageTransform.position;
            var rawImageRotation = rawImageTransform.rotation;

            // Get the raw image's dimensions in world space
            var rawImageRect = testImageManager.RawImageToDisplay.GetComponent<RectTransform>();
            if (rawImageRect == null)
            {
                Debug.LogWarning("Raw image RectTransform is missing. Cannot calculate world position in Editor mode.");
                return null;
            }

            // Calculate the world space dimensions of the raw image
            var imageWidth = rawImageRect.rect.width * rawImageRect.lossyScale.x;
            var imageHeight = rawImageRect.rect.height * rawImageRect.lossyScale.y;

            // Calculate the offset from the center of the image based on percentages
            // perX: 0 = left edge, 1 = right edge
            // perY: 0 = top edge, 1 = bottom edge
            var xOffset = (perX - 0.5f) * imageWidth;
            var yOffset = (perY - 0.5f) * imageHeight;

            // Calculate the world position by offsetting from the raw image's center
            var worldPosition = rawImagePosition + 
                              rawImageRotation * new Vector3(xOffset, yOffset, 0);


            Debug.Log($"[CalculateWorldPosition] UNITY_EDITOR {(worldPosition - debugCamera.transform.position)}; perX: {perX}; perY: {perY}; width {imageWidth}; height: {imageHeight}; Offsets x {xOffset}; y {yOffset}");
            // Create a ray from the camera to this point
            if (debugCamera == null)
            {
                Debug.LogWarning("Main camera not found. Cannot calculate world position in Editor mode.");
                return null;
            }

            var ray = new Ray(debugCamera.transform.position, (worldPosition - debugCamera.transform.position).normalized);
#endif
            var worldPos = environmentRaycast.PlaceGameObjectByScreenPos(ray);
            return worldPos;
        }
    }
} 