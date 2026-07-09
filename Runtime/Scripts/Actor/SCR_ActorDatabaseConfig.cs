using UnityEngine;

namespace Core.Actor
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

            GenerateScriptDatabase(this, "Core.Actor", "ActorID", ActorDatabase.GetIDs(), "SCR_GeneratedActorID.cs", true);
        }
    }
}
