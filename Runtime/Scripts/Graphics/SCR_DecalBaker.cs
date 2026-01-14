using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Core.Graphics
{
    using static Core.CoreUtility;

    [DisallowMultipleComponent]
    public class DecalBaker : MonoBehaviour
    {
        [Header("_")]
        [SerializeField] private bool bakeOnStart = true;
        [SerializeField, Required] private Material bakeMaterial = null;
        [SerializeField, Required] private Mesh bakeMesh = null;

        [Header("_")]
        [SerializeField, Min(1)] private float raycastDistance = 2f;
        [SerializeField] public LayerMask raycastMask = ~0;

        private void Start()
        {
            if (!bakeOnStart)
            {
                return;
            }

            Bake();
        }
        public void Bake()
        {
            if (bakeMaterial == null)
            {
                Debug.LogError("DecalBaker.Bake() bakeMaterial == null");
                return;
            }

            if (!TryGetComponent(out DecalProjector projector))
            {
                Debug.LogError("DecalBaker.Bake() projector == null");
                return;
            }

            if (!Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, raycastDistance, raycastMask))
            {
                Debug.LogError("DecalBaker.Bake() hit == null");
                return;
            }

            Transform t = transform;
            Vector3 s = projector.size;
            Vector3 n = hit.normal;
            Vector3 p = hit.point;
            Quaternion r = Quaternion.LookRotation(-n);

            t.SetPositionAndRotation(p + (n * 0.001f), r);
            t.localScale = new Vector3(s.x, s.y, 1);

            Material m = new(bakeMaterial);
            m.SetVector("_BaseTiling", projector.uvScale);
            m.SetVector("_BaseOffset", projector.uvBias);

            gameObject.AddComponent<MeshFilter>().sharedMesh = bakeMesh;
            gameObject.AddComponent<MeshRenderer>().material = m;
            gameObject.name += "_Baked";

            Destroy(projector);
        }
    }
}