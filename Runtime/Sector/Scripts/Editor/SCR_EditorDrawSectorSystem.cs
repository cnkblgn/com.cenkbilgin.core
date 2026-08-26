using UnityEditor;

namespace Core.Sector.Editor
{
    [CustomEditor(typeof(SectorSystem), true)]
    internal sealed class EditorDrawSectorSystem : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            SectorSystem sector = target as SectorSystem;

            Core.Editor.EditorUtility.DrawButton("Initialize", sector, sector.Initialize);
        }
    }
}