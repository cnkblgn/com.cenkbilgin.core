using UnityEditor;
using UnityEngine;

namespace Core.Editor
{
    [CustomPropertyDrawer(typeof(Info))]
    public class EditorDrawInfo : DecoratorDrawer
    {
        private static readonly GUIStyle GUI_STYLE = new(EditorStyles.helpBox);

        public override float GetHeight()
        {
            GUIContent content = new(((Info)attribute).Text);

            float height = GUI_STYLE.CalcHeight(content, EditorGUIUtility.currentViewWidth - 40f);

            return Mathf.Max(height, 20f) + 6f;
        }

        public override void OnGUI(Rect position)
        {
            position.y += 2f;
            position.height -= 4f;

            EditorGUI.HelpBox(position, ((Info)attribute).Text, MessageType.Info);
        }
    }
}
