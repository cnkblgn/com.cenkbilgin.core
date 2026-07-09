using UnityEditor;
using UnityEditor.Callbacks;
using Core.Actor;
using Core.Audio;
using Core.Localization;

namespace Core.Asset.Editor
{
    using static CoreUtility;

    internal static class EditorLoader
    {
        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            if (TryFindAssetByType(out PrefabDatabaseConfig prefab))
            {
                prefab.Build();
            }

            if (TryFindAssetByType(out LocalizationDatabaseConfig localization))
            {
                localization.Build();
            }

            if (TryFindAssetByType(out SoundDatabaseConfig sound))
            {
                sound.Build();
            }

            if (TryFindAssetByType(out ActorDatabaseConfig actor))
            {
                actor.Build();
            }
        }
    }
}