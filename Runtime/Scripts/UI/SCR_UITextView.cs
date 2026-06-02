using UnityEngine;
using TMPro;

namespace Core.UI
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public class UITextView : MonoBehaviour
    {
        [Header("_")]
        [SerializeField, Required] private TextMeshProUGUI text = null;

        public string Get() => this.text.text;
        public void Set(string text) => this.text.SetText(text);
    }
}
