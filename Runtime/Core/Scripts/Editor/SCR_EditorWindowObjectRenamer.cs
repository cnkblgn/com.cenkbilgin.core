using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Core.Editor
{
    internal sealed class EditorWindowObjectRenamer : EditorWindow
    {
        private enum CaseMode 
        {
            None, 
            PascalCase,
            camelCase, 
            UPPERCASE, 
            lowercase, 
            snake_case 
        }

        private string prefix = "";
        private string suffix = "";
        private string removeBefore = "";
        private string removeAfter = "";
        private string removeWord = "";
        private string findWord = "";
        private string replaceWith = "";
        private string previewName = "";
        private string numberSeparator = "_";
        private int startIndex = 1;
        private int paddingDigits = 2;
        private bool separateNumbers = false;
        private bool removeNumbers = false;
        private bool sequenceNumbering = false;


        private static readonly Regex WordSplitRegex = new(@"[A-Z]+(?=[A-Z][a-z])|[A-Z]?[a-z]+|[A-Z]+|[0-9]+", RegexOptions.Compiled);

        private CaseMode caseMode = CaseMode.None;

        [MenuItem("Tools/Object Renamer")]
        public static void ShowWindow() => GetWindow<EditorWindowObjectRenamer>("Object Renamer");

        private void OnGUI()
        {
            GUILayout.Label("Apply Name to Selected Assets or GameObjects", EditorStyles.boldLabel);

            prefix = EditorGUILayout.TextField("Prefix", prefix);
            suffix = EditorGUILayout.TextField("Suffix", suffix);
            removeBefore = EditorGUILayout.TextField("Remove Before", removeBefore);
            removeAfter = EditorGUILayout.TextField("Remove After", removeAfter);
            removeWord = EditorGUILayout.TextField("Remove Word", removeWord);
            removeNumbers = EditorGUILayout.Toggle("Remove Numbers", removeNumbers);

            if (!removeNumbers)
            {
                separateNumbers = EditorGUILayout.Toggle("Separate Numbers (_)", separateNumbers);
            }

            EditorGUILayout.Space();
            GUILayout.Label("Find & Replace", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Find", GUILayout.Width(35));
            findWord = EditorGUILayout.TextField(findWord);
            EditorGUILayout.LabelField("Replace", GUILayout.Width(50));
            replaceWith = EditorGUILayout.TextField(replaceWith);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            caseMode = (CaseMode)EditorGUILayout.EnumPopup("Case", caseMode);

            EditorGUILayout.Space();
            sequenceNumbering = EditorGUILayout.Toggle("Sequence Numbering", sequenceNumbering);

            if (sequenceNumbering)
            {
                EditorGUI.indentLevel++;
                startIndex = EditorGUILayout.IntField("Start Index", startIndex);
                paddingDigits = Mathf.Max(1, EditorGUILayout.IntField("Padding Digits", paddingDigits));
                numberSeparator = EditorGUILayout.TextField("Separator", numberSeparator);
                EditorGUI.indentLevel--;
            }

            UpdatePreview();

            EditorGUILayout.LabelField("Preview", previewName, EditorStyles.helpBox);

            if (GUILayout.Button("Rename Selected"))
            {
                Rename();
            }
        }
        private void UpdatePreview()
        {
            if (Selection.objects.Length == 0)
            {
                previewName = "(No selection)";
                return;
            }

            string name = ApplyName(Selection.objects[0].name);

            if (sequenceNumbering)
            {
                name = $"{name}{numberSeparator}{startIndex.ToString($"D{paddingDigits}")}";
            }

            previewName = name;
        }

        private void Rename()
        {
            Object[] selectedObjects = Selection.objects;

            if (selectedObjects.Length == 0)
            {
                Debug.LogWarning("No objects selected!");
                return;
            }

            for (int i = 0; i < selectedObjects.Length; i++)
            {
                Object obj = selectedObjects[i];
                string name = ApplyName(obj.name);
                string path = AssetDatabase.GetAssetPath(obj);

                if (sequenceNumbering)
                {
                    int number = startIndex + i;
                    name = $"{name}{numberSeparator}{number.ToString($"D{paddingDigits}")}";
                }

                if (!string.IsNullOrEmpty(path))
                {
                    AssetDatabase.RenameAsset(path, name);
                }
                else
                {
                    Undo.RecordObject(obj, "Rename Object");
                    obj.name = name;
                }
            }

            AssetDatabase.SaveAssets();
            Debug.LogWarning("Selected objects renamed!");
        }

        private static string[] SplitWords(string input, bool removeNumbers)
        {
            var matches = WordSplitRegex.Matches(input);
            var words = new System.Collections.Generic.List<string>();

            foreach (Match m in matches)
            {
                if (removeNumbers && m.Value.All(char.IsDigit))
                {
                    continue;
                }

                words.Add(m.Value);
            }

            return words.ToArray();
        }
        private static string JoinPlain(string[] words, bool separateNumbers, System.Func<string, string> transform)
        {
            StringBuilder sb = new();

            for (int i = 0; i < words.Length; i++)
            {
                string w = words[i];
                bool isNumber = char.IsDigit(w[0]);

                if (i > 0)
                {
                    bool prevIsNumber = char.IsDigit(words[i - 1][^1]);

                    if (separateNumbers && (isNumber != prevIsNumber))
                    {
                        sb.Append('_');
                    }
                }

                sb.Append(transform(w));
            }

            return sb.ToString();
        }
        private string ApplyName(string original)
        {
            string name = original;

            if (!string.IsNullOrEmpty(removeBefore))
            {
                int index = name.IndexOf(removeBefore);

                if (index >= 0)
                {
                    name = name[(index + removeBefore.Length)..];
                }
            }

            if (!string.IsNullOrEmpty(removeAfter))
            {
                int index = name.IndexOf(removeAfter);

                if (index >= 0)
                {
                    name = name[..index];
                }
            }

            if (!string.IsNullOrEmpty(removeWord))
            {
                name = name.Replace(removeWord, "");
            }

            if (!string.IsNullOrEmpty(findWord))
            {
                name = name.Replace(findWord, replaceWith ?? "");
            }

            name = ApplyCase(name, caseMode, removeNumbers, separateNumbers);
            name = name.Replace(" ", "");

            if (!string.IsNullOrEmpty(prefix))
            {
                name = prefix + name;
            }

            if (!string.IsNullOrEmpty(suffix))
            {
                name += suffix;
            }

            return string.IsNullOrEmpty(name) ? original : name;
        }
        private static string ApplyCase(string input, CaseMode mode, bool removeNumbers, bool separateNumbers)
        {
            string[] words = SplitWords(input, removeNumbers);

            if (words.Length == 0)
            {
                return input;
            }

            switch (mode)
            {
                case CaseMode.None:
                    return JoinPlain(words, separateNumbers, w => w);

                case CaseMode.UPPERCASE:
                    return JoinPlain(words, separateNumbers, w => w.ToUpperInvariant());

                case CaseMode.lowercase:
                    return JoinPlain(words, separateNumbers, w => w.ToLowerInvariant());

                case CaseMode.snake_case:
                    return string.Join("_", words.Select(w => w.ToLowerInvariant()));

                case CaseMode.PascalCase:
                    {
                        StringBuilder sb = new();

                        for (int i = 0; i < words.Length; i++)
                        {
                            string w = words[i];
                            bool isNumber = char.IsDigit(w[0]);

                            if (i > 0)
                            {
                                bool prevIsNumber = char.IsDigit(words[i - 1][^1]);

                                if (separateNumbers && (isNumber != prevIsNumber))
                                {
                                    sb.Append('_');
                                }
                            }

                            sb.Append(isNumber ? w : char.ToUpperInvariant(w[0]) + w[1..].ToLowerInvariant());
                        }

                        return sb.ToString();
                    }

                case CaseMode.camelCase:
                    {
                        StringBuilder sb = new();

                        for (int i = 0; i < words.Length; i++)
                        {
                            string w = words[i];
                            bool isNumber = char.IsDigit(w[0]);

                            if (i > 0)
                            {
                                bool prevIsNumber = char.IsDigit(words[i - 1][^1]);

                                if (separateNumbers && (isNumber != prevIsNumber))
                                {
                                    sb.Append('_');
                                }
                            }

                            if (isNumber)
                            {
                                sb.Append(w);
                            }
                            else if (i == 0)
                            {
                                sb.Append(w.ToLowerInvariant());
                            }
                            else
                            {
                                sb.Append(char.ToUpperInvariant(w[0])).Append(w[1..].ToLowerInvariant());
                            }
                        }

                        return sb.ToString();
                    }

                default:
                    return input;
            }
        }
    }
}