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
        [SerializeField, Range(0, 120)] private float rendererFPS = 59;
        [SerializeField, Min(0)] private float cullingDistance = 16;

        [Header("_")]
        [SerializeField, Required] private Transform container = null;

        private readonly List<string> ids = new(4);
        private readonly List<UIViewportView> viewports = new(4);
        private int mask = -1;
        private float time = 0;
        private float interval = 1;

        private void Start()
        {
            rendererCamera.enabled = false;

            mask = LayerMask.GetMask("Viewport");

            time = 0f;
            interval = 1 / rendererFPS;
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

            time += Time.deltaTime;

            for (int i = 0; i < viewports.Count; i++)
            {
                UpdateTick(viewports[i], in ctx);
            }

            if (time > interval)
            {
                time = 0;

                for (int i = 0; i < viewports.Count; i++)
                {
                    UpdateRender(viewports[i]);
                }
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
            Vector2 screenPosition = Vector2.zero;
            ViewportRenderer renderer = null;

            if (Physics.Raycast(ray, out RaycastHit hit, 5.0f, mask, QueryTriggerInteraction.Ignore))
            {
                renderer = hit.collider.GetComponent<ViewportRenderer>();
                screenPosition = hit.textureCoord;
            }

            view.Tick(in ctx, screenPosition, renderer);
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
                UIViewportView view = viewports[i];

                if (!view.IsActive)
                {
                    continue;
                }

                bool isInView = view.CheckVisibility(camera.transform, cullingDistance);

                if (!isInView)
                {
                    view.HideRenderer();
                    continue;
                }

                view.ShowRenderer();
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
