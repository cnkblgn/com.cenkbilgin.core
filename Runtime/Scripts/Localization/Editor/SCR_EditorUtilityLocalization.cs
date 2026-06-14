using Core.Editor;

namespace Core.Localization.Editor
{
    public static class EditorUtilityLocalization
    {
        private const string DEFAULT_PATH = "Assets/LocalizationDatabase.asset";

        private static LocalizationDatabaseConfig database = null;

        internal static LocalizationDatabaseConfig GetDatabase()
        {
            if (database != null)
            {
                return database;
            }

#if UNITY_EDITOR
            LocalizationDatabaseConfig cfg = EditorUtility.FindAssetByType<LocalizationDatabaseConfig>();

            if (cfg == null)
            {
                cfg = EditorUtility.CreateAsset<LocalizationDatabaseConfig>(DEFAULT_PATH);
            }

            database = cfg;
#endif

            return database;
        }
    }
}