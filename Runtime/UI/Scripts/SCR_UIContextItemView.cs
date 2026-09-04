using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Core.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    internal sealed class UIContextItemView : MonoBehaviour
    {
        [Header("_")]
        [SerializeField, Required] private TextMeshProUGUI nameText;

        private Button thisButton;
        private UIContextItemContext thisCtx;
        private IUIContextItemHandler thisHandler;
        private bool hasInitialized;

        public void Initialize(in UIContextItemContext ctx, IUIContextItemHandler handler = null)
        {
            if (hasInitialized)
            {
                return;
            }

            thisCtx = ctx;

            thisHandler = handler;

            thisButton = GetComponent<Button>();
            thisButton.onClick.AddListener(OnClicked);

            nameText.text = thisCtx.Name;

            hasInitialized = true;
        }
        public void Deinitialize()
        {
            if (!hasInitialized)
            {
                return;
            }

            thisButton.onClick.RemoveListener(OnClicked);

            hasInitialized = false;
        }

        private void OnClicked()
        {
            thisCtx.Action();
            thisHandler?.OnSelected();
        }
    }
}
