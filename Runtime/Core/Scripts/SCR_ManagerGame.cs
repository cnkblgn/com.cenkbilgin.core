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
    public sealed class ManagerGame : Manager<ManagerGame>
    {
        public static event Action<GameState> OnGameStateChanged = null;
        public static event Action<float> OnCurrentSceneLoading = null;
        public static event Action<string> OnBeforeSceneChanged = null;
        public static event Action<string> OnAfterSceneChanged = null;
        public static event Action<float> OnTimeScaleChanged = null;

        public bool IsLoading => isLoading;
        public string Version => currentVersion;

        [Header("_")]
        [SerializeField, ReadOnly] private string currentVersion = STRING_NULL;
        [SerializeField, ReadOnly] private GameState currentState = GameState.NULL;

        [Header("_")]
        [SerializeField, Tooltip("eg. Main Menu Scene")] private Scene startingScene = null;
        [SerializeField, ReadOnly] private string[] playableScenes = null;
        [SerializeField, ReadOnly] private string[] buildScenes = null;

        private readonly List<IGameStateHandler> thisHandlers = new();
        private string activeScene = STRING_EMPTY;
        private bool isLoading = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnRuntimeInitialize()
        {
            OnGameStateChanged = null;
            OnCurrentSceneLoading = null;
            OnBeforeSceneChanged = null;
            OnAfterSceneChanged = null;
            OnTimeScaleChanged = null;
        }

        private void Start() => SetCurrentScene(startingScene.Name, LoadSceneMode.Single);
        private void OnEnable() => SceneManager.activeSceneChanged += OnActiveSceneChanged;
        private void OnDisable() => SceneManager.activeSceneChanged -= OnActiveSceneChanged;

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

        private void OnActiveSceneChanged(UnityEngine.SceneManagement.Scene oldScene, UnityEngine.SceneManagement.Scene newScene) => activeScene = newScene.name;

        public void BindHandler(IGameStateHandler value)
        {
            if (thisHandlers.Contains(value))
            {
                return;
            }

            thisHandlers.Add(value);
        }
        public void UnbindHandler(IGameStateHandler value)
        {
            if (!thisHandlers.Contains(value))
            {
                return;
            }

            thisHandlers.Remove(value);
        }

        public void ResumeGame()
        {
            if (isLoading)
            {
                return;
            }

            for (int i = thisHandlers.Count - 1; i >= 0; i--)
            {
                if (!thisHandlers[i].HandleCanResumeGame())
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

            for (int i = thisHandlers.Count - 1; i >= 0; i--)
            {
                if (!thisHandlers[i].HandleCanPauseGame())
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
        private void SetGameState(GameState gameState, bool @override = false)
        {
            if (currentState != gameState || @override)
            {
                currentState = gameState;
                OnGameStateChanged?.Invoke(currentState);

                switch (gameState)
                {
                    case GameState.RESUME:
                        SetTimeScale(1);
                        break;
                    case GameState.PAUSE:
                        SetTimeScale(0);
                        break;
                }
            }
        }

        public void SetTimeScale(float value)
        {
            Time.timeScale = value;
            OnTimeScaleChanged?.Invoke(Time.timeScale);
        }

        private bool GetIsValidScene(string scene) => buildScenes.Any(s => s == scene);
        public string[] GetScenes() => playableScenes;
        public string GetStartingScene() => startingScene.Name;
        public string GetCurrentScene() => activeScene;    
        public void SetCurrentScene(string scene) => SetCurrentScene(scene, LoadSceneMode.Single);
        public void SetCurrentScene(string scene, Action onStartAction = null, Action onFinishAction = null) => SetCurrentScene(scene, LoadSceneMode.Single, onStartAction, onFinishAction);
        public void SetCurrentScene(string scene, LoadSceneMode mode, Action onStartAction = null, Action onFinishAction = null)
        {
            if (!GetIsValidScene(scene))
            {
                return;
            }

            if (isLoading)
            {
                return;
            }

            SetGameState(GameState.NULL, true);
            StartCoroutine(SetCurrentSceneInternal(scene, mode, onStartAction, onFinishAction));
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
            onFinishAction?.Invoke();
            OnAfterSceneChanged?.Invoke(scene);
            SetGameState(GameState.RESUME, true);
        }
    }
}