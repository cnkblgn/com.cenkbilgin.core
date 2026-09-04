using System.Collections.Generic;
using UnityEngine;

namespace Core.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    internal sealed class UIContextMenuController : MonoBehaviour, IUIContextItemHandler, IUICursorStateHandler, IGameStateHandler
    {
        [Header("_")]
        [SerializeField, Required] private RectTransform root;

        [Header("_")]
        [SerializeField, Required] private RectTransform viewContainer;
        [SerializeField, Required] private UIContextItemView viewTemplate;

        private Canvas thisCanvas;
        private readonly List<UIContextItemView> thisItems = new();
        private bool isOpened = false;

        private void Awake()
        {
            thisCanvas = GetComponent<Canvas>();
            thisCanvas.Hide();
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

            Hide();
        }

        public void OnSelected() => Hide();

        public void Show(in UIContextMenuContext ctx)
        {
            if (isOpened)
            {
                return;
            }

            thisCanvas.Show();

            root.anchoredPosition = ctx.Position;

            Populate(in ctx);

            isOpened = true;
        }
        public void Hide()
        {
            if (!isOpened)
            {
                return;
            }

            thisCanvas.Hide();

            Clear();

            isOpened = false;
        }

        private void Populate(in UIContextMenuContext ctx)
        {
            for (int i = 0; i < ctx.Items.Length; i++)
            {
                UIContextItemView itemView = Instantiate(viewTemplate, viewContainer);
                itemView.Initialize(ctx.Items[i]);

                thisItems.Add(itemView);
            }
        }
        private void Clear()
        {
            for (int i = 0; i < thisItems.Count; i++)
            {
                UIContextItemView itemView = thisItems[i];
                itemView.Deinitialize();

                Destroy(itemView.gameObject);
            }

            thisItems.Clear();
        }

        public bool HandleCanShowCursor() => true;
        public bool HandleCanHideCursor()
        {
            if (isOpened)
            {
                return false;
            }

            return true;
        }

        public bool HandleCanResumeGame()
        {
            if (isOpened)
            {
                Hide();
                return false;
            }

            return true;
        }
        public bool HandleCanPauseGame()
        {
            if (isOpened)
            {
                Hide();
                return false;
            }

            return true;
        }
    }
}
