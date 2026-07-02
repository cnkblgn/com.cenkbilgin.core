using System.Collections;
using UnityEngine;

namespace Core.Audio
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public sealed class AudioPlayer : MonoBehaviour
    {
        public bool IsPlaying => audioObject.IsPlaying;
        public bool IsFading => thisCoroutine != null;

        [Header("_")]
        [SerializeField] private AudioEmitter audioObject = null;

        [Header("_")]
        [SerializeField, Required] private AudioClip audioClip = null;
        [SerializeField] private AudioGroup audioGroup = AudioGroup.EFFECT;

        [Header("_")]
        [SerializeField] private bool playOnEnable = false;
        [SerializeField] private bool playOnLoop = false;
        [SerializeField] private bool useOcculusion = true;
        [SerializeField] private bool randomizePitch = false;
        [SerializeField] private bool randomizeVolume = false;

        [Header("_")]
        [SerializeField, Range(0, 1), Tooltip("0 - 2D, 1 - 3D")] private float blend = 1;
        [SerializeField, Range(0, 1)] private float volume = 1;
        [SerializeField, Range(-3, 3)] private float pitch = 1;
        [SerializeField, Min(0)] private float minDistance = 1;
        [SerializeField, Min(0)] private float maxDistance = 500;
        [SerializeField, Min(0)] private float fadeIn = 0;
        [SerializeField, Min(0)] private float fadeOut = 0;
        [SerializeField, Range(1, 22000f)] private float lowpass = 22000f;
        [SerializeField, Range(0, 5)] private float rezonance = 1;

        private Transform thisTransform = null;
        private Coroutine thisCoroutine = null;
        private bool hasPlayedOnce = false;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (TryGetComponent(out AudioEmitter _))
            {
                Debug.LogError($"{"[AudioPlayer]".ToYellow()} and {"[AudioEmitter]".ToYellow()} components can't exist in same gameobject!");
                return;
            }

            if (audioObject == null)
            {
                return;
            }

            audioObject.GetComponent<AudioSource>().minDistance = minDistance;
            audioObject.GetComponent<AudioSource>().maxDistance = maxDistance;
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

#if UNITY_EDITOR
        private void OnEnable()
        {
            if (!playOnEnable || !hasPlayedOnce)
            {
                return;
            }

            Play();
        }
#endif
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

            if (audioObject == null)
            {
                if (thisTransform == null)
                {
                    thisTransform = GetComponent<Transform>();
                }

                AudioManager.Instance.PlaySound(clip, audioGroup, thisTransform.position, blend, volume * (randomizeVolume ? Random.Range(0.75f, 1.15f) : 1), pitch * (randomizePitch ? Random.Range(0.9f, 1.1f) : 1), minDistance, maxDistance, useOcculusion);

                return;
            }

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            audioObject.gameObject.SetActive(true);

            if (fadeIn > 0)
            {
                FadeIn();
            }

            AudioManager m = AudioManager.Instance;

            if (useOcculusion)
            {
                audioObject.Play
                (
                    clip,
                    m.AudioListener,
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
                audioObject.Play
                (
                    clip,
                    m.AudioListener,
                    m.GetAudioGroup(audioGroup),
                    blend,
                    volume * (randomizeVolume ? Random.Range(0.75f, 1.15f) : 1),
                    pitch * (randomizePitch ? Random.Range(0.9f, 1.1f) : 1),
                    minDistance,
                    maxDistance,
                    playOnLoop
                );
            }

            SetLowpass(lowpass / 22000f);
            SetResonance(rezonance);
        }
        public void Stop()
        {
            if (audioObject == null)
            {
                return;
            }

            if (fadeOut <= 0)
            {
                audioObject.Stop();
            }
            else
            {
                FadeOut();
            }         
        }

        public AudioClip GetClip() => audioClip;
        public void SetClip(AudioClip clip) => audioClip = clip;

        public float GetVolume() => audioObject.GetVolume();
        public void SetVolume(float value) => audioObject.SetVolume(value);

        public float GetPitch() => audioObject.GetPitch();
        public void SetPitch(float value) => audioObject.SetPitch(value);

        public float GetLowpass() => audioObject.GetLowpass();
        public void SetLowpass(float value) => audioObject.SetLowpass(value);

        public float GetResonance() => audioObject.GetResonance();
        public void SetResonance(float value) => audioObject.SetResonance(value);

        private void FadeIn()
        {
            if (thisCoroutine != null)
            {
                StopCoroutine(thisCoroutine);
                thisCoroutine = null;
            }

            SetVolume(0);
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
            float startVolume = GetVolume();
            float timer = 0;

            while (timer < duration)
            {
                SetVolume(Mathf.Lerp(startVolume, volume, timer / duration));

                timer += Time.deltaTime;

                yield return null;
            }

            SetVolume(volume);

            if (GetVolume() <= 0)
            {
                audioObject.Stop();
            }

            thisCoroutine = null;
        }
    }
}