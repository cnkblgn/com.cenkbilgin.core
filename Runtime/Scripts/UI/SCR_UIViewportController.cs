using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.UI
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    internal sealed class UIViewportController : MonoBehaviour
    {
        [Header("_")]
        [SerializeField, Required] private Camera rendererCamera = null;
        [SerializeField, Min(0)] private float cullingDistance = 16;
        [SerializeField] private LayerMask viewportDetectionMask = 0;

        [Header("_")]
        [SerializeField, Required] private Transform container = null;

        private readonly List<string> ids = new(4);
        private readonly List<UIViewportView> viewports = new(4);
        private readonly RaycastHit[] hits = new RaycastHit[5];

        private void Awake()
        {
            if (rendererCamera == null)
            {
                throw new NullReferenceException($"Viewport renderer camera not found! {nameof(rendererCamera)}");
            }

            rendererCamera.enabled = false;
        }
        private void OnEnable() => GameManager.OnBeforeSceneChanged += OnBeforeSceneChanged;
        private void OnDisable() => GameManager.OnBeforeSceneChanged -= OnBeforeSceneChanged;

        private void OnBeforeSceneChanged(string obj) => Clear();

        public void Tick(in UIInputContext ctx)
        {
            if (GameManager.Instance.GetGameState() != GameState.RESUME)
            {
                return;
            }

            if (ctx.Camera == null)
            {
                return;
            }

            for (int i = 0; i < viewports.Count; i++)
            {
                UpdateTick(viewports[i], in ctx);
                UpdateRender(viewports[i]);
            }

            CullRender(ctx.Camera);
        }

        private void UpdateTick(UIViewportView view, in UIInputContext ctx)
        {
            if (!view.IsActive)
            {
                return;
            }

            Ray ray = ctx.Camera.ScreenPointToRay(ctx.PointerPosition);
            Vector2 position = Vector2.zero;
            ViewportRenderer renderer = null;

            int count = Physics.RaycastNonAlloc(ray, hits, 5.0f, viewportDetectionMask, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = hits[i];

                if (hit.collider.TryGetComponent(out renderer))
                {
                    position = hit.textureCoord;
                    break;
                }
            }

            view.Tick(in ctx, position, renderer);
        }
        private void UpdateRender(UIViewportView view)
        {
            if (!view.IsActive)
            {
                return;
            }

            PreRender(view);
            StartRender(view);
            PostRender(view);
        }
        private void PreRender(UIViewportView view)
        {
            foreach (UIViewportView i in viewports)
            {
                if (i != view)
                {
                    i.HideRenderer();
                }
            }
        }
        private void StartRender(UIViewportView view)
        {
            foreach (UIViewportView i in viewports)
            {
                if (i == view)
                {
                    rendererCamera.targetTexture = view.Texture;
                    rendererCamera.orthographicSize = view.Size;
                    i.Render();
                    break;
                }
            }

            rendererCamera.Render();
        }
        private void PostRender(UIViewportView view)
        {
            foreach (UIViewportView i in viewports)
            {
                if (i != view)
                {
                    i.ShowRenderer();
                }
            }
        }
        private void CullRender(Camera camera)
        {
            for (int i = 0; i < viewports.Count; i++)
            {
                viewports[i].TryCull(camera.transform, cullingDistance);
            }
        }

        public void Add(UIViewportView prefab)
        {
            if (prefab == null)
            {
                Debug.LogError("You are trying to add null viewport prefab!");
                return;
            }

            if (ids.Contains(prefab.ID))
            {
                Debug.LogWarning($"viewport [{prefab.ID}] is already added to manager!");
                return;
            }

            UIViewportView view = GameObject.Instantiate(prefab, container);
            view.Initialize(rendererCamera);

            rendererCamera.enabled = false;
            ids.Add(view.ID);
            viewports.Add(view);
        }
        public void Remove(string id)
        {
            if (!ids.Contains(id))
            {
                Debug.LogError("you are trying to remove stage object that does not exists!");
                return;
            }

            for (int i = 0; i < viewports.Count; i++)
            {
                if (viewports[i].ID == id)
                {
                    UIViewportView view = viewports[i];

                    ids.Remove(id);
                    viewports.Remove(view);
                    Destroy(view.gameObject);

                    break;
                }
            }
        }
        public void Clear()
        {
            for (int i = 0; i < viewports.Count; i++)
            {
                viewports[i].Deinitialize();
                Destroy(viewports[i].gameObject);
            }

            ids.Clear();
            viewports.Clear();
        }

        public void Show(string id, ViewportRenderer renderer)
        {
            if (renderer == null)
            {
                Debug.LogError("viewport renderer is null!");
                return;
            }

            if (!ids.Contains(id))
            {
                Debug.LogError("You are trying to show viewport that does not exists!");
                return;
            }

            for (int i = 0; i < viewports.Count; i++)
            {
                if (viewports[i].ID == id)
                {
                    viewports[i].ShowViewport(renderer);

                    UpdateRender(viewports[i]);
                }
            }
        }
        public void Hide(string id)
        {
            if (!ids.Contains(id))
            {
                Debug.LogError("You are trying to hide viewport that does not exists!");
                return;
            }

            for (int i = 0; i < viewports.Count; i++)
            {
                if (viewports[i].ID == id)
                {
                    viewports[i].HideViewport();
                }
            }
        }
    }
}
