using System;
using UnityEngine;
using Core;
using Core.Input;

namespace Game
{
    using static CoreUtility;
    using static InputActionDatabase;

    [DisallowMultipleComponent]
    public class ManagerPlayer : Manager<ManagerPlayer>
    {      
        public static event Action<GameEntity> OnPlayerLoaded = null;
        public static event Action<GameEntity> OnPlayerUnloaded = null;

        public GameEntity Player => playerInstance; 

        [Header("_")]
        [SerializeField, Required] private GameEntity playerPrefab = null;

        private GameEntity playerInstance = null;

        private readonly DebugCommandInstanceBase[] debugCommands = new DebugCommandInstanceBase[]
        {
            new DebugCommandInstance($"tcl", $"Toggles Noclip", $"'tcl'", () =>
            {
                if (Instance.Player == null)
                {
                    return;
                }

                if (Instance.Player.TryGetComponent(out MovementProcessorNoclip noclip))
                {
                    noclip.Toggle();
                }
            }),
        };

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
        private void OnEnable()
        {
            for (int i = 0; i < debugCommands.Length; i++)
            {
                DebugCommandData.Insert(debugCommands[i]);
            }

            ManagerCoreGame.OnBeforeSceneChanged += OnBeforeSceneChanged;
        }
        private void OnDisable()
        {
            for (int i = 0; i < debugCommands.Length; i++)
            {
                DebugCommandData.Remove(debugCommands[i]);
            }

            ManagerCoreGame.OnBeforeSceneChanged -= OnBeforeSceneChanged;
        }
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
            if (ManagerCoreGame.Instance.IsBootstrapScene() || ManagerCoreGame.Instance.IsStartingScene())
            {
                return;
            }

            if (playerInstance == null)
            {
                Debug.LogError("ManagerPlayer.DespawnPlayer() playerInstance == null!");
                return;
            }

            OnPlayerUnloaded?.Invoke(playerInstance);
        }
    }
}