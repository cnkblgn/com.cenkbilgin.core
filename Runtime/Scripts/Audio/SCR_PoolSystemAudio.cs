using System;
using UnityEngine;

namespace Core.Audio
{
    using static CoreUtility;

    [Serializable]
    public class PoolSystemAudio : PoolSystem<AudioEmitter>
    {
        private AudioClip lastEmitterClip = null;
        private int lastEmitterFrame = -1;

        protected override void OnInitialize(AudioEmitter item) { }
        protected override void OnReset(AudioEmitter item) { }
        public AudioEmitter Spawn(AudioClip clip, Transform listener, AudioGroup group, Vector3 position, float blend, float volume, float pitch, float minDistance, float maxDistance, bool occulusion)
        {
            if (listener == null)
            {
                Debug.LogError("PoolSystemAudio.Spawn() listener == null");
                return null;
            }

            if (clip == null)
            {
                Debug.LogError("PoolSystemAudio.Spawn() clip == null");
                return null;
            }

            if (lastEmitterFrame == Time.frameCount && lastEmitterClip == clip)
            {
                return null;
            }

            lastEmitterFrame = Time.frameCount;
            lastEmitterClip = clip;

            if (blend > 0)
            {
                float distance = (listener.position - position).sqrMagnitude;

                if (distance > maxDistance * maxDistance)
                {
                    return null;
                }
            }

            AudioEmitter audioEmitter = GetNext();
            audioEmitter.gameObject.SetActive(true);
            audioEmitter.ThisTransform.SetPositionAndRotation(position, Quaternion.identity);
            audioEmitter.Play(clip, listener, group, blend, volume, pitch, minDistance, maxDistance, false, occulusion);
            return audioEmitter;
        }
    }
}