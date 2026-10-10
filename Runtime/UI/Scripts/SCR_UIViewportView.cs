using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.UI
{
    [DisallowMultipleComponent]
    internal sealed class UIViewportView : MonoBehaviour
    {
        [Header("_")]
        [SerializeField] private bool debug = false;

        [Header("_")]
        [SerializeField, Required] private Camera rendererCamera = null;
        [SerializeField, Required] private Camera inputCamera = null;
        [SerializeField, Min(0)] private float cullingDistance = 16;
        [SerializeField, Range(-1, 1)] private float cullingDotThreshold = 0.1f;
        [SerializeField] private LayerMask viewportDetectionMask = 0;

        [Header("_")]
        [Info("priority 0 (uzak / kenarda) = minFPS, priority 1 (yakýn / karþýda) = maxFPS. focus'taki item her zaman maxFPS")]
        [SerializeField, Range(1, 60)] private float minFPS = 2;
        [SerializeField, Range(1, 60)] private float maxFPS = 30;
        [SerializeField, Min(1)] private int maxRendersPerFrame = 1;
        [SerializeField, Min(1)] private float maxBacklogIntervals = 2f;

        [Header("_")]
        [SerializeField, Required] private Transform container = null;

        private UIViewportItem focusedItem = null;
        private readonly List<UIViewportItem> items = new(4);
        private readonly RaycastHit[] collisionBuffer = new RaycastHit[5];
        private readonly List<UIViewportItem> renderQueue = new(8);
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
                throw new NullReferenceException($"Viewport input camera not found! {nameof(inputCamera)}");
            }

            rendererCamera.enabled = false;
            inputCamera.enabled = false;
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

            UpdateInput(in ctx);

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].IsActive)
                {
                    items[i].Tick();
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
            float closestDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = collisionBuffer[i];

                if (hit.distance >= closestDistance)
                {
                    continue;
                }

                if (!hit.collider.TryGetComponent(out ViewportMesh mesh))
                {
                    continue;
                }

                closestDistance = hit.distance;
                targetMesh = mesh;
                texturePosition = hit.textureCoord;
            }

            UIViewportItem targetViewport = null;

            if (targetMesh != null)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    UIViewportItem view = items[i];

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

            if (focusedItem != targetViewport)
            {
                if (focusedItem != null)
                {
                    focusedItem.ClearInput();
                }

                focusedItem = targetViewport;
            }

            if (focusedItem != null)
            {
                inputCamera.targetTexture = focusedItem.Texture;
                inputCamera.orthographicSize = focusedItem.Size;
                focusedItem.UpdateInput(in ctx, texturePosition);
            }

            if (debug)
            {


                Debug.Log
                (
                    $"TARGET: {(targetViewport != null ? targetViewport.ID : null ?? "NULL")} | " +
                    $"FOCUS: {(focusedItem != null ? focusedItem.ID : null ?? "NULL")} | " +
                    $"KEY_DOWN: {ctx.KeyDown} | KEY_UP: {ctx.KeyUp} | " +
                    $"IsActive: {(focusedItem != null ? focusedItem.IsActive : null ?? "NULL")} | " +
                    $"IsRendering: {(focusedItem != null ? focusedItem.IsRendering : null ?? "NULL")} | " +
                    $"IsFocused: {(focusedItem != null ? focusedItem.IsFocused : null ?? "NULL")} | " +
                    $"CanRender: {(focusedItem != null ? focusedItem.CanRender : null ?? "NULL")} | " +
                    $"CanReceiveInput: {(focusedItem != null ? focusedItem.CanReceiveInput : null ?? "NULL")} | " +
                    $"HasTickedOnce: {(focusedItem != null ? focusedItem.HasTickedOnce : null ?? "NULL")} | " +
                    $"HasRenderedOnce: {(focusedItem != null ? focusedItem.HasRenderedOnce : null ?? "NULL")} | " +
                    $"RestShown: {(focusedItem != null ? focusedItem.RestShown : null ?? "NULL")} | " +
                    $"Priority: {(focusedItem != null ? focusedItem.Priority : null ?? "NULL")} | "
                );
            }
        }

        private float GetFPS(UIViewportItem item)
        {
            return item.IsFocused ? maxFPS : Mathf.Lerp(minFPS, maxFPS, item.Priority);
        }
        private float GetInterval(UIViewportItem item)
        {
            return 1f / Mathf.Max(1f, GetFPS(item));
        }

        private void UpdateTimer(int index, float deltaTime)
        {
            if (index < 0 || index >= renderTimers.Length)
            {
                return;
            }

            float interval = GetInterval(items[index]);

            renderTimers[index] = Mathf.Min(renderTimers[index] + deltaTime, interval * maxBacklogIntervals);
        }
        private void RebuildTimers(int removedIndex)
        {
            float[] newTimers = new float[items.Count];

            for (int i = 0; i < items.Count; i++)
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

            if (items.Count > 0)
            {
                newIndex = Mathf.Clamp(newIndex, 0, items.Count - 1);
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
            int count = items.Count;

            if (count == 0)
            {
                return;
            }

            int limit = maxRendersPerFrame;

            renderQueue.Clear();

            for (int n = 0; n < count && renderQueue.Count < limit; n++)
            {
                int index = (renderIndex + n) % count;

                UIViewportItem view = items[index];

                if (!view.IsActive || !view.IsRendering || !view.CanRender)
                {
                    continue;
                }

                float interval = GetInterval(view);

                if (renderTimers[index] < interval)
                {
                    continue;
                }

                renderTimers[index] -= interval;
                renderQueue.Add(view);

                renderIndex = (index + 1) % count;
            }

            if (renderQueue.Count > 0)
            {
                RenderBatch();
            }
        }
        private void RenderBatch()
        {
            for (int i = 0; i < items.Count; i++)
            {
                items[i].SuspendForPass();
            }

            for (int i = 0; i < renderQueue.Count; i++)
            {
                UIViewportItem view = renderQueue[i];

                view.BeginPass();

                rendererCamera.targetTexture = view.Texture;
                rendererCamera.orthographicSize = view.Size;

                view.Render();
                rendererCamera.Render();

                view.SuspendForPass();
            }

            for (int i = 0; i < items.Count; i++)
            {
                items[i].EndPass();
            }
        }
        private void CullRender(Camera camera)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].EnableCulling)
                {
                    items[i].TryCull(camera.transform, cullingDotThreshold, cullingDistance);
                }               
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
                Debug.LogWarning($"Viewport [{prefab.ID}] is already added to manager! ignore if its intented");
#endif
                return;
            }

            UIViewportItem view = GameObject.Instantiate(prefab, container);
            view.Initialize(prefab.CanReceiveInput ? inputCamera : rendererCamera);

            rendererCamera.enabled = false;

            items.Add(view);

            Array.Resize(ref renderTimers, items.Count);
            renderTimers[^1] = 0f;
        }
        public void Remove(string id)
        {
            if (!Contains(id))
            {
#if UNITY_EDITOR
                Debug.LogWarning("you are trying to remove stage object that does not exists! ignore if its intented");
#endif
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].ID == id)
                {
                    UIViewportItem view = items[i];

                    if (focusedItem == view)
                    {
                        view.ClearInput();
                        focusedItem = null;
                    }

                    items.Remove(view);
                    Destroy(view.gameObject);

                    RebuildTimers(i);
                    break;
                }
            }
        }
        public void Clear()
        {
            for (int i = 0; i < items.Count; i++)
            {
                items[i].Deinitialize();
                Destroy(items[i].gameObject);
            }

            focusedItem = null;

            items.Clear();

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

            if (!Contains(id))
            {
                Debug.LogError("You are trying to show viewport that does not exists!");
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].ID != id)
                {
                    continue;
                }

                items[i].ShowViewport(mesh);
                renderTimers[i] = GetInterval(items[i]);
                break;
            }
        }
        public void Hide(string id)
        {
            if (!Contains(id))
            {
                Debug.LogError("You are trying to hide viewport that does not exists!");
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].ID != id)
                {
                    continue;
                }

                items[i].HideViewport();
                break;
            }
        }

        private bool Contains(string id)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].ID == id)
                {
                    return true;
                }
            }

            return false;
        }
    }
}