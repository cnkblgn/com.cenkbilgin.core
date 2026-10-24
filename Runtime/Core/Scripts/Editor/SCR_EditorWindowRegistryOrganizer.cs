using System.Linq;
using UnityEditor;
using UnityEngine;
using Core;

namespace Game.Editor
{
    using Editor = UnityEditor.Editor;

    internal sealed class EditorWindowRegistryOrganizer : EditorWindow
    {
        private Registry[] registries = System.Array.Empty<Registry>();
        private Editor registryEditor;
        private Registry selectedRegistry;
        private Vector2 listScrollPosition;
        private Vector2 inspectorScrollPosition;
        private string searchText = string.Empty;

        private const string NAME = "Registry Organizer";
        private const string PATH = "Tools/" + NAME;

        [MenuItem(PATH)]
        private static void Open()
        {
            EditorWindowRegistryOrganizer window = GetWindow<EditorWindowRegistryOrganizer>();
            window.titleContent = new GUIContent(NAME);
            window.minSize = new Vector2(650f, 350f);
            window.Show();
        }

        private void OnEnable()
        {
            titleContent = new GUIContent(NAME);
            RefreshRegistries();
        }
        private void OnDisable()
        {
            DestroyRegistry();
        }
        private void OnProjectChange()
        {
            RefreshRegistries();
            Repaint();
        }
        private void OnGUI()
        {
            DrawToolbar();

            EditorGUILayout.BeginHorizontal();
            {
                DrawList();
                DrawSeparator();
                DrawInspector();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
            {
                RefreshRegistries();
            }

            if (GUILayout.Button("Reload All", EditorStyles.toolbarButton))
            {
                ReloadRegistries();
            }

            GUILayout.FlexibleSpace();

            searchText = EditorGUILayout.TextField(searchText, EditorStyles.toolbarTextField, GUILayout.Width(200f));

            if (GUILayout.Button("×", EditorStyles.toolbarButton, GUILayout.Width(24f)))
            {
                searchText = string.Empty;
                GUI.FocusControl(null);
            }

            EditorGUILayout.EndHorizontal();
        }
        private void DrawList()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(256f), GUILayout.ExpandHeight(true));
            {
                EditorGUILayout.LabelField($"Registries ({registries.Length})", EditorStyles.boldLabel);

                listScrollPosition = EditorGUILayout.BeginScrollView(listScrollPosition);
                {
                    for (int i = 0; i < registries.Length; i++)
                    {
                        Registry registry = registries[i];

                        if (registry == null)
                        {
                            continue;
                        }

                        if (!string.IsNullOrEmpty(searchText) && !registry.name.Contains( searchText, System.StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        DrawRegistry(registry);
                    }
                }
                EditorGUILayout.EndScrollView();
            }           
            EditorGUILayout.EndVertical();
        }
        private void DrawSeparator()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, GUILayout.Width(1f), GUILayout.ExpandHeight(true));

            EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? new Color(0.12f, 0.12f, 0.12f) : new Color(0.65f, 0.65f, 0.65f));
        }
        private void DrawInspector()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            {
                if (selectedRegistry == null)
                {
                    EditorGUILayout.HelpBox("Select a registry from the list.", MessageType.Info);
                    EditorGUILayout.EndVertical();
                    return;
                }

                DrawHeader();

                inspectorScrollPosition = EditorGUILayout.BeginScrollView(inspectorScrollPosition);
                {
                    if (registryEditor != null)
                    {
                        registryEditor.OnInspectorGUI();
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(selectedRegistry.name, EditorStyles.boldLabel);

                if (GUILayout.Button("Select Asset", GUILayout.Width(90f)))
                {
                    Selection.activeObject = selectedRegistry;
                    EditorGUIUtility.PingObject(selectedRegistry);
                }

                if (GUILayout.Button("Reload", GUILayout.Width(65f)))
                {
                    selectedRegistry.Reload();
                    EditorUtility.SetDirty(selectedRegistry);
                    Repaint();
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4f);
        }
        private void DrawRegistry(Registry registry)
        {
            bool selected = selectedRegistry == registry;

            GUIStyle style = selected ? EditorStyles.whiteLabel : EditorStyles.label;
            Rect rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight + 6f);

            if (selected)
            {
                EditorGUI.DrawRect(rect, new Color(0.20f, 0.38f, 0.58f, 1f));
            }

            rect.x += 5f;
            rect.width -= 10f;

            Rect priorityRect = rect;
            priorityRect.width = 38f;

            Rect nameRect = rect;
            nameRect.x += 42f;
            nameRect.width -= 42f;

            EditorGUI.LabelField(priorityRect, registry.Priority.ToString(), selected ? EditorStyles.centeredGreyMiniLabel : EditorStyles.miniLabel);
            EditorGUI.LabelField(nameRect, registry.name, style);

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
            { 
                SelectRegistry(registry);
                Event.current.Use();
                Repaint();
            }
            
            if (Event.current.type == EventType.MouseMove && rect.Contains(Event.current.mousePosition))
            {
                Repaint();
            }
        }
        private void SelectRegistry(Registry registry)
        {
            if (selectedRegistry == registry)
            {
                return;
            }

            DestroyRegistry();

            selectedRegistry = registry;

            if (selectedRegistry != null)
            {
                registryEditor = UnityEditor.Editor.CreateEditor(selectedRegistry);
            }

            inspectorScrollPosition = Vector2.zero;
        }
        private void DestroyRegistry()
        {
            if (registryEditor != null)
            {
                DestroyImmediate(registryEditor);
                registryEditor = null;
            }
        }
        private void RefreshRegistries()
        {
            Registry previousSelection = selectedRegistry;

            string[] guids = AssetDatabase.FindAssets(
                "t:" + typeof(Registry).Name,
                new[] { "Assets" });

            registries = guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<Registry>)
                .Where(registry => registry != null)
                .OrderBy(registry => registry.Priority)
                .ThenBy(registry => registry.name)
                .ToArray();

            if (previousSelection != null &&
                registries.Contains(previousSelection))
            {
                selectedRegistry = previousSelection;
            }
            else
            {
                selectedRegistry = null;
            }

            if (selectedRegistry == null)
            {
                DestroyRegistry();
            }
            else if (registryEditor == null ||
                     registryEditor.target != selectedRegistry)
            {
                SelectRegistry(selectedRegistry);
            }

            Repaint();
        }
        private void ReloadRegistries()
        {
            for (int i = 0; i < registries.Length; i++)
            {
                Registry registry = registries[i];

                if (registry == null)
                {
                    continue;
                }

                registry.Reload();
                EditorUtility.SetDirty(registry);
            }

            AssetDatabase.SaveAssets();
            RefreshRegistries();
        }
    }
}
