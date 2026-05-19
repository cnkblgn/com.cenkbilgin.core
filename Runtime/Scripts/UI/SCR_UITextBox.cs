using TMPro;
using UnityEngine;

namespace Core.UI
{
    using static CoreUtility;

    public class UITextBox : MonoBehaviour
    {
        public string Text { get; private set; } = STRING_EMPTY;

        [Header("_")]
        [SerializeField] private RectTransform canvasBounds = null;

        [Header("_")]
        [SerializeField, Required] private RectTransform textContainer = null;
        [SerializeField, Required] private TextMeshProUGUI textElement = null;
        [SerializeField] private Vector2 textOffset = new(16, 16);

        private void Awake() => textElement.textWrappingMode = TextWrappingModes.NoWrap;
        public void Set(string value)
        {
            if (Text == value)
            {
                return;
            }

            textElement.SetText(Text = value);
            textContainer.sizeDelta = textElement.GetPreferredValues() + textOffset;
            UpdateBounds(textContainer.anchoredPosition);
        } 
        public void UpdateBounds(Vector2 desiredPosition)
        {
            if (canvasBounds == null)
            {
                return;
            }

            textContainer.ClampToView(canvasBounds, desiredPosition);
        }
    }
}
