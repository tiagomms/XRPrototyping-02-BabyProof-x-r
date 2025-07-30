using System;
using System.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace AI
{
    public class AIAssistant : MonoBehaviour
    {
        public enum State
        {
            None = 0, // when nothing (default)
            Selected = 1, // when selected by user
            OnHold = 2 // when calling the API
        }

        // Singleton instance
        public static AIAssistant Instance { get; private set; }

        [Header("Components")]
        [SerializeField] private MicRecorder micRecorder;
        [SerializeField] private WhisperTranscriber speech2TextAI;
        [SerializeField] private BaseAIReasoning aiReasoning;

        [Header("UI")]
        [SerializeField] private AIAssistantUI aiAssistantUI;

        [Space]
        [Header("Debug")]
        [SerializeField] protected State state;
        public bool IsRecordingUser { get; protected set; }

        public event Action<bool> OnRecordingStateChanged;

        private void Awake()
        {
            // Singleton pattern implementation
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        protected virtual void OnEnable()
        {
            // Validate AppManager is properly initialized
            if (!AppManager.IsValid())
            {
                Debug.LogError("[AIAssistant] CRITICAL ERROR: AppManager is not properly initialized! AIAssistant cannot function without AppManager.");
                enabled = false;
                return;
            }

            if (MicRecorderManager.Instance != null)
            {
                MicRecorderManager.Instance.RegisterRecorder(micRecorder);
            }
            // TODO: load files - create my assistant 
            // NOTE: events  
            micRecorder.onRecordedAudio.AddListener(ProcessUserIntent);
            micRecorder.onDurationPassed.AddListener(ProcessUserIntent);
        }

        protected virtual void OnDisable()
        {
            if (MicRecorderManager.Instance != null)
            {
                MicRecorderManager.Instance.UnregisterRecorder(micRecorder);
            }


            micRecorder.onRecordedAudio.RemoveListener(ProcessUserIntent);
            micRecorder.onDurationPassed.RemoveListener(ProcessUserIntent);
        }

        #region Handle User Recordings
        [Button]
        public virtual void ToggleRecording()
        {
            if (!IsRecordingUser)
            {
                StartRecordingUser();
            }
            else
            {
                StopRecording();
            }
        }

        protected virtual void StartRecordingUser()
        {
            // NOTE: only record when it is over
            if (state == State.OnHold) return;

            state = State.OnHold;
            micRecorder.StartRecording();
            IsRecordingUser = true;

            OnRecordingStateChanged?.Invoke(true);
            aiAssistantUI.ListenToUserRequest();
        }

        protected virtual void StopRecording()
        {
            // this triggers the onRecordedAudio once it is saved and we can proceed with everything
            // from UserRecordedIntent

            // NOTE: check AppManager to see if AI is enabled (if not discard the recording)
            bool isAIEnabled = AppManager.Instance.Config.aiState == AppManagerConfig.AIState.Enabled;
            FileEnumPath saveClipPath = isAIEnabled ? FileEnumPath.Temporary : FileEnumPath.None;
            micRecorder.StopAndSave(isAIEnabled, saveClipPath);

            // TODO: to delete these mic calls every time or on app close 
            IsRecordingUser = false;

            OnRecordingStateChanged?.Invoke(false);
        }

        /// <summary>
        /// Method that will process the user intent, transcribe it, and call the AI Reasoning to parse and handle the intent
        /// </summary>
        /// <param name="arg0">ignored</param>
        protected virtual async void ProcessUserIntent(AudioClip arg0)
        {
            try
            {
                // stop recording if it is still recording
                if (IsRecordingUser)
                {
                    StopRecording();
                }
                aiAssistantUI.LoadingUserRequest();

                VoiceIntent intent;
                bool isAIEnabled = AppManager.Instance.Config.aiState == AppManagerConfig.AIState.Enabled;
                if (isAIEnabled)
                {
                    // AI is enabled: transcribe speech to text, then parse intent
                    
                    // transcribe user intent from mic recording
                    string newUserIntent = await speech2TextAI.TranscribeAsync(micRecorder.GetLastFilePath());

                    // AI Reasoning returns intent
                    intent = await aiReasoning.ParseIntent(newUserIntent);
                }
                else
                {
                    // fake it - returned intent is the opposite of the current state
                    await Task.Delay(2000);
                    intent = AppManager.Instance.IsAppRunning ? VoiceIntent.Deactivate : VoiceIntent.Activate;
                }

                // AI Assistant handles intent
                switch (intent)
                {
                    case VoiceIntent.Activate:
                        if (AppManager.Instance.IsAppRunning)
                        {
                            Debug.Log("AI Assistant: Can't Start App, it is already running");
                            break;
                        }
                        AppManager.Instance.StartApp();
                        Debug.Log("AI Assistant: Activate");
                        break;
                    case VoiceIntent.Deactivate:
                        if (!AppManager.Instance.IsAppRunning)
                        {
                            Debug.Log("AI Assistant: Can't Shutdown App, it is already not running");
                            break;
                        }
                        AppManager.Instance.ShutdownApp();
                        Debug.Log("AI Assistant: Deactivate");
                        break;
                }
                aiAssistantUI.CompletedUserRequest(intent);
                // reset state  
                state = State.None;
            }
            catch (Exception e)
            {
                Debug.LogError($"[{nameof(AIAssistant)}] Error on transcribing user recorded intent: {e}");
                state = State.None;
                return;
            }
        }

        #endregion


    }

}
