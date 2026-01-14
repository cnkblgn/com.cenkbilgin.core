using TMPro;
using UnityEngine;

namespace Core.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public abstract class UITextBinder : MonoBehaviour
    {
        private void Awake() => GetComponent<TextMeshProUGUI>().text = Get();
        protected abstract string Get();
    }
}
