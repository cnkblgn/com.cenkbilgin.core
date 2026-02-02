using UnityEngine;
using Newtonsoft.Json;

namespace Core.Misc
{
    using static CoreUtility;

    public class GameProgress
    {
        [JsonProperty("0")] public readonly string Name = STRING_EMPTY;
        [JsonProperty("1")] public Float3 PlayerPosition = Float3.zero;
        [JsonProperty("2")] public Float3 PlayerRotation = Float3.zero;
        [JsonProperty("3")] public PersistentSceneData SceneData = new();

        [JsonConstructor]
        public GameProgress([JsonProperty("0")] string name, [JsonProperty("1")] Float3 playerPosition, [JsonProperty("2")] Float3 playerRotation, [JsonProperty("3")] PersistentSceneData sceneData)
        {
            Name = name;
            PlayerPosition = playerPosition;
            PlayerRotation = playerRotation;
            SceneData = new(sceneData.ID, sceneData.Database, sceneData.Hashset);
        }
        public GameProgress() { }
    }
}
