using UnityEngine;
using Core;
using Core.Input;

namespace Game
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public class InputOrchestrator : MonoBehaviour
    {
        private void OnEnable() => ManagerCoreGame.OnGameStateChanged += OnGameStateChanged;
        private void OnDisable() => ManagerCoreGame.OnGameStateChanged -= OnGameStateChanged;

        private void OnGameStateChanged(GameState value)
        {
            switch (value)
            {
                case GameState.NULL:
                    ManagerCoreInput.Instance.SwitchMap("UI");
                    break;
                case GameState.RESUME:
                    ManagerCoreInput.Instance.SwitchMap("Gameplay");
                    break;
                case GameState.PAUSE:
                    ManagerCoreInput.Instance.SwitchMap("UI");
                    break;
                default:
                    break;
            }
        }
    }
}
