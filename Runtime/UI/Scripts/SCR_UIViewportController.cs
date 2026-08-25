using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.UI
{
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
        private float[] renderTimers = Array.Empty<float>();
        private int renderIndex = 0;

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

        private void OnBeforeSceneChanged(string obj) => Clear();

        public void Tick(in UIInputContext ctx)
        {
            if (ManagerGame.Instance.GetGameState() != GameState.RESUME)
            {
                return;
            }

            if (ctx.Camera == null)
            {
                return;
            }

            float deltaTime = Time.deltaTime;

            for (int i = 0; i < viewports.Count; i++)
            {
                UpdateTick(viewports[i], in ctx);
                UpdateRenderTimer(i, deltaTime);
            }

            CullRender(ctx.Camera);

            NextRender();
        }

        private void UpdateTick(UIViewportView view, in UIInputContext ctx)
        {
            if (!view.IsActive)
            {
                return;
            }

            Ray ray = ctx.Camera.ScreenPointToRay(ctx.PointerPosition);
            Vector2 position = Vector2.zero;
            ViewportMesh mesh = null;

            int count = Physics.RaycastNonAlloc(ray, hits, 5.0f, viewportDetectionMask, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = hits[i];

                if (hit.collider.TryGetComponent(out mesh))
                {
                    position = hit.textureCoord;
                    break;
                }
            }

            view.Tick(in ctx, position, mesh);
        }
        private void UpdateRenderTimer(int index, float deltaTime)
        {
            if (index < 0 || index >= renderTimers.Length)
            {
                return;
            }

            renderTimers[index] += deltaTime;
        }
        private void RebuildRenderTimers(int removedIndex)
        {
            float[] newTimers = new float[viewports.Count];

            for (int i = 0; i < viewports.Count; i++)
            {
                int oldIndex = i;

                if (oldIndex >= removedIndex)
                {
                    oldIndex++;
                }

                if (oldIndex >= renderTimers.Length)
                {
                    continue;
                }

                newTimers[i] = renderTimers[oldIndex];
            }

            int newIndex = renderIndex;

            if (renderIndex > removedIndex)
            {
                newIndex--;
            }

            if (viewports.Count > 0)
            {
                newIndex = Mathf.Clamp(newIndex, 0, viewports.Count - 1);
            }
            else
            {
                newIndex = 0;
            }

            renderTimers = newTimers;
            renderIndex = newIndex;
        }
        private void NextRender()
        {
            int count = viewports.Count;

            if (count == 0)
            {
                return;
            }

            int safety = count;

            while (safety-- > 0)
            {
                int index = renderIndex;

                renderIndex = (renderIndex + 1) % count;

                UIViewportView view = viewports[index];

                if (!view.IsActive)
                {
                    continue;
                }

                if (!view.IsRendering)
                {
                    continue;
                }

                if (!view.CanRender)
                {
                    continue;
                }

                float interval = 1f / Mathf.Max(1f, view.FPS);

                if (renderTimers[index] < interval)
                {
                    continue;
                }

                renderTimers[index] -= interval;

                StartRender(view);
                break;
            }
        }

        private void PreRender(UIViewportView view)
        {
            for (int i = 0; i < viewports.Count; i++)
            {
                UIViewportView current = viewports[i];

                if (current != view)
                {
                    current.HideRenderer();
                }
            }
        }
        private void StartRender(UIViewportView view)
        {
            PreRender(view);

            rendererCamera.targetTexture = view.Texture;
            rendererCamera.orthographicSize = view.Size;

            view.Render();

            rendererCamera.Render();

            PostRender(view);
        }
        private void PostRender(UIViewportView view)
        {
            for (int i = 0; i < viewports.Count; i++)
            {
                UIViewportView current = viewports[i];

                if (current != view)
                {
                    current.ShowRenderer();
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
#if UNITY_EDITOR
                Debug.LogWarning($"Viewport [{prefab.ID}] is already added to manager! ignore if its intented");
#endif
                return;
            }

            UIViewportView view = GameObject.Instantiate(prefab, container);
            view.Initialize(rendererCamera);

            rendererCamera.enabled = false;

            ids.Add(view.ID);
            viewports.Add(view);

            Array.Resize(ref renderTimers, viewports.Count);
            renderTimers[^1] = 0f;
        }
        public void Remove(string id)
        {
            if (!ids.Contains(id))
            {
#if UNITY_EDITOR
                Debug.LogWarning("you are trying to remove stage object that does not exists! ignore if its intented");
#endif
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

                    RebuildRenderTimers(i);
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

            renderTimers = Array.Empty<float>();
            renderIndex = 0;
        }

        public void Show(string id, ViewportMesh mesh)
        {
            if (mesh == null)
            {
                Debug.LogError("viewport mesh is null!");
                return;
            }

            if (!ids.Contains(id))
            {
                Debug.LogError("You are trying to show viewport that does not exists!");
                return;
            }

            for (int i = 0; i < viewports.Count; i++)
            {
                if (viewports[i].ID != id)
                {
                    continue;
                }

                viewports[i].ShowViewport(mesh);
                renderTimers[i] = 1f / Mathf.Max(1f, viewports[i].FPS);
                break;
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
                if (viewports[i].ID != id)
                {
                    continue;
                }

                viewports[i].HideViewport();
                break;
            }
        }
    }
}
