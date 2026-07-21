using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    using static CoreUtility;

    [RequireComponent(typeof(Slider))]
    public class UISliderFill : MonoBehaviour
    {
        [Header("_")]
        [SerializeField, Required] private Image fill = null;

        private Slider thisSlider = null;

        private void Awake() => thisSlider = GetComponent<Slider>();
        private void OnEnable() => thisSlider.onValueChanged.AddListener(OnValueChanged);
        private void OnDisable() => thisSlider.onValueChanged.RemoveListener(OnValueChanged);
        private void OnValueChanged(float value) => fill.fillAmount = thisSlider.normalizedValue;
    }
}
