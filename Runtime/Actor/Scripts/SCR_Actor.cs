using System;
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
        public ActorMask Mask { get; private set; }

        [Header("_")]
        [SerializeField] private ActorID id;
        [SerializeField] private ActorTag[] tags;

        private Transform origin;
        private bool isRegistered;

        private void Awake() => Refresh();
        private void OnDestroy() => Unregister();

        private void Refresh()
        {
            Unregister();

            Mask = ActorTag.CreateMask(tags);

            Register();
        }

        private void Register()
        {
            if (isRegistered)
            {
                return;
            }

            ActorDatabase.RegisterActor(id, this);
            isRegistered = true;
        }
        private void Unregister()
        {
            if (!isRegistered)
            {
                return;
            }

            ActorDatabase.RemoveActor(this);
            isRegistered = false;
        }

        private void OverrideID(ActorID id)
        {
            if (!id.IsValid)
            {
                throw new ArgumentNullException(nameof(id), $"Actor override failed! {id} is not valid!");
            }

            this.id = id;
        }
        private void OverrideTags(ActorTag[] tags)
        {
            if (tags == null)
            {
                throw new ArgumentNullException(nameof(tags), $"Actor override failed! tags is not valid!");
            }

            this.tags = tags;
        }
        public void Override(ActorID id)
        {
            OverrideID(id);
            Refresh();
        }
        public void Override(ActorTag[] tags)
        {
            OverrideTags(tags);
            Refresh();
        }
        public void Override(ActorID id, ActorTag[] tags)
        {
            OverrideID(id);
            OverrideTags(tags);
            Refresh();
        }
    }
}
