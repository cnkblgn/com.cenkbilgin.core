using UnityEngine;
using TMPro;

namespace Core.UI
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public class UIScrollingElement : MonoBehaviour
    {
        [Header("_")]
        [SerializeField, Required] private TMP_Text[] texts = null;

        [Header("_")]
        [SerializeField, Range(0, 1), Tooltip("x, y")] private int direction = 0;
        [SerializeField] private float speed = 100f;
        [SerializeField] private float spacing = 40f;

        private RectTransform[] rects;
        private float offset;

        private void Start()
        {
            rects = new RectTransform[texts.Length];

            float offset = 0f;

            for (int i = 0; i < texts.Length; i++)
            {
                rects[i] = texts[i].rectTransform;
                Vector2 pos = rects[i].anchoredPosition;

                if (direction == 0)
                {
                    pos.x = offset;
                    offset += rects[i].rect.width + spacing;
                }
                else
                {
                    pos.y = offset;
                    offset += rects[i].rect.height + spacing;
                }

                rects[i].anchoredPosition = pos;
            }

            this.offset = offset;
        }
        private void LateUpdate()
        {
            if (ManagerCoreGame.Instance.GetGameState() != GameState.RESUME)
            {
                return;
            }

            Vector2 dir = direction == 0 ? Vector2.left : Vector2.down;

            for (int i = 0; i < rects.Length; i++)
            {
                rects[i].anchoredPosition += speed * Time.deltaTime * dir;

                if (IsOutOfBounds(rects[i]))
                {
                    ResetToEnd(rects[i]);
                }
            }
        }

        private bool IsOutOfBounds(RectTransform rect)
        {
            if (IsHorizontal())
            {
                return rect.anchoredPosition.x + rect.rect.width < 0;
            }

            return rect.anchoredPosition.y + rect.rect.height < 0;
        }
        private bool IsHorizontal() => direction == 0;

        private void ResetToEnd(RectTransform rect)
        {
            Vector2 position = rect.anchoredPosition;

            if (IsHorizontal())
            {
                position.x = offset;
            }
            else
            {
                position.y = offset;
            }

            rect.anchoredPosition = position;
        }
    }
}
