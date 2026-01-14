using System;
using UnityEngine;

namespace Core.Audio
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(TriggerZone))]
    public class TriggerZoneAudioLowpass : MonoBehaviour
    {
        private enum ResetBehaviour { NULL, CLEAR }

        [Header("_")]
        [SerializeField, Required] private AudioGroup lowpassGroup = AudioGroup.EFFECT;
        [SerializeField, Range(0, 22000)] private float lowpassFrequency = 5000f;

        [Header("_")]
        [SerializeField] private ResetBehaviour exitBehaviour = ResetBehaviour.CLEAR;

        [Header("_")]     
        [SerializeField, Min(0)] private float fadeTime = 0.5f;

        private TriggerZone thisTrigger = null;

        private void Awake() => thisTrigger = GetComponent<TriggerZone>();
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

        private void OnEnter(Collider other) => ManagerCoreAudio.Instance.SetLowpass(lowpassFrequency, fadeTime, lowpassGroup);
        private void OnExit(Collider other)
        {
            if (exitBehaviour == ResetBehaviour.NULL)
            {
                return;
            }

            ManagerCoreAudio.Instance.SetLowpass(22000f, fadeTime, lowpassGroup);
        }
    }
}