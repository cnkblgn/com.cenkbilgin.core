using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Core
{
    internal sealed class DataSerializer
    {
        private const int VERSION = 1;

        public static byte[] Serialize(Dictionary<string, DataNode> data)
        {
            using MemoryStream memory = new();
            using BinaryWriter writer = new(memory);

            writer.Write(VERSION);

            WriteNodes(writer, data);

            return memory.ToArray();
        }
        public static Dictionary<string, DataNode> Deserialize(byte[] bytes)
        {
            using MemoryStream memory = new(bytes);
            using BinaryReader reader = new(memory);

            int version = reader.ReadInt32();

            return version switch
            {
                1 => ReadNodes(reader),
                _ => throw new Exception($"Unsupported version: {version}")
            };
        }

        private static void WriteNodes(BinaryWriter writer, Dictionary<string, DataNode> data)
        {
            writer.Write(data.Count);

            foreach (var kv in data)
            {
                writer.Write(kv.Key);
                WriteNode(writer, kv.Value);
            }
        }
        private static Dictionary<string, DataNode> ReadNodes(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            Dictionary<string, DataNode> result = new(count);

            for (int i = 0; i < count; i++)
            {
                string key = reader.ReadString();
                result[key] = ReadNode(reader);
            }

            return result;
        }

        private static void WriteNode(BinaryWriter writer, DataNode node)
        {
            writer.Write((byte)node.Type);

            switch (node.Type)
            {
                case DataType.FLOAT:
                    writer.Write(node.Value.Float);
                    break;
                case DataType.LONG:
                    writer.Write(node.Value.Long);
                    break;
                case DataType.INT:
                    writer.Write(node.Value.Int);
                    break;
                case DataType.BOOL:
                    writer.Write(node.Value.Bool);
                    break;
                case DataType.STRING:
                    writer.Write(node.Value.String ?? "");
                    break;
                case DataType.VECTOR2:
                    writer.Write(node.Value.Vector2.x);
                    writer.Write(node.Value.Vector2.y);
                    break;
                case DataType.VECTOR3:
                    writer.Write(node.Value.Vector3.x);
                    writer.Write(node.Value.Vector3.y);
                    writer.Write(node.Value.Vector3.z);
                    break;
                case DataType.GUID:
                    writer.Write(node.Value.Guid.ToByteArray());
                    break;
                case DataType.DATA:
                    WriteNodes(writer, node.Value.Data);
                    break;
            }
        }
        private static DataNode ReadNode(BinaryReader reader)
        {
            DataType type = (DataType)reader.ReadByte();
            DataNode node = new() { Type = type };

            switch (type)
            {
                case DataType.FLOAT:
                    node.Value.Float = reader.ReadSingle(); 
                    break;
                case DataType.LONG:
                    node.Value.Long = reader.ReadInt64(); 
                    break;
                case DataType.INT:
                    node.Value.Int = reader.ReadInt32(); 
                    break;
                case DataType.BOOL:
                    node.Value.Bool = reader.ReadBoolean(); 
                    break;
                case DataType.STRING:
                    node.Value.String = reader.ReadString(); 
                    break;
                case DataType.VECTOR2:
                    node.Value.Vector2 = new Vector2(reader.ReadSingle(), reader.ReadSingle()); 
                    break;
                case DataType.VECTOR3:
                    node.Value.Vector3 = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()); 
                    break;
                case DataType.GUID:
                    node.Value.Guid = new Guid(reader.ReadBytes(16)); 
                    break;
                case DataType.DATA:
                    node.Value.Data = ReadNodes(reader);
                    break;
            }

            return node;
        }   
    }
}
