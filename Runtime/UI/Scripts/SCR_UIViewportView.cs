using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.UI
{
    [DisallowMultipleComponent]
    public abstract class UIViewportView : MonoBehaviour
    {
        internal string ID => id;
        internal bool IsActive => isActive;
        internal bool IsRendering => isRendering;
        internal bool CanRender => !renderOnce || !hasRenderedOnce;
        internal bool CanReceiveInput => receiveInput;
        internal bool HasTickedOnce => hasTickedOnce;
        internal bool HasRenderedOnce => hasRenderedOnce;
        internal float Size => canvasSize;
        internal float FPS => isFocused ? maxFPS : Mathf.Lerp(maxFPS, minFPS, distanceRatio);
        protected Camera Camera => data[0].Camera;
        internal Canvas Canvas => data[0].Canvas;
        internal RectTransform Transform => data[0].Transform;
        internal RenderTexture Texture => renderTexture;
        internal ViewportMesh Mesh => mesh;
        protected Vector2 PointerPosition => pointerPosition;

        [Header("_")]
        [SerializeField, Required] private string id = string.Empty;
        [SerializeField, Required] private RenderTexture renderTexture = null;
        [SerializeField, Min(1)] private float canvasSize = 165;

        [Header("_")]
        [SerializeField] private bool receiveInput = false;
        [SerializeField] private bool renderOnce = false;
        [SerializeField, Range(0, 59)] private float minFPS = 1;
        [SerializeField, Range(0, 59)] private float maxFPS = 59;

        private UIViewportCanvas[] data = null;
        private ViewportMesh mesh = null;
        private EventSystem eventSystem = null;
        private PointerEventData eventData = null;
        private GameObject currentPressedObject = null;
        private GameObject currentHoveredObject = null;
        private GameObject currentDraggedObject = null;
        private readonly List<RaycastResult> hitResults = new(16);
        private Vector2 pointerPosition = Vector2.zero;
        private Vector2 lastPixelPosition = Vector2.zero;
        private Vector2 lastPressedPosition = Vector2.zero;
        private float distanceRatio = 0;
        private bool isInitialized = false;
        private bool isRendering = false;
        private bool isActive = false;
        private bool isFocused = false;
        private bool hasRenderedOnce = false;
        private bool hasTickedOnce = false;

        private void OnEnable()
        {
            ManagerGame.OnGameStateChanged += OnGameStateChanged;
        }
        private void OnDisable()
        {
            ManagerGame.OnGameStateChanged -= OnGameStateChanged;

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
        protected abstract void OnShow(ViewportMesh mesh);
        /// <summary> Called when interaction exit. </summary>
        protected abstract void OnHide();
        /// <summary> Called when game state changed. </summary>
        protected virtual void OnGameStateChanged(GameState gameState) { }

        internal void Tick()
        {
            OnTick();
            hasTickedOnce = true;
        }
        internal void Render()
        {
            OnRender();
            hasRenderedOnce = true;
        }

        protected void MarkDirty() => hasRenderedOnce = false;
        protected void EnableInput()
        {
            if (receiveInput)
            {
                return;
            }

            receiveInput = true;
            ClearInput();
        }
        protected void DisableInput()
        {
            if (!receiveInput)
            {
                return;
            }

            receiveInput = false; 
            ClearInput();
        }
        internal void UpdateInput(in UIInputContext ctx, Vector2 screenPosition)
        {
            if (!receiveInput)
            {
                return;
            }

            if (!isActive)
            {
                return;
            }

            isFocused = true;

            Vector2 scrollDelta = ctx.PointerScroll * 32f;
            bool keyDown = ctx.KeyDown;
            bool keyUp = ctx.KeyUp;

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
        internal void ClearInput()
        {
            if (!CanReceiveInput)
            {
                return;
            }

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
            isFocused = false;
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

            if (mesh != null)
            {
                mesh.ShowRenderer();
            }
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

            if (mesh != null)
            {
                mesh.HideRenderer();
            }
        }

        internal void ShowViewport(ViewportMesh mesh)
        {
            if (isActive)
            {
                return;
            }

            if (receiveInput)
            {
                ManagerUI.Instance.ShowCursor();
            }

            hasRenderedOnce = false;
            isActive = true;

            OnShow(this.mesh = mesh);
            ShowRenderer();
        }      
        internal void HideViewport()
        {
            if (!isActive)
            {
                return;
            }

            if (receiveInput)
            {
                ManagerUI.Instance.HideCursor();
            }

            OnHide();
            HideRenderer();
            ClearInput();

            isActive = false;
        }

        internal void TryCull(Transform target, float cullingDistance)
        {
            if (!IsActive)
            {
                return;
            }

            bool isInView = mesh.CheckVisibility(target, cullingDistance, out float actualDistance);

            distanceRatio = Mathf.Clamp01(actualDistance / cullingDistance);

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