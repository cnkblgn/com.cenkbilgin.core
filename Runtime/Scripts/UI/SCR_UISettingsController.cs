using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public class UISettingsController : MonoBehaviour
    {
        public event Action OnApply = null;
        public event Action OnRevert = null;
        public event Action OnLoad = null;

        [Header("_")]
        [SerializeField, Required] private RectTransform container = null;

        [Header("_")]
        [SerializeField, Required] private UIHeader headerPrefab = null;
        [SerializeField, Required] private UIOptionButton buttonPrefab = null;
        [SerializeField, Required] private UIOptionSlider sliderPrefab = null;
        [SerializeField, Required] private UIOptionToggle togglePrefab = null;

        [Header("_")]
        [SerializeField, Required] private Button applyButton = null;
        [SerializeField, Required] private Button closeButton = null;

        private Canvas thisCanvas = null;
        private readonly List<UIOptionBase> settings = new(8);
        private bool isOpened = false;
        private bool isApplied = false;

        private void Awake() => thisCanvas = GetComponent<Canvas>();
        private void OnEnable()
        {
            applyButton.onClick.AddListener(OnApplyButtonClicked);
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }
        private void OnDisable()
        {
            applyButton.onClick.RemoveListener(OnApplyButtonClicked);
            closeButton.onClick.RemoveListener(OnCloseButtonClicked);
        }

        private void OnApplyButtonClicked()
        {
            ApplyAll();
            isApplied = true;
        }
        private void OnRevertButtonClicked()
        {
            RevertAll();
            ApplyAll();
        }
        private void OnCloseButtonClicked()
        {
            if (!isApplied)
            {
                LoadAll();
            }

            Hide();
        }

        private void ApplyAll()
        {
            foreach (UIOptionBase setting in settings)
            {
                setting.Apply();
            }

            OnApply?.Invoke();
        }
        private void RevertAll()
        {
            foreach (UIOptionBase setting in settings)
            {
                setting.Revert();
            }

            OnRevert?.Invoke();
        }
        private void LoadAll()
        {
            foreach (UIOptionBase setting in settings)
            {
                setting.Load();
            }

            OnLoad?.Invoke();
        }

        public void Show()
        {
            if (isOpened)
            {
                return;
            }

            thisCanvas.Show();
            isOpened = true;
            isApplied = false;

            LoadAll();
        }
        public void Hide()
        {
            thisCanvas.Hide();
            isOpened = false;
            isApplied = false;
        }

        public void InsertHeader(string description) => Instantiate(headerPrefab, container).Initialize(description);
        public UIOptionButton InsertButton(int initial, int @default, string description, string[] values, Action<int> onApply, Action<int> onChanged, bool wrapAround = false)
        {
            UIOptionButton obj = Instantiate(buttonPrefab, container).Initialize(initial, @default, description, values, onApply, onChanged, wrapAround);

            settings.Add(obj);

            return obj;
        }
        public UIOptionButton InsertButton(int initial, int @default, string description, int maximumIndex, Action<int> onApply, Action<int> onChanged, bool wrapAround = false)
        {
            UIOptionButton obj = Instantiate(buttonPrefab, container).Initialize(initial, @default, description, maximumIndex, onApply, onChanged, wrapAround);

            settings.Add(obj);

            return obj;
        }
        public UIOptionToggle InsertToggle(bool initial, bool @default, string description, Action<bool> onApply, Action<bool> onChanged)
        {
            UIOptionToggle obj = Instantiate(togglePrefab, container).Initialize(initial, @default, description, onApply, onChanged);

            settings.Add(obj);

            return obj;
        }
        public UIOptionSlider InsertSlider(float initial, float @default, string description, string[] values, Action<float> onApply, Action<float> onChanged)
        {
            UIOptionSlider obj = Instantiate(sliderPrefab, container).Initialize(initial, @default, description, values, onApply, onChanged);

            settings.Add(obj);

            return obj;
        }
        public UIOptionSlider InsertSlider(float initial, float @default, string description, float minValue, float maxValue, bool isInt, Action<float> onApply, Action<float> onChanged)
        {
            UIOptionSlider obj = Instantiate(sliderPrefab, container).Initialize(initial, @default, description, minValue, maxValue, isInt, onApply, onChanged);

            settings.Add(obj);

            return obj;
        }
    }
}