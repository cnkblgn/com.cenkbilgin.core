using UnityEngine;
using Core.Localization;

namespace Core.UI
{
    using static CoreUtility;

    [RequireComponent(typeof(UIPointerEventTooltip))]
    public class UIPointerEventTooltipLocalized : MonoBehaviour
    {
        [Header("_")]
        [SerializeField] private string localizedID = STRING_EMPTY;

        private UIPointerEventTooltip thisTooltip = null;

        private void Awake() => thisTooltip = GetComponent<UIPointerEventTooltip>();
        private void OnEnable()
        {
            ManagerCoreLocalization.OnLocalizationChanged += OnLocalizationChanged;
        }
        private void OnDisable()
        {
            ManagerCoreLocalization.OnLocalizationChanged -= OnLocalizationChanged;
        }
        private void OnLocalizationChanged(int _) => thisTooltip.Initialize(ManagerCoreLocalization.Instance.Get(localizedID), true);
    }
}
