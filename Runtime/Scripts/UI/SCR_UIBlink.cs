using UnityEngine;
using Core;

namespace Game
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public class UIBlink : MonoBehaviour
    {
        [Header("_")]
        [SerializeField] private bool startOnAwake = false;
        [SerializeField, Min(0.1f)] private float speed = 1f;

        private CanvasGroup thisCanvas = null;
        private bool isStarted = false;

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

            if (ManagerCoreGame.Instance.GetGameState() != GameState.RESUME)
            {
                return;
            }

            thisCanvas.alpha = Mathf.PingPong(Time.time * speed, 1f);
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
        }
    }
}
