using UnityEngine;

namespace Core.UI
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public class UIRenderObject : MonoBehaviour
    {
        [Header("_")]
        [SerializeField] private Canvas thisCanvas = null;
        [SerializeField] private Camera thisCamera = null;
        [SerializeField, Min(1)] private float frameRate = 24;

        private float frameTime = 0f;

        private void Awake() => thisCamera.enabled = false;
        private void Update()
        {
            if (!thisCanvas.enabled)
            {
                return;
            }

            if (ManagerCoreGame.Instance.GetGameState() != GameState.RESUME)
            {
                return;
            }

            frameTime += Time.deltaTime;

            if (frameTime > 1 / frameRate)
            {
                frameTime = 0;
                thisCamera.Render();
            }
        }

        public bool IsActive() => thisCanvas.enabled;
        public void Show() => thisCanvas.Show();
        public void Hide() => thisCanvas.Hide();
    }
}
