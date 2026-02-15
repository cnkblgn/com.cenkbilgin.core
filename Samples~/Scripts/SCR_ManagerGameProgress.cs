using UnityEngine;
using Core;

namespace Game
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public class ManagerGameProgress : Manager<ManagerGameProgress>
    {
        private SerializeableFile<GameProgress> thisProgress = default;

        private readonly DebugCommandInstanceBase[] debugCommands = new DebugCommandInstanceBase[]
        {
            new DebugCommandInstance($"save", $"Quick Save", $"'save'", () => Instance.Save()),
            new DebugCommandInstance($"load", $"Quick Load", $"'load'", () => Instance.Load()),
        };

        protected override void Awake()
        {
            base.Awake();

            thisProgress = new("progress");
            thisProgress.Load(true);
        }
        private void OnEnable()
        {
            for (int i = 0; i < debugCommands.Length; i++)
            {
                DebugCommandData.Insert(debugCommands[i]);
            }
        }
        private void OnDisable()
        {
            for (int i = 0; i < debugCommands.Length; i++)
            {
                DebugCommandData.Remove(debugCommands[i]);
            }

            thisProgress.Save(true);
        }

        public void Save()
        {
            GameEntity player = ManagerPlayer.Instance.Player;

            if (player == null)
            {
                Debug.LogWarning("ManagerGameProgress.Save() trying to save without player spawned!");
                return;
            }

            SavePlayerTransform(player);
            SaveSceneData();

            thisProgress.Save(true);
        }
        public void Load()
        {
            if (thisProgress == null)
            {
                Debug.LogError("ManagerGameProgress.Load() thisProgress == null");
                return;
            }

            ManagerCoreGame.Instance.SetCurrentScene(thisProgress.Data.SceneData.ID, null, () =>
            {
                this.WaitFrame(null, () =>
                {
                    LoadPlayerTransform();
                    LoadSceneData();
                });
            });
        }
        public void Clear() => thisProgress.Clear();

        private void SavePlayerTransform(GameEntity player)
        {
            thisProgress.Data.PlayerPosition = (Float3)player.transform.position;
            thisProgress.Data.PlayerRotation = (Float3)player.transform.rotation.eulerAngles;
        }
        private void SaveSceneData()
        {
            thisProgress.Data.SceneData = ManagerCorePersistent.Instance.Export();
        }

        private void LoadPlayerTransform()
        {
            GameEntity player = ManagerPlayer.Instance.Player;

            if (player != null)
            {
                player.transform.SetPositionAndRotation(thisProgress.Data.PlayerPosition, Quaternion.Euler(thisProgress.Data.PlayerRotation));
            }
            else
            {
                player = ManagerPlayer.Instance.SpawnPlayer(thisProgress.Data.PlayerPosition, Quaternion.Euler(thisProgress.Data.PlayerRotation));
            }
        }
        private void LoadSceneData()
        {
            ManagerCorePersistent.Instance.Import(thisProgress.Data.SceneData);
        }
    }
}
