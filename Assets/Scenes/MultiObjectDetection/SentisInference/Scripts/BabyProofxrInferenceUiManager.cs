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
        [SerializeField] private bool shouldDisplayBoxes = false;

        [Header("Dangerous display references")]
        [SerializeField] private Color m_dangerousBoxColor;
        [SerializeField] private Color m_dangerousFontColor;
        

        [Header("Chocking hazard display referneces")]
        [SerializeField] private float chockingHazardMaxSize = 0.032f; // according to studies
        [SerializeField] private Color m_chockingBoxColor;
        [SerializeField] private Color m_chockingFontColor;
        
        private string[] m_dangerousLabels;
        private Dictionary<int, string> m_dangerousLabelAssetDict;

        // Public properties for the filter
        public float DisplayWidth => m_displayImage.rectTransform.rect.width;
        public float DisplayHeight => m_displayImage.rectTransform.rect.height;
        public EnvironmentRayCastSampleManager EnvironmentRaycast => m_environmentRaycast;

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

        /// <summary>
        /// Draws UI boxes for pre-filtered bounding boxes
        /// </summary>
        public void ProcessFilteredEntries(List<BabyProofBoundingBox> filteredBoxes)
        {
            
            OnObjectsDetected?.Invoke(filteredBoxes.Count);
            
            if (shouldDisplayBoxes)
            {
                // Update canvas position
                m_detectionCanvas.UpdatePosition();
                // Clear current boxes
                ClearAnnotations();
                DrawUIBoxes(filteredBoxes);
            }

            hazardPrefabManager.UpdateHazards(filteredBoxes);
        }

        private void DrawUIBoxes(List<BabyProofBoundingBox> filteredBoxes)
        {
            // Draw each filtered box
            for (int i = 0; i < filteredBoxes.Count; i++)
            {
                BabyProofBoundingBox box = filteredBoxes[i];
                // Add to the list of boxes
                BoxDrawn.Add(box.BaseBox);

                Color color = box.IsDangerous ? m_dangerousBoxColor : m_chockingBoxColor;
                Color fontColor = box.IsDangerous ? m_dangerousFontColor : m_chockingFontColor;

                // Draw 2D box
                DrawBox(box.BaseBox, i, color, fontColor);
            }
        }

        // Keep the original method for backward compatibility
        public override void DrawUIBoxes(Tensor<float> output, Tensor<int> labelIDs, float imageWidth, float imageHeight)
        {
            // This method is now deprecated and should not be used
            Debug.LogWarning("Using deprecated DrawUIBoxes method. Please use the filtered version instead.");
        }

        /*
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
        */
        #endregion
    }
}
