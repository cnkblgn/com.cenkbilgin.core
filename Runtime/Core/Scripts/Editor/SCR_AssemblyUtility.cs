using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Core.Editor
{
    public static class AssemblyUtility
    {
        private const string PATH = "Assets/AssemblyGraph.dot"; // proje kök dizinine yazýlýr

        [MenuItem("Tools/Generate Assembly Graph", false, -10)]
        private static void GenerateGraph()
        {
            string[] guids = AssetDatabase.FindAssets("t:asmdef");

            Dictionary<string, AsmdefData> asmdefsByGuid = new();
            Dictionary<string, string> nameToGuid = new();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (!IsProjectAssembly(path))
                {
                    continue;
                }

                string json = File.ReadAllText(path);

                AsmdefData data = JsonUtility.FromJson<AsmdefData>(json);

                if (data == null || string.IsNullOrEmpty(data.name))
                {
                    continue;
                }

                data.guid = guid;
                data.path = path;

                asmdefsByGuid[guid] = data;
                nameToGuid[data.name] = guid;
            }

            Dictionary<string, string> guidToName = new();
            foreach (var kvp in asmdefsByGuid)
            {
                guidToName[kvp.Key] = kvp.Value.name;
            }

            StringBuilder sb = new();
            sb.AppendLine("digraph Assemblies");
            sb.AppendLine("{");
            sb.AppendLine("  rankdir=LR;");
            sb.AppendLine("  node [shape=box, style=filled, fillcolor=\"#cfe8ff\"];");

            foreach (var kvp in asmdefsByGuid)
            {
                AsmdefData data = kvp.Value;
                string fromName = SanitizeName(data.name);

                if (data.references == null || data.references.Length == 0)
                {
                    sb.AppendLine($"  \"{fromName}\";");
                    continue;
                }

                foreach (string reference in data.references)
                {
                    string toName = ResolveName(reference, guidToName);
                    sb.AppendLine($"  \"{fromName}\" -> \"{SanitizeName(toName)}\";");
                }
            }

            sb.AppendLine("}");

            string fullPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, PATH);
            File.WriteAllText(fullPath, sb.ToString());

            string svgPath = Path.ChangeExtension(fullPath, ".svg");
            TryExportSvg(fullPath, svgPath);

            Debug.Log($"Assembly graph created at: {fullPath}. Visualize with https://dreampuf.github.io/GraphvizOnline/");
        }
        private static string ResolveName(string reference, Dictionary<string, string> guidToName)
        {
            const string PREFIX = "GUID:";

            if (reference.StartsWith(PREFIX, StringComparison.Ordinal))
            {
                string guid = reference[PREFIX.Length..];
                return guidToName.TryGetValue(guid, out string name) ? name : $"Unknown_{guid}";
            }

            if (reference.StartsWith("Unity", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return reference;
        }
        private static string SanitizeName(string s) => s.Replace("\"", "'");
        private static bool IsProjectAssembly(string assetPath) => assetPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase);

        private static bool TryExportSvg(string dotPath, string svgPath)
        {
            // PATH'te ararken kullanýlacak olasý dot çalýþtýrýlabilir adlarý
            string dotExe = Application.platform == RuntimePlatform.WindowsEditor ? "dot.exe" : "dot";

            try
            {
                var info = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = dotExe,
                    Arguments = $"-Tsvg \"{dotPath}\" -o \"{svgPath}\"",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using var process = System.Diagnostics.Process.Start(info);
                process.WaitForExit(10000);

                if (process.ExitCode == 0 && File.Exists(svgPath))
                {
                    return true;
                }

                string error = process.StandardError.ReadToEnd();
                Debug.LogWarning($"Assembly graph .svg creation failed: {error}");
                return false;
            }
            catch (System.ComponentModel.Win32Exception)
            {
                Debug.LogWarning("Assembly graph .svg creation failed: GraphViz not found in system! please download from: https://graphviz.org/download/");
                return false;
            }
        }

        [Serializable]
        private class AsmdefData
        {
            public string name;
            public string[] references;

            [NonSerialized] public string guid;
            [NonSerialized] public string path;
        }
    }
}
