using UnityEngine;
using UnityEngine.Events;

namespace PassthroughCameraSamples.MultiObjectDetection
{
    public class BabyProofxrPalmMenuButtonHandlers : MonoBehaviour
    {

        [SerializeField]
        private GameObject _babyProofxrEnabledIcon;

        [SerializeField]
        private GameObject _babyProofxrDisabledIcon;


        [SerializeField]
        private GameObject _boundaryParent;
        [SerializeField]
        private GameObject _boundaryEnabledIcon;

        [SerializeField]
        private GameObject _boundaryDisabledIcon;


        private bool _babyProofxrEnabled;
        private bool _boundaryEnabled;

        public UnityEvent<bool> OnBabyProofxrEnabled;
        public UnityEvent<bool> OnBoundaryEnabled;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            SetBabyProofxrEnabled(false);
            SetBoundaryEnabled(false);
        }

        private void SetBabyProofxrEnabled(bool enabled)
        {
            _babyProofxrEnabled = enabled;
            _babyProofxrEnabledIcon.SetActive(!_babyProofxrEnabled);
            _babyProofxrDisabledIcon.SetActive(_babyProofxrEnabled);
            _boundaryParent.SetActive(_babyProofxrEnabled);
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
            SetBabyProofxrEnabled(!_babyProofxrEnabled);

            // TODO: add a check to see if the boundary is enabled and if so, disable stuff
            OnBabyProofxrEnabled?.Invoke(_babyProofxrEnabled);
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