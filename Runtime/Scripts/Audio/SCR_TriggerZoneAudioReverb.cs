using UnityEngine;

namespace Core.Audio
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TriggerZone))]
    public class TriggerZoneAudioReverb : MonoBehaviour
    {
        private enum ResetBehaviour { NULL, CLEAR, REVERT }

        [Header("_")]
        [SerializeField] private AudioReverbPreset reverbPreset = AudioReverbPreset.Off;
        [SerializeField] private ResetBehaviour exitBehaviour = ResetBehaviour.CLEAR;

        [Header("_")]
        [SerializeField, Min(0)] private float fadeTime = 0.5f;

        private TriggerZone thisTrigger = null;
        private AudioReverbZone thisReverb = null;

        private void Awake()
        {
            thisTrigger = GetComponent<TriggerZone>();

            GameObject reverbObject = new("TempObject");
            reverbObject.transform.parent = gameObject.transform;

            thisReverb = reverbObject.AddComponent<AudioReverbZone>();
            thisReverb.reverbPreset = reverbPreset;

            reverbObject.SetActive(false);
        }
        private void OnEnable()
        {
            thisTrigger.OnEnter += OnEnter;
            thisTrigger.OnExit += OnExit;
        }
        private void OnDisable()
        {
            thisTrigger.OnEnter -= OnEnter;
            thisTrigger.OnExit -= OnExit;
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (thisReverb == null)
            {
                return;
            }

            thisReverb.reverbPreset = reverbPreset;
        }
#endif
        private void OnEnter(Collider other) => ManagerCoreAudio.Instance.SetReverb(thisReverb, fadeTime);
        private void OnExit(Collider other)
        {
            if (exitBehaviour == ResetBehaviour.NULL)
            {
                return;
            }

            switch (exitBehaviour)
            {
                case ResetBehaviour.CLEAR:
                    ManagerCoreAudio.Instance.SetReverb(null, fadeTime);
                    break;
                case ResetBehaviour.REVERT:
                    ManagerCoreAudio.Instance.RevertReverb(fadeTime);
                    break;
            }
        }
    }
}