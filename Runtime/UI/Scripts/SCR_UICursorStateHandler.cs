namespace Core.UI
{
    public interface IUICursorStateHandler
    {
        /// <summary> Called when game wants to show cursor, return 'true' if you give permission to show </summary>
        public bool HandleCanShowCursor();
        /// <summary> Called when game wants to hide cursor, return 'true' if you give permission to hide </summary>
        public bool HandleCanHideCursor();
    }
}