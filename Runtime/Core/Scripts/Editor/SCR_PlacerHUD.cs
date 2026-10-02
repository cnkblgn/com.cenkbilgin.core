using System.Text;
using UnityEditor;
using UnityEngine;

namespace Core.Editor
{
    using static CoreUtility;

    internal static partial class Placer
    {
        private const float HUD_PADDING = 8f;

        private static readonly Vector2 hudOrigin = new(16f, 16f);
        private static readonly Color hudColor = new(0f, 0f, 0f, 0.75f);
        private static GUIStyle hudStyle;

        private static void DrawHudText(string text, Vector2 origin)
        {
            if (hudStyle == null)
            {
                hudStyle = new(EditorStyles.helpBox) { richText = true, fontSize = 12, wordWrap = false };
                hudStyle.normal.textColor = Color.white;
            }

            GUIContent content = new(text);

            Vector2 size = hudStyle.CalcSize(content);
            Rect background = new(origin.x, origin.y, size.x + HUD_PADDING * 2f, size.y + HUD_PADDING * 2f);

            EditorGUI.DrawRect(background, hudColor);

            GUI.Label(new Rect(background.x + HUD_PADDING, background.y + HUD_PADDING, size.x, size.y), content, hudStyle);
        }
        private static void DrawHudIdle()
        {
            StringBuilder sb = new();

            sb.AppendLine("Placer".ToBold());
            sb.Append(KEY_GRAB.ToString().ToYellow()).AppendLine(" < Grab");
            sb.Append(KEY_ROTATE.ToString().ToYellow()).AppendLine(" < Rotate");
            sb.Append(KEY_SCALE.ToString().ToYellow()).AppendLine(" < Scale");
            sb.Append(KEY_SIMULATE.ToString().ToYellow()).AppendLine(" < Simulate");

            Handles.BeginGUI();

            DrawHudText(sb.ToString().TrimEnd(), hudOrigin);

            Handles.EndGUI();
        }
        private static void DrawHudActive()
        {
            if (placerAxis != Axis.NONE)
            {
                Vector3 dir = GetDirection(placerAxis);
                Handles.color = placerAxis == Axis.X ? Color.red : placerAxis == Axis.Y ? Color.green : Color.blue;
                Handles.DrawLine(pivotWorld - dir * 1000f, pivotWorld + dir * 1000f);
            }

            if (isSurfaceActive)
            {
                Handles.color = Color.cyan;
                Handles.DrawLine(surfaceHit, surfaceHit + surfaceNormal * 0.5f);
                Handles.DrawWireDisc(surfaceHit, surfaceNormal, 0.15f);
            }

            Handles.BeginGUI();

            if (!HasNumber && (placerMode == Mode.ROTATE || placerMode == Mode.SCALE))
            {
                Handles.color = Color.white;
                Handles.DrawDottedLine(new(pivotScreen.x, pivotScreen.y, 0f), new(lastMouse.x, lastMouse.y, 0f), 4f);
            }

            string axisText = placerAxis == Axis.NONE ? "Free" : placerAxis.ToString();
            string unitText = HasNumber ? placerMode == Mode.ROTATE ? " °" : placerMode == Mode.GRAB ? " u" : " x" : "";

            string line1 = "<b>" + placerMode + "</b>  |  Axis: " + axisText + "  |  " + description + unitText;
            string line2 =  "<color=#B0B0B0>LMB/Enter: confirm   RMB/Esc: cancel   Ctrl: snap   0-9: exact value" + (placerMode == Mode.GRAB ? "   Shift: surface snap   N: align normal (" + (alignToNormal ? "on" : "off") + ")" : "") + "</color>";
            string desc = line1 + "\n" + line2;

            DrawHudText(desc, hudOrigin);

            Handles.EndGUI();
        }
    }
}
