using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    internal sealed class UINotificationView : MonoBehaviour
    {
        [Header("_")]
        [SerializeField, Range(0, 16)] private int maxSize = 12;
        [SerializeField, Required] private UINotificationItem itemTemplate = null;
        [SerializeField, Required] private RectTransform itemContainer = null;

        [Header("_")]
        [SerializeField, Min(0)] private float yPadding = 8;

        private Canvas thisCanvas = null;
        private List<UINotificationItem> activeEntities = new(1);
        private Vector2 objectOffset = Vector2.zero;
        private Vector2 objectPadding = Vector2.zero;

        private void Awake()
        {
            thisCanvas = GetComponent<Canvas>();
            activeEntities = new(maxSize);

            itemTemplate.gameObject.SetActive(false);

            objectOffset = new(0, itemTemplate.GetComponent<RectTransform>().rect.height);
            objectPadding = new(0, yPadding);

            for (int i = 0; i < maxSize; i++)
            {
                UINotificationItem obj = Instantiate(itemTemplate, itemContainer);

                obj.Initialize();

                activeEntities.Add(obj);
            }
        }

        public void Show(string text, float duration)
        {
            thisCanvas.Show();

            UINotificationItem temp = null;

            foreach (UINotificationItem active in activeEntities)
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
            foreach (UINotificationItem entity in activeEntities)
            {
                Hide(entity);
            }

            Hide();
        }

        private void Show(UINotificationItem notification, string text, float duration)
        {
            if (notification == null)
            {
                return;
            }

            notification.Show(text, duration);
        }
        private void Hide(UINotificationItem notification)
        {
            if (notification == null)
            {
                return;
            }

            notification.Hide();
        }
    }
}