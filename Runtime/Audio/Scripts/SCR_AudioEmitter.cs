using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Core.Audio
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(AudioLowPassFilter))]
    public sealed class AudioEmitter : MonoBehaviour
    {
        internal static event Action<AudioEmitter> OnCreated = null;
        internal static event Action<AudioEmitter> OnDestroyed = null;

        private const float OCCLUSION_DEPTH = 2;
        private const float OCCLUSION_FACTOR = 1 / (OCCLUSION_DEPTH * 3);
        private const float OCCLUSION_INTERVAL = 0.25F;
        private const float MAX_CUTOFF = 22000f;

        public string AudioGroup => thisAudioGroup != null ? thisAudioGroup.name : STRING_NULL;
        public bool IsPlaying => isPlaying; 
        public bool IsPaused => isPaused; 

        [Header("_")]
        [SerializeField] private bool useGizmos = false;
        [SerializeField, HideInInspector] private Transform thisTransform = null;
        [SerializeField, HideInInspector] private AudioSource thisAudioSource = null;
        [SerializeField, HideInInspector] private AudioLowPassFilter thisAudioFilter = null;

        private AudioClip thisAudioClip = null;
        private Transform thisAudioListener = null;
        private AudioMixerGroup thisAudioGroup = null;
        private AnimationCurve occlusionLowpassCurve = null;
        private AnimationCurve occlusionVolumeCurve = null;
        private LayerMask occlusionMask = 0;
        private Vector3 directionToRight = Vector3.zero;
        private Vector3 directionToLeft = Vector3.zero;
        private Vector3 directionToTarget = Vector3.zero;
        private readonly RaycastHit[] occlusionFrontHits = new RaycastHit[(int)OCCLUSION_DEPTH];
        private readonly RaycastHit[] occlusionLeftHits = new RaycastHit[(int)OCCLUSION_DEPTH];
        private readonly RaycastHit[] occlusionRightHits = new RaycastHit[(int)OCCLUSION_DEPTH];
        private float basePitch = 1;
        private float baseVolume = 1;
        private float distanceToListener = float.MinValue;
        private float occlusionValue = 0;
        private float occlusionBlend = 12.5f;
        private float occlusionAngle = 5.0f;
        private float occlusionUpdateTime = 0f;
        private float maxDistanceSqr = 0f;
        private float volumeMultiplier = 1;
        private float pitchMultiplier = 1;
        private float lowpassMultiplier = 1;
        private float resonanceMultiplier = 1;
        private float occlusionVolumeMultiplier = 1;
        private float occlusionLowpassMultiplier = 1;
        private bool occlusionEnabled = false;
        private bool occlusionFront = false;
        private bool occlusionLeft = false;
        private bool occlusionRight = false;
        private bool isInitialized = false; 
        private bool isPlaying = false;
        private bool isPaused = false;
        private bool hasFocus = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnRuntimeInitialize() 
        { 
            OnCreated = null; 
            OnDestroyed = null; 
        }

        private void Start() => OnCreated?.Invoke(this);
        private void OnDestroy() => OnDestroyed?.Invoke(this);
#if UNITY_EDITOR
        private void Reset()
        {
            if (thisTransform == null) thisTransform = GetComponent<Transform>();
            if (thisAudioSource == null) thisAudioSource = GetComponent<AudioSource>();
            if (thisAudioFilter == null) thisAudioFilter = GetComponent<AudioLowPassFilter>();

            thisAudioFilter.cutoffFrequency = MAX_CUTOFF;
            thisAudioSource.priority = 128;
            thisAudioSource.playOnAwake = false;
            thisAudioSource.loop = false;
            distanceToListener = float.MinValue;
        }
        private void OnDrawGizmos()
        {
            if (!useGizmos || thisTransform == null || thisAudioListener == null || !occlusionEnabled)
            {
                return;
            }

            Gizmos.color = occlusionFront ? Color.red : Color.green;
            Gizmos.DrawRay(thisTransform.position, directionToTarget * distanceToListener);

            Gizmos.color = occlusionLeft ? Color.red : Color.green;
            Gizmos.DrawRay(thisTransform.position, directionToLeft * distanceToListener);

            Gizmos.color = occlusionRight ? Color.red : Color.green;
            Gizmos.DrawRay(thisTransform.position, directionToRight * distanceToListener);
        }
#endif
        private void OnApplicationFocus(bool focus) => hasFocus = focus;

        internal void TickState()
        {
            if (!hasFocus)
            {
                return;
            }

            isPlaying = thisAudioSource.enabled && thisAudioSource.isPlaying;

            if (!isInitialized)
            {
                return;
            }

            if (!isPlaying)
            {
                if (!isPaused)
                {
                    Stop();
                }

                return;
            }

            if (thisAudioSource.isVirtual)
            {
                return;
            }

            ApplyVolume(false);
            ApplyLowpass(false);
            ApplyPitch();
            ApplyResonance();
        }
        internal void TickOcclusion()
        {
            if (!hasFocus)
            {
                return;
            }

            if (!isInitialized)
            {
                return;
            }

            if (!isPlaying)
            {
                return;
            }

            if (thisAudioSource.isVirtual)
            {
                return;
            }

            if (!occlusionEnabled)
            {
                return;
            }

            if (Time.unscaledTime < occlusionUpdateTime)
            {
                return;
            }

            occlusionUpdateTime = Time.unscaledTime + OCCLUSION_INTERVAL;

            Occlude();
        }

        private void ApplyVolume(bool immediate)
        {
            float volume = baseVolume * volumeMultiplier * occlusionVolumeMultiplier;

            thisAudioSource.volume = !occlusionEnabled || immediate ? volume : Mathf.Lerp(thisAudioSource.volume, volume, occlusionBlend * Time.deltaTime);
        }
        private void ApplyLowpass(bool immediate)
        {
            float lowpass = MAX_CUTOFF * lowpassMultiplier * occlusionLowpassMultiplier;

            thisAudioFilter.cutoffFrequency = !occlusionEnabled || immediate ? lowpass : Mathf.Lerp(thisAudioFilter.cutoffFrequency, lowpass, occlusionBlend * Time.deltaTime);
        }
        private void ApplyPitch()
        {
            float pitch = basePitch * pitchMultiplier;

            thisAudioSource.pitch = pitch;
        }
        private void ApplyResonance()
        {
            thisAudioFilter.lowpassResonanceQ = resonanceMultiplier;
        }

        public void Play(AudioClip clip, Transform listener, AudioMixerGroup group, float blend, float volume, float pitch, float minDistance, float maxDistance, bool loop, LayerMask occlusionMask, float occlusionAngle, float occlusionBlend, AnimationCurve occlusionLowpass, AnimationCurve occlusionVolume)
        {
            Play(clip, listener, group, blend, volume, pitch, minDistance, maxDistance, loop);

            occlusionEnabled = blend >= 1;

            if (!occlusionEnabled)
            {
                return;
            }

            this.occlusionMask = occlusionMask;
            this.occlusionLowpassCurve = occlusionLowpass;
            this.occlusionVolumeCurve = occlusionVolume;
            this.occlusionBlend = occlusionBlend;
            this.occlusionAngle = occlusionAngle;
            
            occlusionUpdateTime = 0f;
            Occlude(true);
        }
        public void Play(AudioClip clip, Transform listener, AudioMixerGroup group, float blend, float volume, float pitch, float minDistance, float maxDistance, bool loop)
        {
            if (clip == null)
            {
#if UNITY_EDITOR
                Debug.LogError("AudioClip is null!");
#endif
                return;
            }

            ResetValues();

            thisAudioClip = clip;
            thisAudioListener = listener;
            thisAudioGroup = group;

            maxDistanceSqr = maxDistance * maxDistance;

            thisAudioFilter.cutoffFrequency = MAX_CUTOFF;
            thisAudioFilter.lowpassResonanceQ = 1;

            thisAudioSource.playOnAwake = false;
            thisAudioSource.clip = thisAudioClip;
            thisAudioSource.outputAudioMixerGroup = thisAudioGroup;
            thisAudioSource.minDistance = minDistance;
            thisAudioSource.maxDistance = maxDistance;
            thisAudioSource.volume = volume * volumeMultiplier;
            thisAudioSource.spatialBlend = blend;
            thisAudioSource.pitch = pitch * pitchMultiplier;
            thisAudioSource.loop = loop;

            baseVolume = volume;
            basePitch = pitch;

            if (!thisAudioSource.enabled) thisAudioSource.enabled = true;

            thisAudioSource.Play();
            isInitialized = true;
        }
        public void Stop()
        {
            if (!gameObject.activeSelf)
            {
                return;
            }

            thisAudioSource.Stop();
            thisAudioSource.clip = null;
            thisAudioSource.gameObject.SetActive(false);

            ResetValues();
        }
        public void Pause()
        {
            thisAudioSource.Pause();
            isPaused = true;
        }
        public void Resume()
        {
            if (!IsPaused)
            {
                return;
            }

            isPaused = false;
            thisAudioSource.UnPause();
        }
        private void Occlude(bool isFirstTime = false)
        {
            if (thisAudioListener == null)
            {
                return;
            }

            Vector3 sourcePos = thisTransform.position;
            Vector3 listenerPos = thisAudioListener.position;
            Vector3 toListener = listenerPos - sourcePos;

            float sqrDist = toListener.sqrMagnitude;
            if (sqrDist > maxDistanceSqr)
            {
                return;
            }

            float dist = Mathf.Sqrt(sqrDist);
            if (dist <= 0.0001f)
            {
                return;
            }

            directionToTarget = toListener / dist;
            distanceToListener = dist;

            directionToLeft = Quaternion.AngleAxis(-occlusionAngle, Vector3.up) * directionToTarget;
            directionToRight = Quaternion.AngleAxis(occlusionAngle, Vector3.up) * directionToTarget;

            int frontHits = Physics.RaycastNonAlloc(sourcePos, directionToTarget, occlusionFrontHits, dist, occlusionMask, QueryTriggerInteraction.Ignore);
            int leftHits = Physics.RaycastNonAlloc(sourcePos, directionToLeft, occlusionLeftHits, dist, occlusionMask, QueryTriggerInteraction.Ignore);
            int rightHits = Physics.RaycastNonAlloc(sourcePos, directionToRight, occlusionRightHits, dist, occlusionMask, QueryTriggerInteraction.Ignore);

            occlusionFront = frontHits > 0;
            occlusionLeft = leftHits > 0;
            occlusionRight = rightHits > 0;

            float localOcclusion = 0f;
            if (occlusionFront) localOcclusion += frontHits;
            if (occlusionLeft) localOcclusion += leftHits;
            if (occlusionRight) localOcclusion += rightHits;

            occlusionValue = localOcclusion;

            float t = Mathf.Clamp01(occlusionValue * OCCLUSION_FACTOR);
            occlusionLowpassMultiplier = 1f - occlusionLowpassCurve.Evaluate(t);
            occlusionVolumeMultiplier = 1f - occlusionVolumeCurve.Evaluate(t);

            if (isFirstTime)
            {
                ApplyLowpass(true);
                ApplyVolume(true);
                ApplyPitch();
                ApplyResonance();
            }
        }

        private void ResetValues()
        {
            occlusionEnabled = false;
            occlusionFront = false;
            occlusionLeft = false;
            occlusionRight = false;
            occlusionValue = 0;
            occlusionLowpassMultiplier = 1;
            occlusionVolumeMultiplier = 1;

            volumeMultiplier = 1;
            pitchMultiplier = 1;
            lowpassMultiplier = 1;
            resonanceMultiplier = 1;

            isPaused = false;
            isPlaying = false;
        }

        public Vector3 GetPosition() => thisTransform.position;
        public void SetPosition(Vector3 position) => thisTransform.position = position;

        public float GetPitch() => pitchMultiplier;
        public void SetPitch(float value)
        {
            pitchMultiplier = value;
            ApplyPitch();
        }

        public float GetVolume() => volumeMultiplier;
        public void SetVolume(float value)
        {
            volumeMultiplier = value;
            ApplyVolume(true);
        }

        public float GetLowpass() => MAX_CUTOFF * lowpassMultiplier;
        public void SetLowpass(float value)
        {
            lowpassMultiplier = value;
            ApplyLowpass(true);
        }

        public float GetResonance() => resonanceMultiplier;
        public void SetResonance(float value)
        {
            resonanceMultiplier = value;
            ApplyResonance();
        }
    }
}
