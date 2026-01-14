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
        private const int MAX_SIZE = 12;

        [Header("_")]
        [SerializeField, Required] private UINotificationElement notificationTemplate = null;
        [SerializeField, Required] private RectTransform notificationContainer = null;

        [Header("_")]
        [SerializeField, Min(0)] private float yPadding = 16;

        private Canvas thisCanvas = null;
        private readonly List<UINotificationElement> activeElements = new(MAX_SIZE);
        private Vector2 objectOffset = Vector2.zero;
        private Vector2 objectPadding = Vector2.zero;

        private void Awake()
        {
            thisCanvas = GetComponent<Canvas>();
            notificationTemplate.gameObject.SetActive(false);
            objectOffset = new(0, notificationTemplate.GetComponent<RectTransform>().rect.height);
            objectPadding = new(0, yPadding);

            for (int i = 0; i < MAX_SIZE; i++)
            {
                UINotificationElement obj = Instantiate(notificationTemplate, notificationContainer);
                obj.Initialize();

                activeElements.Add(obj);
            }
        }
        private void OnEnable()
        {
            this.WaitUntil(() => ManagerCoreGame.Instance != null, null, () =>
            {
                ManagerCoreGame.Instance.OnBeforeSceneChanged += OnBeforeSceneChanged;
            });
        }
        private void OnDisable()
        {
            if (ManagerCoreGame.Instance != null)
            {
                ManagerCoreGame.Instance.OnBeforeSceneChanged -= OnBeforeSceneChanged;
            }
        }
        private void OnBeforeSceneChanged(string scene) => Hide();

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
        [Obsolete]
        public void ShowOld(string text, float duration)
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

                // Remove from current position and add to end to maintain proper order
                activeElements.RemoveAt(0);
                activeElements.Add(temp);
            }
            else
            {
                // If we found an inactive one, ensure it's at the end of the list
                activeElements.Remove(temp);
                activeElements.Add(temp);
            }

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

            foreach (UINotificationElement obj in activeElements)
            {
                obj.Dispose();
            }
        }
    }
}