using DG.Tweening;
using UnityEngine;

namespace XRLoopPedal.AudioSystem
{
    // Step visualizer (cubes)
    public class LoopSteps3D : MonoBehaviour
    {
        [SerializeField] private Color activeColor;
        [SerializeField] private Color inactiveColor;
        [SerializeField] private float scaleEmphasis = 1.25f;
        private MeshRenderer meshRenderer;
        private Vector3 originalScale;

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            originalScale = transform.localScale;
        }

        public void SetActive(bool isActive)
        {
            Color targetColor = isActive ? activeColor : inactiveColor;
            transform.DOScale(isActive ? originalScale * scaleEmphasis : originalScale, 0.2f);
            meshRenderer.material.DOColor(targetColor, 0.2f);
        }
    }
}