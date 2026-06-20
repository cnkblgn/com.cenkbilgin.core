using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Core.Audio
{
    using static CoreUtility;
    using Random = UnityEngine.Random;

    [DisallowMultipleComponent]
    public class AudioManager : Manager<AudioManager>
    {
        public Transform AudioListener
        {
            get
            {
                if (audioListener == null)
                {
                    AudioListener listener = FindAnyObjectByType<AudioListener>();

                    if (listener != null)
                    {
                        audioListener = listener.transform;
                    }
                    else
                    {
                        Debug.LogError("AudioListener not found in scene!");
                    }
                }

                return audioListener;
            }
        }

        private const string LOWPASS_MASTER = "MasterLowpass";
        private const string LOWPASS_GAME = "GameLowpass";
        private const string LOWPASS_MUSIC = "MusicLowpass";
        private const string LOWPASS_MISC = "MiscLowpass";
        private const string LOWPASS_EFFECT = "EffectLowpass";
        private const string LOWPASS_AMBIENT = "AmbientLowpass";
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

        public AnimationCurve OcclusionLowpass => occlusionLowpass;
        public AnimationCurve OcclusionVolume => occlusionVolume;
        public LayerMask OcclusionMask => occlusionMask;
        public float OcclusionBlend => occlusionBlend;
        public float OcclusionAngle => occlusionAngle;

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
        [SerializeField] private AnimationCurve occlusionLowpass = null;
        [SerializeField] private AnimationCurve occlusionVolume = null;
        [SerializeField, Min(1)] private float occlusionBlend = 12.5f;
        [SerializeField, Range(2.5f, 15f)] private float occlusionAngle = 5.0f;

        [Header("_")]
        [SerializeField, Required] private AudioEmitter audioEmitterPrefab = null;
        [SerializeField, Required] private Transform audioEmitterContainer = null;

        private AudioPool audioEmitterPool = null;
        private readonly List<AudioEmitter> audioEmitterCollection = new();
        private readonly Dictionary<string, AudioGroup> audioGroups = new();
        private Transform audioListener = null;
        private AudioReverbZone audioLastReverbZone = null;
        private Coroutine audioCoroutineReverb = null;
        private float masterVolumeBase = 1;
        private float masterVolumeMult = 1;
        private float masterPitchBase = 1;
        private float masterPitchMult = 1;
        private float masterLowpass = 22000f;
        private float gameVolumeBase = 1;
        private float gameVolumeMult = 1;
        private float gamePitchBase = 1;
        private float gamePitchMult = 1;
        private float gameLowpass = 22000f;
        private float musicVolumeBase = 1;
        private float musicVolumeMult = 1;
        private float musicPitchBase = 1;
        private float musicPitchMult = 1;
        private float musicLowpass = 22000f;
        private float miscVolumeBase = 1;
        private float miscVolumeMult = 1;
        private float miscPitchBase = 1;
        private float miscPitchMult = 1;
        private float miscLowpass = 22000f;
        private float effectsVolumeBase = 1;
        private float effectsVolumeMult = 1;
        private float effectsPitchBase = 1;
        private float effectsPitchMult = 1;
        private float effectsLowpass = 22000f;
        private float ambientVolumeBase = 1;
        private float ambientVolumeMult = 1;
        private float ambientPitchBase = 1;
        private float ambientPitchMult = 1;
        private float ambientLowpass = 22000f;
        private int updateEmitterIndex = 0;

        protected override void Awake()
        {
            if (audioMixer == null) throw new NullReferenceException();
            if (audioMasterGroup == null) throw new NullReferenceException();
            if (audioGameGroup == null) throw new NullReferenceException();
            if (audioMusicGroup == null) throw new NullReferenceException();
            if (audioMiscGroup == null) throw new NullReferenceException();
            if (audioEffectsGroup == null) throw new NullReferenceException();
            if (audioAmbientGroup == null) throw new NullReferenceException();
            if (audioEmitterPrefab == null) throw new NullReferenceException();
            if (audioEmitterContainer == null) throw new NullReferenceException();

            base.Awake();

            InitializeGroups();
            InitializePool();
        }
        private void Update()
        {
            const int updatesPerFrame = 8;

            for (int i = 0; i < updatesPerFrame; i++)
            {
                if (audioEmitterCollection.Count == 0)
                {
                    return;
                }

                updateEmitterIndex %= audioEmitterCollection.Count;

                AudioEmitter emitter = audioEmitterCollection[updateEmitterIndex];

                updateEmitterIndex++;

                if (emitter == null)
                {
                    Debug.LogError($"Invalid (ghost) emitter detected! at: [{i}]");
                    continue;
                }

                emitter.Tick();               
            }
        }
        private void OnEnable()
        {
            AudioEmitter.OnCreated += OnEmitterCreated;
            AudioEmitter.OnDestroyed += OnEmitterDestroyed;

            GameManager.OnGameStateChanged += OnGameStateChanged;
            GameManager.OnBeforeSceneChanged += OnBeforeSceneChanged;
            GameManager.OnAfterSceneChanged += OnAfterSceneChanged;
            GameManager.OnTimeScaleChanged += OnTimeScaleChanged;
        }
        private void OnDisable()
        {
            AudioEmitter.OnCreated -= OnEmitterCreated;
            AudioEmitter.OnDestroyed -= OnEmitterDestroyed;

            GameManager.OnGameStateChanged -= OnGameStateChanged;
            GameManager.OnBeforeSceneChanged -= OnBeforeSceneChanged;
            GameManager.OnAfterSceneChanged -= OnAfterSceneChanged;
            GameManager.OnTimeScaleChanged -= OnTimeScaleChanged;
        }

        private void OnEmitterCreated(AudioEmitter emitter)
        {
            if (emitter == null)
            {
                Debug.LogError("emitter == null");
                return;
            }

            if (audioEmitterCollection.Contains(emitter))
            {
                Debug.LogError("Duplicate emitter!");
                return;
            }

            audioEmitterCollection.Add(emitter);
        }
        private void OnEmitterDestroyed(AudioEmitter emitter)
        {
            if (emitter == null)
            {
                Debug.LogError("emitter == null");
                return;
            }

            if (!audioEmitterCollection.Contains(emitter))
            {
                Debug.LogError("Invalid emitter!");
                return;
            }

            audioEmitterCollection.Remove(emitter);
        }

        private void OnBeforeSceneChanged(string scene)
        {
            ResetPool();

            SetPitchMult(AudioGroup.MASTER, 1);
            SetPitchMult(AudioGroup.GAME, 1);
            SetPitchMult(AudioGroup.MUSIC, 1);
            SetPitchMult(AudioGroup.EFFECT, 1);
            SetPitchMult(AudioGroup.MISC, 1);
            SetPitchMult(AudioGroup.AMBIENT, 1);

            SetVolumeMult(AudioGroup.MASTER, 1);
            SetVolumeMult(AudioGroup.GAME, 1);
            SetVolumeMult(AudioGroup.MUSIC, 1);
            SetVolumeMult(AudioGroup.EFFECT, 1);
            SetVolumeMult(AudioGroup.MISC, 1);
            SetVolumeMult(AudioGroup.AMBIENT, 1);

            SetLowpass(AudioGroup.MASTER, 22000f);
            SetLowpass(AudioGroup.GAME, 22000f);
            SetLowpass(AudioGroup.MUSIC, 22000f);
            SetLowpass(AudioGroup.EFFECT, 22000f);
            SetLowpass(AudioGroup.MISC, 22000f);
            SetLowpass(AudioGroup.AMBIENT, 22000f);
        }
        private void OnAfterSceneChanged(string scene) { }
        private void OnTimeScaleChanged(float timeScale)
        {
            switch (GameManager.Instance.GetGameState())
            {
                case GameState.RESUME:
                    SetPitchMult(AudioGroup.EFFECT, timeScale);
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

            foreach (AudioEmitter emitter in audioEmitterCollection)
            {
                switch (gameState)
                {
                    case GameState.NULL:
                        if (GetAudioGroup(emitter.AudioGroup) != AudioGroup.MASTER)
                        {
                            emitter.Stop();
                        }
                        break;
                    case GameState.RESUME:
                        emitter.Resume();
                        break;
                    case GameState.PAUSE:
                        if (GetAudioGroup(emitter.AudioGroup) != AudioGroup.MASTER)
                        {
                            emitter.Pause();
                        }
                        break;
                }
            }
        }

        private void ResetPool() => audioEmitterPool.Pool.Reset(false, true);
        private void InitializePool() => audioEmitterPool = new(PoolType.RING_BUFFER, audioEmitterPrefab, audioEmitterContainer, 128);

        private void InitializeGroups()
        {
            audioGroups[audioMasterGroup.name] = AudioGroup.MASTER;
            audioGroups[audioMiscGroup.name] = AudioGroup.MISC;
            audioGroups[audioEffectsGroup.name] = AudioGroup.EFFECT;
            audioGroups[audioAmbientGroup.name] = AudioGroup.AMBIENT;
            audioGroups[audioMusicGroup.name] = AudioGroup.MUSIC;
            audioGroups[audioGameGroup.name] = AudioGroup.GAME;
        }
        public AudioGroup GetAudioGroup(string group)
        {
            return audioGroups.TryGetValue(group, out AudioGroup value) ? value : AudioGroup.MASTER;
        }
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

        public void GetLowpass(AudioGroup group, out float current)
        {
            current = group switch
            {
                AudioGroup.MASTER => masterLowpass,
                AudioGroup.MISC => miscLowpass,
                AudioGroup.EFFECT => effectsLowpass,
                AudioGroup.AMBIENT => ambientLowpass,
                AudioGroup.MUSIC => musicLowpass,
                AudioGroup.GAME => gameLowpass,
                _ => 1,
            };
        }
        public void SetLowpass(AudioGroup group, float value)
        {
            string target = STRING_NULL;
            float frequency = 1;

            switch (group)
            {
                case AudioGroup.MASTER:
                    target = LOWPASS_MASTER;
                    frequency = masterLowpass = value;
                    break;
                case AudioGroup.MISC:
                    target = LOWPASS_MISC;
                    frequency = miscLowpass = value;
                    break;
                case AudioGroup.EFFECT:
                    target = LOWPASS_EFFECT;
                    frequency = effectsLowpass = value;
                    break;
                case AudioGroup.AMBIENT:
                    target = LOWPASS_AMBIENT;
                    frequency = ambientLowpass = value;
                    break;
                case AudioGroup.MUSIC:
                    target = LOWPASS_MUSIC;
                    frequency = musicLowpass = value;
                    break;
                case AudioGroup.GAME:
                    target = LOWPASS_GAME;
                    frequency = gameLowpass = value;
                    break;
                default:
                    break;
            }

            audioMixer.SetFloat(target, Mathf.Clamp(frequency, 0, 22000.00f));
        }

        public void GetPitch(AudioGroup group, out float current, out float @base, out float multiplier)
        {
            switch (group)
            {
                case AudioGroup.MASTER:
                    current = masterPitchBase * masterPitchMult;
                    @base = masterPitchBase;
                    multiplier = masterPitchMult;
                    break;
                case AudioGroup.MISC:
                    current = miscPitchBase * miscPitchMult;
                    @base = miscPitchBase;
                    multiplier = miscPitchMult;
                    break;
                case AudioGroup.EFFECT:
                    current = effectsPitchBase * effectsPitchMult;
                    @base = effectsPitchBase;
                    multiplier = effectsPitchMult;
                    break;
                case AudioGroup.AMBIENT:
                    current = ambientPitchBase * ambientPitchMult;
                    @base = ambientPitchBase;
                    multiplier = ambientPitchMult;
                    break;
                case AudioGroup.MUSIC:
                    current = musicPitchBase * musicPitchMult;
                    @base = musicPitchBase;
                    multiplier = musicPitchMult;
                    break;
                case AudioGroup.GAME:
                    current = gamePitchBase * gamePitchMult;
                    @base = gamePitchBase;
                    multiplier = gamePitchMult;
                    break;
                default:
                    current = 1;
                    @base = 1;
                    multiplier = 1;
                    break;
            }
        }
        public float GetPitch(string group)
        {
            audioMixer.GetFloat(group, out float pitch);

            return pitch;
        }
        private void SetPitch(string group, float value) => audioMixer.SetFloat(group, value);
        public void SetPitchBase(AudioGroup group, float value)
        {
            string target = STRING_NULL;
            float @base = 1;
            float mult = 1;

            switch (group)
            {
                case AudioGroup.MASTER:
                    target = PITCH_MASTER;
                    @base = masterPitchBase = value;
                    mult = masterPitchMult;
                    break;
                case AudioGroup.MISC:
                    target = PITCH_MISC;
                    @base = miscPitchBase = value;
                    mult = miscPitchMult;
                    break;
                case AudioGroup.EFFECT:
                    target = PITCH_EFFECT;
                    @base = effectsPitchBase = value;
                    mult = effectsPitchMult;
                    break;
                case AudioGroup.AMBIENT:
                    target = PITCH_AMBIENT;
                    @base = ambientPitchBase = value;
                    mult = ambientPitchMult;
                    break;
                case AudioGroup.MUSIC:
                    target = PITCH_MUSIC;
                    @base = musicPitchBase = value;
                    mult = musicPitchMult;
                    break;
                case AudioGroup.GAME:
                    target = PITCH_GAME;
                    @base = gamePitchBase = value;
                    mult = gamePitchMult;
                    break;
                default:
                    break;
            }

            SetPitch(target, @base * mult);
        }
        public void SetPitchMult(AudioGroup group, float value)
        {
            string target = STRING_NULL;
            float @base = 1;
            float mult = 1;

            switch (group)
            {
                case AudioGroup.MASTER:
                    target = PITCH_MASTER;
                    @base = masterPitchBase;
                    mult = masterPitchMult = value;
                    break;
                case AudioGroup.MISC:
                    target = PITCH_MISC;
                    @base = miscPitchBase;
                    mult = miscPitchMult = value;
                    break;
                case AudioGroup.EFFECT:
                    target = PITCH_EFFECT;
                    @base = effectsPitchBase;
                    mult = effectsPitchMult = value;
                    break;
                case AudioGroup.AMBIENT:
                    target = PITCH_AMBIENT;
                    @base = ambientPitchBase;
                    mult = ambientPitchMult = value;
                    break;
                case AudioGroup.MUSIC:
                    target = PITCH_MUSIC;
                    @base = musicPitchBase;
                    mult = musicPitchMult = value;
                    break;
                case AudioGroup.GAME:
                    target = PITCH_GAME;
                    @base = gamePitchBase;
                    mult = gamePitchMult = value;
                    break;
                default:
                    break;
            }

            SetPitch(target, @base * mult);
        }

        public void GetVolume(AudioGroup group, out float current, out float @base, out float multiplier)
        {
            switch (group)
            {
                case AudioGroup.MASTER:
                    current = masterVolumeBase * masterVolumeMult;
                    @base = masterVolumeBase;
                    multiplier = masterVolumeMult;
                    break;
                case AudioGroup.MISC:
                    current = miscVolumeBase * miscVolumeMult;
                    @base = miscVolumeBase;
                    multiplier = miscVolumeMult;
                    break;
                case AudioGroup.EFFECT:
                    current = effectsVolumeBase * effectsVolumeMult;
                    @base = effectsVolumeBase;
                    multiplier = effectsVolumeMult;
                    break;
                case AudioGroup.AMBIENT:
                    current = ambientVolumeBase * ambientVolumeMult;
                    @base = ambientVolumeBase;
                    multiplier = ambientVolumeMult;
                    break;
                case AudioGroup.MUSIC:
                    current = musicVolumeBase * musicVolumeMult;
                    @base = musicVolumeBase;
                    multiplier = musicVolumeMult;
                    break;
                case AudioGroup.GAME:
                    current = gameVolumeBase * gameVolumeMult;
                    @base = gameVolumeBase;
                    multiplier = gameVolumeMult;
                    break;
                default:
                    current = 1;
                    @base = 1;
                    multiplier = 1;
                    break;
            }
        }
        public float GetVolume(string group)
        { 
            audioMixer.GetFloat(group, out float volume);
            return Mathf.Pow(10, (volume / 20)); 
        }
        private void SetVolume(string group, float volume) => audioMixer.SetFloat(group, Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20);
        public void SetVolumeBase(AudioGroup group, float value)
        {
            string target = STRING_NULL;
            float @base = 1;
            float mult = 1;

            switch (group)
            {
                case AudioGroup.MASTER:
                    target = VOLUME_MASTER;
                    @base = masterVolumeBase = value;
                    mult = masterVolumeMult;
                    break;
                case AudioGroup.MISC:
                    target = VOLUME_MISC;
                    @base = miscVolumeBase = value;
                    mult = miscVolumeMult;
                    break;
                case AudioGroup.EFFECT:
                    target = VOLUME_EFFECT;
                    @base = effectsVolumeBase = value;
                    mult = effectsVolumeMult;
                    break;
                case AudioGroup.AMBIENT:
                    target = VOLUME_AMBIENT;
                    @base = ambientVolumeBase = value;
                    mult = ambientVolumeMult;
                    break;
                case AudioGroup.MUSIC:
                    target = VOLUME_MUSIC;
                    @base = musicVolumeBase = value;
                    mult = musicVolumeMult;
                    break;
                case AudioGroup.GAME:
                    target = VOLUME_GAME;
                    @base = gameVolumeBase = value;
                    mult = gameVolumeMult;
                    break;
                default:
                    break;
            }

            SetVolume(target, @base * mult);
        }
        public void SetVolumeMult(AudioGroup group, float value)
        {
            string target = STRING_NULL;
            float @base = 1;
            float mult = 1;

            switch (group)
            {
                case AudioGroup.MASTER:
                    target = VOLUME_MASTER;
                    @base = masterVolumeBase;
                    mult = masterVolumeMult = value;
                    break;
                case AudioGroup.MISC:
                    target = VOLUME_MISC;
                    @base = miscVolumeBase;
                    mult = miscVolumeMult = value;
                    break;
                case AudioGroup.EFFECT:
                    target = VOLUME_EFFECT;
                    @base = effectsVolumeBase;
                    mult = effectsVolumeMult = value;
                    break;
                case AudioGroup.AMBIENT:
                    target = VOLUME_AMBIENT;
                    @base = ambientVolumeBase;
                    mult = ambientVolumeMult = value;
                    break;
                case AudioGroup.MUSIC:
                    target = VOLUME_MUSIC;
                    @base = musicVolumeBase;
                    mult = musicVolumeMult = value;
                    break;
                case AudioGroup.GAME:
                    target = VOLUME_GAME;
                    @base = gameVolumeBase;
                    mult = gameVolumeMult = value;
                    break;
                default:
                    break;
            }

            SetVolume(target, @base * mult);
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
                float t = timer / fadeTime;

                audioMixer.SetFloat(REVERB_LF_REFERENCE, Mathf.Lerp(startLFReference, endLFReference, t));
                audioMixer.SetFloat(REVERB_ROOM_LF, Mathf.Lerp(startRoomLF, endRoomLF, t));
                audioMixer.SetFloat(REVERB_HF_REFERENCE, Mathf.Lerp(startHFReference, endHFReference, t));
                audioMixer.SetFloat(REVERB_ROOM_HF, Mathf.Lerp(startRoomHF, endRoomHF, t));
                audioMixer.SetFloat(REVERB_DIFFUSION, Mathf.Lerp(startDiffusion, endDiffusion, t));
                audioMixer.SetFloat(REVERB_DENSITY, Mathf.Lerp(startDensity, endDensity, t));
                audioMixer.SetFloat(REVERB_DELAY, Mathf.Lerp(startReverbDelay, endReverbDelay, t));
                audioMixer.SetFloat(REVERB_POWER, Mathf.Lerp(startReverbLevel, endReverbPower, t));
                audioMixer.SetFloat(REVERB_REFLECTION_DELAY, Mathf.Lerp(startReflectionsDelay, endReflectionsDelay, t));
                audioMixer.SetFloat(REVERB_REFLECTION_POWER, Mathf.Lerp(startReflectionsLevel, endReflectionsPower, t));
                audioMixer.SetFloat(REVERB_DECAY_HF_RATIO, Mathf.Lerp(startDecayHFRatio, endDecayHFRatio, t));
                audioMixer.SetFloat(REVERB_DECAY_TIME, Mathf.Lerp(startDecayTime, endDecayTime, t));
                audioMixer.SetFloat(REVERB_ROOM, Mathf.Lerp(startRoom, endRoom, t));

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

        public AudioEmitter PlaySound(AudioClip clip, AudioGroup group, Vector3 position, float blend, float volume, float pitch, float minDistance, float maxDistance, bool occulusion)
        {
            if (!occulusion)
            {
                return audioEmitterPool.Spawn(clip, AudioListener, GetAudioGroup(group), position, blend, volume, pitch, minDistance, maxDistance);
            }
            else
            {
                return audioEmitterPool.Spawn(clip, AudioListener, GetAudioGroup(group), position, blend, volume, pitch, minDistance, maxDistance, occlusionMask, occlusionAngle, occlusionBlend, occlusionLowpass, occlusionVolume);
            }
        }
        public AudioEmitter PlaySound(AudioClip[] clip, AudioGroup group, Vector3 position, float blend, float volume, float pitch, float minDistance, float maxDistance, bool occulusion)
        {
            if (clip == null)
            {
                Debug.LogError("AudioClip == null");
                return null;
            }

            if (clip.Length <= 0)
            {
                return null;
            }

            return PlaySound(clip[Random.Range(0, clip.Length)], group, position, blend, volume, pitch, minDistance, maxDistance, occulusion);
        }
    }
}