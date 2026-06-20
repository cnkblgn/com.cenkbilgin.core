using UnityEngine;

namespace Core.UI
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshCollider))]
    [RequireComponent(typeof(MeshRenderer))]
    public sealed class ViewportRenderer : MonoBehaviour 
    {
        public GameObject Connection => connection;

        [Header("_")]
        [SerializeField, Required] private UIViewportView prefab = null;
        [SerializeField, Required] private GameObject connection = null;

        [Header("_")]
        [SerializeField] private bool addStageOnAwake = true;
        [SerializeField] private bool disableRendererOnAwake = true;

        private new MeshRenderer renderer = null;
        private string id = STRING_NULL;
        private bool isViewportAdded = false;

        private void Awake()
        {
            renderer = GetComponent<MeshRenderer>();

            gameObject.SetLayer(LayerMask.NameToLayer("Viewport"));

            id = prefab.ID;
        }
        private void Start()
        {
            if (addStageOnAwake)
            {
                AddViewport();
            }

            if (disableRendererOnAwake)
            {
                HideRenderer();
            }
            else
            {
                ShowRenderer();
            }
        }

        public void AddViewport()
        {
            if (isViewportAdded)
            {
                Debug.LogWarning($"Viewport [{id}] is already added to manager!");
                return;
            }

            UIManager.Instance.AddViewport(prefab);
            isViewportAdded = true;
        }
        public void ShowViewport()
        {
            if (!isViewportAdded)
            {
                Debug.LogWarning("Trying to show viewport that is not added?");
                return;
            }

            UIManager.Instance.ShowViewport(id, this);
        }
        public void HideViewport()
        {
            if (!isViewportAdded)
            {
                return;
            }

            UIManager.Instance.HideViewport(id);
        }

        public void ShowRenderer() => renderer.enabled = true;
        public void HideRenderer() => renderer.enabled = false;

        public bool CheckVisibility(Transform target, float minDistance)
        {
            if (renderer == null)
            {
                return false;
            }

            Vector3 vector = (renderer.bounds.center - target.position);
            float d = vector.magnitude;

            if (d > minDistance)
            {
                return false;
            }

            if (d < 3.0f)
            {
                return true;
            }

            Transform rendererOrigin = renderer.transform;

            return IsFacingEachOther(rendererOrigin.position, target.position, rendererOrigin.forward, target.forward, 0.1f);
        }
    }
}