using System.Collections.Generic;
using UnityEngine;

namespace Core.Sector
{
    public sealed class Sector
    {
        public readonly int ID;
        public readonly Vector3 Position;
        public readonly Vector3 Size;
        public readonly Dictionary<string, DataNode> Data;

        public Sector(int id, Vector3 position, Vector3 size, Dictionary<string, DataNode> data)
        {
            ID = id;
            Position = position;
            Size = size;
            Data = data ?? new();
        }
    }
}
