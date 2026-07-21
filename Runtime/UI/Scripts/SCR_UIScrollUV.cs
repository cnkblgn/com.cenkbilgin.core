using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public sealed class UIScrollUV : MonoBehaviour
    {
        [Header("_")]
        [SerializeField, Required] private RawImage image = null;

        [Header("_")]
        [SerializeField, Range(0, 1), Tooltip("x, y")] private int direction = 0;
        [SerializeField, Min(0)] private float speed = 16;

        private Rect rect = new();

        private void Awake() => rect = image.uvRect;
        private void LateUpdate()
        {
            if (ManagerGame.Instance.GetGameState() != GameState.RESUME)
            {
                return;
            }

            if (direction == 0)
            {
                rect.x += speed * Time.deltaTime;
            }
            else
            {
                rect.y += speed * Time.deltaTime;
            }

            image.uvRect = rect;
        }
    }
}
