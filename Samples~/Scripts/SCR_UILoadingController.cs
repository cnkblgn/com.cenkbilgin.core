using UnityEngine;
using Core;
using Core.UI;

namespace Game
{
    using static CoreUtility;
    using static TaskUtility;

    [RequireComponent(typeof(Canvas))]
    public class UILoadingController : MonoBehaviour
    {
        private Canvas thisCanvas = null;

        private void Awake()
        {
            thisCanvas = GetComponent<Canvas>();
            thisCanvas.Hide();

            thisCanvas.overrideSorting = true;
            thisCanvas.sortingOrder = 10;
        }
        private void OnEnable()
        {
            ManagerCoreGame.OnBeforeSceneChanged += OnBeforeSceneChanged;
            ManagerCoreGame.OnAfterSceneChanged += OnAfterSceneChanged;
        }
        private void OnDisable()
        {
            ManagerCoreGame.OnBeforeSceneChanged -= OnBeforeSceneChanged;
            ManagerCoreGame.OnAfterSceneChanged -= OnAfterSceneChanged;
        }

        private void OnBeforeSceneChanged(string _)
        {
            thisCanvas.Show();
        }
        private void OnAfterSceneChanged(string _)
        {
            thisCanvas.Hide();
            ManagerCoreUI.Instance.ShowTransitionFadeOut(1f, 0f, null, null);
        }
    }
}
