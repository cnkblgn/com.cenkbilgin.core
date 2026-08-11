using UnityEditor;
using Core.Editor;

namespace Core.Quest.Editor
{
    using static CoreUtility;

    [CustomPropertyDrawer(typeof(QuestID))]
    internal sealed class EditorDrawSearchableQuestID : EditorDrawSearchable<string>
    {
        protected override string GetEmpty() => STRING_EMPTY;

        protected override string GetKey() => "key";
        protected override SearchCollection<string> GetKeys() => QuestDatabase.GetIDs();

        protected override string GetValue(SerializedProperty keyProperty) => keyProperty.stringValue;
        protected override void SetValue(SerializedProperty keyProperty, string value) => keyProperty.stringValue = value;
    }
}
