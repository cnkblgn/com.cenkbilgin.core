using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core.Input;

namespace Core.Misc
{
    using static CoreUtility;
    using static InputActionDatabase;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public class UIConsoleController : MonoBehaviour
    {
        private const string DECORATOR = "] ";
        private const string ELLIPSIS = "...";
        private const int MAX_LOG_COUNT = 64;
        private const int MAX_SUGGESTION_COUNT = 5;

        public bool IsOpen => isOpen;

        [Header("_")]
        [SerializeField, Required] private TMP_InputField consoleInputField = null;

        [Header("_")]
        [SerializeField, Required] private ScrollRect consoleHistoryRect = null;
        [SerializeField, Required] private TMP_Text consoleHistoryText = null;

        [Header("_")]
        [SerializeField, Required] private RectTransform consoleSuggestionContainer = null;
        [SerializeField, Required] private TMP_Text consoleSuggestionText = null;
        [SerializeField] private Vector2 consoleSuggestionOffset = new(16, 16);

        private readonly List<string> logBuffer = new(MAX_LOG_COUNT);
        private readonly List<DebugCommandInstanceBase> suggestionBuffer = new();
        private readonly StringBuilder logBuilder = new(8192);
        private readonly StringBuilder suggestionBuilder = new(1024);
        private Canvas thisCanvas = null;
        private string lastCommand = STRING_EMPTY;
        private string lastLog = STRING_EMPTY;
        private string lastInput = STRING_EMPTY;
        private int lastLogCount = 0;
        private int suggestionIndex = -1; 
        private bool requestSuggestionDraw = false;
        private bool isOpen = false;

        private void Awake()
        {
            thisCanvas = GetComponent<Canvas>();

            Hide();
        }
        private void Start() => OnSubmit("help");
        private void Update()
        {
            if (ManagerCoreInput.Instance.GetButtonDown(Console))
            {
                if (!isOpen)
                {
                    Show();
                }
                else
                {
                    Hide();
                }

                return;
            }

            if (isOpen)
            {
                if (!consoleInputField.isFocused)
                {
                    consoleInputField.ActivateInputField();
                }

                if (suggestionBuffer.Count > 0)
                {
                    if (ManagerCoreInput.Instance.GetButtonDown(UIDown))
                    {
                        MoveSuggestion(1);
                    }
                    else if (ManagerCoreInput.Instance.GetButtonDown(UIUp))
                    {
                        MoveSuggestion(-1);
                    }
                    else if (ManagerCoreInput.Instance.GetButtonDown(Tab))
                    {
                        ApplySuggestion();
                    }
                }
                else
                {
                    if (ManagerCoreInput.Instance.GetButtonDown(UIUp))
                    {
                        if (lastCommand != STRING_EMPTY)
                        {
                            RebuildInput(lastCommand);
                            lastCommand = STRING_EMPTY;
                        }
                    }
                }
            }
        }
        private void LateUpdate()
        {
            if (!isOpen)
            {
                return;
            }

            if (!requestSuggestionDraw)
            {
                return;
            }

            requestSuggestionDraw = false;
            DrawSuggestion();
        }
        private void OnEnable()
        {
            Application.logMessageReceived += OnUnityLogReceived;
            DebugCommandLogger.OnLogReceived += OnCommandLogReceived;
            DebugCommandLogger.OnLogCleared += OnCommandLogCleared;

            consoleInputField.onSubmit.AddListener(OnSubmit);
            consoleInputField.onValueChanged.AddListener(OnInput);

            this.WaitUntil(() => ManagerCoreGame.Instance != null, null, () => ManagerCoreGame.Instance.OnBeforeSceneChanged += OnBeforeSceneChanged);
        }
        private void OnDisable()
        {
            Application.logMessageReceived -= OnUnityLogReceived;
            DebugCommandLogger.OnLogReceived -= OnCommandLogReceived;
            DebugCommandLogger.OnLogCleared -= OnCommandLogCleared;

            if (ManagerCoreGame.Instance != null)
            {
                ManagerCoreGame.Instance.OnBeforeSceneChanged -= OnBeforeSceneChanged;
            }

            consoleInputField.onSubmit.RemoveListener(OnSubmit);
            consoleInputField.onValueChanged.RemoveListener(OnInput);
        }

        private void OnBeforeSceneChanged(string scene) => Hide();
        private void OnCommandLogCleared() => Clear();
        private void OnCommandLogReceived(string value) => Log(value);
        private void OnUnityLogReceived(string value, string stackTrace, LogType type)
        {
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                return;
            }

            switch (type)
            {
                case LogType.Error:
                    Log(value.ToRed());
                    break;
                case LogType.Assert:
                    Log(value.ToYellow());
                    break;
                case LogType.Warning:
                    Log(value.ToYellow());
                    break;
                case LogType.Log:
                    Log(value);
                    break;
                case LogType.Exception:
                    Log(value.ToRed());
                    break;
                default:
                    Log(value);
                    break;
            }
        }
        private void OnInput(string value)
        {
            if (!isOpen)
            {
                return;
            }

            if (ManagerCoreInput.Instance.GetButtonDown(Console))
            {
                return;
            }

            UpdateSuggestion(value);
        }
        private void OnSubmit(string input)
        {
            if (DebugCommandData.Execute(input))
            {
                Log(input);
            }

            if (!string.IsNullOrEmpty(input) && !string.IsNullOrWhiteSpace(input))
            {
                lastCommand = input;
            }

            consoleInputField.text = STRING_EMPTY;
            consoleInputField.ActivateInputField();

            LayoutRebuilder.ForceRebuildLayoutImmediate(consoleHistoryRect.content);
            this.WaitFrame(null, () => consoleHistoryRect.verticalNormalizedPosition = 0);
        }

        public void Show()
        {
            thisCanvas.Show();

            ManagerCoreInput.Instance.SwitchMap("UI");

            consoleInputField.ActivateInputField();

            isOpen = true;
        }
        public void Hide()
        {
            thisCanvas.Hide();

            if (ManagerCoreGame.Instance.GetGameState() == GameState.RESUME)
            {
                ManagerCoreInput.Instance.SwitchMap("Gameplay");
            }

            consoleInputField.DeactivateInputField();

            isOpen = false;
            requestSuggestionDraw = false;

            ClearSuggestion();
        }
        private void Clear()
        {
            logBuffer.Clear();
            lastLog = STRING_EMPTY;
            lastLogCount = 0;
            RebuildText();
        }
        private void Log(string value)
        {
            if (value == lastLog && logBuffer.Count > 0)
            {
                lastLogCount++;
                logBuffer[^1] = ($"{DECORATOR}{value} " + $"(x{lastLogCount})".ToYellow());
                RebuildText();
                return;
            }

            lastLog = value;
            lastLogCount = 1;

            if (logBuffer.Count >= MAX_LOG_COUNT)
            {
                logBuffer.RemoveAt(0);
            }

            logBuffer.Add($"{DECORATOR}{value}");
            RebuildText();
        }
        private void RebuildText()
        {
            logBuilder.Clear();

            foreach (var value in logBuffer)
            {
                logBuilder.AppendLine(value);
            }

            consoleHistoryText.text = logBuilder.ToString();
        }
        private void RebuildInput(string value)
        {
            consoleInputField.text = value;
            consoleInputField.caretPosition = consoleInputField.text.Length;
            consoleInputField.ActivateInputField();
        }

        private void ApplySuggestion()
        {
            if (suggestionIndex < 0 || suggestionIndex >= suggestionBuffer.Count)
            {
                return;
            }

            RebuildInput(suggestionBuffer[suggestionIndex].ID);
        }
        private void MoveSuggestion(int delta)
        {
            int i = suggestionIndex + delta;

            if (i < 0)
            {
                i = suggestionBuffer.Count - 1;
            }
            else if (i >= suggestionBuffer.Count)
            {
                i = 0;
            }

            if (i == suggestionIndex)
            {
                return;
            }

            suggestionIndex = i;
            requestSuggestionDraw = true;
        }
        private void UpdateSuggestion(string value)
        {
            if (value == lastInput)
            {
                return;
            }

            lastInput = value;
            suggestionBuffer.Clear();
            suggestionIndex = -1;

            if (string.IsNullOrWhiteSpace(value))
            {
                ClearSuggestion();
                return;
            }

            int spaceIndex = value.IndexOf(' ');
            string prefix = spaceIndex >= 0 ? value[..spaceIndex] : value;
            IReadOnlyList<DebugCommandInstanceBase> data = DebugCommandData.Data;

            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].ID.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
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
            requestSuggestionDraw = true;
        }
        private void DrawSuggestion()
        {
            suggestionBuilder.Clear();

            int total = suggestionBuffer.Count;
            bool overflow = total > MAX_SUGGESTION_COUNT;
            int count = overflow ? MAX_SUGGESTION_COUNT : total;
            int index = overflow && suggestionIndex >= 0 ? Mathf.Clamp(suggestionIndex - (count - 1), 0, total - count) : 0;

            for (int i = index; i < index + count; i++)
            {
                var command = suggestionBuffer[i];
                bool selected = i == suggestionIndex;

                suggestionBuilder.Append(selected ? OPEN_YELLOW : OPEN_GHOST);
                //suggestionBuilder.Append(selected ? "  > " : "   ");
                suggestionBuilder.Append(selected ? "  > " : "");
                suggestionBuilder.Append(command.ID);
                suggestionBuilder.Append(CLOSE_COLOR);

                if (i < count - 1 || overflow)
                {
                    suggestionBuilder.Append('\n');
                }
            }

            if (overflow)
            {
                string t = OPEN_GHOST + "   " + ELLIPSIS + CLOSE_COLOR + '\n';

                bool insertAtStart = suggestionIndex >= MAX_SUGGESTION_COUNT;
                bool insertAtEnd = suggestionIndex < total - 1;

                if (insertAtStart) suggestionBuilder.Insert(0, t);
                if (insertAtEnd) suggestionBuilder.Append(t);
            }

            consoleSuggestionText.SetText(suggestionBuilder);
            consoleSuggestionContainer.sizeDelta = consoleSuggestionText.GetPreferredValues() + consoleSuggestionOffset;
        }
        private void ClearSuggestion()
        {
            suggestionBuilder.Clear();
            consoleSuggestionText.SetText(STRING_EMPTY);
            consoleSuggestionContainer.sizeDelta = Vector2.zero;
            requestSuggestionDraw = false;
        }
    }
}