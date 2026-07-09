using UnityEngine;

namespace Core
{
    using static CoreUtility;

    [CreateAssetMenu(fileName = "SCO_ActorDatabase", menuName = "Resources/Actor Database Config", order = 10)]
    public class ActorDatabaseConfig : ScriptableObject
    {
        [Header("_")]
        [SerializeField] private string[] ids;
        [SerializeField] private string[] tags;

        [Clickable("Build")]
        public void Build()
        {
            ActorDatabase.Build(ids, tags);

            GenerateScriptDatabase(this, "Core", "ActorTag", ActorDatabase.GetTags(), "SCR_GeneratedActorTag.cs", true, true);
            GenerateScriptDatabase(this, "Core", "ActorID", ActorDatabase.GetIDs(), "SCR_GeneratedActorID.cs", true, false);
        }
    }
}
