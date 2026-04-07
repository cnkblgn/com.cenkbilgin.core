using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Localization
{
    using static CoreUtility;

    [CreateAssetMenu(fileName = "SCO_LocalizationDatabase", menuName = "Resources/Localization Database Config", order = 0)]
    public class LocalizationDatabaseConfig : ScriptableObject
    {
        public string[] Keys => keys;
        public string[] Languages => languages;
        public bool IsParsed => database != null;

        [Header("_")]
        [SerializeField, Required] private TextAsset file = null;
        [SerializeField, HideInInspector] private string[] languages = Array.Empty<string>();
        [SerializeField, HideInInspector] private string[] keys = Array.Empty<string>();
        [SerializeField, HideInInspector] private bool isParsed = false;

        [NonSerialized] private Dictionary<string, string>[] database = null;
         
        public Dictionary<string, string> GetLanguage(int index)
        {
            if (!IsParsed)
            {
                TryParse();
            }

            if (!IsParsed)
            {
                throw new InvalidOperationException($"LocalizationDatabaseConfig.GetLanguage() database is not parsed!");
            }

            if (index < 0 || index >= database.Length)
            {
                throw new ArgumentOutOfRangeException($"LocalizationDatabaseConfig.GetLanguage() invalid access [{index}]");
            }

            return database[index];
        }
        public bool TryParse()
        { 
            if (file == null)
            {
                Debug.LogError("LocalizationDatabase.TryParse() file == null");
                return false;
            }

            try
            {
                database = LocalizationUtility.ParseAll(file.text, ',', out languages, out keys);
                isParsed = true;

                Debug.Log($"LocalizationDatabase.TryParse() parse successfull");
            }
            catch (Exception e)
            {
                database = null;
                keys = Array.Empty<string>();
                languages = Array.Empty<string>();
                isParsed = false;

                Debug.LogError($"LocalizationDatabase.TryParse() {e.Message}", this);
            }

            return isParsed;
        }
    }
}
