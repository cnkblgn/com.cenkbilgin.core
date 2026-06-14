using System.Collections;
using UnityEngine;

namespace Core.Audio
{
    public class SetAudioVolume : MonoBehaviour
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
            return group switch
            {
                AudioGroup.MASTER => ManagerCoreAudio.Instance.GetMasterVolumeMult(),
                AudioGroup.MISC => ManagerCoreAudio.Instance.GetMiscVolumeMult(),
                AudioGroup.EFFECT => ManagerCoreAudio.Instance.GetEffectsVolumeMult(),
                AudioGroup.AMBIENT => ManagerCoreAudio.Instance.GetAmbientVolumeMult(),
                AudioGroup.MUSIC => ManagerCoreAudio.Instance.GetMusicVolumeMult(),
                AudioGroup.GAME => ManagerCoreAudio.Instance.GetGameVolumeMult(),
                _ => 1,
            };
        }
        private void Set(float value)
        {
            switch (group)
            {
                case AudioGroup.MASTER: ManagerCoreAudio.Instance.SetMasterVolumeMult(value); break;
                case AudioGroup.MISC: ManagerCoreAudio.Instance.SetMiscVolumeMult(value); break;
                case AudioGroup.EFFECT: ManagerCoreAudio.Instance.SetEffectsVolumeMult(value); break;
                case AudioGroup.AMBIENT: ManagerCoreAudio.Instance.SetAmbientVolumeMult(value); break;
                case AudioGroup.MUSIC: ManagerCoreAudio.Instance.SetMusicVolumeMult(value); break;
                case AudioGroup.GAME: ManagerCoreAudio.Instance.SetGameVolumeMult(value); break;
            }
        }
    }
}
