using AI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PassthroughCameraSamples.MultiObjectDetection
{
    public class BabyProofxrPalmMenuButtonHandlers : MonoBehaviour
    {
        
        [Header("AI Assistant Icons")]
        [SerializeField]
        private Button _aiAssistantButton;
        [SerializeField]
        private GameObject _aiAssistantEnabledIcon;

        [SerializeField]
        private GameObject _aiAssistantDisabledIcon;

        [Header("BabyProofxr Icons")]
        [SerializeField]
        private Button _babyProofxrButton;
        [SerializeField]
        private GameObject _babyProofxrEnabledIcon;

        [SerializeField]
        private GameObject _babyProofxrDisabledIcon;


        [Header("Boundary Icons")]
        [SerializeField]
        private GameObject _boundaryParent;
        [SerializeField]
        private GameObject _boundaryEnabledIcon;

        [SerializeField]
        private GameObject _boundaryDisabledIcon;

        private bool _boundaryEnabled;

        public UnityEvent<bool> OnBoundaryEnabled;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            SetAiAssistantEnabled(false);

            SetBabyProofxrEnabled(false);
            SetBoundaryEnabled(false);
        }

        private void Start()
        {
            // Subscribe to AppManager state changes
            if (AppManager.Instance != null)
            {
                AppManager.Instance.OnAppStateChanged.AddListener(SetBabyProofxrEnabled);
            }
            else
            {
                Debug.LogWarning("AppManager.Instance is null - BabyProofxr button will be disabled");
            }

            if (AIAssistant.Instance != null)
            {
                AIAssistant.Instance.OnRecordingStateChanged += SetAiAssistantEnabled;
            }
            else
            {
                Debug.LogWarning("AIAssistant.Instance is null - AI Assistant button will be disabled");
            }

            UpdateBabyProofxrButtonState();
            UpdateAiAssistantButtonState();
        }

        private void OnDestroy()
        {
            // Unsubscribe from AppManager events
            if (AppManager.Instance != null)
            {
                AppManager.Instance.OnAppStateChanged.RemoveListener(SetBabyProofxrEnabled);
            }

            if (AIAssistant.Instance != null)
            {
                AIAssistant.Instance.OnRecordingStateChanged -= SetAiAssistantEnabled;
            }
            
        }

        /// <summary>
        /// Updates the BabyProofxr button state based on AppManager's running state.
        /// </summary>
        private void UpdateBabyProofxrButtonState()
        {
            if (AppManager.Instance == null)
            {
                _babyProofxrButton.interactable = false;
                return;
            }

            bool isAppRunning = AppManager.Instance.IsAppRunning;
            SetBabyProofxrEnabled(isAppRunning);
        }

        /// <summary>
        /// Updates the AI Assistant button state.
        /// </summary>
        private void UpdateAiAssistantButtonState()
        {
            if (AIAssistant.Instance == null)
            {
                _aiAssistantButton.interactable = false;
            }

            bool isRecording = AIAssistant.Instance.IsRecordingUser;
            SetAiAssistantEnabled(isRecording);
        }

        /// <summary>
        /// Sets the BabyProofxr enabled state based on app running status.
        /// </summary>
        /// <param name="enabled">Whether the app is running</param>
        private void SetBabyProofxrEnabled(bool enabled)
        {
            _babyProofxrEnabledIcon.SetActive(!enabled);
            _babyProofxrDisabledIcon.SetActive(enabled);
            _boundaryParent.SetActive(enabled);
        }

        /// <summary>
        /// Sets the AI Assistant enabled state.
        /// </summary>
        /// <param name="enabled">Whether the AI assistant is enabled</param>
        private void SetAiAssistantEnabled(bool enabled)
        {
            _aiAssistantEnabledIcon.SetActive(!enabled);
            _aiAssistantDisabledIcon.SetActive(enabled);
        }

        private void SetBoundaryEnabled(bool enabled)
        {
            _boundaryEnabled = enabled;
            _boundaryEnabledIcon.SetActive(!_boundaryEnabled);
            _boundaryDisabledIcon.SetActive(_boundaryEnabled);
        }

        /// <summary>
        /// Toggle whether or not babyProofxr is enabled, and set the icon of the controlling button to display what will happen next time the button is pressed.
        /// </summary>
        public void ToggleBabyProofxrEnabled()
        {
            if (AppManager.Instance == null)
            {
                Debug.LogWarning("Cannot toggle BabyProofxr - AppManager.Instance is null");
                return;
            }

            // Toggle app state through AppManager
            AppManager.Instance.ToggleAppState();
        }

        /// <summary>
        /// Toggle whether or not AI assistant is enabled, and set the icon of the controlling button to display what will happen next time the button is pressed.
        /// </summary>
        public void ToggleAiAssistantEnabled()
        {
            if (AIAssistant.Instance == null)
            {
                Debug.LogWarning("Cannot toggle AI Assistant - AIAssistant.Instance is null");
                return;
            }

            AIAssistant.Instance.ToggleRecording();
        }

        /// <summary>
        /// Toggle whether or not boundary is enabled, and set the icon of the controlling button to display what will happen next time the button is pressed.
        /// </summary>
        public void ToggleBoundaryEnabled()
        {
            SetBoundaryEnabled(!_boundaryEnabled);

            OnBoundaryEnabled?.Invoke(_boundaryEnabled);

        }
    }
}