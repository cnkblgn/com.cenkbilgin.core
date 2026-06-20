using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Core;
using Core.Input;

namespace Game
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public class UIKeyRebindElement : MonoBehaviour
    {
        [Header("_")]
        [SerializeField, Required] private string inputPath = "";

        [Header("_")]
        [SerializeField, Required] private Button rebindButton = null;
        [SerializeField, Required] private Button revertButton = null;
        [SerializeField, Required] private GameObject displayVisualDefault = null;
        [SerializeField, Required] private GameObject displayVisualWaitInput = null;
        [SerializeField, Required] private TextMeshProUGUI displayText = null;

        private void OnEnable()
        {
            rebindButton.onClick.AddListener(OnClick);
            revertButton.onClick.AddListener(OnRevert);
        }
        private void OnDisable()
        {
            rebindButton.onClick.RemoveListener(OnClick);
            revertButton.onClick.RemoveListener(OnRevert);
        }

        private void OnClick() => InputManager.Instance.StartRebind(inputPath, 0, OnRebindStart, OnRebindComplete, OnRebindCancel);
        private void OnRevert()
        {
            OnRebindCancel();

            InputManager.Instance.RevertRebind(inputPath, 0);

            displayText.text = InputManager.Instance.GetDisplay(inputPath, 0);

            InputManager.Instance.Export();
        }
        private void OnRebindStart()
        {
            displayVisualDefault.SetActive(false);
            displayVisualWaitInput.SetActive(true);
        }
        private void OnRebindComplete()
        {
            displayText.text = InputManager.Instance.GetDisplay(inputPath, 0);

            InputManager.Instance.Export();
            displayVisualDefault.SetActive(true);
            displayVisualWaitInput.SetActive(false);
        }
        private void OnRebindCancel()
        {
            displayVisualDefault.SetActive(true);
            displayVisualWaitInput.SetActive(false);
        }
    }
}