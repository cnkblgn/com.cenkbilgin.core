using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    internal sealed class UIWaypointView : MonoBehaviour
    {
        [Header("_")]
        [SerializeField, Required] private UIWaypointItem waypointTemplate;
        [SerializeField, Range(1, 128)] private int waypointCapacity = 16;

        private Canvas thisCanvas;
        private RectTransform thisTransform;
        private Camera cameraController;
        private Transform cameraTransform;
        private UIWaypointPool waypointPool;
        private readonly Dictionary<Guid, UIWaypointItem> waypointTable = new();
        private bool isOpened;

        private void Awake()
        {
            thisCanvas = GetComponent<Canvas>();
            thisTransform = GetComponent<RectTransform>();

            waypointTemplate.gameObject.SetActive(false);
            waypointPool = new(PoolType.RELEASE, waypointTemplate, thisTransform, waypointCapacity);
        }        
        private void LateUpdate()
        {
            if (!isOpened)
            {
                return;
            }

            for (int i = 0; i < waypointCapacity; i++)
            {
                if (!waypointPool.Pool.TryGet(i, out UIWaypointItem entity))
                {
                    continue;
                }

                if (entity.IsCompleted)
                {
                    waypointPool.Pool.Release(entity);
                    continue;
                }

                if (!entity.gameObject.activeSelf)
                {
                    continue;
                }

                entity.Tick(cameraController, cameraTransform);
            }
        }

        public void Show()
        {
            isOpened = true;
            thisCanvas.Show();
        }
        public void Hide()
        {
            thisCanvas.Hide();
            isOpened = false;
        }

        public void Add(in UIWaypointData data, Vector3 offset, Camera camera)
        {
            UIWaypointItem entity = waypointPool.Spawn(data, offset);

            Guid id = data.ID;

            if (entity == null)
            {
                Debug.LogError("waypoint entity not found in pool!");
                return;
            }

            if (camera == null)
            {
                Debug.LogError("waypoint camera is null!");
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
        }
        public void Remove(in Guid id)
        {
            if (!waypointTable.TryGetValue(id, out UIWaypointItem entity))
            {
                Debug.Log($"waypoint [{id}] not found!");
                return;
            }

            entity.Hide();
            waypointTable.Remove(id);
        }
        public void Clear()
        {
            waypointPool.Pool.Reset(false, true);
            Hide();
        }
    }
}