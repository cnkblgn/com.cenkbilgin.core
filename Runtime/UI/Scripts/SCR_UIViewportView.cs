using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.UI
{
    [DisallowMultipleComponent]
    internal sealed class UIViewportView : MonoBehaviour
    {
        [Header("_")]
        [SerializeField, Required] private Camera rendererCamera = null;
        [SerializeField] private LayerMask viewportDetectionMask = 0;

        [Header("_")]
        [SerializeField, Range(1, 60)] private float frameRate = 30;
        [SerializeField, Required] private Transform container = null;

        private UIViewportItem activeItem = null;
        private readonly List<UIViewportItem> items = new(4);
        private readonly RaycastHit[] collisionBuffer = new RaycastHit[5];
        private float renderTimer = 0f;
        private float renderInterval = 0f;

        private void Awake()
        {
            if (rendererCamera == null)
            {
                throw new NullReferenceException($"Viewport renderer camera not found! {nameof(rendererCamera)}");
            }

            rendererCamera.enabled = false;
        }
        private void OnEnable() => ManagerGame.OnBeforeSceneChanged += OnBeforeSceneChanged;
        private void OnDisable() => ManagerGame.OnBeforeSceneChanged -= OnBeforeSceneChanged;

        private void OnBeforeSceneChanged(string _) => Clear();

        public void Tick(in UIInputContext ctx)
        {
            if (ManagerGame.Instance.GetGameState() != GameState.RESUME)
            {
                return;
            }

            if (activeItem == null)
            {
                return;
            }

            activeItem.Tick(Time.deltaTime);

            if (activeItem.HasInput && ctx.Camera != null)
            {
                UpdateInput(in ctx);
            }

            renderInterval = 1f / Mathf.Max(1f, frameRate);

            UpdateRender(Time.deltaTime);
        }
        private void UpdateInput(in UIInputContext ctx)
        {
            Ray ray = ctx.Camera.ScreenPointToRay(ctx.PointerPosition);
            int count = Physics.RaycastNonAlloc(ray, collisionBuffer, 5.0f, viewportDetectionMask, QueryTriggerInteraction.Ignore);

            Vector2 texturePosition = Vector2.zero;
            float closestDistance = float.MaxValue;
            bool hitActiveViewport = false;

            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = collisionBuffer[i];

                if (hit.distance >= closestDistance)
                {
                    continue;
                }

                if (!hit.collider.TryGetComponent(out UIViewportMesh mesh))
                {
                    continue;
                }

                if (mesh != activeItem.Mesh)
                {
                    continue;
                }

                closestDistance = hit.distance;
                texturePosition = hit.textureCoord;
                hitActiveViewport = true;
            }

            if (!hitActiveViewport)
            {
                activeItem.OnPointerExit();
                return;
            }

            activeItem.UpdateInput(in ctx, texturePosition);
        }
        private void UpdateRender(float deltaTime)
        {
            renderTimer += deltaTime;

            if (renderTimer >= renderInterval)
            {
                renderTimer -= renderInterval;

                activeItem.Render();
                rendererCamera.Render();
            }           
        }

        public void Add(UIViewportItem prefab)
        {
            if (prefab == null)
            {
                Debug.LogError("You are trying to add null viewport prefab!");
                return;
            }

            if (Contains(prefab.ID))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"Viewport [{prefab.ID}] is already added to manager! Ignore if intended.");
#endif
                return;
            }

            UIViewportItem view = Instantiate(prefab, container);
            view.Initialize(rendererCamera);
            items.Add(view);
        }
        public void Remove(string id)
        {
            for (int i = 0; i < items.Count; i++)
            {
                UIViewportItem view = items[i];

                if (view.ID != id)
                {
                    continue;
                }

                if (activeItem == view)
                {
                    view.HideViewport();

                    activeItem = null;
                    renderTimer = 0f;
                }

                view.Deinitialize();

                items.RemoveAt(i);

                Destroy(view.gameObject);

                return;
            }

#if UNITY_EDITOR
            Debug.LogWarning($"You are trying to remove viewport [{id}] that does not exist! Ignore if intended.");
#endif
        }
        public void Clear()
        {
            if (activeItem != null)
            {
                activeItem.HideViewport();
            }

            for (int i = 0; i < items.Count; i++)
            {
                items[i].Deinitialize();
                Destroy(items[i].gameObject);
            }

            activeItem = null;
            renderTimer = 0f;
            renderInterval = float.MaxValue;

            items.Clear();
        }
        public void Show(string id, UIViewportMesh mesh)
        {
            if (mesh == null)
            {
                Debug.LogError("Viewport mesh is null!");
                return;
            }

            UIViewportItem view = Find(id);

            if (view == null)
            {
                Debug.LogError($"You are trying to show viewport [{id}] that does not exist!");
                return;
            }

            if (activeItem != null)
            {
                activeItem.HideViewport();
            }

            activeItem = view;
            activeItem.ShowViewport(mesh);

            rendererCamera.targetTexture = activeItem.Texture;
            rendererCamera.orthographicSize = activeItem.Size;
        }
        public void Hide(string id)
        {
            if (activeItem == null || activeItem.ID != id)
            {
                return;
            }

            activeItem.HideViewport();
            activeItem = null;
            renderTimer = 0f;
        }

        private UIViewportItem Find(string id)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].ID == id)
                {
                    return items[i];
                }
            }

            return null;
        }
        private bool Contains(string id) => Find(id) != null;
    }
}