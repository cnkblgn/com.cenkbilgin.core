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
        [SerializeField] private LayerMask viewportDetectionMask = 0;

        [Header("_")]
        [SerializeField, Required] private Transform container = null;

        private UIViewportItem focusedItem = null;
        private readonly List<string> ids = new(4);
        private readonly List<UIViewportItem> items = new(4);
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
                    $"KEY_DOWN: {ctx.KeyDown} | KEY_UP: {ctx.KeyUp}"
                );
            }
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

            int safety = count;

            while (safety-- > 0)
            {
                int index = renderIndex;

                renderIndex = (renderIndex + 1) % count;

                UIViewportItem view = items[index];

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
        private void PreRender(UIViewportItem view)
        {
            for (int i = 0; i < items.Count; i++)
            {
                UIViewportItem current = items[i];

                if (current != view)
                {
                    current.HideRenderer();
                }
            }
        }
        private void StartRender(UIViewportItem view)
        {
            PreRender(view);

            rendererCamera.targetTexture = view.Texture;
            rendererCamera.orthographicSize = view.Size;

            view.Render();

            rendererCamera.Render();

            PostRender(view);
        }
        private void PostRender(UIViewportItem view)
        {
            for (int i = 0; i < items.Count; i++)
            {
                UIViewportItem current = items[i];

                if (current != view)
                {
                    current.ShowRenderer();
                }
            }
        }
        private void CullRender(Camera camera)
        {
            for (int i = 0; i < items.Count; i++)
            {
                items[i].TryCull(camera.transform, cullingDistance);
            }
        }

        public void Add(UIViewportItem prefab)
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

            UIViewportItem view = GameObject.Instantiate(prefab, container);
            view.Initialize(prefab.CanReceiveInput ? inputCamera : rendererCamera);

            rendererCamera.enabled = false;

            ids.Add(view.ID);
            items.Add(view);

            Array.Resize(ref renderTimers, items.Count);
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

                    ids.Remove(id);
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

            ids.Clear();
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

            if (!ids.Contains(id))
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
                renderTimers[i] = 1f / Mathf.Max(1f, items[i].FPS);
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
    }
}
