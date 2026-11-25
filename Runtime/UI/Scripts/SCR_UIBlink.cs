
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
        [SerializeField, Min(0.01f)] private float fadeSpeed = 2f;

        private CanvasGroup thisCanvas;
        private bool isStarted;
        private bool hasTimer;
        private bool isFading;

        private float timer;
        private float blinkTimer;

        private void Awake()
        {
            thisCanvas = GetComponent<CanvasGroup>();

            if (startOnAwake)
            {
                StartBlink();
            }
            else
            {
                thisCanvas.alpha = 0;
            }
        }

        private void LateUpdate()
        {
            if (!isStarted || ManagerGame.Instance.GetGameState() != GameState.RESUME)
            {
                return;
            }

            if (hasTimer)
            {
                timer -= Time.deltaTime;

                if (timer <= 0)
                {
                    isFading = true;
                }
            }

            if (isFading)
            {
                thisCanvas.alpha = Mathf.MoveTowards(thisCanvas.alpha, 0f, fadeSpeed * Time.deltaTime);

                if (thisCanvas.alpha <= 0f)
                {
                    StopBlink();
                }

                return;
            }

            blinkTimer += Time.deltaTime * speed;
            thisCanvas.alpha = Mathf.PingPong(blinkTimer, 1f);
        }

        public void StartBlink(float duration)
        {
            hasTimer = true;
            timer = duration;
            StartBlink();
        }

        public void StartBlink()
        {
            blinkTimer = 0f;
            isStarted = true;
            isFading = false;
            hasTimer = false;
            thisCanvas.alpha = 0f;
        }

        public void StopBlink()
        {
            thisCanvas.alpha = 0f;
            isStarted = false;
            hasTimer = false;
            isFading = false;
            blinkTimer = 0f;
        }
    }
}
