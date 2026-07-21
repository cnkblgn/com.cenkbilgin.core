using UnityEngine;
using System.Collections.Generic;

namespace Core
{
    using static CoreUtility;
    using static TaskUtility;

    public static class CommandDatabase
    {
        public static readonly string ID = "ID:".ToYellow();
        public static readonly string DESCRIPTION = "Description:".ToYellow();
        public static readonly string FORMAT = "Format:".ToYellow();
        public static readonly string ERROR = "Invalid Command!".ToYellow();
        public static readonly string HELP = "See 'help'".ToYellow();

        public static IReadOnlyList<CommandInstanceBase> Data => data;
        private readonly static List<CommandInstanceBase> data = new()
        {
            new CommandInstance($"help", $"Reveals console commands", $"'help'", CMDHelp),
            new CommandInstance($"clear", $"Clears console commands", $"'clear'", CMDClear),
            new CommandInstance<float>($"set_timescale", $"Sets timescale", $"'set_timescale <float>'", CMDTimescale)
        };

        public static bool TryExecute(string commandInput)
        {
            if (!TryExecuteInternal(commandInput, data))
            {
                CommandLogger.Log(ERROR + $" [{commandInput}] ".ToRed() + HELP);
                return false;
            }

            return true;
        }
        public static bool TryExecute(string commandInput, List<CommandInstanceBase> commandData) => TryExecuteInternal(commandInput, commandData);
        private static bool TryExecuteInternal(string commandInput, List<CommandInstanceBase> commandData)
        {
            string[] properties = commandInput.Split(' ');

            bool isSuccessfull = false;

            for (int i = 0; i < commandData.Count; i++)
            {
                CommandInstanceBase commandInstance = commandData[i];

                if (properties[0].ToLower() != commandInstance.ID)
                {
                    continue;
                }

                if (commandInstance as CommandInstance != null)
                {
                    if (properties.Length != 1)
                    {
                        break;
                    }

                    (commandInstance as CommandInstance).Execute();
                    return true;
                }
                else if (commandInstance as CommandInstance<int> != null)
                {
                    if (properties.Length != 2)
                    {
                        break;
                    }

                    if (int.TryParse(properties[1], out int value))
                    {
                        (commandInstance as CommandInstance<int>).Execute(value);
                    }
                    return true;
                }
                else if (commandInstance as CommandInstance<float> != null)
                {
                    if (properties.Length != 2)
                    {
                        break;
                    }

                    if (float.TryParse(properties[1], out float value))
                    {
                        (commandInstance as CommandInstance<float>).Execute(value);
                    }
                    return true;
                }
                else if (commandInstance as CommandInstance<bool> != null)
                {
                    if (properties.Length != 2)
                    {
                        break;
                    }

                    if (bool.TryParse(properties[1], out bool value))
                    {
                        (commandInstance as CommandInstance<bool>).Execute(value);
                    }
                    return true;
                }
                else if (commandInstance as CommandInstance<string> != null)
                {
                    if (properties.Length != 2)
                    {
                        break;
                    }

                    (commandInstance as CommandInstance<string>).Execute(properties[1]);
                    return true;
                }
                else if (commandInstance as CommandInstance<string, int> != null)
                {
                    if (properties.Length != 3)
                    {
                        break;
                    }

                    (commandInstance as CommandInstance<string, int>).Execute(properties[1], int.Parse(properties[2]));
                    return true;
                }
                else if (commandInstance as CommandInstance<float, float> != null)
                {
                    if (properties.Length != 3)
                    {
                        break;
                    }

                    (commandInstance as CommandInstance<float, float>).Execute(float.Parse(properties[1]), float.Parse(properties[2]));
                    return true;
                }
                else if (commandInstance as CommandInstance<int, float> != null)
                {
                    if (properties.Length != 3)
                    {
                        break;
                    }

                    (commandInstance as CommandInstance<int, float>).Execute(int.Parse(properties[1]), float.Parse(properties[2]));
                    return true;
                }
                else if (commandInstance as CommandInstance<int, int> != null)
                {
                    if (properties.Length != 3)
                    {
                        break;
                    }

                    (commandInstance as CommandInstance<int, int>).Execute(int.Parse(properties[1]), int.Parse(properties[2]));
                    return true;
                }
            }

            return isSuccessfull;
        }

        public static void Insert(CommandInstanceBase commandInstance)
        {
            if (commandInstance == null)
            {
                Debug.LogError("commandInstance == null");
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
        public static void Remove(CommandInstanceBase commandInstance)
        {
            if (commandInstance == null)
            {
                Debug.LogError("commandInstance == null");
                return;
            }

            if (!data.Contains(commandInstance))
            {
                Debug.LogError("!data.Contains(commandInstance)");
                return;
            }

            data.Remove(commandInstance);
        }

        private static void CMDHelp()
        {
            for (int i = 0; i < data.Count; i++)
            {
                CommandLogger.Log($"[{ID} {data[i].ID}] [{DESCRIPTION} {data[i].Description}] [{FORMAT} {data[i].Format}]");
            }
        }
        private static void CMDClear()
        {
            CommandLogger.Clear();
        }
        private static void CMDTimescale(float scale)
        {
            ManagerGame.Instance.WaitUntil(WaitResume, () => ManagerGame.Instance.SetTimeScale(scale));
            CommandLogger.Log($"Current Timescale: {scale.ToString().ToYellow()}");
        }
    }
}