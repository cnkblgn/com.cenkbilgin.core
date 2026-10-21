using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Core
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public static class CommandConsole
    {
        public static event Action OnOpened;
        public static event Action OnClosed;

        private const string DECORATOR = "] ";
        private const int MAX_LOG_COUNT = 128;
        private const int MAX_SUGGESTION_COUNT = 5;
        private const int FONT_SIZE = 20;
        private const float PANEL_HEIGHT = 512f;
        private const float PANEL_WIDTH = 1024f;
        private const float PANEL_PADDING = 16f;
        private const float INPUT_HEIGHT = 32f;
        private const float INPUT_PADDING = 6f;
        private const float SUGGESTION_HEIGHT = 32f;
        private const float SCROLL_SPEED = 12f;

        public static bool IsOpen => isOpened;

        private static Updater updater;
        private static readonly StringBuilder logBuilder = new(8192);
        private static readonly RingBufferArray<string> logBuffer = new(MAX_LOG_COUNT);
        private static readonly List<CommandInstanceBase> suggestionBuffer = new();
        private static GUIStyle panelStyle;
        private static GUIStyle logStyle;
        private static GUIStyle inputStyle;
        private static GUIStyle suggestionStyle;
        private static GUIStyle selectedStyle;
        private static Texture2D panelTexture;
        private static Texture2D backgroundTexture;
        private static Texture2D selectedTexture;
        private static Rect windowRect;
        private static Vector2 scrollPosition;
        private static string lastCommand = STRING_EMPTY;
        private static string lastLog = STRING_EMPTY;
        private static string lastInput = STRING_EMPTY;
        private static string currentInput = STRING_EMPTY;
        private static int lastLogCount;
        private static int suggestionIndex = -1;
        private static int suggestionOffset = 0;
        private static bool isOpened;
        private static bool isDirty;
        private static bool isInitialized;
        private static bool hasRequestedFocus;
        private static bool hasRequestedCaret;
        private static readonly Color COLOR_BACKGROUND = new(0.05f, 0.05f, 0.05f, 1f);
        private static readonly Color COLOR_PANEL = new(0.1f, 0.1f, 0.1f, 1f);
        private static readonly Color COLOR_SELECTED = new(0.15f, 0.15f, 0.15f, 1f);
        private static readonly Color COLOR_ACCENT = new(1f, 1f, 1f, 1f);
        private static readonly Color COLOR_MUTED = new(0.55f, 0.55f, 0.55f, 1f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            if (updater != null)
            {
                UnityEngine.Object.DestroyImmediate(updater.gameObject);
                updater = null;
            }

            if (backgroundTexture != null)
            {
                UnityEngine.Object.DestroyImmediate(backgroundTexture);
                backgroundTexture = null;
            }

            if (panelTexture != null)
            {
                UnityEngine.Object.DestroyImmediate(panelTexture);
                panelTexture = null;
            }

            if (selectedTexture != null)
            {
                UnityEngine.Object.DestroyImmediate(selectedTexture);
                selectedTexture = null;
            }

            panelStyle = null;
            logStyle = null;
            inputStyle = null;
            suggestionStyle = null;
            selectedStyle = null;
            isInitialized = false;
            isOpened = false;
            isDirty = false;

            Application.logMessageReceived -= OnUnityLogReceived;
            CommandLogger.OnLogReceived -= OnCommandLogReceived;
            CommandLogger.OnLogCleared -= OnCommandLogCleared;

            OnOpened = null;
            OnClosed = null;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
            if (CommandConsole.updater != null)
            {
                return;
            }

            Application.logMessageReceived += OnUnityLogReceived;
            CommandLogger.OnLogReceived += OnCommandLogReceived;
            CommandLogger.OnLogCleared += OnCommandLogCleared;

            GameObject updater = new("[Command Console]") { hideFlags = HideFlags.HideAndDontSave };
            CommandConsole.updater = updater.AddComponent<Updater>();

            UnityEngine.Object.DontDestroyOnLoad(updater);
        }

        private sealed class Updater : MonoBehaviour
        {
            private void Start()
            {
                Show();

                OnSubmit("help");
            }
            private void OnGUI()
            {
                if (!isOpened)
                {
                    return;
                }

                ValidateStyles();

                Event current = Event.current;

                if (current.type == EventType.KeyDown)
                {
                    HandleKeyboard(current);
                }
                else if (current.type == EventType.ScrollWheel)
                {
                    HandleScroll(current);
                }

                windowRect = new
                (
                    16f,
                    16f,
                    Mathf.Min(Screen.width - 32f, PANEL_WIDTH),
                    Mathf.Min(PANEL_HEIGHT, Screen.height - 32f)
                );

                DrawConsole();
            }
        }
        
        private static void OnCommandLogCleared() => Clear();
        private static void OnCommandLogReceived(string value) => Log(value);
        private static void OnUnityLogReceived(string value, string trace, LogType type)
        {
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                return;
            }

            switch (type)
            {
                case LogType.Error:
                case LogType.Exception:
                    Log(value.ToRed());
                    break;
                case LogType.Warning:
                case LogType.Assert:
                    Log(value.ToYellow());
                    break;
                default:
                    Log(value);
                    break;
            }
        }
        private static void OnSubmit(string value)
        {
            if (CommandDatabase.TryExecute(value))
            {
                Log(value);
            }

            if (!string.IsNullOrWhiteSpace(value))
            {
                lastCommand = value;
            }

            currentInput = STRING_EMPTY;
            lastInput = STRING_EMPTY;

            ClearSuggestion();

            hasRequestedFocus = isOpened;
        }

        public static void Show()
        {
            if (isOpened)
            {
                return;
            }

            isOpened = true;
            hasRequestedFocus = true;
            OnOpened?.Invoke();
        }
        public static void Hide()
        {
            if (!isOpened)
            {
                return;
            }

            isOpened = false;
            ClearSuggestion();
            OnClosed?.Invoke();
        }
        private static void Clear()
        {
            logBuffer.Clear();
            logBuilder.Clear();

            lastLog = STRING_EMPTY;
            lastLogCount = 0;

            scrollPosition = Vector2.zero;
            isDirty = false;
        }
        private static void Log(string value)
        {
            if (value == lastLog && logBuffer.Count > 0)
            {
                lastLogCount++;
                logBuffer.SetLatest($"{DECORATOR}{value} " + $"(x{lastLogCount})".ToYellow());

                Rebuild();
                return;
            }

            lastLog = value;
            lastLogCount = 1;
            logBuffer.Add($"{DECORATOR}{value}");

            Rebuild();
        }
        private static void Rebuild()
        {
            logBuilder.Clear();

            for (int i = 0; i < logBuffer.Count; i++)
            {
                logBuilder.AppendLine(logBuffer.Get(i));
            }

            isDirty = true;
        }

        private static void CreateStyles()
        {
            static Texture2D texture(Color color)
            {
                Texture2D texture = new(1, 1, TextureFormat.RGBA32, false);
                texture.SetPixel(0, 0, color);
                texture.Apply();
                texture.hideFlags = HideFlags.HideAndDontSave;
                return texture;
            }

            backgroundTexture = texture(COLOR_BACKGROUND);
            panelTexture = texture(COLOR_PANEL);
            selectedTexture = texture(COLOR_SELECTED);

            panelStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = backgroundTexture },
                border = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(0, 0, 0, 0)
            };

            logStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = FONT_SIZE,
                richText = true,
                wordWrap = true,
                stretchHeight = true,
                normal = { textColor = COLOR_ACCENT },
                padding = new RectOffset(4, 4, 4, 4)
            };

            inputStyle = new GUIStyle(GUI.skin.textField)
            {
                fontSize = FONT_SIZE,
                richText = false,
                normal = { background = panelTexture, textColor = Color.white },
                focused = { background = panelTexture, textColor = Color.white },
                padding = new RectOffset(8, 8, 6, 6),
                alignment = TextAnchor.MiddleLeft
            };

            suggestionStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = FONT_SIZE,
                alignment = TextAnchor.MiddleLeft,
                margin = new RectOffset(0, 0, 0, 0),
                normal = { background = panelTexture, textColor = COLOR_MUTED },
                hover = { background = selectedTexture, textColor = COLOR_MUTED },
                padding = new RectOffset(8, 8, 2, 2)
            };

            selectedStyle = new GUIStyle(suggestionStyle)
            {
                normal = { background = selectedTexture, textColor = COLOR_ACCENT },
                hover = { background = selectedTexture, textColor = COLOR_ACCENT },
                fontStyle = FontStyle.Bold
            };

            isInitialized = true;
        }
        private static void ValidateStyles()
        {
            if (isInitialized)
            {
                return;
            }

            CreateStyles();
        }

        private static void HandleKeyboard(Event current)
        {
            switch (current.keyCode)
            {
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    OnSubmit(currentInput);
                    current.Use();
                    break;
                case KeyCode.UpArrow:
                    if (suggestionBuffer.Count > 0)
                    {
                        MoveSuggestion(-1);
                    }
                    else if (!string.IsNullOrEmpty(lastCommand))
                    {
                        currentInput = lastCommand;
                        lastCommand = STRING_EMPTY;
                        UpdateSuggestion(currentInput);
                        hasRequestedFocus = true;
                    }
                    current.Use();
                    break;
                case KeyCode.DownArrow:
                    if (suggestionBuffer.Count > 0)
                    {
                        MoveSuggestion(1);
                    }

                    current.Use();
                    break;
                case KeyCode.Tab:
                    if (suggestionBuffer.Count > 0)
                    {
                        ApplySuggestion(suggestionIndex);
                        current.Use();
                    }
                    break;
            }
        }
        private static void HandleScroll(Event current)
        {
            if (suggestionBuffer.Count > 0)
            {
                int direction = current.delta.y > 0f ? 1 : -1;
                MoveSuggestion(direction);
            }
            else
            {
                scrollPosition.y += current.delta.y * SCROLL_SPEED;
            }

            current.Use();
        }

        private static void DrawConsole()
        {
            Color previousColor = GUI.color;

            GUI.color = Color.white;
            GUI.Box(windowRect, GUIContent.none, panelStyle);

            Rect innerRect = new
            (
                x: windowRect.x + PANEL_PADDING,
                y: windowRect.y + PANEL_PADDING,
                width: windowRect.width - PANEL_PADDING * 2f,
                height: windowRect.height - PANEL_PADDING * 2f
            );

            Rect inputRect = new
            (
                x: innerRect.x,
                y: innerRect.yMax - INPUT_HEIGHT,
                width: innerRect.width,
                height: INPUT_HEIGHT
            );

            DrawInput(inputRect);

            int visibleSuggestionCount = Mathf.Min(suggestionBuffer.Count - suggestionOffset, MAX_SUGGESTION_COUNT);

            float suggestionsHeight = visibleSuggestionCount * SUGGESTION_HEIGHT;
            Rect suggestionsRect = new
            (
                x: innerRect.x,
                y: inputRect.y - INPUT_PADDING - suggestionsHeight,
                width: innerRect.width,
                height: suggestionsHeight
            );

            if (visibleSuggestionCount > 0)
            {
                DrawSuggestions(suggestionsRect, visibleSuggestionCount);
            }

            float historyHeight = visibleSuggestionCount > 0 ? suggestionsRect.y : inputRect.y;
            Rect historyRect = new
            (
                x: innerRect.x,
                y: innerRect.y,
                width: innerRect.width,
                height: Mathf.Max(64, historyHeight - INPUT_PADDING - innerRect.y)
            );

            DrawHistory(historyRect);
            GUI.color = previousColor;
        }
        private static void DrawHistory(Rect rect)
        {
            string text = logBuilder.ToString();
            float height = Mathf.Max(rect.height, logStyle.CalcHeight(new GUIContent(text), rect.width));

            Rect viewRect = new
            (
                x: 0f,
                y: 0f,
                width: rect.width,
                height: height
            );

            scrollPosition = GUI.BeginScrollView(rect, scrollPosition, viewRect, false, false, GUIStyle.none, GUIStyle.none);
            GUI.Label(viewRect, text, logStyle);
            GUI.EndScrollView();

            if (isDirty)
            {
                scrollPosition.y = float.MaxValue;
                isDirty = false;
            }
        }
        private static void DrawSuggestions(Rect rect, int visibleCount)
        {
            for (int i = 0; i < visibleCount; i++)
            {
                int index = suggestionOffset + i;

                CommandInstanceBase command = suggestionBuffer[index];
                bool isSelected = index == suggestionIndex;

                GUIStyle style = isSelected ? selectedStyle : suggestionStyle;
                string prefix = isSelected ? ">  " : "   ";

                Rect buttonRect = new
                (
                    x: rect.x,
                    y: rect.y + i * SUGGESTION_HEIGHT,
                    width: rect.width,
                    height: SUGGESTION_HEIGHT
                );

                if (GUI.Button(buttonRect, prefix + command.ID, style))
                {
                    ApplySuggestion(index);
                }
            }
        }
        private static void DrawInput(Rect rect)
        {
            Rect fieldRect = new
            (
                x: rect.x,
                y: rect.y,
                width: rect.width,
                height: rect.height
            );

            GUI.SetNextControlName("ConsoleInput");
            string newInput = GUI.TextField(fieldRect, currentInput, inputStyle);

            if (newInput != currentInput)
            {
                currentInput = newInput;
                UpdateSuggestion(currentInput);
            }

            if (hasRequestedFocus && Event.current.type == EventType.Repaint)
            {
                GUI.FocusControl("ConsoleInput");
                hasRequestedFocus = false;
                hasRequestedCaret = true;
            }

            if (hasRequestedCaret && Event.current.type == EventType.Repaint)
            {
                MoveCaret();
                hasRequestedCaret = false;
            }
        }

        private static void MoveCaret()
        {
            TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);

            if (editor == null)
            {
                return;
            }

            editor.text = currentInput;
            editor.cursorIndex = currentInput.Length;
            editor.selectIndex = currentInput.Length;
        }
        private static void MoveSuggestion(int delta)
        {
            int totalCount = suggestionBuffer.Count;

            if (totalCount == 0)
            {
                return;
            }

            int index = suggestionIndex + delta;

            if (index < 0)
            {
                index = totalCount - 1;
            }
            else if (index >= totalCount)
            {
                index = 0;
            }

            suggestionIndex = index;

            if (suggestionIndex < suggestionOffset)
            {
                suggestionOffset = suggestionIndex;
            }
            else if (suggestionIndex >= suggestionOffset + MAX_SUGGESTION_COUNT)
            {
                suggestionOffset = suggestionIndex - MAX_SUGGESTION_COUNT + 1;
            }
        }
        private static void UpdateSuggestion(string value)
        {
            if (value == lastInput)
            {
                return;
            }

            lastInput = value;

            suggestionBuffer.Clear();
            suggestionIndex = -1;
            suggestionOffset = 0;

            if (string.IsNullOrWhiteSpace(value))
            {
                ClearSuggestion();
                return;
            }

            int spaceIndex = value.IndexOf(' ');

            if (spaceIndex >= 0)
            {
                ClearSuggestion();
                return;
            }

            IReadOnlyList<CommandInstanceBase> data = CommandDatabase.Data;

            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].ID.StartsWith(value, StringComparison.OrdinalIgnoreCase))
                {
                    suggestionBuffer.Add(data[i]);
                }
            }

            if (suggestionBuffer.Count == 0)
            {
                ClearSuggestion();
                return;
            }

            suggestionIndex = suggestionBuffer.Count == 1 ? 0 : -1;
        }
        private static void ApplySuggestion(int index)
        {
            if (index < 0 || index >= suggestionBuffer.Count)
            {
                return;
            }

            currentInput = suggestionBuffer[index].ID;
            lastInput = STRING_EMPTY;

            UpdateSuggestion(currentInput);
            hasRequestedFocus = true;
        }
        private static void ClearSuggestion()
        {
            suggestionBuffer.Clear();
            suggestionIndex = -1;
        }
    }
}