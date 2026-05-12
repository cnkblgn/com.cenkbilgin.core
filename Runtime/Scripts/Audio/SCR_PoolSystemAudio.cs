using UnityEngine;
using UnityEngine.Audio;

namespace Core.Audio
{
    public sealed class PoolSystemAudio : PoolSystem<AudioEmitter>
    {
        public override PoolType Type => PoolType.RING_BUFFER;
        public override string ID => "AUDIO_POOL";

        private AudioClip lastEmitterClip = null;
        private int lastEmitterFrame = -1;

        protected sealed override void OnInitialize(AudioEmitter item) { }
        protected sealed override void OnReset(AudioEmitter item) { }

        private bool TrySpawn(AudioClip clip, Transform listener, Vector3 position, float blend, float maxDistance, out AudioEmitter emitter)
        {
            emitter = null;

            if (listener == null)
            {
                Debug.LogError("AudioListener == null");
                return false;
            }

            if (clip == null)
            {
                Debug.LogError("AudioClip == null");
                return false;
            }

            if (lastEmitterFrame == Time.frameCount && lastEmitterClip == clip)
            {
                return false;
            }

            lastEmitterFrame = Time.frameCount;
            lastEmitterClip = clip;

            if (blend > 0)
            {
                float distance = (listener.position - position).sqrMagnitude;

                if (distance > maxDistance * maxDistance)
                {
                    return false;
                }
            }

            emitter = GetNext();
            emitter.gameObject.SetActive(true);
            emitter.SetPosition(position);

            return true;
        }

        public AudioEmitter Spawn(AudioClip clip, Transform listener, AudioMixerGroup group, Vector3 position, float blend, float volume, float pitch, float minDistance, float maxDistance, LayerMask occlusionMask, float occlusionAngle, float occlusionBlend, AnimationCurve occlusionLowpass, AnimationCurve occlusionVolume)
        {
            if (TrySpawn(clip, listener, position, blend, maxDistance, out AudioEmitter emitter))
            {
                emitter.Play
                (
                    clip,
                    listener,
                    group,
                    blend,
                    volume,
                    pitch,
                    minDistance,
                    maxDistance,
                    false,
                    occlusionMask,
                    occlusionAngle,
                    occlusionBlend,
                    occlusionLowpass,
                    occlusionVolume
                );

                return emitter;
            }

            return null;
        }
        public AudioEmitter Spawn(AudioClip clip, Transform listener, AudioMixerGroup group, Vector3 position, float blend, float volume, float pitch, float minDistance, float maxDistance)
        {
            if (TrySpawn(clip, listener, position, blend, maxDistance, out AudioEmitter emitter))
            {
                emitter.Play
                (
                    clip,
                    listener,
                    group,
                    blend,
                    volume,
                    pitch,
                    minDistance,
                    maxDistance,
                    false
                );

                return emitter;
            }

            return null;
        }
    }
}