using TMPro;
using UnityEngine;

namespace Core.UI
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public class UIKeyActionElement : MonoBehaviour
    {
        [Header("_")]
        [SerializeField, Required] private TextMeshProUGUI thisText = null;

        public void Initialize(int keySprite, string keyDescription) => thisText.text = $"{GetSprite(keySprite)}: {keyDescription}";
    }
}