using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public class ManagerCoreGame : Manager<ManagerCoreGame>
    {
        public interface IContextHandler 
        { 
            /// <summary> Called when game wants to resume </summary>
            public bool OnRequestResume();
            /// <summary> Called when game wants to pause </summary>
            public bool OnRequestPause(); 
        }

        public event Action<GameState> OnGameStateChanged = null;
        public event Action<float> OnCurrentSceneLoading = null;
        public event Action<string> OnBeforeSceneChanged = null;
        public event Action<string> OnAfterSceneChanged = null;
        public event Action<float> OnTimeScaleChanged = null;

        public bool IsLoading => isLoading;
        public string Version => currentVersion;

        [Header("_")]
        [SerializeField, ReadOnly] private string currentVersion = STRING_NULL;
        [SerializeField, ReadOnly] private GameState currentState = GameState.NULL;

        [Header("_")]
        [SerializeField, Tooltip("eg. Main Menu Scene")] private Scene startingScene = null;
        [SerializeField, ReadOnly] private string[] playableScenes = null;
        [SerializeField, ReadOnly] private string[] buildScenes = null;

        private readonly List<IContextHandler> contextHandlers = new();
        private Coroutine sceneCoroutine = null;
        private Coroutine timeCoroutine = null;
        private bool timeCoroutinePause = false;
        private bool isLoading = false;
        private string activeScene = STRING_EMPTY;

        protected override void Awake()
        {
            base.Awake();
        
            OnAfterSceneChanged += OnAfterSceneChangedInternal;
            OnGameStateChanged += OnGameStateChangedInternal;
        }
        private void Start() => SetCurrentScene(startingScene.Name, LoadSceneMode.Single);
        private void OnEnable()
        {
            SceneManager.activeSceneChanged += InitializeActiveScene;
        }
        private void OnDisable()
        {
            SceneManager.activeSceneChanged -= InitializeActiveScene;

            OnAfterSceneChanged -= OnAfterSceneChangedInternal;
            OnGameStateChanged -= OnGameStateChangedInternal;
            OnBeforeSceneChanged = null;
            OnAfterSceneChanged = null;
            OnTimeScaleChanged = null;
            OnGameStateChanged = null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            currentVersion = "v" + PlayerSettings.bundleVersion;

            List<string> validScenes = new();
            List<string> allScenes = new();

            foreach (UnityEditor.EditorBuildSettingsScene scene in UnityEditor.EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    string pathName = scene.path[(scene.path.LastIndexOf('/') + 1)..];
                    string sceneName = pathName[..^6];

                    allScenes.Add(sceneName);

                    if (string.Equals(sceneName, CoreBootstrapper.SCENE_NAME))
                    {
                        continue;
                    }

                    validScenes.Add(sceneName);
                }
            }

            buildScenes = allScenes.ToArray();
            playableScenes = validScenes.ToArray();
        }
#endif

        private void InitializeActiveScene(UnityEngine.SceneManagement.Scene oldScene, UnityEngine.SceneManagement.Scene newScene) => activeScene = newScene.name;
        private void OnAfterSceneChangedInternal(string scene) => SetGameState(GameState.RESUME, true);
        private void OnGameStateChangedInternal(GameState gameState)
        {
            switch (gameState)
            {
                case GameState.RESUME:
                    SetTimeScale(1, -1);
                    timeCoroutinePause = false;
                    break;
                case GameState.PAUSE:
                    timeCoroutinePause = true;
                    SetTimeScale(0, -1);
                    break;
            }
        }

        public void InstertContext(IContextHandler value)
        {
            if (contextHandlers.Contains(value))
            {
                return;
            }

            contextHandlers.Add(value);
        }
        public void RemoveContext(IContextHandler value)
        {
            if (!contextHandlers.Contains(value))
            {
                return;
            }

            contextHandlers.Remove(value);
        }

        public void ResumeGame()
        {
            if (isLoading)
            {
                return;
            }

            for (int i = contextHandlers.Count - 1; i >= 0; i--)
            {
                if (contextHandlers[i].OnRequestResume())
                {
                    return;
                }
            }

            SetGameState(GameState.RESUME);
        }
        public void PauseGame()
        {
            if (isLoading)
            {
                return;
            }

            for (int i = contextHandlers.Count - 1; i >= 0; i--)
            {
                if (contextHandlers[i].OnRequestPause())
                {
                    return;
                }
            }

            SetGameState(GameState.PAUSE);
        }
        public void QuitGame()
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                UnityEditor.EditorApplication.ExitPlaymode();
                return;
            }
#endif
            Application.Quit();
        }

        public GameState GetGameState() => currentState;
        private void SetGameState(GameState gameState, bool isOverride = false)
        {
            if (isOverride)
            {
                currentState = gameState;
                OnGameStateChanged?.Invoke(currentState);
                return;
            }

            if (currentState != gameState)
            {
                currentState = gameState;
                OnGameStateChanged?.Invoke(currentState);
            }
        }
        public void SetTimeScale(float value, float duration)
        {
            if (duration < 0)
            {
                Time.timeScale = value;
                OnTimeScaleChanged?.Invoke(Time.timeScale);
                return;
            }

            if (timeCoroutine != null)
            {
                StopCoroutine(timeCoroutine);
                timeCoroutine = null;
            }

            timeCoroutine = StartCoroutine(SetTimeScaleInternal(value, duration));
        }
        private IEnumerator SetTimeScaleInternal(float value, float duration)
        {
            float timeElapsed = 0;
            float timeDuration = Mathf.Max(0, duration);

            while (true)
            {
                if (timeCoroutinePause)
                {
                    yield return null;
                    continue;
                }

                if (timeElapsed >= timeDuration)
                {
                    break;
                }

                if (Time.timeScale != value)
                {
                    Time.timeScale = value;
                    OnTimeScaleChanged?.Invoke(Time.timeScale);
                }

                timeElapsed += Time.unscaledDeltaTime;

                yield return null;
            }

            SetTimeScale(1, -1);
            timeCoroutine = null;
            yield break;
        }
        public string[] GetAllScenes() => playableScenes;
        public string GetStartingScene() => startingScene.Name;
        public string GetCurrentScene() => activeScene;
        public void SetCurrentScene(string scene) => SetCurrentScene(scene, LoadSceneMode.Single);
        public void SetCurrentScene(string scene, Action onStartAction = null, Action onFinishAction = null) => SetCurrentScene(scene, LoadSceneMode.Single, onStartAction, onFinishAction);
        public void SetCurrentScene(string scene, LoadSceneMode mode, Action onStartAction = null, Action onFinishAction = null)
        {
            if (!IsValidScene(scene))
            {
                return;
            }

            if (sceneCoroutine != null)
            {
                return;
            }

            SetGameState(GameState.NULL, true);
            sceneCoroutine = StartCoroutine(SetCurrentSceneInternal(scene, mode, onStartAction, onFinishAction));
        }
        private IEnumerator SetCurrentSceneInternal(string scene, LoadSceneMode mode, Action onStartAction = null, Action onFinishAction = null)
        {
            isLoading = true;

            onStartAction?.Invoke();

            OnBeforeSceneChanged?.Invoke(GetCurrentScene());

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene, mode);
            asyncLoad.allowSceneActivation = false;

            while (!asyncLoad.isDone)
            {
                OnCurrentSceneLoading?.Invoke(asyncLoad.progress);

                if (asyncLoad.progress >= 0.9f)
                {
                    yield return new WaitForSecondsRealtime(1f);
                    asyncLoad.allowSceneActivation = true;
                }

                yield return null;
            }

            isLoading = false;
            sceneCoroutine = null;

            onFinishAction?.Invoke();
            OnAfterSceneChanged?.Invoke(scene);
        }
        public bool IsValidScene(string scene) => buildScenes.Any(s => s == scene);
        public bool IsStartingScene() => GetCurrentScene() == GetStartingScene();
        public bool IsBootstrapScene() => GetCurrentScene() == CoreBootstrapper.SCENE_NAME;
    }
}