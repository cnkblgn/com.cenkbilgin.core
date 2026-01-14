using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace Core.Audio
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public class ManagerCoreAudio : Manager<ManagerCoreAudio>
    {
        public Transform AudioListener
        {
            get
            {
                if (audioListenerTransform == null)
                {
                    AudioListener listener = FindAnyObjectByType<AudioListener>();

                    if (listener != null)
                    {
                        audioListenerTransform = listener.transform;
                    }
                    else
                    {
                        LogError("ManagerCoreAudio.AudioListener() listener not found in scene!");
                    }
                }

                return audioListenerTransform;
            }
        }

        private const string LOWPASS_AMBIENT = "AmbientLowpassFrequency";
        private const string LOWPASS_EFFECTS = "EffectLowpassFrequency";
        private const string REVERB_LF_REFERENCE = "EffectReverbLFReference";
        private const string REVERB_ROOM_LF = "EffectReverbRoomLF";
        private const string REVERB_HF_REFERENCE = "EffectReverbHFReference";
        private const string REVERB_ROOM_HF = "EffectReverbRoomHF";
        private const string REVERB_DIFFUSION = "EffectReverbDiffusion";
        private const string REVERB_DENSITY = "EffectReverbDensity";
        private const string REVERB_DELAY = "EffectReverbDelay";
        private const string REVERB_POWER = "EffectReverbPower";
        private const string REVERB_REFLECTION_DELAY = "EffectReverbReflectionsDelay";
        private const string REVERB_REFLECTION_POWER = "EffectReverbReflectionsPower";
        private const string REVERB_DECAY_HF_RATIO = "EffectReverbDecayHFRatio";
        private const string REVERB_DECAY_TIME = "EffectReverbDecayTime";
        private const string REVERB_ROOM = "EffectReverbRoom";
        private const string VOLUME_MASTER = "MasterVolume";
        private const string VOLUME_GAME = "GameVolume";
        private const string VOLUME_MUSIC = "MusicVolume";
        private const string VOLUME_MISC = "MiscVolume";
        private const string VOLUME_EFFECT = "EffectVolume";
        private const string VOLUME_AMBIENT = "AmbientVolume";
        private const string PITCH_MASTER = "MasterPitch";
        private const string PITCH_GAME = "GamePitch";
        private const string PITCH_MUSIC = "MusicPitch";
        private const string PITCH_MISC = "MiscPitch";
        private const string PITCH_EFFECT = "EffectPitch";
        private const string PITCH_AMBIENT = "AmbientPitch";

        public AnimationCurve OcclusionLowpassCurve => occlusionLowpassCurve;
        public AnimationCurve OcclusionVolumeCurve => occlusionVolumeCurve;
        public LayerMask OcclusionMask => occlusionMask;
        public float OcclusionLowpassBlendSpeed => occlusionLowpassBlendSpeed;
        public float OcclusionVolumeBlendSpeed => occlusionVolumeBlendSpeed;
        public float OcclusionRaycastAngle => occlusionRaycastAngle;

        [Header("_")]
        [SerializeField, Required] private AudioMixer audioMixer = null;

        [Header("_")]
        [SerializeField, Required] private AudioMixerGroup audioMasterGroup = null;
        [SerializeField, Required] private AudioMixerGroup audioGameGroup = null;
        [SerializeField, Required] private AudioMixerGroup audioMusicGroup = null;
        [SerializeField, Required] private AudioMixerGroup audioMiscGroup = null;
        [SerializeField, Required] private AudioMixerGroup audioEffectsGroup = null;
        [SerializeField, Required] private AudioMixerGroup audioAmbientGroup = null;

        [Header("_")]
        [SerializeField] private LayerMask occlusionMask = 0;
        [SerializeField] private AnimationCurve occlusionLowpassCurve = null;
        [SerializeField] private AnimationCurve occlusionVolumeCurve = null;
        [SerializeField, Min(1)] private float occlusionLowpassBlendSpeed = 12.5f;
        [SerializeField, Min(1)] private float occlusionVolumeBlendSpeed = 12.5f;
        [SerializeField, Range(2.5f, 15f)] private float occlusionRaycastAngle = 5.0f;

        [Header("_")]
        [SerializeField, Required] private AudioEmitter audioEmitterPrefab = null;
        [SerializeField, Required] private Transform audioEmitterContainer = null;

        private readonly PoolSystemAudio audioEmitterPool = new();
        private Transform audioListenerTransform = null;
        private AudioReverbZone audioLastReverbZone = null;
        private Coroutine audioCoroutineReverb = null;
        private Coroutine audioCoroutineLowpassEffects = null;
        private Coroutine audioCoroutineLowpassAmbient = null;
        private float masterVolumeBase = 1;
        private float masterVolumeMult = 1;
        private float masterPitchBase = 1;
        private float masterPitchMult = 1;
        private float gameVolumeBase = 1;
        private float gameVolumeMult = 1;
        private float gamePitchBase = 1;
        private float gamePitchMult = 1;
        private float musicVolumeBase = 1;
        private float musicVolumeMult = 1;
        private float musicPitchBase = 1;
        private float musicPitchMult = 1;
        private float miscVolumeBase = 1;
        private float miscVolumeMult = 1;
        private float miscPitchBase = 1;
        private float miscPitchMult = 1;
        private float effectsVolumeBase = 1;
        private float effectsVolumeMult = 1;
        private float effectsPitchBase = 1;
        private float effectsPitchMult = 1;
        private float ambientVolumeBase = 1;
        private float ambientVolumeMult = 1;
        private float ambientPitchBase = 1;
        private float ambientPitchMult = 1;

        protected override void Awake()
        {
            base.Awake();

            InitializePool();
        }
        private void OnEnable()
        {
            this.WaitUntil(() => ManagerCoreGame.Instance != null, null, () =>
            {
                ManagerCoreGame.Instance.OnGameStateChanged += OnGameStateChanged;
                ManagerCoreGame.Instance.OnBeforeSceneChanged += OnBeforeSceneChanged;
                ManagerCoreGame.Instance.OnAfterSceneChanged += OnAfterSceneChanged;
                ManagerCoreGame.Instance.OnTimeScaleChanged += OnTimeScaleChanged;
            });
        }
        private void OnDisable()
        {
            if (ManagerCoreGame.Instance != null)
            {
                ManagerCoreGame.Instance.OnGameStateChanged -= OnGameStateChanged;
                ManagerCoreGame.Instance.OnBeforeSceneChanged -= OnBeforeSceneChanged;
                ManagerCoreGame.Instance.OnAfterSceneChanged -= OnAfterSceneChanged;
                ManagerCoreGame.Instance.OnTimeScaleChanged -= OnTimeScaleChanged;
            }
        }

        private void OnBeforeSceneChanged(string scene)
        {
            ResetPool();

            SetMasterPitchMult(1);
            SetGamePitchMult(1);
            SetMusicPitchMult(1);
            SetEffectsPitchMult(1);
            SetMiscPitchMult(1);
            SetAmbientPitchMult(1);

            SetMasterVolumeMult(1);
            SetGameVolumeMult(1);
            SetMusicVolumeMult(1);
            SetEffectsVolumeMult(1);
            SetMiscVolumeMult(1);
            SetAmbientVolumeMult(1);

            SetLowpassEffects(22000f, 0);
            SetLowpassAmbient(22000f, 0);
        }
        private void OnAfterSceneChanged(string scene) { }
        private void OnTimeScaleChanged(float timeScale)
        {
            switch (ManagerCoreGame.Instance.GetGameState())
            {
                case GameState.RESUME:
                    SetEffectsPitchMult(timeScale);
                    break;
                case GameState.PAUSE:
                    break;
            }
        }
        private void OnGameStateChanged(GameState gameState)
        {
            switch (gameState)
            {
                case GameState.RESUME:
                    RevertReverb(0);
                    break;
                case GameState.PAUSE:
                    ClearReverb();
                    break;
            }
        }

        private void ResetPool() => audioEmitterPool.Reset();
        private void InitializePool() => audioEmitterPool.Initialize(audioEmitterPrefab, audioEmitterContainer, PoolType.RING_BUFFER, "audio", 128);

        public AudioMixerGroup GetAudioGroup(AudioGroup group)
        {
            return group switch
            {
                AudioGroup.MASTER => audioMasterGroup,
                AudioGroup.MISC => audioMiscGroup,
                AudioGroup.EFFECT => audioEffectsGroup,
                AudioGroup.AMBIENT => audioAmbientGroup,
                AudioGroup.MUSIC => audioMusicGroup,
                AudioGroup.GAME => audioGameGroup,
                _ => null,
            };
        }

        public void SetLowpass(float frequency, float fadeTime, AudioGroup group)
        {
            switch (group)
            {
                case AudioGroup.MASTER:
                    break;
                case AudioGroup.EFFECT:
                    SetLowpassEffects(frequency, fadeTime);
                    break;
                case AudioGroup.AMBIENT:
                    SetLowpassAmbient(frequency, fadeTime);
                    break;
                case AudioGroup.MUSIC:
                    break;
            }
        }
        private IEnumerator SetLowpassInternal(string name, float frequency, float fadeTime)
        {
            float endLowpassFrequency = frequency;

            audioMixer.GetFloat(name, out float startLowpassFrequency);

            for (float timer = 0; timer < fadeTime; timer += Time.deltaTime)
            {
                audioMixer.SetFloat(name, Mathf.Lerp(startLowpassFrequency, endLowpassFrequency, timer / fadeTime));
                yield return null;
            }

            audioMixer.SetFloat(name, Mathf.Clamp(endLowpassFrequency, 0, 22000.00f));
        }

        private void SetLowpassAmbient(float frequency, float fadeTime)
        {
            if (audioCoroutineLowpassAmbient != null)
            {
                StopCoroutine(audioCoroutineLowpassAmbient);
                audioCoroutineLowpassAmbient = null;
            }

            if (fadeTime <= 0)
            {
                audioMixer.SetFloat(LOWPASS_AMBIENT, Mathf.Clamp(frequency, 0, 22000.00f));
                return;
            }

            audioCoroutineLowpassAmbient = StartCoroutine(SetLowpassInternal(LOWPASS_AMBIENT, frequency, fadeTime));
        }
        private void SetLowpassEffects(float frequency, float fadeTime)
        {
            if (audioCoroutineLowpassEffects != null)
            {
                StopCoroutine(audioCoroutineLowpassEffects);
                audioCoroutineLowpassEffects = null;
            }

            if (fadeTime <= 0)
            {
                audioMixer.SetFloat(LOWPASS_EFFECTS, Mathf.Clamp(frequency, 0, 22000.00f));
                return;
            }

            audioCoroutineLowpassEffects = StartCoroutine(SetLowpassInternal(LOWPASS_EFFECTS, frequency, fadeTime));
        }

        private void ClearReverb()
        {
            audioMixer.SetFloat(REVERB_LF_REFERENCE, 250.00f);
            audioMixer.SetFloat(REVERB_ROOM_LF, 0.00f);
            audioMixer.SetFloat(REVERB_HF_REFERENCE, 5000.00f);
            audioMixer.SetFloat(REVERB_ROOM_HF, -10000.00f);
            audioMixer.SetFloat(REVERB_DIFFUSION, 0.00f);
            audioMixer.SetFloat(REVERB_DENSITY, 0.00f);
            audioMixer.SetFloat(REVERB_DELAY, 0.01f);
            audioMixer.SetFloat(REVERB_POWER, 200.00f);
            audioMixer.SetFloat(REVERB_REFLECTION_DELAY, 0.00f);
            audioMixer.SetFloat(REVERB_REFLECTION_POWER, -2602.00f);
            audioMixer.SetFloat(REVERB_DECAY_HF_RATIO, 1.00f);
            audioMixer.SetFloat(REVERB_DECAY_TIME, 1.00f);
            audioMixer.SetFloat(REVERB_ROOM, -10000.00f);
        }      
        public void RevertReverb(float fadeTime)
        {
            if (audioCoroutineReverb != null)
            {
                StopCoroutine(audioCoroutineReverb);
                audioCoroutineReverb = null;
            }

            audioCoroutineReverb = StartCoroutine(SetReverbInternal(audioLastReverbZone, fadeTime));
        }
        public void SetReverb(AudioReverbZone reverbZone, float fadeTime)
        {
            if (audioCoroutineReverb != null)
            {
                StopCoroutine(audioCoroutineReverb);
                audioCoroutineReverb = null;
            }

            audioCoroutineReverb = StartCoroutine(SetReverbInternal(reverbZone, fadeTime));
        }
        private IEnumerator SetReverbInternal(AudioReverbZone reverbZone, float fadeTime)
        {
            audioLastReverbZone = reverbZone;

            if (reverbZone == null)
            {
                ClearReverb();
                yield break;
            }

            float endLFReference = reverbZone.LFReference;
            float endRoomLF = reverbZone.roomLF;
            float endHFReference = reverbZone.HFReference;
            float endRoomHF = reverbZone.roomHF;
            float endDiffusion = reverbZone.diffusion;
            float endDensity = reverbZone.density;
            float endReverbDelay = reverbZone.reverbDelay;
            float endReverbPower = reverbZone.reverb;
            float endReflectionsDelay = reverbZone.reflectionsDelay;
            float endReflectionsPower = reverbZone.reflections;
            float endDecayHFRatio = reverbZone.decayHFRatio;
            float endDecayTime = reverbZone.decayTime;
            float endRoom = reverbZone.room;

            audioMixer.GetFloat(REVERB_LF_REFERENCE, out float startLFReference);
            audioMixer.GetFloat(REVERB_ROOM_LF, out float startRoomLF);
            audioMixer.GetFloat(REVERB_HF_REFERENCE, out float startHFReference);
            audioMixer.GetFloat(REVERB_ROOM_HF, out float startRoomHF);
            audioMixer.GetFloat(REVERB_DIFFUSION, out float startDiffusion);
            audioMixer.GetFloat(REVERB_DENSITY, out float startDensity);
            audioMixer.GetFloat(REVERB_DELAY, out float startReverbDelay);
            audioMixer.GetFloat(REVERB_POWER, out float startReverbLevel);
            audioMixer.GetFloat(REVERB_REFLECTION_DELAY, out float startReflectionsDelay);
            audioMixer.GetFloat(REVERB_REFLECTION_POWER, out float startReflectionsLevel);
            audioMixer.GetFloat(REVERB_DECAY_HF_RATIO, out float startDecayHFRatio);
            audioMixer.GetFloat(REVERB_DECAY_TIME, out float startDecayTime);
            audioMixer.GetFloat(REVERB_ROOM, out float startRoom);

            for (float timer = 0; timer < fadeTime; timer += Time.deltaTime)
            {
                audioMixer.SetFloat(REVERB_LF_REFERENCE, Mathf.Lerp(startLFReference, endLFReference, timer / fadeTime));
                audioMixer.SetFloat(REVERB_ROOM_LF, Mathf.Lerp(startRoomLF, endRoomLF, timer / fadeTime));
                audioMixer.SetFloat(REVERB_HF_REFERENCE, Mathf.Lerp(startHFReference, endHFReference, timer / fadeTime));
                audioMixer.SetFloat(REVERB_ROOM_HF, Mathf.Lerp(startRoomHF, endRoomHF, timer / fadeTime));
                audioMixer.SetFloat(REVERB_DIFFUSION, Mathf.Lerp(startDiffusion, endDiffusion, timer / fadeTime));
                audioMixer.SetFloat(REVERB_DENSITY, Mathf.Lerp(startDensity, endDensity, timer / fadeTime));
                audioMixer.SetFloat(REVERB_DELAY, Mathf.Lerp(startReverbDelay, endReverbDelay, timer / fadeTime));
                audioMixer.SetFloat(REVERB_POWER, Mathf.Lerp(startReverbLevel, endReverbPower, timer / fadeTime));
                audioMixer.SetFloat(REVERB_REFLECTION_DELAY, Mathf.Lerp(startReflectionsDelay, endReflectionsDelay, timer / fadeTime));
                audioMixer.SetFloat(REVERB_REFLECTION_POWER, Mathf.Lerp(startReflectionsLevel, endReflectionsPower, timer / fadeTime));
                audioMixer.SetFloat(REVERB_DECAY_HF_RATIO, Mathf.Lerp(startDecayHFRatio, endDecayHFRatio, timer / fadeTime));
                audioMixer.SetFloat(REVERB_DECAY_TIME, Mathf.Lerp(startDecayTime, endDecayTime, timer / fadeTime));
                audioMixer.SetFloat(REVERB_ROOM, Mathf.Lerp(startRoom, endRoom, timer / fadeTime));

                yield return null;
            }

            audioMixer.SetFloat(REVERB_LF_REFERENCE, endLFReference);
            audioMixer.SetFloat(REVERB_ROOM_LF, endRoomLF);
            audioMixer.SetFloat(REVERB_HF_REFERENCE, endHFReference);
            audioMixer.SetFloat(REVERB_ROOM_HF, endRoomHF);
            audioMixer.SetFloat(REVERB_DIFFUSION, endDiffusion);
            audioMixer.SetFloat(REVERB_DENSITY, endDensity);
            audioMixer.SetFloat(REVERB_DELAY, endReverbDelay);
            audioMixer.SetFloat(REVERB_POWER, endReverbPower);
            audioMixer.SetFloat(REVERB_REFLECTION_DELAY, endReflectionsDelay);
            audioMixer.SetFloat(REVERB_REFLECTION_POWER, endReflectionsPower);
            audioMixer.SetFloat(REVERB_DECAY_HF_RATIO, endDecayHFRatio);
            audioMixer.SetFloat(REVERB_DECAY_TIME, endDecayTime);
            audioMixer.SetFloat(REVERB_ROOM, endRoom);
        }

        private void SetPitch(string group, float pitch) => audioMixer.SetFloat(group, pitch);
        private float GetPitch(string group) { audioMixer.GetFloat(group, out float pitch); return pitch; }

        public float GetMasterPitch() => masterPitchBase * masterPitchMult;
        public float GetMasterPitchBase() => masterPitchBase;
        public float GetMasterPitchMult() => masterPitchMult;
        public void SetMasterPitchBase(float value) { masterPitchBase = value; SetPitch(PITCH_MASTER, masterPitchBase * masterPitchMult); }
        public void SetMasterPitchMult(float value) { masterPitchMult = value; SetPitch(PITCH_MASTER, masterPitchBase * masterPitchMult); }
        public float GetGamePitch() => gamePitchBase * gamePitchMult;
        public float GetGamePitchBase() => gamePitchBase;
        public float GetGamePitchMult() => gamePitchMult;
        public void SetGamePitchBase(float value) { gamePitchBase = value; SetPitch(PITCH_GAME, gamePitchBase * gamePitchMult); }
        public void SetGamePitchMult(float value) { gamePitchMult = value; SetPitch(PITCH_GAME, gamePitchBase * gamePitchMult); }
        public float GetMusicPitch() => musicPitchBase * musicPitchMult;
        public float GetMusicPitchBase() => musicPitchBase;
        public float GetMusicPitchMult() => musicPitchMult;
        public void SetMusicPitchBase(float value) { musicPitchBase = value; SetPitch(PITCH_MUSIC, musicPitchBase * musicPitchMult); }
        public void SetMusicPitchMult(float value) { musicPitchMult = value; SetPitch(PITCH_MUSIC, musicPitchBase * musicPitchMult); }
        public float GetMiscPitch() => miscPitchBase * miscPitchMult;
        public float GetMiscPitchBase() => miscPitchBase;
        public float GetMiscPitchMult() => miscPitchMult;
        public void SetMiscPitchBase(float value) { miscPitchBase = value; SetPitch(PITCH_MISC, miscPitchBase * miscPitchMult); }
        public void SetMiscPitchMult(float value) { miscPitchMult = value; SetPitch(PITCH_MISC, miscPitchBase * miscPitchMult); }
        public float GetEffectsPitch() => effectsPitchBase * effectsPitchMult;
        public float GetEffectsPitchBase() => effectsPitchBase;
        public float GetEffectsPitchMult() => effectsPitchMult;
        public void SetEffectsPitchBase(float value) { effectsPitchBase = value; SetPitch(PITCH_EFFECT, effectsPitchBase * effectsPitchMult); }
        public void SetEffectsPitchMult(float value) { effectsPitchMult = value; SetPitch(PITCH_EFFECT, effectsPitchBase * effectsPitchMult); }
        public float GetAmbientPitch() => ambientPitchBase * ambientPitchMult;
        public float GetAmbientPitchBase() => ambientPitchBase;
        public float GetAmbientPitchMult() => ambientPitchMult;
        public void SetAmbientPitchBase(float value) { ambientPitchBase = value; SetPitch(PITCH_AMBIENT, ambientPitchBase * ambientPitchMult); }
        public void SetAmbientPitchMult(float value) { ambientPitchMult = value; SetPitch(PITCH_AMBIENT, ambientPitchBase * ambientPitchMult); }

        private void SetVolume(string group, float volume) => audioMixer.SetFloat(group, Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20);
        private float GetVolume(string group) { audioMixer.GetFloat(group, out float volume); return Mathf.Pow(10, (volume / 20)); }

        public float GetMasterVolume() => masterVolumeBase * masterVolumeMult;
        public float GetMasterVolumeBase() => masterVolumeBase;
        public float GetMasterVolumeMult() => masterVolumeMult;
        public void SetMasterVolumeBase(float value) { masterVolumeBase = value; SetVolume(VOLUME_MASTER, masterVolumeBase * masterVolumeMult); }
        public void SetMasterVolumeMult(float value) { masterVolumeMult = value; SetVolume(VOLUME_MASTER, masterVolumeBase * masterVolumeMult); }
        public float GetGameVolume() => gameVolumeBase * gameVolumeMult;
        public float GetGameVolumeBase() => gameVolumeBase;
        public float GetGameVolumeMult() => gameVolumeMult;
        public void SetGameVolumeBase(float value) { gameVolumeBase = value; SetVolume(VOLUME_GAME, gameVolumeBase * gameVolumeMult); }
        public void SetGameVolumeMult(float value) { gameVolumeMult = value; SetVolume(VOLUME_GAME, gameVolumeBase * gameVolumeMult); }
        public float GetMusicVolume() => musicVolumeBase * musicVolumeMult;
        public float GetMusicVolumeBase() => musicVolumeBase;
        public float GetMusicVolumeMult() => musicVolumeMult;
        public void SetMusicVolumeBase(float value) { musicVolumeBase = value; SetVolume(VOLUME_MUSIC, musicVolumeBase * musicVolumeMult); }
        public void SetMusicVolumeMult(float value) { musicVolumeMult = value; SetVolume(VOLUME_MUSIC, musicVolumeBase * musicVolumeMult); }
        public float GetMiscVolume() => miscVolumeBase * miscVolumeMult;
        public float GetMiscVolumeBase() => miscVolumeBase;
        public float GetMiscVolumeMult() => miscVolumeMult;
        public void SetMiscVolumeBase(float value) { miscVolumeBase = value; SetVolume(VOLUME_MISC, miscVolumeBase * miscVolumeMult); }
        public void SetMiscVolumeMult(float value) { miscVolumeMult = value; SetVolume(VOLUME_MISC, miscVolumeBase * miscVolumeMult); }
        public float GetEffectsVolume() => effectsVolumeBase * effectsVolumeMult;
        public float GetEffectsVolumeBase() => effectsVolumeBase;
        public float GetEffectsVolumeMult() => effectsVolumeMult;
        public void SetEffectsVolumeBase(float value) { effectsVolumeBase = value; SetVolume(VOLUME_EFFECT, effectsVolumeBase * effectsVolumeMult); }
        public void SetEffectsVolumeMult(float value) { effectsVolumeMult = value; SetVolume(VOLUME_EFFECT, effectsVolumeBase * effectsVolumeMult); }
        public float GetAmbientVolume() => ambientVolumeBase * ambientVolumeMult;
        public float GetAmbientVolumeBase() => ambientVolumeBase;
        public float GetAmbientVolumeMult() => ambientVolumeMult;
        public void SetAmbientVolumeBase(float value) { ambientVolumeBase = value; SetVolume(VOLUME_AMBIENT, ambientVolumeBase * ambientVolumeMult); }
        public void SetAmbientVolumeMult(float value) { ambientVolumeMult = value; SetVolume(VOLUME_AMBIENT, ambientVolumeBase * ambientVolumeMult); }

        public AudioEmitter PlaySound(AudioClip clip, AudioGroup group, Vector3 position, float blend, float volume, float pitch, float minDistance, float maxDistance, bool occulusion) => audioEmitterPool.Spawn(clip, AudioListener, group, position, blend, volume, pitch, minDistance, maxDistance, occulusion);
        public AudioEmitter PlaySound(AudioClip[] clip, AudioGroup group, Vector3 position, float blend, float volume, float pitch, float minDistance, float maxDistance, bool occulusion)
        {
            if (clip == null)
            {
                LogError("ManagerCoreAudio.PlaySound() clip == null");
                return null;
            }

            return PlaySound(clip[Random.Range(0, clip.Length)], group, position, blend, volume, pitch * Random.Range(0.9f, 1.1f), minDistance, maxDistance, occulusion);
        }
    }
}