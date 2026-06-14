using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core;
using Core.Input;
using Core.UI;

namespace Game
{
    using static CoreUtility;
    using static InputDatabase;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public class UIConsoleController : MonoBehaviour
    {
        public event Action OnOpened = null;
        public event Action OnClosed = null;

        private const string DECORATOR = "] ";
        private const string ELLIPSIS = "...";
        private const int MAX_LOG_COUNT = 64;
        private const int MAX_SUGGESTION_COUNT = 5;
        public bool IsOpen => isOpen;

        [Header("_")]
        [SerializeField] private TMP_InputField consoleInputField = null;

        [Header("_")]
        [SerializeField] private ScrollRect consoleHistoryRect = null;
        [SerializeField] private TMP_Text consoleHistoryText = null;

        [Header("_")]
        [SerializeField] private RectTransform consoleSuggestionContainer = null;
        [SerializeField] private TMP_Text consoleSuggestionText = null;
        [SerializeField] private Vector2 consoleSuggestionOffset = new(16, 16);

        private readonly RingBufferArray<string> logBuffer = new(MAX_LOG_COUNT);
        private readonly List<DebugCommandInstanceBase> suggestionBuffer = new();
        private readonly StringBuilder logBuilder = new(8192);
        private readonly StringBuilder suggestionBuilder = new(1024);
        private Canvas thisCanvas = null;
        private string lastCommand = STRING_EMPTY;
        private string lastLog = STRING_EMPTY;
        private string lastInput = STRING_EMPTY;
        private string previousInputMap = InputDatabase.UIMap;
        private int lastLogCount = 0;
        private int suggestionIndex = -1;
        private bool requestSuggestionDraw = false;
        private bool isOpen = false;
        private bool isDirty = false;

        private void Awake()
        {
            thisCanvas = GetComponent<Canvas>();

            consoleInputField.onFocusSelectAll = false;
            consoleInputField.resetOnDeActivation = false;
            consoleInputField.restoreOriginalTextOnEscape = false;
        }
        private void Start()
        {
            Hide();
            OnSubmit("help");
        }
        private void Update()
        {
            if (ManagerCoreGame.Instance.IsLoading)
            {
                return;
            }

            if (Console.GetKeyDown())
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
                    if (UIDown.GetKeyDown())
                    {
                        MoveSuggestion(1);
                    }
                    else if (UIUp.GetKeyDown())
                    {
                        MoveSuggestion(-1);
                    }
                    else if (Tab.GetKeyDown())
                    {
                        ApplySuggestion();
                    }
                }
                else
                {
                    if (UIUp.GetKeyDown() && lastCommand != STRING_EMPTY)
                    {
                        RebuildInput(lastCommand);
                        lastCommand = STRING_EMPTY;
                    }
                }
            }
            else
            {
                if (consoleInputField.isFocused)
                {
                    consoleInputField.DeactivateInputField();
                }
            }
        }
        private void LateUpdate()
        {
            if (!isOpen)
            {
                return;
            }

            if (isDirty)
            {
                RebuildText();
                LayoutRebuilder.ForceRebuildLayoutImmediate(consoleHistoryRect.content);
                consoleHistoryRect.verticalNormalizedPosition = 0;
                isDirty = false;
            }

            if (requestSuggestionDraw)
            {
                requestSuggestionDraw = false;
                DrawSuggestion();
            }
        }
        private void OnEnable()
        {
            Application.logMessageReceived += OnUnityLogReceived;
            DebugCommandLogger.OnLogReceived += OnCommandLogReceived;
            DebugCommandLogger.OnLogCleared += OnCommandLogCleared;
            ManagerCoreGame.OnBeforeSceneChanged += OnBeforeSceneChanged;

            consoleInputField.onSubmit.AddListener(OnSubmit);
            consoleInputField.onValueChanged.AddListener(OnInput);
        }
        private void OnDisable()
        {
            Application.logMessageReceived -= OnUnityLogReceived;
            DebugCommandLogger.OnLogReceived -= OnCommandLogReceived;
            DebugCommandLogger.OnLogCleared -= OnCommandLogCleared;
            ManagerCoreGame.OnBeforeSceneChanged -= OnBeforeSceneChanged;

            consoleInputField.onSubmit.RemoveListener(OnSubmit);
            consoleInputField.onValueChanged.RemoveListener(OnInput);
        }

        private void OnBeforeSceneChanged(string scene) => Hide();
        private void OnCommandLogCleared() => Clear();
        private void OnCommandLogReceived(string value) => Log(value);
        private void OnUnityLogReceived(string value, string stackTrace, UnityEngine.LogType type)
        {
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                return;
            }

            switch (type)
            {
                case UnityEngine.LogType.Error:
                case UnityEngine.LogType.Exception: Log(value.ToRed()); break;
                case UnityEngine.LogType.Warning:
                case UnityEngine.LogType.Assert: Log(value.ToYellow()); break;
                default: Log(value); break;
            }
        }
        private void OnInput(string value)
        {
            if (!isOpen || Console.GetKeyDown())
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

            if (!string.IsNullOrWhiteSpace(input))
            {
                lastCommand = input;
            }

            consoleInputField.text = STRING_EMPTY;

            if (isOpen)
            {
                consoleInputField.ActivateInputField();
            }
        }

        public void Show()
        {
            isOpen = true;
            thisCanvas.Show();
            consoleInputField.ActivateInputField();

            previousInputMap = ManagerCoreInput.Instance.GetMap();
            ManagerCoreInput.Instance.SwitchMap(InputDatabase.UIMap);

            OnOpened?.Invoke();
        }
        public void Hide()
        {
            isOpen = false;
            thisCanvas.Hide();
            consoleInputField.DeactivateInputField();

            requestSuggestionDraw = false;
            ClearSuggestion();

            if (!string.IsNullOrEmpty(previousInputMap)) ManagerCoreInput.Instance.SwitchMap(new(previousInputMap));

            OnClosed?.Invoke();
        }
        private void Clear()
        {
            logBuffer.Clear();
            lastLog = STRING_EMPTY;
            lastLogCount = 0;
            isDirty = true;
        }
        private void Log(string value)
        {
            if (value == lastLog && logBuffer.Count > 0)
            {
                lastLogCount++;
                logBuffer.SetLatest($"{DECORATOR}{value} " + $"(x{lastLogCount})".ToYellow());
                isDirty = true;
                return;
            }

            lastLog = value;
            lastLogCount = 1;

            logBuffer.Add($"{DECORATOR}{value}");
            isDirty = true;
        }

        private void RebuildText()
        {
            logBuilder.Clear();

            for (int i = 0; i < logBuffer.Count; i++)
            {
                logBuilder.AppendLine(logBuffer.Get(i));
            }

            consoleHistoryText.SetText(logBuilder);
        }
        private void RebuildInput(string value)
        {
            consoleInputField.text = value;
            consoleInputField.caretPosition = value.Length;
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
            int count = Mathf.Min(total, MAX_SUGGESTION_COUNT);

            for (int i = 0; i < count; i++)
            {
                DebugCommandInstanceBase command = suggestionBuffer[i];
                bool selected = i == suggestionIndex;

                suggestionBuilder.Append(selected ? OPEN_YELLOW : OPEN_GHOST);
                suggestionBuilder.Append(selected ? "  > " : "");
                suggestionBuilder.Append(command.ID);
                suggestionBuilder.Append(CLOSE_COLOR);

                if (i < count - 1)
                {
                    suggestionBuilder.Append('\n');
                }
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