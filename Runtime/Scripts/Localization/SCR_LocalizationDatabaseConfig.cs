using UnityEngine;

namespace Core.Localization
{
    using static CoreUtility;

    [CreateAssetMenu(fileName = "SCO_LocalizationDatabase", menuName = "Resources/Localization Database Config", order = 0)]
    internal sealed class LocalizationDatabaseConfig : ScriptableObject
    {
        [Header("_")]
        [SerializeField, Required] private TextAsset file = null;

        [Clickable("Parse")]
        public bool TryParse() => LocalizationDatabase.TryParse(file);
    }
}
