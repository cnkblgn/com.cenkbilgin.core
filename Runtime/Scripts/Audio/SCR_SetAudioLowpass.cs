using System.Collections;
using UnityEngine;

namespace Core.Audio
{
    public class SetAudioLowpass : MonoBehaviour
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
            return group switch
            {
                AudioGroup.MASTER => ManagerCoreAudio.Instance.GetLowpass(AudioGroup.MASTER),
                AudioGroup.MISC => ManagerCoreAudio.Instance.GetLowpass(AudioGroup.MISC),
                AudioGroup.EFFECT => ManagerCoreAudio.Instance.GetLowpass(AudioGroup.EFFECT),
                AudioGroup.AMBIENT => ManagerCoreAudio.Instance.GetLowpass(AudioGroup.AMBIENT),
                AudioGroup.MUSIC => ManagerCoreAudio.Instance.GetLowpass(AudioGroup.MUSIC),
                AudioGroup.GAME => ManagerCoreAudio.Instance.GetLowpass(AudioGroup.GAME),
                _ => 1,
            };
        }
        private void Set(float value)
        {
            switch (group)
            {
                case AudioGroup.MASTER: ManagerCoreAudio.Instance.SetLowpass(value, 0, AudioGroup.MASTER); break;
                case AudioGroup.MISC: ManagerCoreAudio.Instance.SetLowpass(value, 0, AudioGroup.MISC); break;
                case AudioGroup.EFFECT: ManagerCoreAudio.Instance.SetLowpass(value, 0, AudioGroup.EFFECT); break;
                case AudioGroup.AMBIENT: ManagerCoreAudio.Instance.SetLowpass(value, 0, AudioGroup.AMBIENT); break;
                case AudioGroup.MUSIC: ManagerCoreAudio.Instance.SetLowpass(value, 0, AudioGroup.MUSIC); break;
                case AudioGroup.GAME: ManagerCoreAudio.Instance.SetLowpass(value, 0, AudioGroup.GAME); break;
            }
        }
    }
}
