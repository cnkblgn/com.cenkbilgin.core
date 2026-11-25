
using UnityEngine;
using Core;

namespace Game
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class UIBlink : MonoBehaviour
    {
        [Header("_")]
        [SerializeField] private bool startOnAwake = false;
        [SerializeField, Min(0.1f)] private float speed = 1f;
        [SerializeField, Min(0.01f)] private float fadeDuration = 0.5f;

        private CanvasGroup thisCanvas;
        private bool isStarted;
        private bool hasTimer;
        private bool isFading;

        private float timer;
        private float blinkTimer;
        private float fadeTimer;
        private float fadeStartAlpha;

        private void Awake()
        {
            thisCanvas = GetComponent<CanvasGroup>();

            if (startOnAwake)
            {
                StartBlink();
            }
            else
            {
                thisCanvas.alpha = 0f;
            }
        }

        private void LateUpdate()
        {
            if (!isStarted || ManagerGame.Instance.GetGameState() != GameState.RESUME)
            {
                return;
            }

            if (isFading)
            {
                fadeTimer += Time.deltaTime;

                float t = Mathf.Clamp01(fadeTimer / fadeDuration);
                thisCanvas.alpha = Mathf.Lerp(fadeStartAlpha, 0f, t);

                if (t >= 1f)
                {
                    StopBlink();
                }

                return;
            }

            if (hasTimer)
            {
                timer -= Time.deltaTime;

                if (timer <= 0f)
                {
                    BeginFade();
                    return;
                }
            }

            blinkTimer += Time.deltaTime * speed;
            thisCanvas.alpha = Mathf.PingPong(blinkTimer, 1f);
        }

        private void BeginFade()
        {
            isFading = true;
            fadeTimer = 0f;
            fadeStartAlpha = thisCanvas.alpha;
        }

        public void StartBlink(float duration)
        {
            StartBlink();
            hasTimer = true;
            timer = duration;
        }

        public void StartBlink()
        {
            isStarted = true;
            hasTimer = false;
            isFading = false;
            blinkTimer = 0f;
            thisCanvas.alpha = 0f;
        }

        public void StopBlink()
        {
            isStarted = false;
            hasTimer = false;
            isFading = false;
            thisCanvas.alpha = 0f;
        }
    }
}
