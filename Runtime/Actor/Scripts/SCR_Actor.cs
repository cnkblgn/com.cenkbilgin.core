using UnityEngine;

namespace Core.Actors
{
    using static CoreUtility;

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
#if UNITY_EDITOR
        private static GUIStyle GizmosStyle
        {
            get
            {
                gizmosStyle ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, };
                gizmosStyle.normal.textColor = COLOR_GREEN;

                return gizmosStyle;
            }
        } private static GUIStyle gizmosStyle;

        private void OnDrawGizmos() => DrawGizmos(selected: false);
        private void OnDrawGizmosSelected() => DrawGizmos(selected: true);

        private void DrawGizmos(bool selected)
        {
            Gizmos.color = selected ? Color.red : Color.greenYellow;
            Gizmos.DrawWireSphere(Origin.position, 0.1f);

            using (new UnityEditor.Handles.DrawingScope())
            {
                UnityEditor.Handles.Label(Origin.position + Vector3.up, id.Key, GizmosStyle);
            }
        }
#endif
    }
}
