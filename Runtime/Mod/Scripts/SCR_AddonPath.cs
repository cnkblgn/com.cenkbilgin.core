using System.IO;
using UnityEngine;

namespace Core.Mod
{
    public static class AddonPath
    {
        private const string PATH_EDITOR = "ModTemplates";
        private const string PATH_BUILD = "Mods";

        public static string GetPath()
        {
#if UNITY_EDITOR
            return Path.Combine(Application.dataPath, PATH_EDITOR);
#else
            return Path.Combine(Application.dataPath, "..", BuildRoot);
#endif
        }
    }
}
