using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    internal sealed class UIContextMenuController : MonoBehaviour, IUIContextItemHandler, IUICursorStateHandler, IGameStateHandler
    {
        public bool IsActive => thisHandle != default;

        [Header("_")]
        [SerializeField, Required] private RectTransform root;

        [Header("_")]
        [SerializeField, Required] private RectTransform viewContainer;
        [SerializeField, Required] private UIContextItemView viewTemplate;

        private Canvas thisCanvas;
        private UIContextMenuHandle thisHandle;
        private readonly List<UIContextItemView> thisItems = new();

        private void Awake()
        {
            thisCanvas = GetComponent<Canvas>();
            thisCanvas.Hide();

            root.AlignBottomLeft();

            viewTemplate.gameObject.SetActive(false);
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

        public UIContextMenuHandle Show(in UIContextMenuContext ctx)
        {
            if (thisHandle != default)
            {
                return default;
            }

            thisCanvas.Show();
            ManagerUI.Instance.ShowCursor();

            root.localPosition = ctx.Position + (Vector2.right * root.rect.width);

            Populate(in ctx);

            return thisHandle = new(Guid.NewGuid(), this);
        }
        public void Hide(UIContextMenuHandle handle)
        {
            if (thisHandle != handle)
            {
                return;
            }

            Hide();
        }
        private void Hide()
        {
            if (thisHandle == default)
            {
                return;
            }

            thisHandle = default;
            thisCanvas.Hide();
            ManagerUI.Instance.HideCursor();
            Clear();
        }

        private void Populate(in UIContextMenuContext ctx)
        {
            for (int i = 0; i < ctx.Items.Length; i++)
            {
                UIContextItemView itemView = Instantiate(viewTemplate, viewContainer);
                itemView.gameObject.SetActive(true);
                itemView.Initialize(ctx.Items[i], this);

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
        public bool HandleCanHideCursor() => !IsActive;
        public bool HandleCanResumeGame()
        {
            if (!IsActive)
            {
                return true;
            }

            Hide();
            return false;
        }
        public bool HandleCanPauseGame()
        {
            if (!IsActive)
            {
                return true;
            }

            Hide();
            return false;
        }
    }
}
