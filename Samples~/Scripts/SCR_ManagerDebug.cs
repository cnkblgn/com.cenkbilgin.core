using System;
using System.Collections.Generic;
using UnityEngine;
using Core;
using Core.Input;

namespace Game
{
    using static CoreUtility;

    public class ManagerDebug : Manager<ManagerDebug>
    {
        private readonly static List<DebugCommandInstanceBase> data = new()
        {
            new DebugCommandInstance($"get_maps", $"Get Scenes List", $"'get_map'", CMDGetScene),
            new DebugCommandInstance<string>($"set_map", $"Set Scene", $"'set_map <string=id>'", CMDSetScene),
            new DebugCommandInstance($"tcl", $"Toggles Noclip", $"'tcl'", CMDToggleNoclip),
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

        private static void CMDToggleNoclip()
        {
            if (ManagerPlayer.Player == null)
            {
                return;
            }

            if (ManagerPlayer.Player.TryGetComponent(out MovementModuleNoclip noclip))
            {
                noclip.Toggle();
            }
        }

        private static void CMDGetScene()
        {
            string[] scenes = ManagerCoreGame.Instance.GetAllScenes();

            foreach (string scene in scenes)
            {
                DebugCommandLogger.Log($"Map ID: ".ToYellow() + $"[{scene}]");
            }
        }
        private static void CMDSetScene(string id)
        {
            for (int i = 0; i < data.Count; i++)
            {
                ManagerCoreGame.Instance.SetCurrentScene(id);
            }
        }
    }
}
