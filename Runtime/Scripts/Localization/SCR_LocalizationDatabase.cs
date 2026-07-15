using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Core.Localization
{
    public static class LocalizationDatabase
    {
        public static event Action<int> OnLocalizationChanged = null;
        internal static bool IsParsed => database != null;

        private static string[] languages = Array.Empty<string>();
        private static string[] keys = Array.Empty<string>();
        private static Dictionary<string, string>[] database = null;
        private static readonly Dictionary<string, LocalizationInterpolator> interpolators = new(StringComparer.OrdinalIgnoreCase);

        private static Dictionary<string, string> currentLanguageData = null;
        private static int currentLanguageIndex = 0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnRuntimeInitialize() => OnLocalizationChanged = null;

        public static string GetString(string key)
        {
            TryGetString(key, out string value);

            return value;
        }
        public static string GetString(string key, string arg0)
        {
            if (TryGetString(key, out string value))
            {
                if (!string.IsNullOrEmpty(value))
                {
                    return string.Format(value, arg0);
                }
            }

            return value;
        }
        public static string GetString(string key, string arg0, string arg1)
        {
            if (TryGetString(key, out string value))
            {
                if (!string.IsNullOrEmpty(value))
                {
                    return string.Format(value, arg0, arg1);
                }
            }

            return value;
        }
        public static string GetString(string key, params object[] args)
        {
            if (TryGetString(key, out string value))
            {
                if (!string.IsNullOrEmpty(value))
                {
                    return args.Length > 0 ? string.Format(value, args) : value;
                }
            }

            return value;
        }

        internal static string GetStringInterpolated(string key) => Interpolate(GetString(key));
        internal static string GetStringInterpolated(string key, string arg0) => Interpolate(GetString(key, arg0));
        internal static string GetStringInterpolated(string key, string arg0, string arg1) => Interpolate(GetString(key, arg0, arg1));
        internal static string GetStringInterpolated(string key, params object[] args) => Interpolate(GetString(key, args));

        private static bool TryGetString(string key, out string value)
        {
            if (!IsParsed)
            {
                throw new InvalidOperationException($"Localization database is not parsed!");
            }

            if (currentLanguageData.TryGetValue(key, out value))
            {
                return true;
            }

            Debug.LogWarning($"Missing localization for '{key}' in '{GetLanguage()}'");
            value = $"[{key}]";
            return false;
        }

        internal static void Build(TextAsset file, IEnumerable<LocalizationInterpolator> values)
        {
            BuildText(file);
            BuildInterpolators(values);
        }
        private static void BuildText(TextAsset file)
        {
            if (file == null)
            {
                Debug.LogError("Localization parse failed! file == null");
                return;
            }

            try
            {
                database = ParseAll(file.text, ',', out languages, out keys);

                currentLanguageIndex = 0;
                currentLanguageData = GetLanguage(currentLanguageIndex);

                Debug.Log($"Localization parse successfull!");
            }
            catch (Exception e)
            {
                database = null;
                keys = Array.Empty<string>();
                languages = Array.Empty<string>();

                Debug.LogError($"Localization parse failed: {e.Message}");
            }
        }
        private static void BuildInterpolators(IEnumerable<LocalizationInterpolator> values)
        {
            interpolators.Clear();

            if (values == null)
            {
                return;
            }

            foreach (LocalizationInterpolator interpolator in values)
            {
                if (interpolator == null || string.IsNullOrEmpty(interpolator.Key))
                {
                    continue;
                }

                if (!interpolators.TryAdd(interpolator.Key, interpolator))
                {
                    throw new InvalidOperationException($"Duplicate localization interpolator key '{interpolator.Key}'.");
                }
            }
        }

        internal static string Interpolate(string text)
        {
            int tokenStart = text.IndexOf('{');

            if (tokenStart < 0)
            {
                return text;
            }

            StringBuilder builder = new(text.Length + 32);
            int position = 0;

            while (tokenStart >= 0)
            {
                int tokenEnd = text.IndexOf('}', tokenStart + 1);

                if (tokenEnd < 0)
                {
                    break;
                }

                int separator = text.IndexOf(':', tokenStart + 1, tokenEnd - tokenStart - 1);

                if (separator < 0)
                {
                    tokenStart = text.IndexOf('{', tokenStart + 1);
                    continue;
                }

                int keyStart = tokenStart + 1;
                int keyLength = separator - keyStart;

                builder.Append(text, position, tokenStart - position);

                if (!TryInterpolate(text, keyStart, keyLength, separator + 1, tokenEnd - separator - 1, builder))
                {
                    builder.Append(text, tokenStart, tokenEnd - tokenStart + 1);
                }

                position = tokenEnd + 1;
                tokenStart = text.IndexOf('{', position);
            }

            if (position == 0)
            {
                return text;
            }

            builder.Append(text, position, text.Length - position);
            return builder.ToString();
        }
        private static bool TryInterpolate(string text, int keyStart, int keyLength, int valueStart, int valueLength, StringBuilder builder)
        {
            foreach (KeyValuePair<string, LocalizationInterpolator> pair in interpolators)
            {
                string key = pair.Key;

                if (key.Length != keyLength)
                {
                    continue;
                }

                if (string.Compare(text, keyStart, key, 0, keyLength, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    continue;
                }

                return pair.Value.TryInterpolate(text, valueStart, valueLength, builder);
            }

            return false;
        }

        internal static Dictionary<string, string> Parse(string csvFile, int languageIndex, char separator, out string[] languages, out string[] keys)
        {
            Dictionary<string, string>[] database = ParseAll(csvFile, separator, out languages, out keys);

            return database[languageIndex];        
        }
        internal static Dictionary<string, string>[] ParseAll(string csvFile, char separator, out string[] languages, out string[] keys)
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

        public static IReadOnlyList<string> GetKeys() => keys;
        public static IReadOnlyList<string> GetLanguages() => languages;
        public static string GetLanguage() => GetLanguages()[currentLanguageIndex];
        public static void SetLanguage(int index)
        {
            if (currentLanguageIndex == index || index < 0 || index >= GetLanguages().Count)
            {
                return;
            }

            currentLanguageIndex = index;
            currentLanguageData = GetLanguage(currentLanguageIndex);
            OnLocalizationChanged?.Invoke(currentLanguageIndex);
        }
        public static Dictionary<string, string> GetLanguage(int index)
        {
            if (!IsParsed)
            {
                throw new InvalidOperationException($"Localization database is not parsed!");
            }

            if (index < 0 || index >= database.Length)
            {
                throw new ArgumentOutOfRangeException($"Invalid access [{index}]");
            }

            return database[index];
        }
    }
}
