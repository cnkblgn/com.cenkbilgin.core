using System;
using UnityEngine;

namespace Core.UI
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public class UIWaypointController : MonoBehaviour
    {
        public static event Action<UIWaypointData> OnWaypointAdded = null;
        public static event Action<UIWaypointData> OnWaypointRemoved = null;

        [Header("_")]
        [SerializeField, Required] private UIWaypointEntity waypointTemplate = null;

        private Canvas thisCanvas = null;
        private RectTransform thisTransform = null;
        private Camera cameraController = null;
        private Transform cameraTransform = null;
        private UIWaypointPool waypointPool = null;
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
            waypointPool = new(PoolType.RELEASE, waypointTemplate, thisTransform, 16);
        }        
        private void Update()
        {
            if (!isOpened)
            {
                return;
            }

            for (int i = 0; i < waypointPool.Pool.TotalCount; i++)
            {
                if (waypointPool.Pool.TryGet(i, out UIWaypointEntity entity) && !entity.gameObject.activeSelf)
                {
                    continue;
                }

                entity.Tick(cameraController, cameraTransform);

                if (entity.IsCompleted)
                {
                    OnWaypointRemoved?.Invoke(entity.Data);
                    waypointPool.Pool.Release(entity);
                }
            }
        }

        public void Show(in UIWaypointData data, Vector3 offset, Camera camera, Func<bool> destroyUntil)
        {
            UIWaypointEntity entity = waypointPool.Spawn(data, offset, destroyUntil);

            if (entity == null)
            {
                Debug.LogError("UIWaypointEntity == null!");
                return;
            }

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
            OnWaypointAdded?.Invoke(data);

            Show();
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
            waypointPool.Pool.Reset(false, true);
            Hide();
        }
    }
}