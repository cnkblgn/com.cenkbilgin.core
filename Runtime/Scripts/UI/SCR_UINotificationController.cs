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
        [Header("_")]
        [SerializeField, Range(0, 16)] private int maxSize = 12;
        [SerializeField, Required] private UINotificationElement notificationTemplate = null;
        [SerializeField, Required] private RectTransform notificationContainer = null;

        [Header("_")]
        [SerializeField, Min(0)] private float yPadding = 16;

        private Canvas thisCanvas = null;
        private List<UINotificationElement> activeElements = new(1);
        private Vector2 objectOffset = Vector2.zero;
        private Vector2 objectPadding = Vector2.zero;

        private void Awake()
        {
            thisCanvas = GetComponent<Canvas>();
            activeElements = new(maxSize);

            notificationTemplate.gameObject.SetActive(false);

            objectOffset = new(0, notificationTemplate.GetComponent<RectTransform>().rect.height);
            objectPadding = new(0, yPadding);

            for (int i = 0; i < maxSize; i++)
            {
                UINotificationElement obj = Instantiate(notificationTemplate, notificationContainer);
                obj.Initialize();

                activeElements.Add(obj);
            }
        }

        public void Show(string text, float duration)
        {
            thisCanvas.Show();

            UINotificationElement temp = null;

            foreach (UINotificationElement element in activeElements)
            {
                if (!element.IsActive)
                {
                    temp = element;
                    break;
                }
            }

            if (temp == null)
            {
                temp = activeElements[0];
                temp.Dispose();
            }

            activeElements.Remove(temp);
            activeElements.Add(temp);

            for (int i = 0; i < activeElements.Count; i++)
            {
                if (activeElements[i].IsActive)
                {
                    activeElements[i].Offset(-objectOffset - objectPadding);
                }
            }

            temp.Show(text, duration);
        }
        public void Hide()
        {
            thisCanvas.Hide();
        }
        public void Clear()
        {
            foreach (UINotificationElement obj in activeElements)
            {
                obj.Dispose();
            }

            Hide();
        }
    }
}