// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using System.Collections;
using System.Collections.Generic;
using Meta.XR.Samples;
using Unity.Sentis;
using UnityEngine;
using PassthroughCameraSamples.MultiObjectDetection;
using UnityEngine.InputSystem;

namespace PassthroughCameraSamples.MultiObjectDetection
{
    //[MetaCodeSample("PassthroughCameraApiSamples-MultiObjectDetection")]
    public class BabyProofxrInferenceRunManager : SentisInferenceRunManager
    {
        [Header("UI display references")]
        [SerializeField] private BabyProofxrInferenceUiManager m_babyProofxrUiInference;

        [Header("BabyProofxr filter")]
        [SerializeField] protected TextAsset m_dangerousLabelAssets;
        [SerializeField] protected TextAsset m_ignoreLabelAssets;
        [SerializeField] private float chockingHazardMaxSize = 0.032f;
        [SerializeField] private BoundingZoneManager boundingDangerZonesManager;
        [SerializeField] protected WebCamTextureManager m_webCamTextureManager;
        protected PassthroughCameraEye CameraEye => m_webCamTextureManager.Eye;

        [Header("Inference Filter Setup")]
        [SerializeField] private InputActionReference toggleFilterAction;

        [Space(40)]
        [Header("Debug")]
        [SerializeField] private Vector2Int debugImgResolution = new(1280, 960);
        [SerializeField] protected TestImageManager m_testImageManager;
        [SerializeField] protected Camera m_debugCamera;


        #region Babyproofxr private variables
        private bool m_isPartOfRiskObjects = false;
        public BabyProofxrFilter InferenceFilter {get; private set;}
        private string[] m_labels;
        private List<BabyProofxrInferenceUiManager.BabyProofBoundingBox> filteredBoxes = new();
        private Dictionary<int, string> m_ignoreLabelDict;

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

            // Initialize ignore labels dictionary
            m_ignoreLabelDict = new Dictionary<int, string>();
            if (m_ignoreLabelAssets != null)
            {
                var ignoreLabelsSplit = m_ignoreLabelAssets.text.Split('\n');
                foreach (string ignoreLabel in ignoreLabelsSplit)
                {
                    int mlClassificationIndex = Array.IndexOf(m_labels, ignoreLabel);
                    if (mlClassificationIndex >= 0)
                    {
                        m_ignoreLabelDict.Add(mlClassificationIndex, ignoreLabel);
                    }
                }
            }

            if (m_testImageManager == null || m_debugCamera == null)
            {
                Debug.LogWarning($"[{nameof(BabyProofxrInferenceRunManager)} - Play mode testing not possible. Needs a debug camera and TestImageManager]");
            }

            InferenceFilter = new BabyProofxrFilter(chockingHazardMaxSize, dangerousLabelDict, m_ignoreLabelDict, boundingDangerZonesManager, CameraEye, m_testImageManager, m_debugCamera);
            toggleFilterAction.action.started += AdjustInferenceFilter;

            LoadModel();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            toggleFilterAction.action.started -= AdjustInferenceFilter;
        }

        #endregion

        #region Public Functions

        #endregion

        #region Inference Functions

        private void AdjustInferenceFilter(InputAction.CallbackContext context)
        {
            XRDebugLogViewer.Log($"[{nameof(BabyProofxrInferenceRunManager)}] - AdjustInferenceFilter");
            InferenceFilter.ToggleIgnoreDangerZoneFilter();
        }

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
                        filteredBoxes = InferenceFilter.FilterResults(
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
                        m_isWaiting = false;
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
                    m_isWaiting = false;

                    filteredBoxes.Clear();

                    m_output?.Dispose();
                    m_labelIDs?.Dispose();
                    break;
            }
        }
        #endregion
    }
}
