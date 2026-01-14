using TMPro;
using UnityEngine;

namespace Core.UI
{
    using static CoreUtility;

    public class UITextBox : MonoBehaviour
    {
        [Header("_")]
        [SerializeField] private RectTransform canvasBounds = null;

        [Header("_")]
        [SerializeField, Required] private RectTransform textContainer = null;
        [SerializeField, Required] private TextMeshProUGUI textElement = null;
        [SerializeField] private Vector2 textOffset = new(16, 16);

        private string cachedText = STRING_EMPTY;

        private void Awake() => textElement.textWrappingMode = TextWrappingModes.NoWrap;
        public void Set(string value)
        {
            if (cachedText == value)
            {
                return;
            }

            textElement.SetText(cachedText = value);
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
