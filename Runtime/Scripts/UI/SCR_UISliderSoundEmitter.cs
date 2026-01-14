using Core.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    using static CoreUtility;

    [RequireComponent(typeof(Slider))]
    public class UISliderSoundEmitter : MonoBehaviour
    {
        [Header("_")]
        [SerializeField, Required] private AudioClip[] sounds = null;
        [SerializeField, Min(0.1f)] private float cooldown = 0.1f;
        [SerializeField, Min(0.1f)] private float threshold = 0.1f;

        private Slider thisSlider = null;
        private float lastTime = 0f;
        private float lastValue = 0f;

        private void Awake() => thisSlider = GetComponent<Slider>();
        private void OnEnable() => thisSlider.onValueChanged.AddListener(OnValueChanged);
        private void OnDisable() => thisSlider.onValueChanged.RemoveListener(OnValueChanged);

        private void OnValueChanged(float value)
        {
            if (Mathf.Abs(value - lastValue) >= threshold || Time.time - lastTime >= cooldown)
            {
                ManagerCoreAudio.Instance.PlaySound(sounds, AudioGroup.MASTER, Vector3.zero, 0, 1, 1, 1, 10, false);
                lastTime = Time.time;
                lastValue = value;
            }
        }
    }
}
