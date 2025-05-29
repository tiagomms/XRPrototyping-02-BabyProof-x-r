// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using System.Collections;
using System.Collections.Generic;
using Meta.XR.Samples;
using Unity.Sentis;
using UnityEngine;
using PassthroughCameraSamples.MultiObjectDetection;

namespace PassthroughCameraSamples.MultiObjectDetection
{
    //[MetaCodeSample("PassthroughCameraApiSamples-MultiObjectDetection")]
    public class BabyProofxrInferenceRunManager : SentisInferenceRunManager
    {
        [Header("UI display references")]
        [SerializeField] private BabyProofxrInferenceUiManager m_babyProofxrUiInference;

        [Header("BabyProofxr filter")]
        [SerializeField] protected TextAsset m_dangerousLabelAssets;
        [SerializeField] private float chockingHazardMaxSize = 0.032f;
        [SerializeField] private BoundingZoneManager boundingDangerZonesManager;
        [SerializeField] protected WebCamTextureManager m_webCamTextureManager;
        protected PassthroughCameraEye CameraEye => m_webCamTextureManager.Eye;

        [Space(40)]
        [Header("Debug")]
        [SerializeField] private Vector2Int debugImgResolution = new(1280, 960);
        [SerializeField] protected TestImageManager m_testImageManager;
        [SerializeField] protected Camera m_debugCamera;


        #region Babyproofxr private variables
        private bool m_isPartOfRiskObjects = false;
        private BabyProofxrFilter m_filter;
        private string[] m_labels;
        private List<BabyProofxrInferenceUiManager.BabyProofBoundingBox> filteredBoxes = new();

        #endregion

        #region Unity Functions
        protected override IEnumerator Start()
        {
            // Wait for the UI to be ready because when Sentis load the model it will block the main thread.
            yield return new WaitForSeconds(0.05f);

            m_babyProofxrUiInference.SetLabels(m_labelsAsset, m_dangerousLabelAssets);
            m_labels = m_labelsAsset.text.Split('\n');

            // Initialize the filter
            var dangerousLabelDict = new Dictionary<int, string>();
            var dangerousLabelsSplit = m_dangerousLabelAssets.text.Split('\n');
            foreach (string dangerousLabel in dangerousLabelsSplit)
            {
                int mlClassificationIndex = Array.IndexOf(m_labels, dangerousLabel);
                if (mlClassificationIndex >= 0)
                {
                    dangerousLabelDict.Add(mlClassificationIndex, dangerousLabel);
                }
            }

            if (m_testImageManager == null || m_debugCamera == null)
            {
                Debug.LogWarning($"[{nameof(BabyProofxrInferenceRunManager)} - Play mode testing not possible. Needs a debug camera and TestImageManager]");
            }

            m_filter = new BabyProofxrFilter(chockingHazardMaxSize, dangerousLabelDict, boundingDangerZonesManager, CameraEye, m_testImageManager, m_debugCamera);

            LoadModel();
        }

        #endregion

        #region Public Functions

        #endregion

        #region Inference Functions

        protected override void GetInferencesResults()
        {
            // Get the different outputs in diferent frames to not block the main thread.
            switch (m_download_state)
            {
                case 1:
                    if (!m_isWaiting)
                    {
                        PollRequestOuput();
                    }
                    else
                    {
                        if (m_pullOutput.IsReadbackRequestDone())
                        {
                            m_output = m_pullOutput.ReadbackAndClone();
                            m_isWaiting = false;

                            if (m_output.shape[0] > 0)
                            {
                                Debug.Log("Sentis: m_output ready");
                                m_download_state = 2;
                            }
                            else
                            {
                                Debug.LogError("Sentis: m_output empty");
                                m_download_state = 4;
                            }
                        }
                    }
                    break;
                case 2:
                    if (!m_isWaiting)
                    {
                        PollRequestLabelIDs();
                    }
                    else
                    {
                        if (m_pullLabelIDs.IsReadbackRequestDone())
                        {
                            m_labelIDs = m_pullLabelIDs.ReadbackAndClone();
                            m_isWaiting = false;

                            if (m_labelIDs.shape[0] > 0)
                            {
                                Debug.Log("Sentis: m_labelIDs ready");
                                m_download_state = 3;
                            }
                            else
                            {
                                Debug.LogError("Sentis: m_labelIDs empty");
                                m_download_state = 4;
                            }
                        }
                    }
                    break;
                case 3:
                    if (!m_isWaiting)
                    {
                        // Get camera resolution
                        Vector2Int camRes;
#if !UNITY_EDITOR
                        var intrinsics = PassthroughCameraUtils.GetCameraIntrinsics(CameraEye);
                        camRes = intrinsics.Resolution;
#else
                        camRes = debugImgResolution;
#endif
                        // Filter the results
                        filteredBoxes = m_filter.FilterResults(
                            m_output,
                            m_labelIDs,
                            m_labels,
                            m_babyProofxrUiInference.DisplayWidth,
                            m_babyProofxrUiInference.DisplayHeight,
                            m_inputSize.x,
                            m_inputSize.y,
                            camRes,
                            m_babyProofxrUiInference.EnvironmentRaycast
                        );

                        m_isWaiting = true;
                    }
                    else
                    {
                        // Update UI with filtered results
                        m_babyProofxrUiInference.ProcessFilteredEntries(filteredBoxes);
                        m_download_state = 5;
                    }
                    break;
                case 4:
                    m_babyProofxrUiInference.OnObjectDetectionError();
                    m_download_state = 5;
                    break;
                case 5:
                    m_download_state++;
                    m_started = false;

                    filteredBoxes.Clear();

                    m_output?.Dispose();
                    m_labelIDs?.Dispose();
                    break;
            }
        }
        #endregion
    }
}
