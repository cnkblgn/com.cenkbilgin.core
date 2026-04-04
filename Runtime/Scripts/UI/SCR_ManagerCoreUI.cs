using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.UI
{
    using static CoreUtility;
    using static TaskUtility;

    [DisallowMultipleComponent]
    public class ManagerCoreUI : Manager<ManagerCoreUI>
    {
        public Canvas MainCanvasController => mainCanvasController;

        [Header("_")]
        [SerializeField, Required] private Canvas mainCanvasController = null;
        [SerializeField, Required] private EventSystem mainCanvasEvents = null;

        [Header("_")]
        [SerializeField, Required] private UICursorController cursorController = null;
        [SerializeField, Required] private UITooltipController tooltipController = null;
        [SerializeField, Required] private UIWaypointController waypointController = null;
        [SerializeField, Required] private UINotificationController notificationController = null;
        [SerializeField, Required] private UIMessageController messageController = null;
        [SerializeField, Required] private UIConfirmationController confirmationController = null;
        [SerializeField, Required] private UITransitionController transitionController = null;
        [SerializeField, Required] private UIKeyActionController keyActionController = null;

        protected override void Awake()
        {
            base.Awake();

            SetCursor();
            HideCursor();
        }
        private void OnEnable()
        {
            ManagerCoreGame.OnBeforeSceneChanged += OnBeforeSceneChanged;
            ManagerCoreGame.OnAfterSceneChanged += OnAfterSceneChanged;
        }
        private void OnDisable()
        {
            ManagerCoreGame.OnBeforeSceneChanged -= OnBeforeSceneChanged;
            ManagerCoreGame.OnAfterSceneChanged -= OnAfterSceneChanged;
        }

        private void OnAfterSceneChanged(string _) => waypointController.Hide();
        private void OnBeforeSceneChanged(string _) => notificationController.Hide();

        public void ShowNotification(string text, float duration = 5) => notificationController.Show(text, duration);
        public void HideNotification() => notificationController.Hide();
        public void ClearNotification() => notificationController.Clear();

        public void ShowWaypoint(Camera camera, Transform targetTransform, Vector3 targetOffset, Func<bool> destroyUntil, Sprite iconSprite, Color iconColor, string iconText = "", float duration = -1) => waypointController.Show(camera, targetTransform, targetOffset, destroyUntil, iconSprite, iconColor, iconText, duration);
        public void ShowWaypoint() => waypointController.Show();
        public void HideWaypoint() => waypointController.Hide();
        public void ClearWaypoint() => waypointController.Clear();

        /// <summary> 0 -> 1, fades to black </summary>
        public void ShowTransitionFadeIn(float fadeTime, float waitTime, Action onStartEvent, Action onFinishEvent) => transitionController.FadeIn(fadeTime, waitTime, onStartEvent, onFinishEvent);
        /// <summary> 1 -> 0, fades to white </summary>
        public void ShowTransitionFadeOut(float fadeTime, float waitTime, Action onStartEvent, Action onFinishEvent) => transitionController.FadeOut(fadeTime, waitTime, onStartEvent, onFinishEvent);
        public void HideTransition() => transitionController.Hide();

        public void ShowMessage(string text, float duration = 2.5f) => messageController.Show(text, duration);
        public void ShowMessage() => messageController.Show();
        public void HideMessage() => messageController.Hide();
        public void ClearMessage() => messageController.Clear();

        public void MoveTooltip(Vector2 screenPosition) => tooltipController.Move(screenPosition);
        public void ShowTooltip(string value, Vector2 screenPosition) => tooltipController.Show(value, screenPosition);
        public void HideTooltip() => tooltipController.Hide();

        public void ShowConfirmation(string text, Action onAcceptEvent, Action onCancelEvent) => confirmationController.Show(text, onAcceptEvent, onCancelEvent);
        public void HideConfirmation() => confirmationController.Hide();

        public void SetEnableAction(bool value)
        {
            if (value)
            {
                keyActionController.Enable();
            }
            else
            {
                keyActionController.Disable();
            }
        }
        public bool IsActionActive(UIKeyActionType keyActionType) => keyActionController.IsActive(keyActionType);
        public void ShowAction(UIKeyActionType keyActionType) => keyActionController.Show(keyActionType);
        public void HideAction(UIKeyActionType keyActionType) => keyActionController.Hide(keyActionType);
        public void InsertAction(UIKeyActionType keyActionType, KeyActionData[] data) => keyActionController.Insert(keyActionType, data);
        public void RemoveAction(UIKeyActionType keyActionType) => keyActionController.Remove(keyActionType);

        public void MoveCursor(Vector2 pointerPosition) => cursorController.MoveCursor(pointerPosition);
        public void SetCursor(string id = "default") => cursorController.SetCursor(id);
        public void ShowCursor() => cursorController.ShowCursor();
        public void HideCursor() => cursorController.HideCursor();

        public GameObject GetSelectedGameObject() => mainCanvasEvents.currentSelectedGameObject;
        public void SetSelectedGameObject(GameObject gameObject) => mainCanvasEvents.SetSelectedGameObject(gameObject);
    }
}