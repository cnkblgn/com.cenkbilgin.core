using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    [Serializable]
    public struct DataValue
    {
        public float Float;
        public long Long;
        public int Int;
        public bool Bool;
        public string String;
        public Vector3 Vector3;
        public Vector2 Vector2;
        public Guid Guid;

        public Dictionary<string, DataNode> Data;
    }
}