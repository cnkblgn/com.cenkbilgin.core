using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.UI
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public class UIWaypointController : MonoBehaviour
    {
        [Header("_")]
        [SerializeField, Required] private UIWaypointElement waypointTemplate = null;

        private Canvas thisCanvas = null;
        private RectTransform thisTransform = null;
        private Camera mainCameraController = null;
        private Transform mainCameraTransform = null;
        private readonly List<UIWaypointElement> thisObjects = new(8);
        private bool isOpened = false;

        private void Awake()
        {
            thisCanvas = GetComponent<Canvas>();
            thisTransform = GetComponent<RectTransform>();
            waypointTemplate.gameObject.SetActive(false);
        }
        private void Update()
        {
            if (!isOpened)
            {
                return;
            }

            for (int i = 0; i < thisObjects.Count; i++)
            {
                thisObjects[i].Tick(mainCameraController, mainCameraTransform);
            }

            thisObjects.RemoveAll(e => e.IsFinished);
        }
        private void OnEnable() => this.WaitUntil(() => ManagerCoreGame.Instance != null, null, () => ManagerCoreGame.Instance.OnAfterSceneChanged += OnAfterSceneChanged);
        private void OnDisable()
        {
            if (ManagerCoreGame.Instance != null)
            {
                ManagerCoreGame.Instance.OnAfterSceneChanged -= OnAfterSceneChanged;
            }
        }
        private void OnAfterSceneChanged(string scene) => Hide();
        public void Show(Camera mainCamera, Transform targetTransform, Vector3 targetOffset, Func<bool> destroyUntil, Sprite iconSprite, Color iconColor, string iconText = "", float duration = -1)
        {
            if (mainCamera == null)
            {
                Debug.LogError("UIWaypointController.Show() mainCamera == null!");
                return;
            }

            if (waypointTemplate == null)
            {
                Debug.LogError("UIWaypointController.Show() waypointTemplate == null!");
                return;
            }

            mainCameraController = mainCamera;
            mainCameraTransform = mainCamera.transform;
            thisCanvas.Show();

            UIWaypointElement waypointObject = Instantiate(waypointTemplate, thisTransform);
            waypointObject.Initialize(targetTransform, targetOffset, destroyUntil, iconSprite, iconColor, iconText, duration);
            waypointObject.gameObject.SetActive(true);

            thisObjects.Add(waypointObject);

            isOpened = true;
        }
        public void Hide()
        {
            thisCanvas.Hide();
            isOpened = false;
        }
        public void Clear()
        {
            for (int i = 0; i < thisObjects.Count; i++)
            {
                thisObjects[i].Deinitialize();
            }

            thisObjects.Clear();

            Hide();
        }
    }
}