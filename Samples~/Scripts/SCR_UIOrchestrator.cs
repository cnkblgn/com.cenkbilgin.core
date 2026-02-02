using UnityEngine;
using Core;
using Core.UI;
using Core.Input;

namespace Game
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public class UIOrchestrator : MonoBehaviour
    {
        [Header("_")]
        [SerializeField, Required] private Canvas gameCanvas = null;
        [SerializeField, Required] private Canvas pauseCanvas = null;

        private void LateUpdate() => ManagerCoreUI.Instance.MoveCursor(ManagerCoreInput.Instance.PointerPosition);
        private void OnEnable()
        {
            ManagerCoreGame.OnBeforeSceneChanged += OnBeforeSceneChanged;
            ManagerCoreGame.OnGameStateChanged += OnGameStateChanged;
        }
        private void OnDisable()
        {
            ManagerCoreGame.OnBeforeSceneChanged -= OnBeforeSceneChanged;
            ManagerCoreGame.OnGameStateChanged -= OnGameStateChanged;
        }


        private void OnBeforeSceneChanged(string _)
        {
            ManagerCoreUI.Instance.ClearMessage();
        }

        private void OnGameStateChanged(GameState value)
        {
            switch (value)
            {
                case GameState.NULL:
                    break;
                case GameState.RESUME:
                    pauseCanvas.Hide();
                    gameCanvas.Show();


                    ManagerCoreUI.Instance.ShowWaypoint();
                    ManagerCoreUI.Instance.ShowMessage();
                    ManagerCoreUI.Instance.ShowCrosshair();
                    ManagerCoreUI.Instance.HideCursor();

                    break;
                case GameState.PAUSE:
                    pauseCanvas.Show();
                    gameCanvas.Hide();


                    ManagerCoreUI.Instance.HideWaypoint();
                    ManagerCoreUI.Instance.HideMessage();
                    ManagerCoreUI.Instance.HideCrosshair();
                    ManagerCoreUI.Instance.ShowCursor();
                    break;
                default:
                    break;
            }
        }
    }
}