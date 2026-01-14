using UnityEngine;

namespace Core.Audio
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(AudioLowPassFilter))]
    public class AudioEmitter : MonoBehaviour
    {
        private const float OCCLUSION_DEPTH = 2;
        private const float OCCLUSION_FACTOR = 1 / (OCCLUSION_DEPTH * 3);

        public Transform ThisTransform => thisTransform;
        public bool IsPlaying => isPlaying; 
        public bool IsPaused => isPaused; 

        [Header("_")]
        [SerializeField] private bool useGizmos = false;
        [SerializeField, HideInInspector] private Transform thisTransform = null;
        [SerializeField, HideInInspector] private AudioSource thisAudioSource = null;
        [SerializeField, HideInInspector] private AudioLowPassFilter thisAudioFilter = null;
 
        private AudioClip thisAudioClip = null;
        private Transform thisAudioListener = null;
        private AudioGroup thisAudioGroup = AudioGroup.MASTER;
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
        private float targetVolume = 1;
        private float targetLowpass = 22000;
        private float distanceToListener = float.MinValue;
        private float occlusionValue = 0;
        private float occlusionLowpassBlendSpeed = 12.5f;
        private float occlusionVolumeBlendSpeed = 12.5f;
        private float occlusionRaycastAngle = 5.0f;
        private float volumeMultiplier = 1;
        private float pitchMultiplier = 1;
        private float lowpassMultiplier = 1;
        private bool occlusionEnabled = false;
        private bool occlusionFront = false;
        private bool occlusionLeft = false;
        private bool occlusionRight = false;
        private bool isInitialized = false; 
        private bool isPlaying = false;
        private bool isPaused = false;
        private bool hasFocus = true;

        private void Update()
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

            if (!thisAudioSource.loop)
            {
                if (!isPlaying && !IsPaused)
                {
                    Stop();
                    return;
                }

                if (thisAudioSource.time >= thisAudioSource.clip.length - 0.001f)
                {
                    Stop();
                    return;
                }
            }
            else
            {
                if (!isPlaying && !IsPaused)
                {
                    Stop();
                    return;
                }
            }

            if (thisAudioSource.isVirtual)
            {
                return;
            }

            if (!isPlaying)
            {
                return;
            }

            if (!occlusionEnabled)
            {
                thisAudioSource.volume = baseVolume * volumeMultiplier;
                thisAudioSource.pitch = basePitch * pitchMultiplier;
                return;
            }

            Occlude();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            if (thisTransform == null)
            {
                thisTransform = GetComponent<Transform>();
            }

            if (thisAudioSource == null)
            {
                thisAudioSource = GetComponent<AudioSource>();
            }

            if (thisAudioFilter == null)
            {
                thisAudioFilter = GetComponent<AudioLowPassFilter>();
            }

            thisAudioFilter.cutoffFrequency = 22000;
            thisAudioSource.priority = 128;
            thisAudioSource.playOnAwake = false;
            thisAudioSource.loop = false;
            distanceToListener = float.MinValue;
        }
        private void OnDrawGizmos()
        {
            if (!useGizmos)
            {
                return;
            }

            if (ThisTransform == null)
            {
                return;
            }

            if (thisAudioListener == null)
            {
                return;
            }

            if (!occlusionEnabled)
            {
                return;
            }

            Gizmos.color = occlusionFront ? Color.red : Color.green;
            Gizmos.DrawRay(ThisTransform.position, directionToTarget * distanceToListener);

            Gizmos.color = occlusionLeft ? Color.red : Color.green;
            Gizmos.DrawRay(ThisTransform.position, directionToLeft * distanceToListener);

            Gizmos.color = occlusionRight ? Color.red : Color.green;
            Gizmos.DrawRay(ThisTransform.position, directionToRight * distanceToListener);
        }
#endif
        private void OnEnable()
        {
            if (ManagerCoreGame.Instance != null)
            {
                ManagerCoreGame.Instance.OnGameStateChanged += OnGameStateChanged;
            }
        }
        private void OnDisable()
        {
            if (ManagerCoreGame.Instance != null)
            {
                ManagerCoreGame.Instance.OnGameStateChanged -= OnGameStateChanged;
            }
        }
        private void OnApplicationFocus(bool focus) => hasFocus = focus;

        private void OnGameStateChanged(GameState gameState)
        {
            switch (gameState)
            {
                case GameState.NULL:
                    if (thisAudioGroup != AudioGroup.MASTER)
                    {
                        Stop();
                    }
                    break;
                case GameState.RESUME:
                    Resume();
                    break;
                case GameState.PAUSE:
                    if (thisAudioGroup != AudioGroup.MASTER)
                    {
                        Pause();
                    }
                    break;
            }
        }

        private void Occlude(bool isFirstTime = false)
        {
            if (!occlusionEnabled)
            {
                return;
            }

            if (thisAudioListener == null)
            {
                return;
            }

            Vector3 listenerVector = thisAudioListener.position - ThisTransform.position;
            directionToTarget = listenerVector.normalized;
            distanceToListener = listenerVector.magnitude;

            if (distanceToListener > thisAudioSource.maxDistance)
            {
                return;
            }

            Vector3 thisPosition = ThisTransform.position;

            directionToLeft = Quaternion.AngleAxis(-occlusionRaycastAngle, Vector3.up) * directionToTarget;
            directionToRight = Quaternion.AngleAxis(occlusionRaycastAngle, Vector3.up) * directionToTarget;

            int frontHits = Physics.RaycastNonAlloc(thisPosition, directionToTarget, occlusionFrontHits, distanceToListener, occlusionMask, QueryTriggerInteraction.Ignore);
            int leftHits = Physics.RaycastNonAlloc(thisPosition, directionToLeft, occlusionLeftHits, distanceToListener, occlusionMask, QueryTriggerInteraction.Ignore);
            int rightHits = Physics.RaycastNonAlloc(thisPosition, directionToRight, occlusionRightHits, distanceToListener, occlusionMask, QueryTriggerInteraction.Ignore);

            occlusionFront = frontHits > 0;
            occlusionLeft = leftHits > 0;
            occlusionRight = rightHits > 0;

            if (occlusionFront)
            {
                occlusionValue += 1 * frontHits;
            }
            if (occlusionLeft)
            {
                occlusionValue += 1 * leftHits;
            }
            if (occlusionRight)
            {
                occlusionValue += 1 * rightHits;
            }

            targetLowpass = 22000f * (1 - occlusionLowpassCurve.Evaluate(occlusionValue * OCCLUSION_FACTOR)) * lowpassMultiplier;
            targetVolume = baseVolume * (1 - occlusionVolumeCurve.Evaluate(occlusionValue * OCCLUSION_FACTOR));

            if (isFirstTime)
            {
                thisAudioFilter.cutoffFrequency = targetLowpass;
                thisAudioSource.volume = targetVolume;
            }
            else
            {
                thisAudioFilter.cutoffFrequency = thisAudioSource.clip.length <= Time.deltaTime ? targetLowpass : Mathf.Lerp(thisAudioFilter.cutoffFrequency, targetLowpass, occlusionLowpassBlendSpeed * Time.deltaTime);
                thisAudioSource.volume = thisAudioSource.clip.length <= Time.deltaTime ? targetVolume : Mathf.Lerp(thisAudioSource.volume, targetVolume, occlusionVolumeBlendSpeed * Time.deltaTime) * volumeMultiplier;
                thisAudioSource.pitch = basePitch * pitchMultiplier;
            }

            occlusionValue = 0;
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
            volumeMultiplier = 1;
            pitchMultiplier = 1;
            lowpassMultiplier = 1;

            isPlaying = false;
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

        public void Play(AudioClip clip, Transform listener, AudioGroup group = AudioGroup.EFFECT, float blend = 1, float volume = 1, float pitch = 1, float minDistance = 1, float maxDistance = 100, bool loop = false, bool occulusion = false)
        {
            if (clip == null)
            {
#if UNITY_EDITOR
                Debug.LogError("AudioEmitter.Play() clip is null!");
#endif
                return;
            }

            ManagerCoreAudio m = ManagerCoreAudio.Instance;
            occlusionMask = m.OcclusionMask;
            occlusionLowpassCurve = m.OcclusionLowpassCurve;
            occlusionVolumeCurve = m.OcclusionVolumeCurve;
            occlusionLowpassBlendSpeed = m.OcclusionLowpassBlendSpeed;
            occlusionVolumeBlendSpeed = m.OcclusionVolumeBlendSpeed;
            occlusionRaycastAngle = m.OcclusionRaycastAngle;

            thisAudioClip = clip;
            thisAudioListener = listener;
            thisAudioGroup = group;

            occlusionEnabled = occulusion && blend >= 1;
            distanceToListener = float.MinValue;

            thisAudioFilter.cutoffFrequency = 22000;
            thisAudioFilter.lowpassResonanceQ = 1;
            thisAudioSource.playOnAwake = false;

            thisAudioSource.clip = thisAudioClip;
            thisAudioSource.outputAudioMixerGroup = ManagerCoreAudio.Instance.GetAudioGroup(thisAudioGroup);
            thisAudioSource.minDistance = minDistance;
            thisAudioSource.maxDistance = maxDistance;
            thisAudioSource.volume = volume * volumeMultiplier;
            thisAudioSource.spatialBlend = blend;
            thisAudioSource.pitch = pitch * pitchMultiplier;
            thisAudioSource.loop = loop;
            baseVolume = volume;
            basePitch = pitch;

            if (!thisAudioSource.enabled)
            {
                thisAudioSource.enabled = true;
            }

            thisAudioSource.Play();
            Occlude(true);

            isInitialized = true;
        }

        public float GetPitch() => pitchMultiplier;
        public void SetPitch(float value) => pitchMultiplier = value;

        public float GetVolume() => volumeMultiplier;
        public void SetVolume(float value) => volumeMultiplier = value;

        public float GetLowpass() => 22000f * lowpassMultiplier;
        public void SetLowpass(float value)
        {
            lowpassMultiplier = value;

            if (!occlusionEnabled)
            {
                thisAudioFilter.cutoffFrequency = 22000f * lowpassMultiplier;
            }
        }

        public float GetResonance() => thisAudioFilter.lowpassResonanceQ;
        public void SetResonance(float value) => thisAudioFilter.lowpassResonanceQ = value;
    }
}
