using UnityEngine;
using UnityEngine.UI;
using Core.Graphics;

namespace Core.UI
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(LayoutGroup))]
    public class UILayoutRebuilder : MonoBehaviour
    {
        private LayoutGroup thisLayout = null;

        private void Awake() => thisLayout = GetComponent<LayoutGroup>();
        private void Start() => thisLayout.enabled = false;
        private void OnEnable()
        {
            this.WaitUntil(() => ManagerCoreGraphics.Instance != null, null, () =>
            {
                ManagerCoreGraphics.Instance.OnResolutionChanged += OnResolutionChanged;
            });
        }
        private void OnDisable()
        {
            if (ManagerCoreGraphics.Instance != null)
            {
                ManagerCoreGraphics.Instance.OnResolutionChanged -= OnResolutionChanged;
            }
        }
        private void OnResolutionChanged(Int2 resolution)
        {
            thisLayout.enabled = true;
            LayoutRebuilder.ForceRebuildLayoutImmediate(thisLayout.transform as RectTransform);
            thisLayout.enabled = false;
        }
    }
}