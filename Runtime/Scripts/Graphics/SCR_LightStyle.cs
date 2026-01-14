using UnityEngine;

namespace Core.Graphics
{
    public static class LightStyle
    {
        public enum ID
        {
            DEFAULT,
            FLICKER_GENERIC,
            FLICKER_STRONG,
            FLICKER_FLUORESCENT,
            CANDLE_01,
            CANDLE_02,
            PULSE_SLOW,
            PULSE_STRONG,
            STROBE_FAST,
            STROBE_SLOW
        }

        // a = 0; m = 0.5; z = 1; for intensity
        private static readonly string[] data = new string[]
        {
            "z", // DEFAULT
            "mmnmmommommnonmmonqnmmo", // FLICKER_GENERIC
            "omnzzozzoomnonoonqnzmo", // FLICKER_STRONG
            "mmamammmmammamamaaamammma", // FLICKER_FLUORESCENT
            "mmmmmaaaaammmmmaaaaaabcdefgabcdefg", // CANDLE_01
            "mmmaaaabcdefgmmmmaaaammmaamm", // CANDLE_02
            "abcdefghijklmnopqrrqponmlkjihgfedcba", // PULSE_SLOW
            "abcdefghijklmnopqrstuvwxyzyxwvutsrqponmlkjihgfedcba", // PULSE_STRONG
            "mamamamamama", //  STROBE_FAST
            "aaaaaaaazzzzzzzz", // STROBE_SLOW
        };

        public static string Get(ID id) => data[(int)id];
        public static float Calculate(ID id, float rate)
        {
            if (id == ID.DEFAULT)
            {
                return 1;
            }

            const float f = 25;
            const char a = 'a';

            string style = Get(id);

            int index = (int)(Time.time * rate) % style.Length;

            float brightness = (style[index] - a) / f;

            return brightness;
        }
    }
}