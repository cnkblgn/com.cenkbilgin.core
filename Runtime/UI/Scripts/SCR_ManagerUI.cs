using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.UI
{
    [DisallowMultipleComponent]
    public sealed class ManagerUI : Manager<ManagerUI>
    {
        public Canvas Canvas => canvas;

        [Header("_")]
        [SerializeField, Required] private Canvas canvas = null;
        [SerializeField, Required] private EventSystem events = null;

        [Header("_")]
        [SerializeField, Required] private UICursorView cursorView = null;
        [SerializeField, Required] private UITooltipView tooltipView = null;
        [SerializeField, Required] private UIWaypointView waypointView = null;
        [SerializeField, Required] private UINotificationView notificationView = null;
        [SerializeField, Required] private UIContextMenuView contextMenuView = null;
        [SerializeField, Required] private UIPromptView promptView = null;
        [SerializeField, Required] private UITransitionView transitionView = null;
        [SerializeField, Required] private UISubtitleView subtitleView = null;
        [SerializeField, Required] private UIViewportView viewportView = null;

        private UIInputContext inputContext = default;
        private bool hasInputContext = false;

        protected override void Awake()
        {
            base.Awake();

            if (canvas == null) throw new NullReferenceException();
            if (events == null) throw new NullReferenceException();
            if (cursorView == null) throw new NullReferenceException();
            if (tooltipView == null) throw new NullReferenceException();
            if (waypointView == null) throw new NullReferenceException();
            if (notificationView == null) throw new NullReferenceException();
            if (contextMenuView == null) throw new NullReferenceException();
            if (promptView == null) throw new NullReferenceException();
            if (transitionView == null) throw new NullReferenceException();
            if (viewportView == null) throw new NullReferenceException();

            SetCursor();
            HideCursor();
        }
        private void Update()
        {
            if (!hasInputContext)
            {
                Debug.LogWarning("Please update UIInput context via UIManager.UpdateContext()!");
                return;
            }

            viewportView.TickFixed();
        }
        private void LateUpdate()
        {
            if (!hasInputContext)
            {
                return;
            }

            cursorView.MoveCursor(inputContext.PointerPosition);
            viewportView.Tick(in inputContext);
        }
        private void OnEnable() => ManagerGame.OnBeforeSceneChanged += OnBeforeSceneChanged;
        private void OnDisable() => ManagerGame.OnBeforeSceneChanged -= OnBeforeSceneChanged;

        private void OnBeforeSceneChanged(string _)
        {
            HideWaypoints();
            HideNotification();
            HideSubtitle();
            ClearViewports();
        }

        public void ShowNotification(string text, float duration = 5) => notificationView.Show(text, duration);
        public void HideNotification() => notificationView.Hide();
        public void ClearNotification() => notificationView.Clear();

        public void AddWaypoint(in UIWaypointData data, Vector3 offset, bool show = true)
        {
            waypointView.Add(data, offset, inputContext.Camera);

            if (show)
            {
                ShowWaypoints();
            }
        }
        public void RemoveWaypoint(in Guid id) => waypointView.Remove(id);
        public void ShowWaypoints() => waypointView.Show();
        public void HideWaypoints() => waypointView.Hide();
        public void ClearWaypoints() => waypointView.Clear();

        /// <summary> 0 -> 1, fades to black </summary>
        public void ShowTransitionFadeIn(UITransitionContext ctx) => transitionView.FadeIn(ctx);
        /// <summary> 1 -> 0, fades to white </summary>
        public void ShowTransitionFadeOut(UITransitionContext ctx) => transitionView.FadeOut(ctx);
        public void HideTransition() => transitionView.Hide();

        public void MoveTooltip(Vector2 screenPosition) => tooltipView.Move(screenPosition);
        public void ShowTooltip(string value, Vector2 screenPosition) => tooltipView.Show(value, screenPosition);
        public void HideTooltip() => tooltipView.Hide();

        public UIPromptHandle ShowPrompt<TContext>(string description, in TContext ctx) where TContext : struct => promptView.Show(description, in ctx);
        public void HidePrompt(UIPromptHandle handle) => promptView.Hide(handle);

        public UIContextMenuHandle ShowContextMenu(in UIContextMenuContext ctx) => contextMenuView.Show(in ctx);
        public void HideContextMenu(UIContextMenuHandle handle) => contextMenuView.Hide(handle);

        public void ShowSubtitle(string text) => subtitleView.Show(text);
        public void HideSubtitle() => subtitleView.Hide();

        public void SetCursor(string id = "default") => cursorView.SetCursor(id);
        public void ShowCursor() => cursorView.ShowCursor();
        public void HideCursor() => cursorView.HideCursor();

        public bool TryGetViewport(Guid instanceID, out UIViewportEntry entry) => viewportView.TryGet(instanceID, out entry);
        public IReadOnlyList<UIViewportEntry> GetViewports() => viewportView.GetViewports();
        public void ClearViewports() => viewportView.Clear();
        public void AddViewport(UIViewportItem prefab, Guid? instanceID) => viewportView.Add(prefab, instanceID);
        public void RemoveViewport(string id) => viewportView.Remove(id);
        public void RemoveViewport(Guid instanceID) => viewportView.Remove(instanceID);
        public void ShowViewport(string id, UIViewportMesh mesh, bool isInputFocused) => viewportView.Show(id, mesh, isInputFocused);
        public void ShowViewport(Guid instanceID, UIViewportMesh mesh, bool isInputFocused) => viewportView.Show(instanceID, mesh, isInputFocused);
        public void HideViewport(string id) => viewportView.Hide(id);
        public void HideViewport(Guid instanceID) => viewportView.Hide(instanceID);
        public void FocusViewport(string id, bool isFocused) => viewportView.Focus(id, isFocused);
        public void FocusViewport(Guid instanceID, bool isFocused) => viewportView.Focus(instanceID, isFocused);

        public GameObject GetSelectedGameObject() => events.currentSelectedGameObject;
        public void SetSelectedGameObject(GameObject gameObject) => events.SetSelectedGameObject(gameObject);

        public void SetInputContext(in UIInputContext ctx) { inputContext = ctx; hasInputContext = true; }
    }
}