using UnityEngine;

namespace Core.Audio
{
    public static class SoundUtility
    {
        public static AudioClip GetClip(this SoundID id)
        {
            SoundDatabase.TryGet(id, out AudioClip clip);

            return clip;
        }

        public static void Play(this SoundID id, AudioGroup group) => AudioManager.Instance.PlaySound(id.GetClip(), group, Vector3.zero, 0, 1, 1, 1, 10, false);
        public static void Play(this SoundID id, AudioGroup group, Vector3 position, float blend = 0, float volume = 1, float pitch = 1, float minDistance = 1, float maxDistance = 10, bool occulusion = false) => AudioManager.Instance.PlaySound(id.GetClip(), group, position, blend, volume, pitch, minDistance, maxDistance, occulusion);
    }
}
