using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Core.UI
{
    using static CoreUtility;

    [RequireComponent(typeof(Image))]
    public class UIPointerEventHighlight : UIPointerEvent
    {
        [Header("_")]
        [SerializeField] private Color highlightColor = COLOR_YELLOW;

        [Header("_")]
        [SerializeField] private EaseType fadeEasingType = EaseType.LINEAR;
        [SerializeField, Min(0.01f)] private float fadeDuration = 0.1f;

        [Header("_")]
        [SerializeField] private bool influenceChild = false;

        private Image thisImage = null;
        private Image[] childImages = null;
        private TweenInstanceFadeImage thisTween = null;
        private TweenInstanceFadeImage[] childTweens = null;
        private Color defaultColor = COLOR_WHITE;

        private void Awake()
        {
            thisImage = GetComponent<Image>();
            defaultColor = thisImage.color;

            if (influenceChild)
            {
                childImages = GetComponentsInChildren<Image>();
                childTweens = new TweenInstanceFadeImage[childImages.Length];
            }
        }

        protected override void OnSelectInternal(BaseEventData eventData)
        {
            base.OnSelectInternal(eventData);

            HighlightColor();
        }
        protected override void OnDeselectInternal(BaseEventData eventData)
        {
            base.OnDeselectInternal(eventData);

            RevertColor();
        }
        protected override void OnPointerEnterInternal(PointerEventData eventData)
        {
            base.OnPointerEnterInternal(eventData);

            HighlightColor();
        }
        protected override void OnPointerExitInternal(PointerEventData eventData)
        {
            base.OnPointerExitInternal(eventData);

            RevertColor();
        }

        private void RevertColor()
        {
            if (!gameObject.activeSelf)
            {
                return;
            }

            if (fadeDuration <= 0)
            {
                RevertColorInstant();
                return;
            }

            thisTween?.Kill();
            thisTween = thisImage.Fade(defaultColor, fadeDuration, 0, TweenType.UNSCALED, fadeEasingType);

            if (influenceChild)
            {
                for (int i = 0; i < childImages.Length; i++)
                {
                    childTweens[i]?.Kill();
                    childTweens[i] = childImages[i].Fade(defaultColor, fadeDuration, 0, TweenType.UNSCALED, fadeEasingType);
                }
            }
        }
        private void RevertColorInstant()
        {
            thisImage.color = defaultColor;

            if (influenceChild)
            {
                for (int i = 0; i < childImages.Length; i++)
                {
                    childImages[i].color = defaultColor;
                }
            }
        }
        private void HighlightColor()
        {
            if (!gameObject.activeSelf)
            {
                return;
            }

            if (fadeDuration <= 0)
            {
                HighlightColorInstant();
                return;
            }

            thisTween?.Kill();
            thisTween = thisImage.Fade(highlightColor, fadeDuration, 0, TweenType.UNSCALED, fadeEasingType);

            if (influenceChild)
            {
                for (int i = 0; i < childImages.Length; i++)
                {
                    childTweens[i]?.Kill();
                    childTweens[i] = childImages[i].Fade(highlightColor, fadeDuration, 0, TweenType.UNSCALED, fadeEasingType);
                }
            }
        }
        private void HighlightColorInstant()
        {
            thisImage.color = highlightColor;

            if (influenceChild)
            {
                for (int i = 0; i < childImages.Length; i++)
                {
                    childImages[i].color = highlightColor;
                }
            }
        }
    }
}