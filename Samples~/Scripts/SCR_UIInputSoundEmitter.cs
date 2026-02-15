using UnityEngine;
using TMPro;
using Core;
using Core.Audio;

namespace Game
{
    using static CoreUtility;

    [RequireComponent(typeof(TMP_InputField))]
    public class UIInputSoundEmitter : MonoBehaviour
    {
        [Header("_")]
        [SerializeField, Required] private AudioClip[] sounds = null;
        [SerializeField, Min(0)] private float cooldown = 0;

        private TMP_InputField thisInput = null;
        private float lastTime = 0f;

        private void Awake() => thisInput = GetComponent<TMP_InputField>();
        private void OnEnable() => thisInput.onValueChanged.AddListener(OnValueChanged);
        private void OnDisable() => thisInput.onValueChanged.RemoveListener(OnValueChanged);

        private void OnValueChanged(string value)
        {
            if (Time.time - lastTime >= cooldown)
            {
                ManagerCoreAudio.Instance.PlaySound(sounds, AudioGroup.MASTER, Vector3.zero, 0, 1, 1, 1, 10, false);
                lastTime = Time.time;
            }
        }
    }
}
