using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Core.UI
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public class UITextSlider : MonoBehaviour
    {
        [Header("_")]
        [SerializeField, Required] private Image slider = null;
        [SerializeField, Required] private TextMeshProUGUI text = null;

        public void SetText(string text) => this.text.text = text;
        public void SetFill(float fill) => slider.fillAmount = fill;
    }
}
