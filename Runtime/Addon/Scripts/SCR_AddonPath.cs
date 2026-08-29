using System.IO;
using UnityEngine;

namespace Core.Addon
{
    public static class AddonPath
    {
        private const string PATH_EDITOR = "AddonTemplates";
        private const string PATH_BUILD = "Addons";

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
