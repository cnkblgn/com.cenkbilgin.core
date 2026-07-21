using UnityEngine;

namespace Core
{
    public interface IGameStateHandler
    {
        /// <summary> Called when game wants to resume, return 'true' if you give access to resume game </summary>
        public bool HandleCanResumeGame();
        /// <summary> Called when game wants to pause, return 'true' if you give access to pause game </summary>
        public bool HandleCanPauseGame();
    }
}
