using System.Collections.Generic;
using UnityEngine;

namespace Core.Localization
{
    using static CoreUtility;

    [CreateAssetMenu(fileName = "SCO_LocalizationDatabase", menuName = "Resources/Localization Database Config", order = 0)]
    public class LocalizationDatabaseConfig : ScriptableObject
    {
        [Header("_")]
        [SerializeField, Required] private TextAsset file = null;

        public string[] GetKeys()
        {
            if (!LocalizationDatabase.GetIsParsed())
            {
                TryParse();
            }

            return LocalizationDatabase.GetKeys();
        }
        public string[] GetLanguages()
        {
            if (!LocalizationDatabase.GetIsParsed())
            {
                TryParse();
            }

            return LocalizationDatabase.GetLanguages();
        }
        public Dictionary<string, string> GetLanguage(int index)
        {
            if (!LocalizationDatabase.GetIsParsed())
            {
                TryParse();
            }

            return LocalizationDatabase.GetLanguage(index);
        }

        [Clickable("Parse")]
        public bool TryParse() => LocalizationDatabase.TryParse(file);
    }
}
