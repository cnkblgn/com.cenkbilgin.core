using UnityEngine;

namespace Core.Audio
{
    [DisallowMultipleComponent]
    public sealed class SetAudioReverb : MonoBehaviour
    {
        [Header("_")]
        [SerializeField] private bool debug;

        [Header("_")]
        [SerializeField] private float fade = 0;
        [SerializeField] private AudioReverbPreset preset = AudioReverbPreset.Off;

        private AudioReverbZone reverbZone = null;

        private void Awake()
        {
            GameObject obj = new("SetAudioReverbZone");
            obj.transform.parent = gameObject.transform;

            reverbZone = obj.AddComponent<AudioReverbZone>();
            reverbZone.reverbPreset = preset;
            reverbZone.gameObject.SetActive(false);
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (reverbZone == null)
            {
                return;
            }

            reverbZone.gameObject.SetActive(true);
            reverbZone.reverbPreset = preset;
            reverbZone.gameObject.SetActive(false);

            if (debug)
            {
                Debug.Log("Reverb zone preset changed to: " + preset);
            }
        }
#endif
        public void Revert()
        {
            ManagerAudio.Instance.RevertReverb(fade);

            if (debug)
            {
                Debug.Log("Reverb zone reverting to: default");
            }
        }
        public void Fade()
        {
            ManagerAudio.Instance.SetReverb(reverbZone, fade);

            if (debug)
            {
                Debug.Log("Reverb zone fading in to: " + preset);
            }
        }
    }
}
