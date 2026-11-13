using UnityEngine;

namespace Core.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public abstract class UISurfaceItem : MonoBehaviour
    {
        internal string ID => id;
        internal bool IsActive => isActive;
        internal bool HasInput => receiveInput;
        internal Canvas Canvas => thisCanvas;
        internal UISurfaceMesh Mesh => thisMesh;

        [Header("_")]
        [SerializeField, Required] private string id = string.Empty;
        [SerializeField] private bool receiveInput = false;

        private Canvas thisCanvas;
        private Camera thisCamera;
        private UISurfaceMesh thisMesh;
        private bool isInitialized = false;
        private bool isActive = false;

        private void OnDisable() => OnDeinitialized();

        /// <summary> Called every frame. </summary>
        protected abstract void OnTick(float deltaTime);
        /// <summary> Called when created. </summary>
        protected abstract void OnInitialized();
        /// <summary> Called when destroyed. </summary>
        protected abstract void OnDeinitialized();
        /// <summary> Called when show. </summary>
        protected abstract void OnShow(UISurfaceMesh surface);
        /// <summary> Called when hide. </summary>
        protected abstract void OnHide();

        internal void Tick(float deltaTime) => OnTick(deltaTime);

        internal void Initialize(Camera camera)
        {
            if (isInitialized)
            {
                return;
            }

            thisCamera = camera;
            thisCanvas = GetComponent<Canvas>();
            thisCanvas.Hide();

            isInitialized = true;
            isActive = false;

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

        internal void ShowSurface(UISurfaceMesh mesh)
        {
            if (isActive)
            {
                return;
            }

            isActive = true;

            transform.SetPositionAndRotation(mesh.transform.position, Quaternion.LookRotation(mesh.transform.forward));
            transform.localScale = Vector3.one * mesh.Scale;

            OnShow(this.thisMesh = mesh);
            ShowCanvas();
        }
        internal void HideSurface()
        {
            if (!isActive)
            {
                return;
            }

            OnHide();
            HideCanvas();
   
            isActive = false;
        }

        private void ShowCanvas()
        {
            if (!isActive)
            {
                return;
            }

            thisCanvas.Show();

            if (receiveInput)
            {
                Canvas.worldCamera = thisCamera;
            }
        }
        private void HideCanvas()
        {
            if (!isActive)
            {
                return;
            }

            thisCanvas.Hide();
        }
    }
}