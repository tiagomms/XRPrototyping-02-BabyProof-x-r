// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using System.Collections;
using System.Collections.Generic;
using Meta.XR.Samples;
using Unity.Sentis;
using UnityEngine;

namespace PassthroughCameraSamples.MultiObjectDetection
{
    //[MetaCodeSample("PassthroughCameraApiSamples-MultiObjectDetection")]
    public class BabyProofxrInferenceRunManager : SentisInferenceRunManager
    {
        [Header("UI display references")]
        [SerializeField] private BabyProofxrInferenceUiManager m_babyProofxrUiInference;

        [Header("BabyProofxr filter")]
        [SerializeField] protected TextAsset m_dangerousLabelAssets;

        [Space(40)]

        #region Babyproofxr private variables
        private bool m_isPartOfRiskObjects = false;

        #endregion

        #region Unity Functions
        protected override IEnumerator Start()
        {
            // Wait for the UI to be ready because when Sentis load the model it will block the main thread.
            yield return new WaitForSeconds(0.05f);

            m_babyProofxrUiInference.SetLabels(m_labelsAsset, m_dangerousLabelAssets);

            LoadModel();
        }

        #endregion

        #region Public Functions

        #endregion

        #region Inference Functions

        // NOTE: Right now it is exactly the same as in the <<nameof(SentisInferenceRunManager)>>
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
                /*
                    // NOTE: stage 3 - where I need to filter and send to accurate ui box
                    if (!m_isWaiting)
                    {
                        FilterResults();
                    }
                    else
                    {
                        // NOTE: this is sync op for now
                        if (m_isPartOfRiskObjects)
                        {
                            // TODO: based on filter results determine what we do
                            m_babyProofxrUiInference.DrawUIBoxes(m_output, m_labelIDs, m_inputSize.x, m_inputSize.y);
                        }
                        m_isWaiting = false;
                        m_babyProofxrUiInference.DrawUIBoxes(m_output, m_labelIDs, m_inputSize.x, m_inputSize.y);

                        m_download_state = 5;
                    }
                */
                
                    m_babyProofxrUiInference.DrawUIBoxes(m_output, m_labelIDs, m_inputSize.x, m_inputSize.y);
                    m_download_state = 5;

                    break;
                case 4:
                    m_babyProofxrUiInference.OnObjectDetectionError();
                    m_download_state = 5;
                    break;
                case 5:
                    m_download_state++;
                    m_started = false;
                    m_output?.Dispose();
                    m_labelIDs?.Dispose();
                    break;
            }
        }
        #endregion
    }
}
