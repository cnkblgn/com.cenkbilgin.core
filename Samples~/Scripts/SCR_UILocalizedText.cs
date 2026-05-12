using UnityEngine;
using TMPro;
using Core;
using Core.Localization;

namespace Game
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class UILocalizedText : MonoBehaviour
    {
        [Header("_")]
        [SerializeField] private string prefix = STRING_EMPTY;
        [SerializeField] private LocalizedString key = default;

        private TextMeshProUGUI thisText = null;

        private void Awake() => thisText = GetComponent<TextMeshProUGUI>();
        private void OnEnable()
        {
            ManagerCoreLocalization.OnLocalizationChanged += OnLocalizationChanged;
        }
        private void OnDisable()
        {
            ManagerCoreLocalization.OnLocalizationChanged -= OnLocalizationChanged;
        }
        private void OnLocalizationChanged(int _) => thisText.text = prefix + key.Get();
    }
}
