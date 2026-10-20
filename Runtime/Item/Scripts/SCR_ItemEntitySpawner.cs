using UnityEngine;

namespace Core.Item
{
    public delegate GameObject ItemEntitySpawner(string prefabID, Vector3 position, Quaternion rotation, Transform parent);
}
