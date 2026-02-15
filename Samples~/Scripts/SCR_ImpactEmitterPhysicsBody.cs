using UnityEngine;
using Core;
using Core.Audio;

namespace Game
{
    using static CoreUtility;

    [RequireComponent(typeof(PhysicsBody))]
    public class ImpactEmitterPhysicsBody : MonoBehaviour
    {
        [Header("_")]
        [SerializeField, Required] private ImpactConfig impactConfig = null;

        private PhysicsBody thisBody = null;

        private void Awake() => thisBody = GetComponent<PhysicsBody>();
        private void OnEnable() => thisBody.OnCollision += OnBodyCollision;
        private void OnDisable() => thisBody.OnCollision -= OnBodyCollision;
        private void OnBodyCollision(Collision collision)
        {
            if (collision.relativeVelocity.sqrMagnitude < 1)
            {
                return;
            }

            ContactPoint contactPoint = collision.GetContact(0);
            impactConfig.Spawn(collision.collider, contactPoint.point, contactPoint.normal, AudioGroup.EFFECT);
        }
    }
}
