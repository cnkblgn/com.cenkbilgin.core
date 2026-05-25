using System;
using System.Collections.Generic;
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
        private readonly Dictionary<Guid, UIWaypointEntity> waypointTable = new();
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

            Guid id = data.ID;

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

            if (waypointTable.ContainsKey(id))
            {
                Debug.LogError($"duplicate waypoint [{id}]");
                return;
            }

            cameraController = camera;
            cameraTransform = camera.transform;
            OnWaypointAdded?.Invoke(data);

            waypointTable.Add(id, entity);

            ShowAll();
        }
        public void ShowAll()
        {
            thisCanvas.Show();
            isOpened = true;
        }
        public void Hide(in Guid id)
        {
            if (!waypointTable.TryGetValue(id, out UIWaypointEntity entity))
            {
                Debug.Log($"waypoint [{id}] not found!");
                return;
            }

            entity.Hide();
            waypointTable.Remove(id);
        }
        public void HideAll()
        {
            thisCanvas.Hide();
            isOpened = false;
        }
        public void Clear()
        {
            waypointPool.Pool.Reset(false, true);
            HideAll();
        }
    }
}