namespace Core.Editor
{
    internal static class EditorUtilityPrefab
    {
        private const string DEFAULT_PATH = "Assets/PrefabDatabase.asset";

        private static PrefabDatabaseConfig database = null;

        internal static PrefabDatabaseConfig GetDatabase()
        {
            if (database != null)
            {
                return database;
            }

#if UNITY_EDITOR
            PrefabDatabaseConfig cfg = EditorUtility.FindAssetByType<PrefabDatabaseConfig>();

            if (cfg == null)
            {
                cfg = EditorUtility.CreateAsset<PrefabDatabaseConfig>(DEFAULT_PATH);
            }

            database = cfg;
#endif

            return database;
        }
    }
}
