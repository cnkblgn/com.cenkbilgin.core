using UnityEngine;

namespace Core.Audio
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioPlayer))]
    public sealed class AudioVolume : MonoBehaviour
    {
        [Header("_")]
        [SerializeField] private bool showGizmos = true;
        [SerializeField, HideInInspector] private Transform thisTransform = null;
        [SerializeField, HideInInspector] private AudioPlayer thisPlayer = null;

        [Header("_")]
        [Info("x: distance, y: volume")]
        [SerializeField] private AnimationCurve falloff = AnimationCurve.Linear(0, 1, 1, 0);
        [Info("Local Space, inner = 1 volume, outer = 0 volume")]
        [SerializeField] private Vector3 innerSize = new(2, 2, 2);
        [SerializeField] private Vector3 outerSize = new(6, 6, 6);

        [Header("_")]
        [Info("If toggled system simulates pseudo-3D effect")] 
        [SerializeField] private bool use3DPanning;
        [SerializeField, Range(0f, 1f)] private float maxStereoPan = 0.75f;

        private Vector3 innerHalf;
        private Vector3 outerHalf;
        private float cullRadiusSqr;
        private float currentVolume;
        private float targetVolume;

        private void OnEnable()
        {
            innerHalf = innerSize * 0.5f;
            outerHalf = outerSize * 0.5f;

            float maxOuter = Mathf.Max(outerHalf.x, Mathf.Max(outerHalf.y, outerHalf.z));

            cullRadiusSqr = maxOuter * maxOuter * 3f;

            ManagerAudio.Instance.RegisterVolume(this);
        }
        private void OnDisable()
        {
            if (!ManagerAudio.HasInstance)
            {
                return;
            }

            thisPlayer.Stop();

            ManagerAudio.Instance.UnregisterVolume(this);
        }

        internal void TickCheck(Vector3 listenerPos)
        {
            Vector3 delta = listenerPos - thisTransform.position;

            if (delta.sqrMagnitude > cullRadiusSqr)
            {
                targetVolume = 0f;
                return;
            }

            Vector3 local = thisTransform.InverseTransformPoint(listenerPos);

            float ax = Mathf.Abs(local.x);
            float ay = Mathf.Abs(local.y);
            float az = Mathf.Abs(local.z);
            float tx = SafeInverseLerp(innerHalf.x, outerHalf.x, ax);
            float ty = SafeInverseLerp(innerHalf.y, outerHalf.y, ay);
            float tz = SafeInverseLerp(innerHalf.z, outerHalf.z, az);
            float t = Mathf.Max(tx, Mathf.Max(ty, tz));

            targetVolume = falloff.Evaluate(t);

            if (use3DPanning)
            {
                thisPlayer.SetStereoPan(Mathf.Clamp(local.x / outerHalf.x, -1f, 1f) * maxStereoPan);
            }
        }
        internal void TickState(float deltaTime)
        {
            currentVolume = Mathf.Lerp(thisPlayer.GetVolumeMult(), targetVolume, deltaTime * 5f);

            thisPlayer.SetVolumeMult(currentVolume);
        }

#if UNITY_EDITOR
        private static GUIStyle GizmosStyle
        {
            get
            {
                gizmosStyle ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, };
                gizmosStyle.normal.textColor = COLOR_GREEN;

                return gizmosStyle;
            }
        } private static GUIStyle gizmosStyle;

        private void OnValidate()
        {
            TryGetComponent(out thisPlayer);
            TryGetComponent(out thisTransform);

            thisPlayer.spread = 180f;
            thisPlayer.blend = 0f;
        }
        private void OnDrawGizmos() => DrawGizmos(selected: false);
        private void OnDrawGizmosSelected() => DrawGizmos(selected: true);

        private void DrawGizmos(bool selected)
        {
            if (!showGizmos)
            {
                return;
            }

            if (thisTransform == null)
            {
                return;
            }

            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = thisTransform.localToWorldMatrix;

            Gizmos.color = selected ? new Color(1f, 0.4f, 0f, 1f) : new Color(1f, 0.4f, 0f, 0.5f);
            Gizmos.DrawWireCube(Vector3.zero, outerSize);

            Gizmos.color = selected ? new Color(0.2f, 1f, 0.2f, 1f) : new Color(0.2f, 1f, 0.2f, 0.5f);
            Gizmos.DrawWireCube(Vector3.zero, innerSize);

            if (selected)
            {
                Gizmos.color = new Color(1f, 0.4f, 0f, 0.06f);
                Gizmos.DrawCube(Vector3.zero, outerSize);
            }

            Gizmos.matrix = oldMatrix;

            if (selected)
            {
                UnityEditor.Handles.Label(thisTransform.position + Vector3.up * (outerSize.y * 0.5f + 0.3f), $"Volume: {currentVolume:F2}", GizmosStyle);
            }
        }
#endif
    }
}
