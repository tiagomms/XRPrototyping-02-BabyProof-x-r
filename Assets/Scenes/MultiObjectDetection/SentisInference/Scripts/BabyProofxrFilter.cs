using System.Collections.Generic;
using UnityEngine;
using PassthroughCameraSamples.MultiObjectDetection;

namespace PassthroughCameraSamples.MultiObjectDetection
{
    /// <summary>
    /// Handles filtering of detected objects based on various criteria for BabyProofxr.
    /// </summary>
    public class BabyProofxrFilter
    {
        private readonly float chockingHazardMaxSize;
        private readonly Dictionary<int, string> dangerousLabelDict;

        public BabyProofxrFilter(float chockingHazardMaxSize, Dictionary<int, string> dangerousLabelDict)
        {
            this.chockingHazardMaxSize = chockingHazardMaxSize;
            this.dangerousLabelDict = dangerousLabelDict;
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
            var boxesFound = output.shape[0];
            if (boxesFound <= 0)
            {
                return new List<BabyProofxrInferenceUiManager.BabyProofBoundingBox>();
            }

            var scaleX = displayWidth / imageWidth;
            var scaleY = displayHeight / imageHeight;
            var halfWidth = displayWidth / 2;
            var halfHeight = displayHeight / 2;

            List<BabyProofxrInferenceUiManager.BabyProofBoundingBox> filteredBoxes = new();

            for (var n = 0; n < boxesFound; n++)
            {
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

                // Check if object is a chocking hazard
                bool isChockingHazard = IsChockingHazard(surroundBoxWorldDistance);

                // Check if object is in dangerous objects list
                bool isDangerousObject = dangerousLabelDict.ContainsKey(labelIDs[n]);

                // Skip if object is neither dangerous nor a chocking hazard
                if (!isDangerousObject && !isChockingHazard)
                {
                    continue;
                }

                string label = labels[labelIDs[n]].Trim().Replace(" ", "_").Replace("\n", "_").Replace("\r", "_").Replace("\t", "_");

                // Create bounding box
                var box = new BabyProofxrInferenceUiManager.BabyProofBoundingBox
                {
                    BaseBox = new BoundingBox
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

        private Vector3? CalculateWorldPosition(
            float perX, float perY,
            Vector2Int camRes,
            EnvironmentRayCastSampleManager environmentRaycast)
        {
            var centerPixel = new Vector2Int(
                Mathf.RoundToInt(perX * camRes.x),
                Mathf.RoundToInt((1.0f - perY) * camRes.y)
            );

#if !UNITY_EDITOR
            var ray = PassthroughCameraUtils.ScreenPointToRayInWorld(CameraEye, centerPixel);
#else
            // In editor mode, we'll need to handle this differently
            // For now, return null to indicate we can't calculate world position
            return null;
#endif
            return environmentRaycast.PlaceGameObjectByScreenPos(ray);
        }
    }
} 