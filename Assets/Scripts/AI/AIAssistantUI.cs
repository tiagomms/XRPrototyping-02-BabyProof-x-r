using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;
using AI;
using System;
using NaughtyAttributes;

/**
 * Pure UI view for in-game HUD. No game logic, only exposes methods to update UI elements.
 */
namespace AI
{
    public class AIAssistantUI : MonoBehaviour
    {
        public enum UIState
        {
            Normal = 0,
            Processing = 1,
            Completed = 2
        }
        private UIState _uiState = UIState.Normal;

        [Header("UI - Main")]
        [SerializeField] private GameObject uiRoot;
        [SerializeField] private Animator animator;
        [SerializeField] private Vector3 _uiOriginalScale;

        [Header("UI - Circle")]
        [SerializeField] private Image circleSection;
        [SerializeField] private Color circleNormalColor = new(0.9411765f, 0.2f, 0.2152631f, 1f);
        [SerializeField] private Color circleProcessingColor = new(0.9411765f, 0.8138311f, 0.2f, 1f);
        [SerializeField] private Color circleCompletedColor = new(0.2f, 0.6039216f, 0.9411765f, 1f);

        [Header("UI - Processing Section")]
        [SerializeField] private GameObject processingSection;
        [SerializeField] private TMP_Text processingText;

        [Header("UI - App Section")]
        [SerializeField] private GameObject appSection;
        [SerializeField] private Image appIconImage;
        [SerializeField] private Sprite appIconSprite;
        [SerializeField] private Sprite appErrorSprite;

        [Space]
        [Header("Animation Settings")]
        [SerializeField, Range(0.1f, 1f)] 
        private float scaleUpDuration = 0.5f;

        [SerializeField, Range(0.1f, 3f)] 
        private float changeColorDuration = 1f;


        [SerializeField, Range(0f, 10f)] 
        private float processingSectionFloatingYOffset = 5f;

        [SerializeField, Range(0.1f, 1f)] 
        private float processingSectionFloatingDuration = 0.8f;
        [SerializeField, Range(0.1f, 1f)] 
        private float processingTextDuration = 0.3f;


        [SerializeField, Range(0.1f, 1f)] 
        private float completedHideProcessingDuration = 0.2f;
        [SerializeField, Range(0.1f, 2f)] 
        private float completedFillAmountDuration = 0.5f;
        [SerializeField, Range(0.1f, 2f)] 
        private float completedErrorDuration = 1f;
        [SerializeField, Range(0.1f, 5f)] 
        private float completedUserWaitDuration = 2f;

        [SerializeField, Range(0.1f, 1f)] 
        private float completedPunchScaleDuration = 0.2f;

        [SerializeField, Range(0.1f, 1f)] 
        private float completedScaleDownDuration = 0.3f;
        

        private Sequence _stateTweenSequence;
        private Tween _floatingTween; // For continuous floating animation
        private Sequence _dotsSequence; // For animated dots

        private void Start()
        {
            uiRoot.SetActive(false);
        }

        /// <summary>
        /// Shows the UI in normal state, ready to listen for user input
        /// </summary>
        [Button]
        public void ListenToUserRequest()
        {
            _uiState = UIState.Normal;
            
            // Complete previous animations and kill floating/dots
            _stateTweenSequence?.Complete();
            _floatingTween?.Kill();
            _dotsSequence?.Kill();
            _stateTweenSequence = DOTween.Sequence();

            // Set initial state
            animator.enabled = false;
            circleSection.color = circleNormalColor;
            appIconImage.fillAmount = 0f;
            uiRoot.SetActive(true);
            appSection.SetActive(false);
            processingSection.SetActive(false);
            
            uiRoot.transform.localScale = Vector3.zero;
            // Scale up to original size - this animation should complete
            _stateTweenSequence.Append(uiRoot.transform.DOScale(_uiOriginalScale, scaleUpDuration)
                .SetEase(Ease.OutBack)
                .SetUpdate(true)
                .SetLink(uiRoot)
                .OnComplete(() => animator.enabled = true));
        }

        /// <summary>
        /// Shows the UI in processing state with animated dots and floating text
        /// </summary>
        [Button]
        public void LoadingUserRequest()
        {
            _uiState = UIState.Processing;
            
            // Complete previous animations and kill floating/dots
            _stateTweenSequence?.Complete();
            _floatingTween?.Kill();
            _dotsSequence?.Kill();
            _stateTweenSequence = DOTween.Sequence();

            // Set initial state
            processingSection.SetActive(true);
            processingText.text = "";
            
            // Change circle color
            _stateTweenSequence.Append(circleSection.DOColor(circleProcessingColor, changeColorDuration).SetEase(Ease.OutBack));
            
            // Start animated dots using callbacks
            _dotsSequence = DOTween.Sequence();
            _dotsSequence.AppendCallback(() => processingText.text = ".");
            _dotsSequence.AppendInterval(processingTextDuration);
            _dotsSequence.AppendCallback(() => processingText.text = "..");
            _dotsSequence.AppendInterval(processingTextDuration);
            _dotsSequence.AppendCallback(() => processingText.text = "...");
            _dotsSequence.AppendInterval(processingTextDuration);
            _dotsSequence.SetLoops(-1, LoopType.Restart); // Infinite loop
            
            var resetYPosition = processingSection.transform.localPosition;
            resetYPosition.y = 0f;
            processingSection.transform.localPosition = resetYPosition;
            processingSection.transform.localScale = Vector3.one;

            // Start continuous floating animation for processing text
            _floatingTween = processingSection.transform
                .DOLocalMoveY(processingSection.transform.localPosition.y + processingSectionFloatingYOffset, processingSectionFloatingDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo); // Infinite loop with yoyo
        }

        /// <summary>
        /// Shows the UI in completed state with fill animation and punch scale effect
        /// </summary>
        [Button]
        public void CompletedUserRequest(VoiceIntent intent = VoiceIntent.Activate)
        {
            _uiState = UIState.Completed;
            
            // Complete previous animations and kill floating/dots
            _stateTweenSequence?.Complete();
            _floatingTween?.Kill();
            _dotsSequence?.Kill();
            _stateTweenSequence = DOTween.Sequence();

            // Set initial state
            appIconImage.fillAmount = 0f;
            appSection.SetActive(true);
            
            // Hide processing section and change circle color simultaneously
            _stateTweenSequence.Append(processingSection.transform.DOScale(0f, completedHideProcessingDuration)
                .SetEase(Ease.OutBack)
                .OnComplete(() => processingSection.SetActive(false)));
            _stateTweenSequence.Join(circleSection.DOColor(circleCompletedColor, changeColorDuration).SetEase(Ease.OutBack));
            
            // Small pause
            _stateTweenSequence.AppendInterval(0.3f);

            // Handle fill animation based on intent
            switch (intent)
            {
                case VoiceIntent.Activate:
                    appIconImage.sprite = appIconSprite;
                    appIconImage.fillAmount = 0f;
                    _stateTweenSequence.Append(appIconImage.DOFillAmount(1f, completedFillAmountDuration).SetEase(Ease.OutBack));
                    break;
                    
                case VoiceIntent.Deactivate:
                    appIconImage.sprite = appIconSprite;
                    appIconImage.fillAmount = 1f;
                    _stateTweenSequence.Append(appIconImage.DOFillAmount(0f, completedFillAmountDuration).SetEase(Ease.OutBack));
                    break;
                    
                default:
                    // For unknown intent, just fill to show completion
                    appIconImage.sprite = appErrorSprite;
                    appIconImage.fillAmount = 1f;
                    _stateTweenSequence.Append(appIconImage.transform.DOPunchScale(appIconImage.transform.localScale * 1.2f, completedErrorDuration).SetEase(Ease.OutBack));
                    break;
            }

            // Wait for user to see the completed state
            _stateTweenSequence.AppendInterval(completedUserWaitDuration);

            // Disable animator
            _stateTweenSequence.AppendCallback(() => { animator.enabled = false; });
            
            // Punch scale effect - scale up then down to zero
            _stateTweenSequence.Append(uiRoot.transform.DOScale(_uiOriginalScale * 1.2f, completedPunchScaleDuration).SetEase(Ease.OutBack));
            _stateTweenSequence.Append(uiRoot.transform.DOScale(Vector3.zero, completedScaleDownDuration).SetEase(Ease.InBack));
            
            // Hide UI when animation completes
            _stateTweenSequence.AppendCallback(() => uiRoot.SetActive(false));
        }
    }
}