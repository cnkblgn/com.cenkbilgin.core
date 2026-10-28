using System.Collections.Generic;

namespace Core.Environment
{
    internal static class EnvironmentLightDatabase
    {
        public static IReadOnlyList<EnvironmentLight> Lights => lights;

        private static readonly List<EnvironmentLight> lights = new(64);

        public static void Register(EnvironmentLight light)
        {
            if (!lights.Contains(light))
            {
                lights.Add(light);
            }
        }
        public static void Unregister(EnvironmentLight light)
        {
            lights.Remove(light);
        }
    }
}
