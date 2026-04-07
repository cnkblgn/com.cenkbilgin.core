using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Core.Editor;

namespace Core.Localization
{
    public static class LocalizationUtility
    {
        private const string DEFAULT_PATH = "Assets/LocalizationDatabase.asset";

        private static LocalizationDatabaseConfig database = null;

        public static LocalizationDatabaseConfig GetDatabase()
        {
            if (database != null)
            {
                return database;
            }

            LocalizationDatabaseConfig cfg = EditorUtility.FindAssetByType<LocalizationDatabaseConfig>();

            if (cfg == null)
            {
                cfg = EditorUtility.CreateAsset<LocalizationDatabaseConfig>(DEFAULT_PATH);
            }

            database = cfg;
            return database;
        }

        public static Dictionary<string, string> Parse(string csvFile, int languageIndex, char separator, out string[] languages, out string[] keys)
        {
            Dictionary<string, string>[] database = ParseAll(csvFile, separator, out languages, out keys);

            return database[languageIndex];        
        }
        public static Dictionary<string, string>[] ParseAll(string csvFile, char separator, out string[] languages, out string[] keys)
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
            int languageCount = languages.Length;

            Dictionary<string, string>[] result = new Dictionary<string, string>[languageCount];

            for (int i = 0; i < languageCount; i++)
            {
                result[i] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            List<string> keyCache = new();

            for (int row = 1; row < lines.Count; row++)
            {
                List<string> cells = Split(lines[row], separator);

                if (cells.Count == 0)
                {
                    continue;
                }

                string key = cells[0].Trim();

                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                keyCache.Add(key);

                for (int lang = 0; lang < languageCount; lang++)
                {
                    int columnIndex = lang + 1;
                    string value = columnIndex < cells.Count ? cells[columnIndex] : string.Empty;

                    result[lang][key] = value;
                }
            }

            keys = keyCache.ToArray();
            return result;
        }
        private static List<string> Split(string line, char seperator)
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
        private static IEnumerable<string> Read(string csvFile)
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
