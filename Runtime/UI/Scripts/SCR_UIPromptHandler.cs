namespace Core.UI
{
    public interface IUIPromptHandler
    {
        public void Hide();
        public void Accept();
        public void Cancel();
    }

    public interface IUIPromptHandler<TContext> : IUIPromptHandler where TContext : struct
    {
        public void Show(in TContext context);
    }
}