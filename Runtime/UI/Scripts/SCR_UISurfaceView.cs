using System.Collections.Generic;
using UnityEngine;
using System;

namespace Core.UI
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-1)]
    internal sealed class UISurfaceView : MonoBehaviour
    {
        [Header("_")]
        [SerializeField, Required] private Transform container = null;

        private readonly List<UISurfaceEntry> items = new(4);
        private Camera thisCamera;

        private void OnEnable() => ManagerGame.OnBeforeSceneChanged += OnBeforeSceneChanged;
        private void OnDisable() => ManagerGame.OnBeforeSceneChanged -= OnBeforeSceneChanged;

        private void OnBeforeSceneChanged(string obj) => Clear();

        public void Tick(in UIInputContext ctx)
        {
            thisCamera = ctx.Camera;

            if (thisCamera == null)
            {
                Debug.LogWarning("UI Surface view tick failed! camera is missing!?");
                return;
            }

            float deltaTime = Time.deltaTime;
            Transform cameraTransform = thisCamera.transform;

            for (int i = 0; i < items.Count; i++)
            {
                UISurfaceItem item = items[i].Item;

                if (item.IsActive && item.Mesh.TryCull(cameraTransform, out _, out _))
                {
                    items[i].Item.Tick(deltaTime);
                }
                else
                {
                    item.HideSurface();
                }
            }
        }

        public IReadOnlyList<UISurfaceEntry> GetSurfaces() => items;
        public void Add(UISurfaceItem prefab, Guid? instanceID)
        {
            if (prefab == null)
            {
                Debug.LogError("You are trying to add null surface prefab!");
                return;
            }

            if ((instanceID.HasValue && TryGet(instanceID.Value, out _)) || TryGet(prefab.ID, out _))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"Surface [{prefab.ID}] is already added to system! ignore if its intented");
#endif
            }

            UISurfaceItem item = GameObject.Instantiate(prefab, container);
            item.Initialize(thisCamera);
            items.Add(new(instanceID ?? Guid.NewGuid(), item));
        }
        public void Remove(string id)
        {
            if (!TryGet(id, out UISurfaceEntry entry))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"Surface [{id}] is already added to system! ignore if its intented");
#endif
            }

            entry.Item.Deinitialize();
            items.Remove(entry);
            Destroy(entry.Item.gameObject);
        }
        public void Remove(Guid instanceID)
        {
            if (!TryGet(instanceID, out UISurfaceEntry entry))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"Surface [{instanceID}] is already added to system! ignore if its intented");
#endif
            }

            entry.Item.Deinitialize();
            items.Remove(entry);
            Destroy(entry.Item.gameObject);
        }
        public void Clear()
        {
            for (int i = 0; i < items.Count; i++)
            {
                items[i].Item.Deinitialize();
                Destroy(items[i].Item.gameObject);
            }

            items.Clear();
        }

        public void Show(string id, UISurfaceMesh mesh)
        {
            if (mesh == null)
            {
                Debug.LogError("Overlay is null!");
                return;
            }

            if (!TryGet(id, out UISurfaceEntry entry))
            {
                return;
            }

            entry.Item.ShowSurface(mesh);
        }
        public void Show(Guid instanceID, UISurfaceMesh mesh)
        {
            if (mesh == null)
            {
                Debug.LogError("Overlay is null!");
                return;
            }

            if (!TryGet(instanceID, out UISurfaceEntry entry))
            {
                return;
            }

            entry.Item.ShowSurface(mesh);
        }
        public void Hide(string id)
        {
            if (!TryGet(id, out UISurfaceEntry entry))
            {
                return;
            }

            entry.Item.HideSurface();
        }
        public void Hide(Guid instanceID)
        {
            if (!TryGet(instanceID, out UISurfaceEntry entry))
            {
                return;
            }

            entry.Item.HideSurface();
        }

        public bool TryGet(string id, out UISurfaceEntry entry)
        {
            entry = default;

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Item.ID == id)
                {
                    entry = items[i];
                    return true;
                }
            }

            return false;
        }
        public bool TryGet(Guid instanceID, out UISurfaceEntry entry)
        {
            entry = default;

            for (int i = 0; i < items.Count; i++)
            {
                if (instanceID == items[i].InstanceID)
                {
                    entry = items[i];
                    return true;
                }
            }

            return false;
        }
    }
}