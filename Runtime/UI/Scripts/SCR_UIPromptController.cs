using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    internal sealed class UIPromptController : MonoBehaviour, IUICursorStateHandler, IGameStateHandler
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
        private void OnEnable()
        {
            ManagerGame.BindHandler(this);

            UICursorController.BindHandler(this);
        }
        private void OnDisable()
        {
            ManagerGame.UnbindHandler(this);

            UICursorController.UnbindHandler(this);
        }

        public void Show<TContext>(string description, in TContext context) where TContext : struct
        {
            for (int i = 0; i < views.Length; i++)
            {
                if (views[i].TryShow(description, context))
                {
                    ManagerUI.Instance.ShowCursor();
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
                    ManagerUI.Instance.HideCursor();
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

        public bool HandleCanShowCursor() => true;
        public bool HandleCanHideCursor()
        {
            for (int i = 0; i < views.Length; i++)
            {
                if (views[i].IsActive)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
