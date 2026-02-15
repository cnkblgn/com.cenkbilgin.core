using UnityEngine;

namespace Core.Misc
{
    public class PhysicsBone
    {
        public readonly Collider Collider;
        public readonly Rigidbody Rigidbody;

        public PhysicsBone(Rigidbody rigidbody)
        {
            Rigidbody = rigidbody;
            Collider = Rigidbody.GetComponent<Collider>();
        }
    }
}
