using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIViewportItem : MonoBehaviour
    {
        internal string ID => id;
        internal bool IsActive => isActive;
        internal bool HasInput => receiveInput;
        internal float Size => canvasSize;
        internal RenderTexture Texture => renderTexture;
        internal UIViewport Viewport => viewport;
        protected Camera Camera => canvases[0].Camera;
        protected Canvas Canvas => canvases[0].Canvas;
        protected RectTransform Transform => canvases[0].Transform;
        protected Vector2 PointerPosition => pointerPosition;

        [Header("_")]
        [SerializeField] private bool debug = false;

        [Header("_")]
        [SerializeField, Required] private string id = string.Empty;
        [SerializeField, Required] private RenderTexture renderTexture = null;
        [SerializeField, Min(1)] private float canvasSize = 165;
        [SerializeField] private bool receiveInput = false;

        private UIViewportCanvas[] canvases = null;
        private UIViewport viewport = null;
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
        private bool isActive = false;

        private void OnEnable()
        {
            ManagerGame.OnStateChanged += OnGameStateChanged;
        }
        private void OnDisable()
        {
            ManagerGame.OnStateChanged -= OnGameStateChanged;

            if (isInitialized)
            {
                OnDeinitialized();
            }
        }

        /// <summary> Called every frame. </summary>
        protected abstract void OnTick(float deltaTime);
        /// <summary> Called every render frame. </summary>
        protected abstract void OnRender();
        /// <summary> Called when created. </summary>
        protected abstract void OnInitialized();
        /// <summary> Called when destroyed. </summary>
        protected abstract void OnDeinitialized();
        /// <summary> Called when show. </summary>
        protected abstract void OnShow(UIViewport mesh);
        /// <summary> Called when hide. </summary>
        protected abstract void OnHide();
        protected virtual void OnGameStateChanged(GameState gameState) { }

        internal void Tick(float deltaTime)
        {
            if (!isActive)
            {
                return;
            }

            OnTick(deltaTime);
        }
        internal void Render()
        {
            if (!isActive)
            {
                return;
            }

            OnRender();
        }

        internal void UpdateInput(in UIInputContext ctx, Vector2 screenPosition)
        {
            if (!receiveInput || !isActive)
            {
                return;
            }

            float renderWidth = Texture.width;
            float renderHeight = Texture.height;

            pointerPosition = new Vector2
            (
                Mathf.Clamp01(screenPosition.x) * renderWidth,
                Mathf.Clamp01(screenPosition.y) * renderHeight
            );

            eventData.Reset();

            eventData.delta = pointerPosition - lastPixelPosition;
            lastPixelPosition = pointerPosition;

            eventData.position = pointerPosition;
            eventData.scrollDelta = ctx.PointerScroll * 32f;

            hitResults.Clear();

            for (int i = 0; i < canvases.Length; i++)
            {
                GraphicRaycaster raycaster = canvases[i].Raycaster;

                if (raycaster != null)
                {
                    raycaster.Raycast(eventData, hitResults);
                }
            }

            FilterRaycasts();
            SortRaycasts();

            GameObject topObject = hitResults.Count > 0 ? hitResults[0].gameObject : null;
            RaycastResult topRaycast = hitResults.Count > 0 ? hitResults[0] : default;

            OnHover(topObject);

            if (eventData.scrollDelta.sqrMagnitude > 0f && topObject != null)
            {
                OnScroll(topObject);
            }

            if (ctx.KeyDown)
            {
                OnPointerDown(topObject, topRaycast);
            }

            OnDrag();

            if (ctx.KeyUp)
            {
                OnPointerUp(topObject);
            }

            if (debug)
            {
                Debug.Log
                (
                    $"VIEW={ID} | " +
                    $"TOP={(topObject != null ? topObject.name : "NULL")} | " +
                    $"HITS={hitResults.Count}"
                );
            }
        }
        protected void EnableInput()
        {
            if (receiveInput)
            {
                return;
            }

            receiveInput = true;
            OnPointerExit();
        }
        protected void DisableInput()
        {
            if (!receiveInput)
            {
                return;
            }

            receiveInput = false;
            OnPointerExit();
        }

        internal void Initialize(Camera camera)
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
            isActive = false;

            Canvas[] canvases = GetComponentsInChildren<Canvas>();

            this.canvases = new UIViewportCanvas[canvases.Length];

            for (int i = 0; i < canvases.Length; i++)
            {
                this.canvases[i] = new
                (
                    camera,
                    canvases[i],
                    canvases[i].GetComponent<RectTransform>(),
                    canvases[i].GetComponent<GraphicRaycaster>()
                );
            }

            Canvas.Hide();

            eventSystem = EventSystem.current;
            eventData = new PointerEventData(eventSystem);

            OnInitialized();
        }
        internal void Deinitialize()
        {
            if (!isInitialized)
            {
                return;
            }

            OnPointerExit();
            OnDeinitialized();

            isInitialized = false;
        }

        internal void ShowViewport(UIViewport mesh)
        {
            if (isActive)
            {
                return;
            }

            isActive = true;

            if (receiveInput)
            {
                ManagerUI.Instance.ShowCursor();
            }

            OnShow(viewport = mesh);
            Canvas.Show();
            viewport.ShowRenderer();
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

            OnPointerExit();
            OnHide();
            Canvas.Hide();
            viewport.HideRenderer();

            viewport = null;
            isActive = false;
        }

        private void FilterRaycasts()
        {
            for (int i = hitResults.Count - 1; i >= 0; i--)
            {
                GameObject gameObject = hitResults[i].gameObject;

                if (gameObject.TryGetComponent(out Graphic graphic) && !graphic.raycastTarget)
                {
                    hitResults.RemoveAt(i);
                    continue;
                }

                if (!gameObject.activeInHierarchy)
                {
                    hitResults.RemoveAt(i);
                    continue;
                }

                if (gameObject.TryGetComponent(out LayoutElement layout) && layout.ignoreLayout)
                {
                    hitResults.RemoveAt(i);
                }
            }
        }
        private void SortRaycasts()
        {
            static int compare(RaycastResult a, RaycastResult b)
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
            }

            hitResults.Sort(compare);
        }

        internal void OnDrag()
        {
            if (currentPressedObject != null && currentDraggedObject == null)
            {
                const float DRAG_START_DISTANCE = 8f;

                float distance = Vector2.Distance(lastPressedPosition, pointerPosition);

                if (distance >= DRAG_START_DISTANCE)
                {
                    currentDraggedObject = currentPressedObject;

                    ExecuteEvents.Execute(currentDraggedObject, eventData, ExecuteEvents.beginDragHandler);
                }
            }

            if (currentDraggedObject != null)
            {
                ExecuteEvents.Execute(currentDraggedObject, eventData, ExecuteEvents.dragHandler);
            }
        }
        internal void OnScroll(GameObject topObject)
        {
            ExecuteEvents.ExecuteHierarchy(topObject, eventData, ExecuteEvents.scrollHandler);
        }
        internal void OnHover(GameObject topObject)
        {
            if (topObject == currentHoveredObject)
            {
                return;
            }

            if (currentHoveredObject != null)
            {
                ExecuteEvents.Execute(currentHoveredObject, eventData, ExecuteEvents.pointerExitHandler );
            }

            if (topObject != null)
            {
                ExecuteEvents.Execute(topObject, eventData, ExecuteEvents.pointerEnterHandler);
            }

            currentHoveredObject = topObject;
        }
        internal void OnPointerDown(GameObject topObject, RaycastResult topRaycast)
        {
            currentPressedObject = topObject;

            lastPressedPosition = pointerPosition;

            eventData.pressPosition = pointerPosition;
            eventData.pointerPressRaycast = topRaycast;
            eventData.pointerCurrentRaycast = topRaycast;
            eventData.button = PointerEventData.InputButton.Left;
            eventData.eligibleForClick = true;
            eventData.useDragThreshold = true;

            if (topObject != null)
            {
                ExecuteEvents.Execute(topObject, eventData, ExecuteEvents.pointerDownHandler);
            }
        }
        internal void OnPointerUp(GameObject topObject)
        {
            if (currentDraggedObject != null)
            {
                ExecuteEvents.Execute(currentDraggedObject, eventData, ExecuteEvents.endDragHandler);

                currentDraggedObject = null;
            }

            if (topObject != null)
            {
                ExecuteEvents.Execute(topObject, eventData, ExecuteEvents.pointerUpHandler);
            }

            if (currentPressedObject != null && topObject == currentPressedObject) 
            {
                ExecuteEvents.Execute(topObject, eventData, ExecuteEvents.pointerClickHandler); 
            }

            currentPressedObject = null;
        }
        internal void OnPointerExit()
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
            lastPressedPosition = Vector2.zero;

            currentPressedObject = null;
            currentHoveredObject = null;
            currentDraggedObject = null;

            eventData.Reset();

            hitResults.Clear();
        }
    }
}