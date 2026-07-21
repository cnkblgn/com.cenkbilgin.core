using System.Collections;
using UnityEngine;

namespace Core.Audio
{
    public sealed class SetAudioPitch : MonoBehaviour
    {
        [Header("_")]
        [SerializeField] private AudioGroup group = AudioGroup.GAME;
        [SerializeField] private float fade = 0;

        [Header("_")]
        [SerializeField] private bool useGlobalPitch = false;
        [SerializeField, Range(-3, 3)] private float startPitch = 1;
        [SerializeField, Range(-3, 3)] private float targetPitch = 1;

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
                Set(targetPitch);
                return;
            }

            coroutine = StartCoroutine(FadeInternal());
        }
        private IEnumerator FadeInternal()
        {
            float p = useGlobalPitch ? Get() : startPitch;

            for (float t = 0; t < fade; t += Time.deltaTime)
            {
                Set(Mathf.Lerp(p, targetPitch, t / fade));
                yield return null;
            }

            Set(targetPitch);
        }

        private float Get()
        {
            ManagerAudio.Instance.GetPitch(group, out float _, out float _, out float multiplier);

            return multiplier;
        }
        private void Set(float value)
        {
            ManagerAudio.Instance.SetPitchMult(group, value);
        }
    }
}
