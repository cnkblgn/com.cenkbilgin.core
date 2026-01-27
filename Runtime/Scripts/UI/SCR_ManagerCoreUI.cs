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
        [SerializeField, Required] private UICrosshairController crosshairController = null;
        [SerializeField, Required] private UITransitionController transitionController = null;
        [SerializeField, Required] private UIKeyActionController keyActionController = null;
        [SerializeField, Required] private UISettingsController settingsController = null;

        protected override void Awake()
        {
            base.Awake();

            SetCursor(UICursorType.DEFAULT);
            HideCursor();
        }
        private void OnEnable()
        {
            ManagerCoreGame.OnGameStateChanged += OnGameStateChanged;
            ManagerCoreGame.OnBeforeSceneChanged += OnBeforeSceneChanged;
            ManagerCoreGame.OnAfterSceneChanged += OnAfterSceneChanged;
        }
        private void OnDisable()
        {
            ManagerCoreGame.OnGameStateChanged -= OnGameStateChanged;
            ManagerCoreGame.OnBeforeSceneChanged -= OnBeforeSceneChanged;
            ManagerCoreGame.OnAfterSceneChanged -= OnAfterSceneChanged;
        }

        private void OnGameStateChanged(GameState gameState)
        {
            switch (gameState)
            {
                case GameState.NULL:
                    settingsController.Hide();
                    break;
                case GameState.RESUME:
                    settingsController.Hide();
                    break;
                case GameState.PAUSE:
                    break;
                default:
                    break;
            }
        }
        private void OnAfterSceneChanged(string _)
        {
            waypointController.Hide();
        }
        private void OnBeforeSceneChanged(string _)
        {
            notificationController.Hide();
        }

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

        public void ShowCrosshair() => crosshairController.Show();
        public void HideCrosshair() => crosshairController.Hide();

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

        public void ShowSettings() => settingsController.Show();
        public void HideSettings() => settingsController.Hide();
        public void InsertSettingEvent(Action onApply, Action onRevert, Action onLoad)
        {
            settingsController.OnApply += onApply;
            settingsController.OnRevert += onRevert;
            settingsController.OnLoad += onLoad;
        }
        public void RemoveSettingEvent(Action onApply, Action onRevert, Action onLoad)
        {
            settingsController.OnApply -= onApply;
            settingsController.OnRevert -= onRevert;
            settingsController.OnLoad -= onLoad;
        }
        public void InsertSettingHeader(string description) => settingsController.InsertHeader(description);
        public UIOptionButton InsertSettingButton(int initial, int @default, string description, string[] values, Action<int> onApply, Action<int> onChanged) => settingsController.InsertButton(initial, @default, description, values, onApply, onChanged);
        public UIOptionButton InsertSettingButton(int initial, int @default, string description, int maximumIndex, Action<int> onApply, Action<int> onChanged) => settingsController.InsertButton(initial, @default, description, maximumIndex, onApply, onChanged);
        public UIOptionToggle InsertSettingToggle(bool initial, bool @default, string description, Action<bool> onApply, Action<bool> onChanged) => settingsController.InsertToggle(initial, @default, description, onApply, onChanged);
        public UIOptionSlider InsertSettingSlider(float initial, float @default, string description, string[] values, Action<float> onApply, Action<float> onChanged) => settingsController.InsertSlider(initial, @default, description, values, onApply, onChanged);
        public UIOptionSlider InsertSettingSlider(float initial, float @default, string description, float minValue, float maxValue, bool isInt, Action<float> onApply, Action<float> onChanged) => settingsController.InsertSlider(initial, @default, description, minValue, maxValue, isInt, onApply, onChanged);

        public void MoveCursor(Vector2 pointerPosition) => cursorController.MoveCursor(pointerPosition);
        public void SetCursor(UICursorType type = UICursorType.DEFAULT) => cursorController.SetCursor(type);
        public void ShowCursor() => cursorController.ShowCursor();
        public void HideCursor() => cursorController.HideCursor();

        public GameObject GetSelectedGameObject() => mainCanvasEvents.currentSelectedGameObject;
        public void SetSelectedGameObject(GameObject gameObject) => mainCanvasEvents.SetSelectedGameObject(gameObject);
    }
}