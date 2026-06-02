using Core.Input;

public static class InputDatabase
{
    // ===== GLOBAL (ALWAYS ENABLED) =====
    public static readonly InputAction Any = new("Global.Any");
    public static readonly InputAction Menu = new("Global.Menu");
    public static readonly InputAction Tab = new("Global.Tab");
    public static readonly InputAction Console = new("Global.Console");

    // ===== GAMEPLAY =====
    public static readonly InputAction Move = new("Gameplay.Move");
    public static readonly InputAction Look = new("Gameplay.Look");
    public static readonly InputAction Primary = new("Gameplay.Primary");
    public static readonly InputAction Secondary = new("Gameplay.Secondary");
    public static readonly InputAction Jump = new("Gameplay.Jump");
    public static readonly InputAction Sprint = new("Gameplay.Sprint");
    public static readonly InputAction Crouch = new("Gameplay.Crouch");
    public static readonly InputAction Interact = new("Gameplay.Interact");
    public static readonly InputAction Walk = new("Gameplay.Walk");
    public static readonly InputAction LeanLeft = new("Gameplay.LeanLeft");
    public static readonly InputAction LeanRight = new("Gameplay.LeanRight");
    public static readonly InputAction PDA = new("Gameplay.PDA");
    public static readonly InputAction Suit = new("Gameplay.Suit");
    public static readonly InputAction Flashlight = new("Gameplay.Flashlight");
    public static readonly InputAction Placement = new("Gameplay.Placement");
    public static readonly InputAction Slots = new("Gameplay.Slots");
    public static readonly InputAction Drop = new("Gameplay.Drop");
    public static readonly InputAction Grab = new("Gameplay.Grab");

    // ===== UI =====
    public static readonly InputAction UIUp = new("UI.NavigateUp");
    public static readonly InputAction UIDown = new("UI.NavigateDown");
    public static readonly InputAction UIRight = new("UI.NavigateRight");
    public static readonly InputAction UILeft = new("UI.NavigateLeft");
    public static readonly InputAction UISubmit = new("UI.Submit");
    public static readonly InputAction UICancel = new("UI.Cancel");
    public static readonly InputAction UIContext = new("UI.Context");
    public static readonly InputAction UIClick = new("UI.Click");
}
