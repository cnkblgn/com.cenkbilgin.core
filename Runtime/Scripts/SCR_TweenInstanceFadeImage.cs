using System;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    using static CoreUtility;

    public class TweenInstanceFadeImage : TweenInstance
    {
        protected override bool CanUpdate => thisImage != null;

        private readonly Image thisImage = null;
        private readonly Color startValue = COLOR_WHITE;
        private readonly Color targetValue = COLOR_WHITE;

        public TweenInstanceFadeImage(Image image, Color targetValue, float fadeSeconds, float waitSeconds, TweenType tweenType, EaseType easeType, Action onComplete) : base(fadeSeconds, waitSeconds, tweenType, easeType, onComplete)
        {
            if (image == null)
            {
                throw new ArgumentNullException("TweenInstanceFadeImage() << " + nameof(image));
            }

            this.thisImage = image;
            this.targetValue = targetValue;
            this.startValue = this.thisImage.color;
        }

        protected override void OnFadeUpdate(float time) => thisImage.color = Color.Lerp(startValue, targetValue, time);
        protected override void OnFadeComplete() => thisImage.color = targetValue;
    }
}
