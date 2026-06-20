using System.Collections;
using UnityEngine;

namespace Core.Audio
{
    public sealed class SetAudioVolume : MonoBehaviour
    {
        [Header("_")]
        [SerializeField] private AudioGroup group = AudioGroup.GAME;
        [SerializeField] private float fade = 0;

        [Header("_")]
        [SerializeField] private bool useGlobalVolume = false;
        [SerializeField, Range(0, 1)] private float startVolume = 0;
        [SerializeField, Range(0, 1)] private float targetVolume = 1;

        private Coroutine coroutine = null;

        public void Fade()
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }

            if (fade <= 0)
            {
                Set(targetVolume);
                return;
            }

            coroutine = StartCoroutine(FadeInternal());
        }
        private IEnumerator FadeInternal()
        {
            float v = useGlobalVolume ? Get() : startVolume;

            for (float t = 0; t < fade; t += Time.deltaTime)
            {
                Set(Mathf.Lerp(v, targetVolume, t / fade));
                yield return null;
            }

            Set(targetVolume);
        }

        private float Get()
        {
            AudioManager.Instance.GetVolume(group, out float _, out float _, out float multiplier);

            return multiplier;
        }
        private void Set(float value)
        {
            AudioManager.Instance.SetVolumeMult(group, value);
        }
    }
}
