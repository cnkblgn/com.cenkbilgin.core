using System.Collections;
using UnityEngine;

namespace Core.Audio
{
    public class SetAudioPitch : MonoBehaviour
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
            return group switch
            {
                AudioGroup.MASTER => ManagerCoreAudio.Instance.GetMasterPitchMult(),
                AudioGroup.MISC => ManagerCoreAudio.Instance.GetMiscPitchMult(),
                AudioGroup.EFFECT => ManagerCoreAudio.Instance.GetEffectsPitchMult(),
                AudioGroup.AMBIENT => ManagerCoreAudio.Instance.GetAmbientPitchMult(),
                AudioGroup.MUSIC => ManagerCoreAudio.Instance.GetMusicPitchMult(),
                AudioGroup.GAME => ManagerCoreAudio.Instance.GetGamePitchMult(),
                _ => 1,
            };
        }
        private void Set(float value)
        {
            switch (group)
            {
                case AudioGroup.MASTER: ManagerCoreAudio.Instance.SetMasterPitchMult(value); break;
                case AudioGroup.MISC: ManagerCoreAudio.Instance.SetMiscPitchMult(value); break;
                case AudioGroup.EFFECT: ManagerCoreAudio.Instance.SetEffectsPitchMult(value); break;
                case AudioGroup.AMBIENT: ManagerCoreAudio.Instance.SetAmbientPitchMult(value); break;
                case AudioGroup.MUSIC: ManagerCoreAudio.Instance.SetMusicPitchMult(value); break;
                case AudioGroup.GAME: ManagerCoreAudio.Instance.SetGamePitchMult(value); break;
            }
        }
    }
}
