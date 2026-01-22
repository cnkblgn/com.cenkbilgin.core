using UnityEngine;

namespace Core
{
    public static class EaseEvaluater
    {
        public static float Evaluate(EaseType type, float time)
        {
            switch (type)
            {
                case EaseType.LINEAR:
                    return time;
                case EaseType.EASE_IN_SINE:
                    return 1 - Mathf.Cos((time * Mathf.PI) / 2);
                case EaseType.EASE_OUT_SINE:
                    return Mathf.Sin((time * Mathf.PI) / 2);
                case EaseType.EASE_IN_OUT_SINE:
                    return -(Mathf.Cos(Mathf.PI * time) - 1) / 2;
                case EaseType.EASE_IN_QUART:
                    return time * time * time * time;
                case EaseType.EASE_OUT_QUART:
                    return 1 - Mathf.Pow(1 - time, 4);
                case EaseType.EASE_IN_OUT_QUART:
                    return time < 0.5f ? 8 * time * time * time * time : 1 - Mathf.Pow(-2 * time + 2, 4) / 2;
                case EaseType.EASE_OUT_ELASTIC:
                    if (time == 0) return 0;
                    if (time == 1) return 1;
                    {
                        float c4 = (2 * Mathf.PI) / 3;
                        return Mathf.Pow(2, -10 * time) * Mathf.Sin((time * 10 - 0.75f) * c4) + 1;
                    }
                case EaseType.EASE_OUT_BOUNCE:
                    if (time < 1 / 2.75f)
                    {
                        return 7.5625f * time * time;
                    }
                    else if (time < 2 / 2.75f)
                    {
                        time -= 1.5f / 2.75f;
                        return 7.5625f * time * time + 0.75f;
                    }
                    else if (time < 2.5f / 2.75f)
                    {
                        time -= 2.25f / 2.75f;
                        return 7.5625f * time * time + 0.9375f;
                    }
                    else
                    {
                        time -= 2.625f / 2.75f;
                        return 7.5625f * time * time + 0.984375f;
                    }
                default:
                    return time;
            }
        }
    }
}
