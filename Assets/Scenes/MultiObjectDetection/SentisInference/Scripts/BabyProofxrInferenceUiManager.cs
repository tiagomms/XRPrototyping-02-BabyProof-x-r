// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using System.Collections.Generic;
using System.Data.Common;
using DG.Tweening.Plugins.Options;
using Meta.XR.Samples;
using Unity.Burst.Intrinsics;
using Unity.Sentis;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PassthroughCameraSamples.MultiObjectDetection
{
    //[MetaCodeSample("PassthroughCameraApiSamples-MultiObjectDetection")]
    public class BabyProofxrInferenceUiManager : SentisInferenceUiManager
    {
        [Space(10)]
        [Header("Sign display references")]
        [SerializeField] private HazardOverlayManager hazardPrefabManager;

        [Header("Dangerous display references")]
        [SerializeField] private Color m_dangerousBoxColor;
        [SerializeField] private Color m_dangerousFontColor;
        

        [Header("Chocking hazard display referneces")]
        [SerializeField] private float chockingHazardMaxSize = 0.032f; // according to studies
        [SerializeField] private Color m_chockingBoxColor;
        [SerializeField] private Color m_chockingFontColor;
        
        [Header("Test in play mode")]
        [SerializeField] protected TestImageManager m_testImageManager;
        [SerializeField] protected Camera m_debugCamera;
        
        private string[] m_dangerousLabels;
        private Dictionary<int, string> m_dangerousLabelAssetDict;

        //bounding box data
        public struct BabyProofBoundingBox
        {
            public BoundingBox BaseBox;
            public int Id;
            public bool IsDangerous;
            public bool IsChockingHazard;
        }

        #region Detection Functions
        public override void OnObjectDetectionError()
        {
            base.OnObjectDetectionError();
            hazardPrefabManager.UpdateHazards(new ());
        }
        #endregion

        #region BoundingBoxes functions
        public void SetLabels(TextAsset labelsAsset, TextAsset dangerousLabels)
        {
            //Parse neural net m_labels
            m_labels = labelsAsset.text.Split('\n');

            // Register the labels of considered dangerous objects for babies
            var dangerousLabelsSplit = dangerousLabels.text.Split('\n');

            // Create dictionary            
            m_dangerousLabelAssetDict = new Dictionary<int, string>();
            foreach (string dangerousLabel in dangerousLabelsSplit)
            {
                int mlClassificationIndex = Array.IndexOf(m_labels, dangerousLabel);
                if (mlClassificationIndex >= 0)
                {
                    m_dangerousLabelAssetDict.Add(mlClassificationIndex, dangerousLabel);
                }
            }
        }

        public override void DrawUIBoxes(Tensor<float> output, Tensor<int> labelIDs, float imageWidth, float imageHeight)
        {
            // Updte canvas position
            m_detectionCanvas.UpdatePosition();

            // Clear current boxes
            ClearAnnotations();

            var displayWidth = m_displayImage.rectTransform.rect.width;
            var displayHeight = m_displayImage.rectTransform.rect.height;

            var scaleX = displayWidth / imageWidth;
            var scaleY = displayHeight / imageHeight;

            var halfWidth = displayWidth / 2;
            var halfHeight = displayHeight / 2;

            var boxesFound = output.shape[0];
            if (boxesFound <= 0)
            {
                hazardPrefabManager.UpdateHazards(new ());
                OnObjectsDetected?.Invoke(0);
                return;
            }
            var maxBoxes = Mathf.Min(boxesFound, 200);

#if !UNITY_EDITOR

            //Get the camera intrinsics
            var intrinsics = PassthroughCameraUtils.GetCameraIntrinsics(CameraEye);
            var camRes = intrinsics.Resolution;
            XRDebugLogViewer.Log($"Camera Resolution {camRes}");
#else
            // TODO: hardcoded, in the future, get image original resolution
            var camRes = new Vector2Int(1280, 960); 
#endif 
            int boxesDetected = 0;
            List<BabyProofBoundingBox> tempBoundingBoxes = new();

            //Draw the bounding boxes
            for (var n = 0; n < boxesFound; n++)
            {
                // Get bounding box center coordinates
                var centerX = output[n, 0] * scaleX - halfWidth;
                var centerY = output[n, 1] * scaleY - halfHeight;
                var boxWidth = output[n, 2] * scaleX;
                var boxHeight = output[n, 3] * scaleY;

                var centerPerX = (centerX + halfWidth) / displayWidth;
                var centerPerY = (centerY + halfHeight) / displayHeight;
                Vector3? centerWorldPos = CalculateWorldPosition(ref camRes, centerPerX, centerPerY);

                // Calculate surrounding box size in the real world (to understand if it is a chocking hazard)
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

                    float perX = (centerX + halfWidth + v2.x) / displayWidth;
                    float perY = (centerY + halfHeight + v2.y) / displayHeight;
                    Vector3? worldPos = CalculateWorldPosition(ref camRes, perX, perY);

                    surroundBoxWorldDistance[i] = worldPos != null ? Vector3.Distance((Vector3)centerWorldPos, (Vector3)worldPos) : Mathf.Infinity;
                }

                // If any of the sizes is a chocking hazard (object small on both sides)
                bool isChockingHazard = surroundBoxWorldDistance[0] + surroundBoxWorldDistance[1] < chockingHazardMaxSize
                    && surroundBoxWorldDistance[2] + surroundBoxWorldDistance[3] < chockingHazardMaxSize;

                // check if it is on the list of dangerous objects
                bool isDangerousObject = m_dangerousLabelAssetDict.ContainsKey(labelIDs[n]);

                // remove if object is not dangerous nor chockingHazard
                if (!isDangerousObject && !isChockingHazard)
                {
                    continue;
                }
                var label = m_labels[labelIDs[n]];

                XRDebugLogViewer.Log($"{label}: Dangerous: {isDangerousObject} ; ChockingHazard {isChockingHazard};\nsurroundingDistance {String.Join(';', surroundBoxWorldDistance)}\n");


                // filtered - let's do this
                // Get object class name
                var classname = m_labels[labelIDs[n]].Replace(" ", "_");
                // Create a new bounding box
                var box = new BabyProofBoundingBox
                {
                    BaseBox = new BoundingBox
                    {
                        CenterX = centerX,
                        CenterY = centerY,
                        Width = boxWidth,
                        Height = boxHeight,
                        //Label = $"Id: {n} Class: {classname} Center (px): {(int)centerX},{(int)centerY} Center (%): {centerPerX:0.00},{centerPerY:0.00}",
                        Label = $"{label}",
                        WorldPos = centerWorldPos,
                        ClassName = classname
                    },
                    Id = n,
                    IsDangerous = isDangerousObject,
                    IsChockingHazard = isChockingHazard
                };

                boxesDetected++;
                tempBoundingBoxes.Add(box);
            }
            // limit results
            boxesDetected = Mathf.Min(boxesDetected, 200);

            OnObjectsDetected?.Invoke(boxesDetected);

            for (int i = 0; i < boxesDetected; i++)
            {
                BabyProofBoundingBox box = tempBoundingBoxes[i];
                // Add to the list of boxes
                BoxDrawn.Add(box.BaseBox);

                Color color = box.IsDangerous ? m_dangerousBoxColor : m_chockingBoxColor;
                Color fontColor = box.IsDangerous ? m_dangerousFontColor : m_chockingFontColor;

                // Draw 2D box
                DrawBox(box.BaseBox, i, color, fontColor);
            }

            hazardPrefabManager.UpdateHazards(tempBoundingBoxes);
            tempBoundingBoxes.Clear();
        }


        private Vector3? CalculateWorldPosition(ref Vector2Int camRes, float perX, float perY)
        {
            // Get the 3D marker world position using Depth Raycast
            var centerPixel = new Vector2Int(Mathf.RoundToInt(perX * camRes.x), Mathf.RoundToInt((1.0f - perY) * camRes.y));
#if !UNITY_EDITOR
            var ray = PassthroughCameraUtils.ScreenPointToRayInWorld(CameraEye, centerPixel);
#else
            if (m_testImageManager == null)
            {
                Debug.LogWarning("TestImageManager reference is missing. Cannot calculate world position in Editor mode.");
                return null;
            }

            // Get the raw image's transform
            var rawImageTransform = m_testImageManager.transform;
            var rawImagePosition = rawImageTransform.position;
            var rawImageRotation = rawImageTransform.rotation;

            // Get the raw image's dimensions in world space
            var rawImageRect = m_testImageManager.RawImageToDisplay.GetComponent<RectTransform>();
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
            // perY: 0 = bottom edge, 1 = top edge
            //var xOffset = perX * imageWidth;
            //var yOffset = (0.5f - perY)* imageHeight; // Invert Y to match Unity's coordinate system
            var xOffset = (perX - 0.5f) * imageWidth;
            var yOffset = (perY - 0.5f) * imageHeight; // Invert Y to match Unity's coordinate system

            // Calculate the world position by offsetting from the raw image's center
            var worldPosition = rawImagePosition + 
                              rawImageRotation * new Vector3(xOffset, yOffset, 0);


            Debug.Log($"[CalculateWorldPosition] UNITY_EDITOR {(worldPosition - m_debugCamera.transform.position)}; perX: {perX}; perY: {perY}; width {imageWidth}; height: {imageHeight}; Offsets x {xOffset}; y {yOffset}");
            // Create a ray from the camera to this point
            if (m_debugCamera == null)
            {
                Debug.LogWarning("Main camera not found. Cannot calculate world position in Editor mode.");
                return null;
            }

            var ray = new Ray(m_debugCamera.transform.position, (worldPosition - m_debugCamera.transform.position).normalized);
#endif
            var worldPos = m_environmentRaycast.PlaceGameObjectByScreenPos(ray);
            return worldPos;
        }

        #endregion
    }
}
