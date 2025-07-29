using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PassthroughCameraSamples.MultiObjectDetection
{
    public class BabyProofxrPalmMenuButtonHandlers : MonoBehaviour
    {
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

            UpdateBabyProofxrButtonState();
        }

        private void OnDestroy()
        {
            // Unsubscribe from AppManager events
            if (AppManager.Instance != null)
            {
                AppManager.Instance.OnAppStateChanged.RemoveListener(SetBabyProofxrEnabled);
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
                DisableBabyProofxrButton();
                return;
            }

            bool isAppRunning = AppManager.Instance.IsAppRunning;
            SetBabyProofxrEnabled(isAppRunning);
        }

        /// <summary>
        /// Disables the BabyProofxr button when AppManager is not available.
        /// </summary>
        private void DisableBabyProofxrButton()
        {
            // Show disabled state
            _babyProofxrEnabledIcon.SetActive(false);
            _babyProofxrDisabledIcon.SetActive(true);
            _boundaryParent.SetActive(false);
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
        /// Toggle whether or not boundary is enabled, and set the icon of the controlling button to display what will happen next time the button is pressed.
        /// </summary>
        public void ToggleBoundaryEnabled()
        {
            SetBoundaryEnabled(!_boundaryEnabled);

            OnBoundaryEnabled?.Invoke(_boundaryEnabled);
        }
    }
}