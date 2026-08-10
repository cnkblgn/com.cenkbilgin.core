using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.Audio
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public sealed class AudioPlayer : MonoBehaviour
    {
        public bool IsPlaying => emitter.IsPlaying;
        public bool IsFading => thisCoroutine != null;

        [Header("_")]
        [Info("Optional, but if its missing player tries to play via manager")]
        [SerializeField, FormerlySerializedAs("audioObject")] private AudioEmitter emitter = null;

        [Header("_")]
        [SerializeField, Required] private AudioClip audioClip = null;
        [SerializeField] private AudioGroup audioGroup = AudioGroup.EFFECT;

        [Header("_")]
        [SerializeField] internal bool playOnEnable = false;
        [SerializeField] internal bool playOnLoop = false;
        [SerializeField] internal bool useOcculusion = true;
        [SerializeField] internal bool randomizePitch = false;
        [SerializeField] internal bool randomizeVolume = false;

        [Header("_")]
        [SerializeField, Range(0, 1), Tooltip("0 - 2D, 1 - 3D")] internal float blend = 1;
        [SerializeField, Range(0, 1)] internal float volume = 1;
        [SerializeField, Range(-3, 3)] internal float pitch = 1;
        [SerializeField, Min(0)] internal float minDistance = 1;
        [SerializeField, Min(0)] internal float maxDistance = 500;
        [SerializeField, Min(0)] internal float fadeIn = 0;
        [SerializeField, Min(0)] internal float fadeOut = 0;
        [SerializeField, Range(0, 360)] internal float spread = 0;
        [SerializeField, Range(-1, 1)] internal float pan = 0;
        [SerializeField, Range(1, 22000f)] internal float lowpass = 22000f;
        [SerializeField, Range(0, 5)] internal float resonance = 1;

        private Transform thisTransform = null;
        private Coroutine thisCoroutine = null;
        private bool hasPlayedOnce = false;

#if UNITY_EDITOR
        private void OnEnable()
        {
            if (!playOnEnable || !hasPlayedOnce)
            {
                return;
            }

            Play();
        }
        private void OnValidate()
        {
            if (TryGetComponent(out AudioEmitter _))
            {
                Debug.LogError($"{"[AudioPlayer]".ToYellow()} and {"[AudioEmitter]".ToYellow()} components can't exist in same gameobject!");
                return;
            }

            if (emitter == null)
            {
                return;
            }

            if (emitter.thisAudioSource != null)
            {
                emitter.thisAudioSource.spread = spread;
                emitter.thisAudioSource.panStereo = pan;
                emitter.thisAudioSource.minDistance = minDistance;
                emitter.thisAudioSource.maxDistance = maxDistance;
            }

            if (emitter.thisAudioFilter != null)
            {
                emitter.thisAudioFilter.lowpassResonanceQ = resonance;
            }
        }
#endif
        private void Start()
        {
            if (!playOnEnable)
            {
                return;
            }

            Play();
        }

        public void Play() => Play(audioClip, blend);
        public void Play(float blend) => Play(audioClip, blend);
        public void Play(AudioClip clip, float blend)
        {
            if (clip == null)
            {
                Debug.LogWarning("clip == null!");
                return;
            }

            hasPlayedOnce = true;

            if (emitter == null)
            {
                if (thisTransform == null)
                {
                    thisTransform = GetComponent<Transform>();
                }

                ManagerAudio.Instance.PlaySound(clip, audioGroup, thisTransform.position, blend, volume * (randomizeVolume ? Random.Range(0.75f, 1.15f) : 1), pitch * (randomizePitch ? Random.Range(0.9f, 1.1f) : 1), minDistance, maxDistance, useOcculusion);

                return;
            }

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            emitter.gameObject.SetActive(true);

            if (fadeIn > 0)
            {
                FadeIn();
            }

            ManagerAudio m = ManagerAudio.Instance;

            if (useOcculusion)
            {
                emitter.Play
                (
                    clip,
                    m.GetListener(),
                    m.GetAudioGroup(audioGroup),
                    blend,
                    volume * (randomizeVolume ? Random.Range(0.75f, 1.15f) : 1),
                    pitch * (randomizePitch ? Random.Range(0.9f, 1.1f) : 1),
                    minDistance,
                    maxDistance,
                    playOnLoop,
                    m.OcclusionMask,
                    m.OcclusionAngle,
                    m.OcclusionBlend,
                    m.OcclusionLowpass,
                    m.OcclusionVolume
                );
            }
            else
            {
                emitter.Play
                (
                    clip,
                    m.GetListener(),
                    m.GetAudioGroup(audioGroup),
                    blend,
                    volume * (randomizeVolume ? Random.Range(0.75f, 1.15f) : 1),
                    pitch * (randomizePitch ? Random.Range(0.9f, 1.1f) : 1),
                    minDistance,
                    maxDistance,
                    playOnLoop
                );
            }

            SetLowpassMult(lowpass / 22000f);
        }
        public void Stop()
        {
            if (emitter == null)
            {
                return;
            }

            if (fadeOut <= 0)
            {
                emitter.Stop();
            }
            else
            {
                FadeOut();
            }         
        }

        public AudioClip GetClip() => audioClip;
        public void SetClip(AudioClip clip) => audioClip = clip;

        public float GetVolumeMult() => emitter.GetVolumeMult();
        public void SetVolumeMult(float value) => emitter.SetVolumeMult(value);

        public float GetPitchMult() => emitter.GetPitchMult();
        public void SetPitchMult(float value) => emitter.SetPitchMult(value);

        public float GetLowpassMult() => emitter.GetLowpassMult();
        public void SetLowpassMult(float value) => emitter.SetLowpassMult(value);

        public float GetStereoPan()
        {
            return pan;
        }
        public void SetStereoPan(float value)
        {
            pan = value;
            emitter.thisAudioSource.panStereo = pan;
        }

        public float GetSpread()
        {
            return spread;
        }
        public void SetSpread(float value)
        {
            spread = value;
            emitter.thisAudioSource.spread = spread;
        }

        public float GetBlend()
        {
            return blend;
        }
        public void SetBlend(float value)
        {
            blend = value;
            emitter.thisAudioSource.panStereo = blend;
        }

        private void FadeIn()
        {
            if (thisCoroutine != null)
            {
                StopCoroutine(thisCoroutine);
                thisCoroutine = null;
            }

            SetVolumeMult(0);
            thisCoroutine = StartCoroutine(Fade(fadeIn, 1));
        }
        private void FadeOut()
        {
            if (thisCoroutine != null)
            {
                StopCoroutine(thisCoroutine);
                thisCoroutine = null;
            }

            thisCoroutine = StartCoroutine(Fade(fadeOut, 0));
        }
        private IEnumerator Fade(float duration, float volume)
        {
            float startVolume = GetVolumeMult();
            float timer = 0;

            while (timer < duration)
            {
                SetVolumeMult(Mathf.Lerp(startVolume, volume, timer / duration));

                timer += Time.deltaTime;

                yield return null;
            }

            SetVolumeMult(volume);

            if (GetVolumeMult() <= 0)
            {
                emitter.Stop();
            }

            thisCoroutine = null;
        }
    }
}