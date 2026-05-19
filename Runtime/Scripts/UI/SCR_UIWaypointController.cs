using System;
using UnityEngine;

namespace Core.UI
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public class UIWaypointController : MonoBehaviour
    {
        public static event Action<UIWaypointEntity> OnWaypointAdded = null;
        public static event Action<UIWaypointEntity> OnWaypointRemoved = null;

        [Header("_")]
        [SerializeField, Required] private UIWaypointEntity waypointTemplate = null;

        private Canvas thisCanvas = null;
        private RectTransform thisTransform = null;
        private Camera cameraController = null;
        private Transform cameraTransform = null;
        private readonly PoolSystemUIWaypoint waypointPool = new();
        private bool isOpened = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RESET()
        {
            OnWaypointAdded = null;
            OnWaypointRemoved = null;
        }

        private void Awake()
        {
            thisCanvas = GetComponent<Canvas>();
            thisTransform = GetComponent<RectTransform>();
            waypointPool.Initialize(waypointTemplate, thisTransform, 16);
        }        
        private void Update()
        {
            if (!isOpened)
            {
                return;
            }

            for (int i = 0; i < waypointPool.TotalCount; i++)
            {
                UIWaypointEntity entity = waypointPool.Get(i);

                if (!entity.gameObject.activeSelf)
                {
                    continue;
                }

                entity.Tick(cameraController, cameraTransform);

                if (entity.IsCompleted)
                {
                    OnWaypointRemoved?.Invoke(entity);
                    waypointPool.Release(entity);
                }
            }
        }

        public void Show(Camera camera, Transform target, Vector3 offset, Sprite icon, Color color, string text, float duration, Func<bool> destroyUntil)
        {
            if (camera == null)
            {
                Debug.LogError("mainCamera == null!");
                return;
            }

            if (waypointTemplate == null)
            {
                Debug.LogError("waypointTemplate == null!");
                return;
            }

            cameraController = camera;
            cameraTransform = camera.transform;
            UIWaypointEntity entity = waypointPool.Spawn(target, offset, icon, color, text, duration, destroyUntil);

            Show();

            OnWaypointAdded?.Invoke(entity);
        }
        public void Show()
        {
            thisCanvas.Show();
            isOpened = true;
        }
        public void Hide()
        {
            thisCanvas.Hide();
            isOpened = false;
        }
        public void Clear()
        {
            waypointPool.Reset();
            Hide();
        }
    }
}