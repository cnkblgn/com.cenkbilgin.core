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
        [SerializeField] private bool addOnAwake = true;
        [SerializeField] private bool showOnAwake = false;

        private new MeshRenderer renderer = null;
        private string id = STRING_NULL;

        private void Awake()
        {
            renderer = GetComponent<MeshRenderer>();

            gameObject.SetLayer(LayerMask.NameToLayer("Viewport"));

            id = prefab.ID;
        }
        private void Start()
        {
            if (addOnAwake)
            {
                AddViewport();

                if (showOnAwake)
                {
                    ShowViewport();
                }
            }
        }

        public void AddViewport() => UIManager.Instance.AddViewport(prefab);
        public void ShowViewport() => UIManager.Instance.ShowViewport(id, this);
        public void HideViewport() => UIManager.Instance.HideViewport(id);

        internal void ShowRenderer() => renderer.enabled = true;
        internal void HideRenderer() => renderer.enabled = false;

        internal bool CheckVisibility(Transform target, float minDistance)
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