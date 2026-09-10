namespace Core.UI
{
    public interface IUIPromptHandler
    {
        public void HandleHide();
        public void HandleAccept();
        public void HandleCancel();
    }

    public interface IUIPromptHandler<TContext> : IUIPromptHandler where TContext : struct
    {
        public void HandleShow(in TContext context);
    }
}