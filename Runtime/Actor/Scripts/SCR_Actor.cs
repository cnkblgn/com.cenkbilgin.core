using UnityEngine;

namespace Core.Actors
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-1)]
    public sealed class Actor : MonoBehaviour
    {
        public Transform Origin
        {
            get
            {
                if (origin == null)
                {
                    origin = transform;
                }

                return origin;
            }
        } 
        public ActorID ID => id;
        public ulong Tags { get; private set; }

        [Header("_")]
        [SerializeField] private ActorID id;
        [SerializeField] private ActorTag[] tags;

        private Transform origin = null;

        private void Awake()
        {
            Tags = ActorTag.CreateMask(tags);

            ActorDatabase.RegisterActor(id, this);
        }
        private void OnDestroy() => ActorDatabase.RemoveActor(this);
    }
}
