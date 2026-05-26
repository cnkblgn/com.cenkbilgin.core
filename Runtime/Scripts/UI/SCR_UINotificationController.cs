using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.UI
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public class UINotificationController : MonoBehaviour
    {
        public static event Action<UINotification> OnNotificationAdded = null;
        public static event Action<UINotification> OnNotificationRemoved = null;

        [Header("_")]
        [SerializeField, Range(0, 16)] private int maxSize = 12;
        [SerializeField, Required] private UINotification notificationTemplate = null;
        [SerializeField, Required] private RectTransform notificationContainer = null;

        [Header("_")]
        [SerializeField, Min(0)] private float yPadding = 16;

        private Canvas thisCanvas = null;
        private List<UINotification> activeEntities = new(1);
        private Vector2 objectOffset = Vector2.zero;
        private Vector2 objectPadding = Vector2.zero;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RESET()
        {
            OnNotificationAdded = null;
            OnNotificationRemoved = null;
        }

        private void Awake()
        {
            thisCanvas = GetComponent<Canvas>();
            activeEntities = new(maxSize);

            notificationTemplate.gameObject.SetActive(false);

            objectOffset = new(0, notificationTemplate.GetComponent<RectTransform>().rect.height);
            objectPadding = new(0, yPadding);

            for (int i = 0; i < maxSize; i++)
            {
                UINotification obj = Instantiate(notificationTemplate, notificationContainer);
                obj.Initialize();

                activeEntities.Add(obj);
            }
        }

        public void Show(string text, float duration)
        {
            thisCanvas.Show();

            UINotification temp = null;

            foreach (UINotification active in activeEntities)
            {
                if (!active.IsActive)
                {
                    temp = active;
                    break;
                }
            }

            if (temp == null)
            {
                temp = activeEntities[0];
                Hide(temp);
            }

            activeEntities.Remove(temp);
            activeEntities.Add(temp);

            for (int i = 0; i < activeEntities.Count; i++)
            {
                if (activeEntities[i].IsActive)
                {
                    activeEntities[i].Offset(-objectOffset - objectPadding);
                }
            }

            Show(temp, text, duration);
        }
        public void Hide()
        {
            thisCanvas.Hide();
        }
        public void Clear()
        {
            foreach (UINotification entity in activeEntities)
            {
                Hide(entity);
            }

            Hide();
        }

        private void Show(UINotification entity, string text, float duration)
        {
            if (entity == null)
            {
                return;
            }

            entity.Show(text, duration);
            OnNotificationAdded?.Invoke(entity);
        }
        private void Hide(UINotification entity)
        {
            if (entity == null)
            {
                return;
            }

            entity.Hide();
            OnNotificationRemoved?.Invoke(entity);
        }
    }
}