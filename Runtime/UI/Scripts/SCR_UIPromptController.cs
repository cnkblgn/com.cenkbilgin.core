using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    internal sealed class UIPromptController : MonoBehaviour, IGameStateHandler
    {
        [Header("_")]
        [SerializeField, Required] private UIPromptView[] views;

        private Canvas thisCanvas = null;

        private void Start()
        {
            thisCanvas = GetComponent<Canvas>();
            thisCanvas.Show();
            Hide();
        }
        private void OnEnable() => ManagerGame.BindHandler(this);
        private void OnDisable() => ManagerGame.UnbindHandler(this);

        public void Show<TContext>(string description, in TContext context) where TContext : struct
        {
            for (int i = 0; i < views.Length; i++)
            {
                if (views[i].TryShow(description, context))
                {
                    return;
                }
            }
        }
        public void Hide()
        {
            for (int i = 0; i < views.Length; i++)
            {
                if (views[i].TryHide())
                {
                    return;
                }
            }
        }

        public bool HandleCanResumeGame()
        {
            for (int i = 0; i < views.Length; i++)
            {
                if (views[i].IsActive)
                {
                    views[i].TryHide();
                    return false;
                }
            }

            return true;
        }
        public bool HandleCanPauseGame()
        {
            for (int i = 0; i < views.Length; i++)
            {
                if (views[i].IsActive)
                {
                    views[i].TryHide();
                    return false;
                }
            }

            return true;
        }
    }
}
