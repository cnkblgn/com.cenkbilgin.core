using System;

namespace Game
{
    [Flags]
    public enum MaterialID
    {
        DEFAULT = 1 << 0,
        STONE = 1 << 1,
        WOOD = 1 << 2,
        DIRT = 1 << 3,
        METAL = 1 << 4,
        CLOTH = 1 << 5,
        FLESH = 1 << 6,
    }
}