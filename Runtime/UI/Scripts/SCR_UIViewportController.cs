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
        [SerializeField, Required] private Camera inputCamera = null;
        [SerializeField, Min(0)] private float cullingDistance = 16;
        [SerializeField] private LayerMask viewportDetectionMask = 0;

        [Header("_")]
        [SerializeField, Required] private Transform container = null;

        private readonly List<string> ids = new(4);
        private readonly List<UIViewportView> viewports = new(4);
        private UIViewportView focusedViewport = null;
        private readonly RaycastHit[] collisionBuffer = new RaycastHit[5];
        private float[] renderTimers = Array.Empty<float>();
        private int renderIndex = 0;

        private void Awake()
        {
            if (rendererCamera == null)
            {
                throw new NullReferenceException($"Viewport renderer camera not found! {nameof(rendererCamera)}");
            }

            if (inputCamera == null)
            {
                throw new NullReferenceException($"Viewport renderer camera not found! {nameof(inputCamera)}");
            }

            rendererCamera.enabled = false;
            inputCamera.enabled = false;
        }
        private void OnEnable() => ManagerGame.OnBeforeSceneChanged += OnBeforeSceneChanged;
        private void OnDisable() => ManagerGame.OnBeforeSceneChanged -= OnBeforeSceneChanged;

        private void OnBeforeSceneChanged(string obj) => Clear();

        internal void Tick(in UIInputContext ctx)
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

            UpdateInput(in ctx);

            for (int i = 0; i < viewports.Count; i++)
            {
                if (viewports[i].IsActive)
                {
                    viewports[i].Tick();
                }

                UpdateTimer(i, deltaTime);
            }

            CullRender(ctx.Camera);
            NextRender();
        }

        private void UpdateInput(in UIInputContext ctx)
        {
            Ray ray = ctx.Camera.ScreenPointToRay(ctx.PointerPosition);

            int count = Physics.RaycastNonAlloc(ray, collisionBuffer, 5.0f, viewportDetectionMask, QueryTriggerInteraction.Ignore);

            ViewportMesh targetMesh = null;
            Vector2 texturePosition = Vector2.zero;

            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = collisionBuffer[i];

                if (hit.collider.TryGetComponent(out ViewportMesh mesh))
                {
                    targetMesh = mesh;
                    texturePosition = hit.textureCoord;
                    break;
                }
            }

            UIViewportView targetViewport = null;

            if (targetMesh != null)
            {
                for (int i = 0; i < viewports.Count; i++)
                {
                    UIViewportView view = viewports[i];

                    if (!view.IsActive)
                    {
                        continue;
                    }

                    if (!view.CanReceiveInput)
                    {
                        continue;
                    }

                    if (view.Mesh != targetMesh)
                    {
                        continue;
                    }

                    targetViewport = view;
                    break;
                }
            }

            if (focusedViewport != targetViewport)
            {
                if (focusedViewport != null)
                {
                    focusedViewport.ClearInput();
                }

                focusedViewport = targetViewport;
            }

            if (focusedViewport != null)
            {
                focusedViewport.UpdateInput(in ctx, texturePosition);
            }

            //Debug.Log(
            //    $"TARGET: {targetViewport?.ID ?? "NULL"} | " +
            //    $"FOCUS: {focusedViewport?.ID ?? "NULL"} | " +
            //    $"MouseDown: {ctx.KeyDown} | MouseUp: {ctx.KeyUp}");
        }
        private void UpdateTimer(int index, float deltaTime)
        {
            if (index < 0 || index >= renderTimers.Length)
            {
                return;
            }

            renderTimers[index] += deltaTime;
        }
        private void RebuildTimers(int removedIndex)
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
            view.Initialize(rendererCamera, inputCamera);

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

                    if (focusedViewport == view)
                    {
                        view.ClearInput();
                        focusedViewport = null;
                    }

                    ids.Remove(id);
                    viewports.Remove(view);
                    Destroy(view.gameObject);

                    RebuildTimers(i);
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

            focusedViewport = null;

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
