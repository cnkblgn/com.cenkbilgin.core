using UnityEngine;

namespace Core
{
    public class Actor : MonoBehaviour
    {
        public Transform ThisTransform
        {
            get
            {
                if (thisTransform == null)
                {
                    thisTransform = transform;
                }

                return thisTransform;
            }
        } private Transform thisTransform = null;

        public ActorID ID => id;
        public ulong Tags { get; private set; }

        [Header("_")]
        [SerializeField] private ActorID id = ActorID.NONE;
        [SerializeField] private ActorTag[] tags;

        private void Awake()
        {
            Tags = ActorTag.CreateMask(tags);
            ActorDatabase.RegisterActor(id, this);
        }
        private void OnDestroy() => ActorDatabase.RemoveActor(this);
    }
}
