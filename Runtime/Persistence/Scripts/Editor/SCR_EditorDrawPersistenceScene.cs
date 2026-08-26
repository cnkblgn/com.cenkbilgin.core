using UnityEditor;

namespace Core.Persistence.Editor
{
    [CustomEditor(typeof(PersistentScene), true)]
    internal sealed class EditorDrawPersistenceScene : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            PersistentScene scene = target as PersistentScene;

            Core.Editor.EditorUtility.DrawButton("Populate", scene, scene.Populate);
        }
    }
}
