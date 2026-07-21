using UnityEngine;

namespace Core.Damage
{
    public readonly struct DamageData
    {
        public readonly Transform Owner;
        public readonly Collider Collider;
        public readonly Vector3 Position;
        public readonly Vector3 Normal;
        public readonly Vector3 Direction;
        public readonly DamageMode Mode;
        public readonly ulong Tags;
        public readonly uint Context;
        public readonly float Radius;
        public readonly float Damage;
        public readonly float Force;

        public DamageData(Transform owner, Collider collider, Vector3 position, Vector3 normal, Vector3 direction, DamageMode mode, ulong tags, uint context, float radius, float damage, float force)
        {
            Owner = owner;
            Collider = collider;
            Position = position;
            Normal = normal;
            Direction = direction;
            Mode = mode;
            Tags = tags;
            Context = context;
            Radius = radius;
            Damage = damage;
            Force = force;
        }
        public DamageData(Transform owner, Collider collider, Vector3 position, Vector3 normal, Vector3 direction, DamageMode mode, DamageTag[] tags, uint context, float radius, float damage, float force) : this(owner, collider, position, normal, direction, mode, tags.CreateMask(), context, radius, damage, force) { }
    }
}
