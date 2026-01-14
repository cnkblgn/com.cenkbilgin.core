using UnityEngine;

namespace Core.UI
{
    [RequireComponent(typeof(Canvas))]
    public class UILoadingCanvasController : MonoBehaviour
    {
        private Canvas thisCanvas = null;

        private void Awake()
        {
            thisCanvas = GetComponent<Canvas>();
            thisCanvas.Hide();
        }
        private void OnEnable()
        {
            this.WaitUntil(() => ManagerCoreGame.Instance != null, null, () =>
            {
                ManagerCoreGame.Instance.OnBeforeSceneChanged += OnBeforeSceneChanged;
                ManagerCoreGame.Instance.OnAfterSceneChanged += OnAfterSceneChanged;
            });
        }
        private void OnDisable()
        {
            if (ManagerCoreGame.Instance != null)
            {
                ManagerCoreGame.Instance.OnBeforeSceneChanged -= OnBeforeSceneChanged;
                ManagerCoreGame.Instance.OnAfterSceneChanged -= OnAfterSceneChanged;
            }
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
