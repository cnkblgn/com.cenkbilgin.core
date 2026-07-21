using UnityEngine;
using TMPro;

namespace Core.UI
{
    using static CoreUtility;

    public sealed class UITextBox : MonoBehaviour
    {
        public string Text { get; private set; } = STRING_EMPTY;

        [Header("_")]
        [SerializeField] private bool overrideWrapping = true;

        [Header("_")]
        [SerializeField] private RectTransform canvasBounds = null;
        [SerializeField, Required] private RectTransform textContainer = null;
        [SerializeField, Required] private TextMeshProUGUI textElement = null;
        [SerializeField] private Vector2 textOffset = new(16, 16);

        private void Awake()
        {
            if (overrideWrapping)
            {
                textElement.textWrappingMode = TextWrappingModes.NoWrap;
            }
        }

        public void Set(string value)
        {
            if (Text == value)
            {
                return;
            }

            textElement.text = Text = value;

            if (overrideWrapping)
            {
                textContainer.sizeDelta = textElement.GetPreferredValues() + textOffset;
                UpdateBounds(textContainer.anchoredPosition);
            }
        } 
        public void UpdateBounds(Vector2 desiredPosition)
        {
            if (canvasBounds == null)
            {
                textContainer.anchoredPosition = desiredPosition;
                return;
            }

            textContainer.ClampToView(canvasBounds, desiredPosition);
        }
    }
}
