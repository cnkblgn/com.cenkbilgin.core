using System.Collections.Generic;
using UnityEngine;
using System;

namespace Core.UI
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-1)]
    internal sealed class UIViewportView : MonoBehaviour
    {
        [Header("_")]
        [SerializeField, Required] private Transform container = null;

        private readonly List<UIViewportEntry> items = new(4);
        private Camera thisCamera;

        private void OnEnable() => ManagerGame.OnBeforeSceneChanged += OnBeforeSceneChanged;
        private void OnDisable() => ManagerGame.OnBeforeSceneChanged -= OnBeforeSceneChanged;

        private void OnBeforeSceneChanged(string obj) => Clear();

        public void Tick(in UIInputContext ctx)
        {
            thisCamera = ctx.Camera;

            if (thisCamera == null)
            {
                Debug.LogWarning("UI Viewport view tick failed! camera is missing!?");
                return;
            }

            float deltaTime = Time.deltaTime;
            Transform cameraTransform = thisCamera.transform;

            for (int i = 0; i < items.Count; i++)
            {
                UIViewportItem item = items[i].Item;

                if (!item.TryCull(cameraTransform))
                {
                    item.Tick(thisCamera, deltaTime);
                }
            }
        }

        public IReadOnlyList<UIViewportEntry> GetViewports() => items;
        public void Add(UIViewportItem prefab, Guid? instanceID)
        {
            if (prefab == null)
            {
                Debug.LogError("You are trying to add null viewport prefab!");
                return;
            }

            // Instance ID verilmiþse, sadece ayný instance zaten varsa engelle.
            if (instanceID.HasValue)
            {
                if (TryGet(instanceID.Value, out _))
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"Viewport [{instanceID.Value}] is already added to system! Ignore if this is intended.");
#endif
                    return;
                }
            }
            // Instance ID yoksa, Base ID üzerinden tek bir tane olmasýna izin ver.
            else if (TryGet(prefab.ID, out _))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"Viewport [{prefab.ID}] is already added to system! Ignore if this is intended.");
#endif
                return;
            }

            Guid id = instanceID ?? Guid.NewGuid();

            UIViewportItem item = Instantiate(prefab, container);
            item.Initialize(thisCamera, id);
            items.Add(new(id, item));
        }
        public void Remove(string id)
        {
            if (!TryGet(id, out UIViewportEntry entry))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"Viewport [{id}] is already added to system! ignore if its intented");
#endif
            }

            entry.Item.Deinitialize();
            items.Remove(entry);
            Destroy(entry.Item.gameObject);
        }
        public void Remove(Guid instanceID)
        {
            if (!TryGet(instanceID, out UIViewportEntry entry))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"Viewport [{instanceID}] is already added to system! ignore if its intented");
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

        public void Show(string id, UIViewportMesh mesh)
        {
            if (mesh == null)
            {
                Debug.LogError("Viewport show failed! mesh is null!");
                return;
            }

            if (!TryGet(id, out UIViewportEntry entry))
            {
                return;
            }

            entry.Item.ShowViewport(mesh);
        }
        public void Show(Guid instanceID, UIViewportMesh mesh)
        {
            if (mesh == null)
            {
                Debug.LogError("Viewport show failed! mesh is null!");
                return;
            }

            if (!TryGet(instanceID, out UIViewportEntry entry))
            {
                return;
            }

            entry.Item.ShowViewport(mesh);
        }
        public void Hide(string id)
        {
            if (!TryGet(id, out UIViewportEntry entry))
            {
                return;
            }

            entry.Item.HideViewport();
        }
        public void Hide(Guid instanceID)
        {
            if (!TryGet(instanceID, out UIViewportEntry entry))
            {
                return;
            }

            entry.Item.HideViewport();
        }
        public void Focus(string id, bool isFocused)
        {
            if (!TryGet(id, out UIViewportEntry entry))
            {
                return;
            }

            entry.Item.FocusViewport(isFocused);
        }
        public void Focus(Guid instanceID, bool isFocused)
        {
            if (!TryGet(instanceID, out UIViewportEntry entry))
            {
                return;
            }

            entry.Item.FocusViewport(isFocused);
        }

        public bool TryGet(string id, out UIViewportEntry entry)
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
        public bool TryGet(Guid instanceID, out UIViewportEntry entry)
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