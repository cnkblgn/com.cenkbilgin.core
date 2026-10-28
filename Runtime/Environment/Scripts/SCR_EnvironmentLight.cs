using UnityEngine;

namespace Core.Environment
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class EnvironmentLight : MonoBehaviour
    {
        [SerializeField, ColorUsage(false, true)] private Color color = Color.white;
        [SerializeField, Min(0f)] private float intensity = 1f;
        [SerializeField, Min(0.01f)] private float range = 10f;

        public Color Color => color;
        public float Intensity => intensity;
        public float Range => range;
        public Vector3 Position => transform.position;

        private void OnEnable() => EnvironmentLightDatabase.Register(this);
        private void OnDisable() => EnvironmentLightDatabase.Unregister(this);

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = color;
            Gizmos.DrawWireSphere(transform.position, range);
        }
#endif
    }
}
