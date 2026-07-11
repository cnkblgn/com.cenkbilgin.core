using UnityEngine;

namespace Core
{
    using static CoreUtility;

    [CreateAssetMenu(fileName = "SCO_ActorRegistry", menuName = "Resources/Actor Registry", order = 10)]
    internal sealed class RegistryActor : Registry
    {
        [Header("_")]
        [SerializeField] private string[] ids;
        [SerializeField] private string[] tags;

        public override void OnAwake() => ActorDatabase.Build(ids, tags);
        public override void OnInitialize() => Generate();

#if UNITY_EDITOR
        [Clickable("Build")]
        public void Generate()
        {
            ActorDatabase.Build(ids, tags);

            GenerateScriptDatabase(this, "Core", "ActorTag", ActorDatabase.GetTags(), "SCR_GeneratedActorTag.cs", true, true);
            GenerateScriptDatabase(this, "Core", "ActorID", ActorDatabase.GetIDs(), "SCR_GeneratedActorID.cs", true, false);
        }
#endif
    }
}
