using UnityEngine;
using Core.Localization;

namespace Core.UI
{
    using static CoreUtility;

    [RequireComponent(typeof(UIPointerEventTooltip))]
    public class UIPointerEventTooltipLocalized : MonoBehaviour
    {
        [Header("_")]
        [SerializeField] private LocalizedString localizedID = default;

        private UIPointerEventTooltip thisTooltip = null;

        private void Awake() => thisTooltip = GetComponent<UIPointerEventTooltip>();
        private void Start() => OnLocalizationChanged(-1);
        private void OnEnable() => LocalizationDatabase.OnLocalizationChanged += OnLocalizationChanged;
        private void OnDisable() => LocalizationDatabase.OnLocalizationChanged -= OnLocalizationChanged;
        private void OnLocalizationChanged(int _) => thisTooltip.Initialize(localizedID.Get(), true);
    }
}
