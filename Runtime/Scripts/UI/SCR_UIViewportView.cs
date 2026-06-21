using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.UI
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public abstract class UIViewportView : MonoBehaviour
    {
        internal string ID => id;
        internal bool IsActive => isRendering;
        internal bool IsRendering => isActive;
        internal float Size => canvasSize;
        protected Camera Camera => data[0].Camera;
        internal Canvas Canvas => data[0].Canvas;
        internal RectTransform Transform => data[0].Transform;
        internal RenderTexture Texture => renderTexture;
        protected Vector2 PointerPosition => pointerPosition;

        [Header("_")]
        [SerializeField, Required] private string id = string.Empty;
        [SerializeField, Required] private RenderTexture renderTexture = null;
        [SerializeField, Range(0, 120)] private float rendererFPS = 59;

        [Header("_")]
        [SerializeField] private bool canvasInput = false;
        [SerializeField, Min(1)] private float canvasSize = 165;

        private UIViewportCanvas[] data = null;
        private new ViewportRenderer renderer = null;
        private EventSystem eventSystem = null;
        private PointerEventData eventData = null;
        private GameObject currentPressedObject = null;
        private GameObject currentHoveredObject = null;
        private GameObject currentDraggedObject = null;
        private readonly List<RaycastResult> hitResults = new(16);
        private Vector2 pointerPosition = Vector2.zero;
        private Vector2 lastPixelPosition = Vector2.zero;
        private Vector2 lastPressedPosition = Vector2.zero;
        private bool isInitialized = false;
        private bool isRendering = false;
        private bool isActive = false;
        private float renderTime = 0;
        private float renderInterval = 1;

        private void OnEnable()
        {
            GameManager.OnGameStateChanged += OnGameStateChanged;
        }
        private void OnDisable()
        {
            GameManager.OnGameStateChanged -= OnGameStateChanged;

            OnDeinitialized();
        }

        /// <summary> Called every frame. </summary>
        protected abstract void OnTick();
        /// <summary> Called after tick. </summary>
        protected abstract void OnRender();
        /// <summary> Called when created. </summary>
        protected abstract void OnInitialized();
        /// <summary> Called when destroyed. </summary>
        protected abstract void OnDeinitialized();
        /// <summary> Called when interaction enter. </summary>
        protected abstract void OnShow(ViewportRenderer renderer);
        /// <summary> Called when interaction exit. </summary>
        protected abstract void OnHide();

        protected virtual void OnGameStateChanged(GameState gameState) { }

        internal void Tick(in UIInputContext ctx, Vector2 screenPosition, ViewportRenderer renderer)
        {
            if (canvasInput)
            {                
                if (renderer == this.renderer)
                {
                    UpdateInput(screenPosition, ctx.PointerScroll * 32f, ctx.KeyDown, ctx.KeyUp);
                }
                else
                {
                    ClearInput();
                }
            }

            OnTick();
        }
        internal void Render()
        {
            renderTime += Time.deltaTime;

            if (renderTime > renderInterval)
            {
                renderTime = 0;

                OnRender();
            }
        }

        protected void EnableInput() => canvasInput = true;
        protected void DisableInput() 
        {
            if (!canvasInput)
            {
                return;
            }

            canvasInput = false; 
            ClearInput();
        }
        private void UpdateInput(Vector2 screenPosition, Vector2 scrollDelta, bool keyDown, bool keyUp)
        {
            float renderWidth = Texture.width;
            float renderHeight = Texture.height;

            pointerPosition = new(Mathf.Clamp01(screenPosition.x) * renderWidth, Mathf.Clamp01(screenPosition.y) * renderHeight);

            eventData.delta = pointerPosition - lastPixelPosition;
            lastPixelPosition = pointerPosition;

            eventData.Reset();
            eventData.position = pointerPosition;
            eventData.scrollDelta = scrollDelta;

            hitResults.Clear();
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].Raycaster != null)
                {
                    data[i].Raycaster.Raycast(eventData, hitResults);
                }
            }

            // Filtre
            hitResults.RemoveAll(r =>
            {
                GameObject gameObject = r.gameObject;

                if (gameObject.TryGetComponent(out Graphic graphic) && !graphic.raycastTarget)
                {
                    return true;
                }

                if (!gameObject.activeInHierarchy)
                {
                    return true;
                }

                if (gameObject.TryGetComponent(out LayoutElement layout) && layout.ignoreLayout)
                {
                    return true;
                }

                return false;
            });

            // Sort
            hitResults.Sort((a, b) =>
            {
                int sortOrder = b.sortingOrder.CompareTo(a.sortingOrder);

                if (sortOrder != 0)
                {
                    return sortOrder;
                }

                int depth = b.depth.CompareTo(a.depth);

                if (depth != 0)
                {
                    return depth;
                }

                return a.distance.CompareTo(b.distance);
            });

            GameObject topObject = hitResults.Count > 0 ? hitResults[0].gameObject : null;
            RaycastResult topRaycast = hitResults.Count > 0 ? hitResults[0] : default;

            if (topObject != currentHoveredObject)
            {
                if (currentHoveredObject != null)
                {
                    // Pointer Exit
                    ExecuteEvents.Execute(currentHoveredObject, eventData, ExecuteEvents.pointerExitHandler);
                }

                if (topObject != null)
                {
                    // Pointer Enter
                    ExecuteEvents.Execute(topObject, eventData, ExecuteEvents.pointerEnterHandler);
                }

                currentHoveredObject = topObject;
            }

            // Scroll
            if (scrollDelta.sqrMagnitude > 0.0f && topObject != null)
            {
                ExecuteEvents.ExecuteHierarchy(topObject, eventData, ExecuteEvents.scrollHandler);
            }

            // Pointer Down
            if (keyDown)
            {
                currentPressedObject = topObject;
                lastPressedPosition = this.pointerPosition;
                eventData.pressPosition = this.pointerPosition;
                eventData.pointerPressRaycast = topRaycast;
                eventData.pointerCurrentRaycast = topRaycast;
                eventData.button = PointerEventData.InputButton.Left;
                eventData.eligibleForClick = true;
                eventData.useDragThreshold = true;

                ExecuteEvents.Execute(topObject, eventData, ExecuteEvents.pointerDownHandler);
            }

            // -------- Drag Logic --------
            if (currentPressedObject != null && currentDraggedObject == null)
            {
                float dist = Vector2.Distance(lastPressedPosition, this.pointerPosition);

                const float dragStartDistance = 8f;
                if (dist >= dragStartDistance)
                {
                    currentDraggedObject = currentPressedObject;

                    // Begin Drag
                    ExecuteEvents.Execute(currentDraggedObject, eventData, ExecuteEvents.beginDragHandler);
                }
            }

            if (currentDraggedObject != null)
            {
                // Drag
                ExecuteEvents.Execute(currentDraggedObject, eventData, ExecuteEvents.dragHandler);
            }

            if (keyUp)
            {
                // End Drag
                if (currentDraggedObject != null)
                {
                    ExecuteEvents.Execute(currentDraggedObject, eventData, ExecuteEvents.endDragHandler);
                    currentDraggedObject = null;
                }

                // Pointer Up
                ExecuteEvents.Execute(topObject, eventData, ExecuteEvents.pointerUpHandler);

                if (currentPressedObject != null && topObject == currentPressedObject)
                {
                    // Pointer Click
                    ExecuteEvents.Execute(topObject, eventData, ExecuteEvents.pointerClickHandler);
                }

                currentPressedObject = null;
            }
        }
        private void ClearInput()
        {
            if (eventData == null)
            {
                return;
            }

            if (currentHoveredObject != null)
            {
                ExecuteEvents.Execute(currentHoveredObject, eventData, ExecuteEvents.pointerExitHandler);
            }

            if (currentDraggedObject != null)
            {
                ExecuteEvents.Execute(currentDraggedObject, eventData, ExecuteEvents.endDragHandler);
            }

            eventData.position = Vector2.zero;
            eventData.delta = Vector2.zero;
            pointerPosition = Vector2.zero;
            lastPixelPosition = Vector2.zero;
            currentPressedObject = null;
            currentDraggedObject = null;
            currentHoveredObject = null;
            eventData.Reset();
            hitResults.Clear();
        }

        internal void Initialize(Camera camera)
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
            isRendering = false;
            isActive = false;

            renderTime = 0f;
            renderInterval = 1 / rendererFPS;

            Canvas[] canvases = GetComponentsInChildren<Canvas>();
            data = new UIViewportCanvas[canvases.Length];

            for (int i = 0; i < data.Length; i++)
            {
                data[i] = new
                (
                    camera, 
                    canvases[i], 
                    canvases[i].GetComponent<RectTransform>(), 
                    canvases[i].GetComponent<GraphicRaycaster>()
                );
            }           

            Canvas.Hide();

            camera.targetTexture = renderTexture;
            eventSystem = EventSystem.current;
            eventData = new(eventSystem);

            OnInitialized();
        }
        internal void Deinitialize()
        {
            if (!isInitialized)
            {
                return;
            }

            isInitialized = false;
            OnDeinitialized();
        }

        internal void ShowRenderer()
        {
            if (!isActive)
            {
                return;
            }

            if (isRendering)
            {
                return;
            }

            Canvas.Show();
            isRendering = true;

            if (renderer != null) renderer.ShowRenderer();
        }
        internal void HideRenderer()
        {
            if (!isActive)
            {
                return;
            }

            if (!isRendering)
            {
                return;
            }

            Canvas.Hide();
            isRendering = false;

            if (renderer != null) renderer.HideRenderer();
        }

        internal void ShowViewport(ViewportRenderer renderer)
        {
            if (isActive)
            {
                return;
            }

            if (canvasInput)
            {
                UIManager.Instance.ShowCursor();
            }

            isActive = true;
            this.renderer = renderer;

            OnShow(this.renderer);
            ShowRenderer();
        }      
        internal void HideViewport()
        {
            if (!isActive)
            {
                return;
            }

            if (canvasInput)
            {
                UIManager.Instance.HideCursor();
            }

            OnHide();
            HideRenderer();

            isActive = false;
        }

        internal void TryCull(Transform target, float distance)
        {
            if (!IsActive)
            {
                return;
            }

            bool isInView = renderer.CheckVisibility(target, distance);

            if (!isInView)
            {
                HideRenderer();
            }
            else
            {
                ShowRenderer();
            }
        }
    }
}