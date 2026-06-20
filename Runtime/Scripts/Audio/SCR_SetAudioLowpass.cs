using System.Collections;
using UnityEngine;

namespace Core.Audio
{
    public sealed class SetAudioLowpass : MonoBehaviour
    {
        [Header("_")]
        [SerializeField] private AudioGroup group = AudioGroup.GAME;
        [SerializeField] private float fade = 0;

        [Header("_")]
        [SerializeField] private bool useGlobalLowpass = false;
        [SerializeField, Range(0, 22000)] private float startLowpass = 22000;
        [SerializeField, Range(0, 22000)] private float targetLowpasss = 10000;

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
                Set(targetLowpasss);
                return;
            }

            coroutine = StartCoroutine(FadeInternal());
        }
        private IEnumerator FadeInternal()
        {
            float l = useGlobalLowpass ? Get() : startLowpass;

            for (float t = 0; t < fade; t += Time.deltaTime)
            {
                Set(Mathf.Lerp(l, targetLowpasss, t / fade));
                yield return null;
            }

            Set(targetLowpasss);
        }

        private float Get()
        {
            AudioManager.Instance.GetLowpass(group, out float current);

            return current;
        }
        private void Set(float value)
        {
            AudioManager.Instance.SetLowpass(group, value);
        }
    }
}
