using System.Collections.Generic;

namespace Core.Input
{
    internal static class InputIcons
    {
        public static readonly Dictionary<string, int> Database = new()
        {
            // mouse
            { "leftButton", 1 },
            { "rightButton", 2 },
            { "middleButton", 3 },
            { "scroll", 3 },

            // keyboard
            { "space", 45 },
            { "enter", 38 },
            { "escape", 39 },
            { "leftShift", 44 },
            { "leftCtrl", 35 },
            { "leftAlt", 31 },
            { "r", 64 },
            { "f", 52 },
            { "g", 53 },
            { "q", 63 },
            { "e", 51 },
            { "v", 68 },
            { "b", 48 },
            { "tab", 46 },
            { "upArrow", 8 },
            { "downArrow", 5 },
            { "leftArrow", 6 },
            { "rightArrow", 7 },

            // gamepad
            { "buttonSouth", -1 },
            { "buttonEast", -1 },
            { "buttonWest", -1 },
            { "buttonNorth", -1 },
            { "leftStick", -1 },
            { "rightStick", -1 },
            { "leftShoulder", -1 },
            { "rightShoulder", -1 },
        };
    }
}