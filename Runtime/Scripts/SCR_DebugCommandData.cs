using UnityEngine;
using System.Collections.Generic;

namespace Core
{
    using static CoreUtility;

    public static class DebugCommandData
    {
        public static readonly string ID = "ID:".ToYellow();
        public static readonly string DESCRIPTION = "Description:".ToYellow();
        public static readonly string FORMAT = "Format:".ToYellow();
        public static readonly string ERROR = "Invalid Command!".ToYellow();
        public static readonly string HELP = "See 'help'".ToYellow();

        public static IReadOnlyList<DebugCommandInstanceBase> Data => data;
        private readonly static List<DebugCommandInstanceBase> data = new()
        {
            new DebugCommandInstance($"help", $"Reveals console commands", $"'help'", CMDHelp),
            new DebugCommandInstance($"clear", $"Clears console commands", $"'clear'", CMDClear),
            new DebugCommandInstance<float, float>($"set_timescale", $"Sets timescale", $"'set_timescale <float = scale, float = duration>'", CMDTimescale)
        };

        public static bool Execute(string commandInput)
        {
            if (!ExecuteInternal(commandInput, data))
            {
                DebugCommandLogger.Log(ERROR + $" [{commandInput}] ".ToRed() + HELP);
                return false;
            }

            return true;
        }
        public static bool Execute(string commandInput, List<DebugCommandInstanceBase> commandData) => ExecuteInternal(commandInput, commandData);
        private static bool ExecuteInternal(string commandInput, List<DebugCommandInstanceBase> commandData)
        {
            string[] properties = commandInput.Split(' ');

            bool isSuccessfull = false;

            for (int i = 0; i < commandData.Count; i++)
            {
                DebugCommandInstanceBase commandInstance = commandData[i];

                if (properties[0].ToLower() != commandInstance.ID)
                {
                    continue;
                }

                if (commandInstance as DebugCommandInstance != null)
                {
                    if (properties.Length != 1)
                    {
                        break;
                    }

                    (commandInstance as DebugCommandInstance).Execute();
                    return true;
                }
                else if (commandInstance as DebugCommandInstance<int> != null)
                {
                    if (properties.Length != 2)
                    {
                        break;
                    }

                    if (int.TryParse(properties[1], out int value))
                    {
                        (commandInstance as DebugCommandInstance<int>).Execute(value);
                    }
                    return true;
                }
                else if (commandInstance as DebugCommandInstance<float> != null)
                {
                    if (properties.Length != 2)
                    {
                        break;
                    }

                    if (float.TryParse(properties[1], out float value))
                    {
                        (commandInstance as DebugCommandInstance<float>).Execute(value);
                    }
                    return true;
                }
                else if (commandInstance as DebugCommandInstance<bool> != null)
                {
                    if (properties.Length != 2)
                    {
                        break;
                    }

                    if (bool.TryParse(properties[1], out bool value))
                    {
                        (commandInstance as DebugCommandInstance<bool>).Execute(value);
                    }
                    return true;
                }
                else if (commandInstance as DebugCommandInstance<string> != null)
                {
                    if (properties.Length != 2)
                    {
                        break;
                    }

                    (commandInstance as DebugCommandInstance<string>).Execute(properties[1]);
                    return true;
                }
                else if (commandInstance as DebugCommandInstance<string, int> != null)
                {
                    if (properties.Length != 3)
                    {
                        break;
                    }

                    (commandInstance as DebugCommandInstance<string, int>).Execute(properties[1], int.Parse(properties[2]));
                    return true;
                }
                else if (commandInstance as DebugCommandInstance<float, float> != null)
                {
                    if (properties.Length != 3)
                    {
                        break;
                    }

                    (commandInstance as DebugCommandInstance<float, float>).Execute(float.Parse(properties[1]), float.Parse(properties[2]));
                    return true;
                }
            }

            return isSuccessfull;
        }
        public static void Insert(DebugCommandInstanceBase commandInstance)
        {
            if (commandInstance == null)
            {
                Debug.LogError("DebugCommandData.Insert() commandInstance == null");
                return;
            }

            for (int i = 0; i < data.Count; i++)
            {
                if (string.Equals(data[i].ID, commandInstance.ID))
                {
                    return;
                }
            }

            data.Add(commandInstance);
        }
        public static void Remove(DebugCommandInstanceBase commandInstance)
        {
            if (commandInstance == null)
            {
                Debug.LogError("DebugCommandData.Remove() commandInstance == null");
                return;
            }

            if (!data.Contains(commandInstance))
            {
                Debug.LogError("DebugCommandData.Remove() !data.Contains(commandInstance)");
                return;
            }

            data.Remove(commandInstance);
        }

        private static void CMDHelp()
        {
            for (int i = 0; i < data.Count; i++)
            {
                DebugCommandLogger.Log($"[{ID} {data[i].ID}] [{DESCRIPTION} {data[i].Description}] [{FORMAT} {data[i].Format}]");
            }
        }
        private static void CMDClear()
        {
            DebugCommandLogger.Clear();
        }
        private static void CMDTimescale(float scale, float duration)
        {
            ManagerCoreGame.Instance.WaitUntil(() => ManagerCoreGame.Instance.GetGameState() == GameState.RESUME, null, () => ManagerCoreGame.Instance.SetTimeScale(scale, duration));

            DebugCommandLogger.Log($"Current Timescale: {scale.ToString().ToYellow()}");
        }
    }
}