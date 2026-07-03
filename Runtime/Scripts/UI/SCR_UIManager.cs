using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.UI
{
    using static CoreUtility;
    using static TaskUtility;

    [DisallowMultipleComponent]
    public sealed class UIManager : Manager<UIManager>
    {
        public Canvas Canvas => canvas;

        [Header("_")]
        [SerializeField, Required] private Canvas canvas = null;
        [SerializeField, Required] private EventSystem events = null;

        [Header("_")]
        [SerializeField, Required] private UICursorController cursorController = null;
        [SerializeField, Required] private UITooltipController tooltipController = null;
        [SerializeField, Required] private UIWaypointController waypointController = null;
        [SerializeField, Required] private UINotificationController notificationController = null;
        [SerializeField, Required] private UIConfirmationController confirmationController = null;
        [SerializeField, Required] private UITransitionController transitionController = null;
        [SerializeField, Required] private UISubtitleController subtitleController = null;
        [SerializeField, Required] private UIViewportController viewportController = null;

        private UIInputContext ctx = default;
        private bool hasCtx = false;

        protected override void Awake()
        {
            base.Awake();

            if (canvas == null) throw new NullReferenceException();
            if (events == null) throw new NullReferenceException();
            if (cursorController == null) throw new NullReferenceException();
            if (tooltipController == null) throw new NullReferenceException();
            if (waypointController == null) throw new NullReferenceException();
            if (notificationController == null) throw new NullReferenceException();
            if (confirmationController == null) throw new NullReferenceException();
            if (transitionController == null) throw new NullReferenceException();
            if (viewportController == null) throw new NullReferenceException();

            SetCursor();
            HideCursor();
        }
        private void Update()
        {
            if (!hasCtx)
            {
                Debug.LogWarning("Please update UIInput context via UIManager.UpdateContext()!");
                return;
            }
        }
        private void LateUpdate()
        {
            if (!hasCtx)
            {
                return;
            }

            cursorController.MoveCursor(ctx.PointerPosition);
            viewportController.Tick(in ctx);
        }
        private void OnEnable() => GameManager.OnBeforeSceneChanged += OnBeforeSceneChanged;
        private void OnDisable() => GameManager.OnBeforeSceneChanged -= OnBeforeSceneChanged;

        private void OnBeforeSceneChanged(string _)
        {
            HideWaypoints();
            HideNotification();
            HideSubtitle();
        }

        public void UpdateInput(in UIInputContext ctx)
        {
            this.ctx = ctx;
            hasCtx = true;
        }

        public void ShowNotification(string text, float duration = 5) => notificationController.Show(text, duration);
        public void HideNotification() => notificationController.Hide();
        public void ClearNotification() => notificationController.Clear();

        public void ShowWaypoint(in UIWaypointData data, Vector3 offset) => waypointController.Show(data, offset, ctx.Camera);
        public void ShowWaypoints() => waypointController.ShowAll();
        public void HideWaypoint(in Guid id) => waypointController.Hide(id);
        public void HideWaypoints() => waypointController.HideAll();
        public void ClearWaypoints() => waypointController.Clear();

        /// <summary> 0 -> 1, fades to black </summary>
        public void ShowTransitionFadeIn(UITransitionContext ctx) => transitionController.FadeIn(ctx);
        /// <summary> 1 -> 0, fades to white </summary>
        public void ShowTransitionFadeOut(UITransitionContext ctx) => transitionController.FadeOut(ctx);
        public void HideTransition() => transitionController.Hide();

        public void MoveTooltip(Vector2 screenPosition) => tooltipController.Move(screenPosition);
        public void ShowTooltip(string value, Vector2 screenPosition) => tooltipController.Show(value, screenPosition);
        public void HideTooltip() => tooltipController.Hide();

        public void ShowConfirmation(in UIConfirmationContext ctx) => confirmationController.Show(in ctx);
        public void HideConfirmation() => confirmationController.Hide();

        public void ShowSubtitle(string text) => subtitleController.Show(text);
        public void HideSubtitle() => subtitleController.Hide();

        public void SetCursor(string id = "default") => cursorController.SetCursor(id);
        public void ShowCursor() => cursorController.ShowCursor();
        public void HideCursor() => cursorController.HideCursor();

        public void AddViewport(UIViewportView prefab) => viewportController.Add(prefab);
        public void RemoveViewport(string id) => viewportController.Remove(id);
        public void ClearViewports() => viewportController.Clear();
        public void ShowViewport(string id, ViewportRenderer renderer) => viewportController.Show(id, renderer);
        public void HideViewport(string id) => viewportController.Hide(id);

        public GameObject GetSelectedGameObject() => events.currentSelectedGameObject;
        public void SetSelectedGameObject(GameObject gameObject) => events.SetSelectedGameObject(gameObject);
    }
}