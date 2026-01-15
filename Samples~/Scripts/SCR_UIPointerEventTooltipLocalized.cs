using UnityEngine;
using Core.Localization;

namespace Core.UI
{
    using static CoreUtility;

    [RequireComponent(typeof(UIPointerEventTooltip))]
    public class UIPointerEventTooltipLocalized : MonoBehaviour
    {
        [Header("_ Localization")]
        [SerializeField] private string localizedID = STRING_EMPTY;

        private string localizedText = STRING_EMPTY;

        private void Start()
        {
            if (string.IsNullOrEmpty(localizedID))
            {
                return;
            }

            localizedText = ManagerCoreLocalization.Instance.Get(localizedID);
            GetComponent<UIPointerEventTooltip>().Initialize(localizedText, true);
        }
    }
}
