using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;

namespace Core.Localization
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public class ManagerCoreLocalization : Manager<ManagerCoreLocalization>
    {
        public string[] Languages => languages; private string[] languages = null;

        [Header("_")]
        [SerializeField, Required] private TextAsset database = null;

        private Dictionary<string, string> currentLocalizationData = null;
        private int currentLocalizationIndex = -1;       
        private bool isInitialized = false;

        private void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            int target = currentLocalizationIndex;

            if (target == -1)
            {
                LogError("ManagerCoreLocalization.Initialize() currentLocalizationIndex == -1, setting force to 0");
                target = 0;
            }

            Set(target);
            isInitialized = true;
        }
        public void Set(int localizationIndex)
        {
            if (database == null)
            {
                LogError("ManagerCoreLocalization.Set() database == null!");
                return;
            }

            if (currentLocalizationIndex == localizationIndex)
            {
                return;
            }

            currentLocalizationData = Parse(database.text, currentLocalizationIndex = localizationIndex);
        }
        public string[] Get()
        {
            if (!isInitialized)
            {
                Initialize();
            }

            return languages;
        }
        public string Get(string key)
        {
            if (!isInitialized)
            {
                Initialize();
            }

            if (currentLocalizationData.TryGetValue(key, out var format))
            {
                if (!string.IsNullOrEmpty(format))
                {
                    return format;
                }
            }

            LogWarning($"Missing localization for '{key}' in '{languages[currentLocalizationIndex]}'");
            return $"[{key}]";
        }
        public string Get(string key, string arg0)
        {
            if (!isInitialized)
            {
                Initialize();
            }

            if (currentLocalizationData.TryGetValue(key, out var format))
            {
                if (!string.IsNullOrEmpty(format))
                {
                    return string.Format(format, arg0);
                }
            }

            LogWarning($"Missing localization for '{key}' in '{languages[currentLocalizationIndex]}'");
            return $"[{key}]";
        }
        public string Get(string key, params object[] args)
        {
            if (!isInitialized)
            {
                Initialize();
            }

            if (currentLocalizationData.TryGetValue(key, out var format))
            {
                if (!string.IsNullOrEmpty(format))
                {
                    return args.Length > 0 ? string.Format(format, args) : format;
                }
            }

            LogWarning($"Missing localization for '{key}' in '{languages[currentLocalizationIndex]}'");
            return $"[{key}]";
        }
        private Dictionary<string, string> Parse(string csvFile, int languageIndex, char separator = ',')
        {
            List<string> lines = Read(csvFile).ToList();

            if (lines.Count < 2)
            {
                throw new Exception("lines.Length < 2. CSV must have header + at least one data row.");
            }

            List<string> header = Split(lines[0], separator);
            if (header.Count < 2 || header[0] != "key")
            {
                throw new Exception("First header cell must be 'key' and at least one language column.");
            }

            languages = header.Skip(1).ToArray();
            languageIndex = Mathf.Clamp(languageIndex, 0, languages.Length - 1);
            int columnIndex = languageIndex + 1;

            Dictionary<string, string> languageData = new(StringComparer.OrdinalIgnoreCase);

            for (int rowIndex = 1; rowIndex < lines.Count; rowIndex++)
            {
                List<string> cells = Split(lines[rowIndex], separator);
                if (cells.Count == 0)
                {
                    continue;
                }

                string key = cells[0].Trim();
                if (key.Length == 0)
                {
                    continue;
                }

                string value = columnIndex < cells.Count ? cells[columnIndex] : string.Empty;
                languageData[key] = value;
            }

            return languageData;
        }
        private List<string> Split(string line, char seperator)
        {
            List<string> stringCells = new();
            StringBuilder stringBuilder = new();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        stringBuilder.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == seperator && !inQuotes)
                {
                    stringCells.Add(stringBuilder.ToString());
                    stringBuilder.Clear();
                }
                else
                {
                    stringBuilder.Append(c);
                }
            }

            stringCells.Add(stringBuilder.ToString());
            return stringCells;
        }
        private IEnumerable<string> Read(string csvFile)
        {
            string[] lines = csvFile.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            StringBuilder stringBuilder = new();
            bool inQuotes = false;

            foreach (var raw in lines)
            {
                stringBuilder.Append(raw);

                int quoteCount = raw.Count(c => c == '"');

                if (quoteCount % 2 == 1)
                {
                    inQuotes = !inQuotes;
                }

                if (!inQuotes)
                {
                    yield return stringBuilder.ToString();
                    stringBuilder.Clear();
                }
                else
                {
                    stringBuilder.Append("\n");
                }
            }

            if (stringBuilder.Length > 0)
            {
                yield return stringBuilder.ToString();
            }
        }
    }
}