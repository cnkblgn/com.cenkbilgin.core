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
        [SerializeField, Required] private UIPromptView[] views = null;

        private Canvas thisCanvas = null;
        private UIPromptView activeView = null;

        private void Start()
        {
            thisCanvas = GetComponent<Canvas>();

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

        private bool HasActivePrompt()
        {
            if (activeView == null)
            {
                return false;
            }

            if (activeView.IsActive)
            {
                return true;
            }

            activeView = null;
            return false;
        }

        public UIPromptHandle Show<TContext>(string description, in TContext context) where TContext : struct
        {
            if (HasActivePrompt())
            {
                return default;
            }

            for (int i = 0; i < views.Length; i++)
            {
                UIPromptHandle handle = views[i].Show(description, context);

                if (handle == default)
                {
                    continue;
                }

                activeView = views[i];
                thisCanvas.Show();
                ManagerUI.Instance.ShowCursor();
                return handle;
            }

            return default;
        }
        public void Hide(UIPromptHandle handle)
        {
            if (handle == default || !HasActivePrompt())
            {
                return;
            }

            if (!activeView.TryHide(handle))
            {
                return;
            }

            Hide();
        }
        private void Hide()
        {
            if (activeView != null)
            {
                activeView.Hide();
                activeView = null;
            }

            thisCanvas.Hide();
            ManagerUI.Instance.HideCursor();
        }

        public bool HandleCanResumeGame()
        {
            if (!HasActivePrompt())
            {
                return true;
            }

            Hide();
            return false;
        }
        public bool HandleCanPauseGame()
        {
            if (!HasActivePrompt())
            {
                return true;
            }

            Hide();
            return false;
        }
        public bool HandleCanShowCursor() => true;
        public bool HandleCanHideCursor() => !HasActivePrompt();
    }
}