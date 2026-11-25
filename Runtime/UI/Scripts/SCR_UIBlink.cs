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

        private CanvasGroup thisCanvas = null;
        private bool isStarted = false;
        private bool hasTimer = false;
        private float timer;
        private float duration;

        private void Awake()
        {
            thisCanvas = GetComponent<CanvasGroup>();

            if (startOnAwake)
            {
                StartBlink();
            }
        }
        private void LateUpdate()
        {
            if (!isStarted)
            {
                return;
            }

            if (ManagerGame.Instance.GetGameState() != GameState.RESUME)
            {
                return;
            }

            if (hasTimer)
            {
                timer -= Time.deltaTime;

                if (timer <= 0)
                {
                    thisCanvas.alpha = Mathf.Lerp(thisCanvas.alpha, 0, Time.deltaTime * 2f);

                    if (thisCanvas.alpha <= 0.000001f)
                    {
                        StopBlink();
                    }

                    return;
                }
            }

            thisCanvas.alpha = Mathf.PingPong(Time.time * speed, 1f);
        }

        public void StartBlink(float duration)
        {
            hasTimer = true;
            this.duration = duration;
            this.timer = this.duration;
        }
        public void StartBlink()
        {
            thisCanvas.alpha = 0;
            isStarted = true;
        }
        public void StopBlink()
        {
            thisCanvas.alpha = 0;
            isStarted = false;
            hasTimer = false;
        }
    }
}
