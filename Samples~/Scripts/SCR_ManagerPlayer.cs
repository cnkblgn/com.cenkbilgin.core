using System;
using UnityEngine;
using Core;
using Core.Input;

namespace Game
{
    using static CoreUtility;
    using static InputDatabase;

    [DisallowMultipleComponent]
    public class ManagerPlayer : Manager<ManagerPlayer>
    {
        public static event Action<GameEntity> OnPlayerLoaded = null;
        public static event Action<GameEntity> OnPlayerUnloaded = null;

        public static GameEntity Player => playerInstance;

        [Header("_")]
        [SerializeField, Required] private GameEntity playerPrefab = null;

        private static GameEntity playerInstance = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RESET()
        {
            OnPlayerLoaded = null;
            OnPlayerUnloaded = null;
            playerInstance = null;
        }

        private void Update()
        {
            if (playerInstance == null)
            {
                return;
            }

            if (Menu.GetKeyDown())
            {
                switch (ManagerCoreGame.Instance.GetGameState())
                {
                    case GameState.RESUME:
                        ManagerCoreGame.Instance.PauseGame();
                        break;
                    case GameState.PAUSE:
                        ManagerCoreGame.Instance.ResumeGame();
                        break;
                }
            }
        }
        private void OnEnable() => ManagerCoreGame.OnBeforeSceneChanged += OnBeforeSceneChanged;
        private void OnDisable() => ManagerCoreGame.OnBeforeSceneChanged -= OnBeforeSceneChanged;

        private void OnBeforeSceneChanged(string scene) => DespawnPlayer();

        public GameEntity SpawnPlayer(Vector3 position, Quaternion rotation) => SpawnPlayer(playerPrefab, position, rotation);
        public GameEntity SpawnPlayer(GameEntity playerPrefab, Vector3 position, Quaternion rotation)
        {
            playerInstance = Instantiate(playerPrefab, position, rotation);
            OnPlayerLoaded?.Invoke(playerInstance);

            return playerInstance;
        }
        private void DespawnPlayer()
        {
            if (ManagerCoreGame.Instance.IsBootstrapScene())
            {
                return;
            }

            if (playerInstance == null)
            {
                Debug.LogError("ManagerPlayer.DespawnPlayer() playerInstance == null!");
                return;
            }

            OnPlayerUnloaded?.Invoke(playerInstance);
            playerInstance = null;
        }
    }
}