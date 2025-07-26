using UnityEngine;

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
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            // yes, silly - we enable to true, so they become false
            _babyProofxrEnabled = true;
            ToggleBabyProofxrEnabled();
            
            _boundaryEnabled = true;
            ToggleBoundaryEnabled();
            _boundaryParent.SetActive(false);
        }

        /// <summary>
        /// Toggle whether or not babyProofxr is enabled, and set the icon of the controlling button to display what will happen next time the button is pressed.
        /// </summary>
        public void ToggleBabyProofxrEnabled()
        {
            _babyProofxrEnabled = !_babyProofxrEnabled;
            _babyProofxrEnabledIcon.SetActive(!_babyProofxrEnabled);
            _babyProofxrDisabledIcon.SetActive(_babyProofxrEnabled);
            
            _boundaryParent.SetActive(_babyProofxrEnabled);
            // TODO: add a check to see if the boundary is enabled and if so, disable stuff
        }

        /// <summary>
        /// Toggle whether or not boundary is enabled, and set the icon of the controlling button to display what will happen next time the button is pressed.
        /// </summary>
        public void ToggleBoundaryEnabled()
        {
            _boundaryEnabled = !_boundaryEnabled;
            _boundaryEnabledIcon.SetActive(!_boundaryEnabled);
            _boundaryDisabledIcon.SetActive(_boundaryEnabled);
        }
    }
}