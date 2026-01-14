using System.Collections;
using UnityEngine;

namespace Core.Audio
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public class AudioPlayer : MonoBehaviour
    {
        public bool IsPlaying => audioObject.IsPlaying;
        public bool IsFading => thisCoroutine != null;

        [Header("_")]
        [SerializeField] protected AudioEmitter audioObject = null;

        [Header("_")]
        [SerializeField, Required] protected AudioClip audioClip = null;
        [SerializeField] protected AudioGroup audioGroup = AudioGroup.EFFECT;

        [Header("_")]
        [SerializeField] protected bool playOnEnable = false;
        [SerializeField] protected bool playOnLoop = false;
        [SerializeField] protected bool useOcculusion = true;
        [SerializeField] protected bool randomizePitch = false;
        [SerializeField] protected bool randomizeVolume = false;

        [Header("_")]
        [SerializeField, Range(0, 1), Tooltip("0 - 2D, 1 - 3D")] protected float blend = 1;
        [SerializeField, Range(0, 1)] protected float volume = 1;
        [SerializeField, Range(-3, 3)] protected float pitch = 1;
        [SerializeField, Min(0)] protected float minDistance = 1;
        [SerializeField, Min(0)] protected float maxDistance = 500;
        [SerializeField, Min(0)] protected float fadeIn = 0;
        [SerializeField, Min(0)] protected float fadeOut = 0;
        [SerializeField, Range(1, 22000f)] protected float lowpass = 22000f;
        [SerializeField, Range(0, 5)] protected float rezonance = 1;

        private Transform thisTransform = null;
        private Coroutine thisCoroutine = null;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (TryGetComponent(out AudioEmitter _))
            {
                LogError($"{"AudioPlayer.Validate() [AudioPlayer]".ToYellow()} and {"[AudioEmitter]".ToYellow()} components can't exist in same gameobject!");
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
        private void OnEnable()
        {
            if (!playOnEnable)
            {
                return;
            }

            Play();
        }

        public void Play() => Play(blend);
        public void Play(float blend)
        {
            if (audioObject == null)
            {
                if (thisTransform == null)
                {
                    thisTransform = GetComponent<Transform>();
                }

                ManagerCoreAudio.Instance.PlaySound(audioClip, audioGroup, thisTransform.position, blend, volume * (randomizeVolume ? Random.Range(0.75f, 1.15f) : 1), pitch * (randomizePitch ? Random.Range(0.9f, 1.1f) : 1), minDistance, maxDistance, useOcculusion);

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

            audioObject.Play(audioClip, ManagerCoreAudio.Instance.AudioListener, audioGroup, blend, volume * (randomizeVolume ? Random.Range(0.75f, 1.15f) : 1), pitch * (randomizePitch ? Random.Range(0.9f, 1.1f) : 1), minDistance, maxDistance, playOnLoop, useOcculusion);

            SetLowpass(lowpass / 22000f);
            SetResonance(rezonance);

            return;
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