using UnityEngine;

namespace Core.Audio
{
    using static CoreUtility;

    public sealed class SetAudioReverb : MonoBehaviour
    {
        [Header("_")]
        [SerializeField] private float fade = 0;
        [SerializeField] private AudioReverbPreset preset = AudioReverbPreset.Off;

        private AudioReverbZone reverbZone = null;

        private void Awake()
        {
            GameObject obj = new("SetAudioReverbZone");
            obj.hideFlags = HideFlags.HideInHierarchy;
            obj.transform.parent = gameObject.transform;

            reverbZone = obj.AddComponent<AudioReverbZone>();
            reverbZone.reverbPreset = preset;

            obj.SetActive(false);
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (reverbZone == null)
            {
                return;
            }

            reverbZone.reverbPreset = preset;
        }
#endif
        public void Revert() => ManagerAudio.Instance.RevertReverb(fade);
        public void Fade() => ManagerAudio.Instance.SetReverb(reverbZone, fade);
    }
}
