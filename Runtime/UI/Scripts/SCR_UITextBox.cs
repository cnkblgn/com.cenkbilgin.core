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
        [Info("'Canvas Bounds' pivot must be (0,0)")]
        [SerializeField] private RectTransform canvasBounds = null;
        [SerializeField, Required] private RectTransform textContainer = null;
        [SerializeField, Required] private TextMeshProUGUI textElement = null;
        [SerializeField] private Vector2 textOffset = new(16, 16);

        private void Awake()
        {
            textContainer.AlignBottomLeft();

            if (overrideWrapping)
            {
                textElement.textWrappingMode = TextWrappingModes.NoWrap;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (canvasBounds == null)
            {
                return;
            }

            if (canvasBounds != null && canvasBounds.pivot != Vector2.zero)
            {
                Debug.LogWarning($"UI Text box target canvas bound pivot is invalid: [{canvasBounds.pivot}] Pivot must be (0,0)", gameObject);
            }
        }
#endif

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
        public void UpdateBounds(Vector2 desiredPosition, Camera camera = null)
        {
            if (canvasBounds == null)
            {
                textContainer.anchoredPosition = desiredPosition;
                return;
            }

            textContainer.ClampToView(canvasBounds, desiredPosition, camera);
        }
    }
}
