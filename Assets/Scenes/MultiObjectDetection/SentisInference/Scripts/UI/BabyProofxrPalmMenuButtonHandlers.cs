using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace PassthroughCameraSamples.MultiObjectDetection
{
    public class BabyProofxrPalmMenuButtonHandlers : MonoBehaviour
    {
        [Header("BabyProofxr Icons")]
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

        [Header("Audio")]
        [SerializeField]
        private AudioSource? _ambientSound;

        [SerializeField]
        private AudioSource? _activateSound;

        [SerializeField]
        private AudioSource? _deactivateSound;


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

        private void SetEnableGameSounds(bool enabled)
        {
            if (enabled)
            {
                BabyProofxrAudioManager.Instance.FadeAllToSilence();
                BabyProofxrAudioManager.Instance.FadeSFXToFull();
                
                _deactivateSound.SafeStop();
                
                _ambientSound.SafePlayDelayed(0.5f);
                // NOTE: if robot voice (0.5f) , if activate sound - immediately
                _activateSound.SafePlayDelayed(0.5f);
                //_activateSound.SafePlay();

                BabyProofxrAudioManager.Instance.FadeMusicToFull(duration: 1.0f, delay: 2.0f);
            }
            else
            {
                _activateSound.SafeStop();
                _deactivateSound.SafePlayDelayed(0.5f);
                BabyProofxrAudioManager.Instance.FadeAllToSilence(
                    duration: 1.0f, 
                    delay: 0.5f, 
                    onComplete: () => { _ambientSound.SafeStop(); }
                );                

            }

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
            SetEnableGameSounds(_babyProofxrEnabled);

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