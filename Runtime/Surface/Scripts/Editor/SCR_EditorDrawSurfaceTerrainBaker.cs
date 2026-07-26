using UnityEditor;

namespace Core.Surface.Editor
{
    [CustomEditor(typeof(SurfaceTerrainBaker), true)]
    internal sealed class EditorDrawSurfaceTerrainBaker : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            SurfaceTerrainBaker baker = target as SurfaceTerrainBaker;
            Core.Editor.EditorUtility.DrawButton("Bake", baker, baker.Bake);
        }
    }
}
