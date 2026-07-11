using UnityEngine;

namespace Core.UI
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public sealed class UIScrollRect : MonoBehaviour
    {
        [Header("_")]
        [SerializeField, Required] private RectTransform[] transforms = null;

        [Header("_")]
        [SerializeField, Range(0, 1), Tooltip("x, y")] private int direction = 0;
        [SerializeField] private float speed = 100f;
        [SerializeField] private float spacing = 40f;

        private Rect[] rects;
        private float offset;

        private void Start()
        {
            rects = new Rect[transforms.Length];
            float offset = 0f;

            for (int i = 0; i < transforms.Length; i++)
            {
                Vector2 pos = transforms[i].anchoredPosition;
                rects[i] = transforms[i].rect;

                if (direction == 0)
                {
                    pos.x = offset;
                    offset += rects[i].width + spacing;
                }
                else
                {
                    pos.y = offset;
                    offset += rects[i].height + spacing;
                }

                transforms[i].anchoredPosition = pos;
            }

            this.offset = offset;
        }
        private void LateUpdate()
        {
            if (ManagerGame.Instance.GetGameState() != GameState.RESUME)
            {
                return;
            }

            Vector2 dir = direction == 0 ? Vector2.left : Vector2.down;

            for (int i = 0; i < transforms.Length; i++)
            {
                RectTransform t = transforms[i];
                Rect r = rects[i];

                t.anchoredPosition += speed * Time.deltaTime * dir;

                if (IsOutOfBounds(t.anchoredPosition.x, t.anchoredPosition.y, r.width, r.height))
                {
                    ResetToEnd(transforms[i]);
                }
            }
        }

        private bool IsOutOfBounds(float x, float y, float width, float height)
        {
            if (IsHorizontal())
            {
                return x + width < 0;
            }

            return y + height < 0;
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
