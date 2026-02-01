namespace Core.Input
{
    public static class InputActionDatabase
    {
        // ===== GLOBAL (ALWAYS ENABLED) =====
        public static readonly InputActionType Any = new("Global.Any");
        public static readonly InputActionType Menu = new("Global.Menu");
        public static readonly InputActionType Tab = new("Global.Tab");
        public static readonly InputActionType Console = new("Global.Console");

        // ===== GAMEPLAY =====
        public static readonly InputActionType Move = new("Gameplay.Move");
        public static readonly InputActionType Look = new("Gameplay.Look");
        public static readonly InputActionType Primary = new("Gameplay.Primary");
        public static readonly InputActionType Secondary = new("Gameplay.Secondary");
        public static readonly InputActionType Jump = new("Gameplay.Jump");
        public static readonly InputActionType Sprint = new("Gameplay.Sprint");
        public static readonly InputActionType Crouch = new("Gameplay.Crouch");
        public static readonly InputActionType Interact = new("Gameplay.Interact");
        public static readonly InputActionType Walk = new("Gameplay.Walk");

        // ===== UI =====
        public static readonly InputActionType UIUp = new("UI.NavigateUp");
        public static readonly InputActionType UIDown = new("UI.NavigateDown");
        public static readonly InputActionType UIRight = new("UI.NavigateRight");
        public static readonly InputActionType UILeft = new("UI.NavigateLeft");
        public static readonly InputActionType Submit = new("UI.Submit");
        public static readonly InputActionType Cancel = new("UI.Cancel");
        public static readonly InputActionType Context = new("UI.Context");
    }
}