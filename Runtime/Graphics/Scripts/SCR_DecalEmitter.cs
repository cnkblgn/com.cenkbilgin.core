using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Core.Graphics
{
    [DisallowMultipleComponent]
    public sealed class DecalEmitter : MonoBehaviour
    {
        public Transform ThisTransform => thisTransform;
        public int Projectors => projectors.Length;

        private Transform thisTransform = null;
        private DecalProjector[] projectors = null;
        private Vector3[] scales = null;
        private float[] opacities = null;
        private float alpha = 1;

        private void Awake()
        {
            thisTransform = GetComponent<Transform>();
            projectors = GetComponentsInChildren<DecalProjector>();
            scales = new Vector3[projectors.Length];
            opacities = new float[projectors.Length];

            for (int i = 0; i < scales.Length; i++)
            {
                scales[i] = projectors[i].size;
                opacities[i] = projectors[i].fadeFactor;
            }
        }

        public void Adjust(Vector3 position, Quaternion rotation, float scale, Transform parent)
        {
            thisTransform.SetParent(parent != null ? parent : thisTransform.parent, true);
            thisTransform.SetPositionAndRotation(position, rotation);

            for (int i = 0; i < projectors.Length; i++)
            {
                DecalProjector p = projectors[i];
                Vector3 s = scales[i];

                p.size = new(s.x * scale, s.y * scale, 0.1f);
                p.pivot = Vector3.forward * 0.03f;
            }
        }
        public void Adjust(Vector3 position, Vector3 normal, Vector2 rotation, float scale, Transform parent) => Adjust(position, Quaternion.Euler(Quaternion.LookRotation(normal, Vector3.up).eulerAngles + (Vector3.forward * Random.Range(rotation.x, rotation.y))), scale, parent);
        public void Adjust(RaycastHit raycastHit, Vector2 rotation, float scale, Transform parent) => Adjust(raycastHit.point, -raycastHit.normal, rotation, scale, parent);

        public float GetAlpha() => alpha;
        public void SetAlpha(float value)
        {
            alpha = value;

            for (int i = 0; i < projectors.Length; i++)
            {
                projectors[i].fadeFactor = opacities[i] * alpha;
            }
        }
    }
}