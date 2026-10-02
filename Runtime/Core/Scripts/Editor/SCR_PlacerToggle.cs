using System;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine.UIElements;

namespace Core.Editor
{
    internal static partial class Placer
    {
        private const string PREFS_KEY = "Placer.Enabled";

        public static event Action<bool> OnStateChanged;

        public static bool IsEnabled() => isEnabled;
        public static void SetEnabled(bool value)
        {
            if (isEnabled == value)
            {
                return;
            }

            isEnabled = value;

            EditorPrefs.SetBool(PREFS_KEY, value);

            if (!isEnabled && placerMode != Mode.NONE)
            {
                End(false);
            }

            OnStateChanged?.Invoke(isEnabled);

            SceneView.RepaintAll();
        }

        private static bool isEnabled = true;
    }

    [EditorToolbarElement(TOOLBAR_ID, typeof(SceneView))]
    internal sealed class PlacerToolbarToggle : EditorToolbarToggle
    {
        public const string TOOLBAR_ID = "Placer/Toggle";

        public PlacerToolbarToggle()
        {
            text = "Placer";
            tooltip = "Placer: G (taþý) / E (döndür) / R (ölçekle) kýsayollarýný aç/kapat";

            SetValueWithoutNotify(Placer.IsEnabled());

            this.RegisterValueChangedCallback(OnValueChanged);

            RegisterCallback<AttachToPanelEvent>(OnAttach);
            RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }

        private void OnValueChanged(ChangeEvent<bool> evt)
        {
            Placer.SetEnabled(evt.newValue);
        }
        private void OnAttach(AttachToPanelEvent evt)
        {
            Placer.OnStateChanged += OnStateChanged;
            Sync();
        }
        private void OnDetach(DetachFromPanelEvent evt)
        {
            Placer.OnStateChanged -= OnStateChanged;
        }
        private void OnStateChanged(bool state)
        {
            SetValueWithoutNotify(state);
        }
        private void Sync() => OnStateChanged(Placer.IsEnabled());
    }
}
