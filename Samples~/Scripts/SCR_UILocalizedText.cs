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
        [SerializeField, Required] private string key = STRING_EMPTY;

        private TextMeshProUGUI thisText = null;

        private void Start()
        {
            thisText = GetComponent<TextMeshProUGUI>();
            thisText.text = prefix + ManagerCoreLocalization.Instance.Get(key);
        } 
    }
}
