using Core;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    using static CoreUtility;

    public class ManagerGameDebug : Manager<ManagerGameDebug>
    {
        private readonly static List<DebugCommandInstanceBase> data = new()
        {
            new DebugCommandInstance($"getm", $"Get Scene List", $"'getm'", CMDGetScene),
            new DebugCommandInstance<string>($"setm", $"Set Scene", $"'setm'", CMDSetScene),
        };

        private void OnEnable()
        {
            foreach (var item in data)
            {
                DebugCommandData.Insert(item);
            }        
        }
        private void OnDisable()
        {
            foreach (var item in data)
            {
                DebugCommandData.Remove(item);
            }
        }

        private static void CMDSetScene(string id)
        {
            for (int i = 0; i < data.Count; i++)
            {
                ManagerCoreGame.Instance.SetCurrentScene(id);
            }
        }
        private static void CMDGetScene()
        {
            string[] scenes = ManagerCoreGame.Instance.GetAllScenes();

            for (int i = 0; i < scenes.Length; i++)
            {
                DebugCommandLogger.Log($"Map ID: ".ToYellow() + $"[{scenes[i]}]");
            }
        }
    }
}
