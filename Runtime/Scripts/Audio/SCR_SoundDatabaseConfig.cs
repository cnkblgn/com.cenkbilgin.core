
using System.Linq;
using UnityEngine;

namespace Core.Audio
{
    using static CoreUtility;

    [CreateAssetMenu(fileName = "SCO_SoundDatabase", menuName = "Resources/Sound Database Config", order = 0)]
    public sealed class SoundDatabaseConfig : ScriptableObject
    {
        [Header("_")]
        [SerializeField, Required] private AudioClip[] collection = null;

        [Clickable("Build")]
        public void Build()
        {
            SoundDatabase.Build(collection);

            GenerateScriptDatabase(this, "Core.Audio", "SoundID", SoundDatabase.GetIDs().Zip(SoundDatabase.GetIndices(), (key, index) => (key, index)), "SCR_GeneratedSoundID.cs", true);
        }
    }
}
