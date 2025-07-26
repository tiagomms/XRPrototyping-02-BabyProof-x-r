using Oculus.Interaction;
using UnityEngine;

namespace PassthroughCameraSamples.MultiObjectDetection
{
    public class BabyProofxrPalmMenu : MonoBehaviour
    {
        [SerializeField]
        private PokeInteractable _menuInteractable;

        [SerializeField]
        private GameObject _menuParent;

        [SerializeField]
        private RectTransform _menuPanel;

        [SerializeField]
        private RectTransform[] _buttons;



        [SerializeField]
        private AudioSource _showMenuAudio;

        [SerializeField]
        private AudioSource _hideMenuAudio;

        
        /// <summary>
        /// Show/hide the menu.
        /// </summary>
        public void ToggleMenu()
        {
            if (_menuParent.activeSelf)
            {
                _hideMenuAudio.Play();
                _menuParent.SetActive(false);
            }
            else
            {
                _showMenuAudio.Play();
                _menuParent.SetActive(true);
            }
        }
    }
}