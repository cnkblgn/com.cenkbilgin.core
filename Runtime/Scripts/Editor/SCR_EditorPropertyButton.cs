using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Core.Editor
{
    [CustomEditor(typeof(Object), true)]
    public class ButtonAttributeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            MethodInfo[] methods = target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (MethodInfo method in methods)
            {
                CoreUtility.EditorButton attribute = method.GetCustomAttribute<CoreUtility.EditorButton>();

                if (attribute == null)
                {
                    continue;
                }

                string label = string.IsNullOrEmpty(attribute.Label) ? method.Name : attribute.Label;

                if (GUILayout.Button(label))
                {
                    method.Invoke(target, null);

                    UnityEditor.EditorUtility.SetDirty(target);
                }
            }
        }
    }
}
