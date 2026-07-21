using UnityEngine;

namespace Core.Damage
{
    public delegate bool DamageProcessor(ulong tags, Collider collider, Vector3 direction, float damage);
}
