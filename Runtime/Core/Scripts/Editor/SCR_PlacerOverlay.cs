using UnityEditor;
using UnityEditor.Overlays;

namespace Core.Editor
{      
    [Overlay(typeof(SceneView), OVERLAY_ID, OVERLAY_NAME, defaultDisplay = true, defaultDockZone = DockZone.TopToolbar, defaultDockPosition = DockPosition.Bottom)]
    internal sealed class PlacerToolbarOverlay : ToolbarOverlay
    {
        private const string OVERLAY_ID = "Placer.ToggleOverlay";
        private const string OVERLAY_NAME = "Placer";

        public PlacerToolbarOverlay() : base(PlacerToolbarToggle.TOOLBAR_ID) { }
    }
}
