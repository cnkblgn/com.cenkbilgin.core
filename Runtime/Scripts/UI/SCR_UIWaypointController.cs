using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.UI
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    internal sealed class UIWaypointController : MonoBehaviour
    {
        [Header("_")]
        [SerializeField, Required] private UIWaypointView waypointTemplate = null;

        private Canvas thisCanvas = null;
        private RectTransform thisTransform = null;
        private Camera cameraController = null;
        private Transform cameraTransform = null;
        private UIWaypointPool waypointPool = null;
        private readonly Dictionary<Guid, UIWaypointView> waypointTable = new();
        private bool isOpened = false;

        private void Awake()
        {
            thisCanvas = GetComponent<Canvas>();
            thisTransform = GetComponent<RectTransform>();

            waypointTemplate.gameObject.SetActive(false);
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
                if (waypointPool.Pool.TryGet(i, out UIWaypointView entity) && !entity.gameObject.activeSelf)
                {
                    continue;
                }

                entity.Tick(cameraController, cameraTransform);

                if (entity.IsCompleted)
                {
                    waypointPool.Pool.Release(entity);
                }
            }
        }

        public void Show(in UIWaypointData data, Vector3 offset, Camera camera, Func<bool> destroyUntil)
        {
            UIWaypointView entity = waypointPool.Spawn(data, offset, destroyUntil);

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
            if (!waypointTable.TryGetValue(id, out UIWaypointView entity))
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