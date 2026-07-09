using UnityEngine;

namespace Core.Actor
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
        public ulong Mask { get; private set; }

        [Header("_")]
        [SerializeField] private ActorID id;
        [SerializeField] private ActorTag[] tags;

        private void Awake()
        {
            Mask = tags.CreateMask();

            ActorDatabase.RegisterActor(id, this);
        }
        private void OnDestroy() => ActorDatabase.RemoveActor(id);
    }
}
