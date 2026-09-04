namespace Core.Item
{
    public readonly struct ItemActionResult
    {
        public readonly ItemActionResultType Type;

        public ItemActionResult(ItemActionResultType type) => Type = type;

        public static ItemActionResult Completed => new(ItemActionResultType.Completed);
        public static ItemActionResult RequiresAmount => new(ItemActionResultType.RequiresAmount);
        public static ItemActionResult RequiresTarget => new(ItemActionResultType.RequiresTarget);
    }
}