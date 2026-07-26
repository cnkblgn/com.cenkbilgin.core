using UnityEditor;
using UnityEngine;

namespace Core.Editor
{
    [CustomPropertyDrawer(typeof(Info))]
    public class EditorDrawInfo : DecoratorDrawer
    {
        public override float GetHeight()
        {
            GUIContent content = new(((Info)attribute).Text);

            float width = Mathf.Max(Screen.width - 60f, 100f);

            return EditorStyles.helpBox.CalcHeight(content, width) + 6f;
        }

        public override void OnGUI(Rect position)
        {
            position.y += 2f;
            position.height -= 4f;

            EditorGUI.HelpBox(position, ((Info)attribute).Text, MessageType.Info);
        }
    }
}
