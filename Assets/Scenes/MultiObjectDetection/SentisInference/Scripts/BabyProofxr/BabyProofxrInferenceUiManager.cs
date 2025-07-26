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


        #region Detection Functions
        public override void OnObjectDetectionError()
        {
            base.OnObjectDetectionError();
            m_detectionPrefabManager.UpdatePrefabs(new ());
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
        public void ProcessFilteredEntries()
        {
            OnObjectsDetected?.Invoke(CurrentBoundingBoxList.Count);
            m_detectionPrefabManager.UpdatePrefabs(CurrentBoundingBoxList);
        }

        public override void DrawBoundingBoxes()
        {
            // Draw each filtered box
            for (int i = 0; i < CurrentBoundingBoxList.Count; i++)
            {
                BoundingBox box = CurrentBoundingBoxList[i];

                Color color = box.IsDangerous ? m_dangerousBoxColor : m_chockingBoxColor;
                Color fontColor = box.IsDangerous ? m_dangerousFontColor : m_chockingFontColor;

                // Draw 2D box
                DrawBox(box, i, color, fontColor);
            }
        }
        #endregion
    }
}
