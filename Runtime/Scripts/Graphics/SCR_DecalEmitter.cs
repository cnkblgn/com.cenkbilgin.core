#define URP_DECAL
using UnityEngine;

namespace Core.Graphics
{
#if URP_DECAL
    using UnityEngine.Rendering.Universal;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(DecalProjector))]
    public class DecalEmitter : MonoBehaviour
    {
        public DecalProjector ThisProjector => thisProjector;
        public Transform ThisTransform => thisTransform;

        private DecalProjector thisProjector = null;
        private Transform thisTransform = null;
        private Vector3 defaultSize = Vector3.one;

        private void Awake()
        {
            thisTransform = GetComponent<Transform>();

            thisProjector = GetComponent<DecalProjector>();
            defaultSize = ThisProjector.size;
        }
        public void Adjust(Vector3 position, Quaternion rotation, float scale, Transform parent)
        {
            thisTransform.SetParent(parent != null ? parent : thisTransform.parent, true);
            thisTransform.SetPositionAndRotation(position, rotation);
            thisProjector.size = new(defaultSize.x * scale, defaultSize.y * scale, 0.1f);
            thisProjector.pivot = Vector3.forward * 0.03f;
        }
        public void Adjust(Vector3 position, Vector3 normal, Vector2 rotation, float scale, Transform parent) => Adjust(position, Quaternion.Euler(Quaternion.LookRotation(normal, Vector3.up).eulerAngles + (Vector3.forward * Random.Range(rotation.x, rotation.y))), scale, parent);
        public void Adjust(RaycastHit raycastHit, Vector2 rotation, float scale, Transform parent) => Adjust(raycastHit.point, -raycastHit.normal, rotation, scale, parent);
    }
#else
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshRenderer))]
    public class DecalObject : MonoBehaviour
    {
        public Transform ThisTransform => thisTransform;

        [Header("_")]
        [SerializeField] private Vector2 offset = Vector2.zero;
        [SerializeField] private Vector2 tiling = Vector2.one;

        private Transform thisTransform = null;
        private MeshRenderer thisRenderer = null;
        private MaterialPropertyBlock thisProptery = null;
        private Vector3 defaultSize = Vector3.one;

        private void Awake()
        {
            thisTransform = GetComponent<Transform>();
            thisRenderer = GetComponent<MeshRenderer>();

            thisProptery = new();

            defaultSize = thisTransform.lossyScale;

            Apply();
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (thisRenderer == null)
            {
                thisRenderer = GetComponent<MeshRenderer>();
            }

            if (thisProptery == null)
            {
                thisProptery = new();
            }

            Apply();
        }
#endif
        private void Apply()
        {
            thisRenderer.GetPropertyBlock(thisProptery);
            thisProptery.SetVector("_BaseTiling", tiling);
            thisProptery.SetVector("_BaseOffset", offset);
            thisRenderer.SetPropertyBlock(thisProptery);
        }

        public void Adjust(Vector3 position, Quaternion rotation, float scale, Transform parent)
        {
            thisTransform.SetParent(null, true);
            thisTransform.localScale = new(defaultSize.x * scale, defaultSize.y * scale, 0.1f);
            thisTransform.SetPositionAndRotation(position + (thisTransform.forward * 0.0125f), rotation);
            thisTransform.SetParent(parent != null ? parent : thisTransform.parent, true);
        }
        public void Adjust(Vector3 position, Vector3 normal, Vector2 rotation, float scale, Transform parent) => Adjust(position, Quaternion.Euler(Quaternion.LookRotation(normal, Vector3.up).eulerAngles + (Vector3.forward * Random.Range(rotation.x, rotation.y))), scale, parent);
        public void Adjust(RaycastHit raycastHit, Vector2 rotation, float scale, Transform parent) => Adjust(raycastHit.point, -raycastHit.normal, rotation, scale, parent);
    }
#endif
}