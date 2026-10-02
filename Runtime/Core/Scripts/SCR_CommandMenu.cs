using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    using static CoreUtility;

    public static class CommandMenu
    {
        private const float WINDOW_WIDTH = 760f;
        private const float WINDOW_HEIGHT = 520f;
        private const float LEFT_PANEL_WIDTH = 150f;

        private const string TRUE_TEXT = "True";
        private const string FALSE_TEXT = "False";

        public static bool IsVisible => isVisible;

        private static readonly List<Category> categories = new();
        private static readonly Dictionary<string, Category> categoryLookup = new();
        private static Rect windowRect;
        private static GUIStyle windowStyle;
        private static GUIStyle categoryButtonStyle;
        private static GUIStyle categorySelectedStyle;
        private static GUIStyle headerStyle;
        private static GUIStyle labelStyle;
        private static GUIStyle valueStyle;
        private static GUIStyle buttonStyle;
        private static Updater runner;
        private static readonly GUILayoutOption lineHeight = GUILayout.Height(1f);
        private static readonly GUILayoutOption labelWidth = GUILayout.Width(130f);
        private static readonly GUILayoutOption valueWidth = GUILayout.Width(55f);
        private static readonly GUILayoutOption valueMinWidth = GUILayout.MinWidth(100f);
        private static readonly GUILayoutOption closeWidth = GUILayout.Width(70f);
        private static readonly GUILayoutOption leftPanelWidth = GUILayout.Width(LEFT_PANEL_WIDTH);
        private static readonly GUI.WindowFunction windowFunction = DrawWindow;

        private static bool isInitialized;
        private static bool isVisible;
        private static int selectedCategory;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            if (runner != null)
            {
                UnityEngine.Object.DestroyImmediate(runner.gameObject);
                runner = null;
            }

            categories.Clear();
            categoryLookup.Clear();

            isInitialized = false;
            isVisible = false;
            selectedCategory = 0;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
            if (runner != null)
            {
                return;
            }

            GameObject updater = new("[Debug Menu]") { hideFlags = HideFlags.HideAndDontSave };
            runner = updater.AddComponent<Updater>();

            UnityEngine.Object.DontDestroyOnLoad(updater);
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;

            windowRect = new(40f, 40f, WINDOW_WIDTH, WINDOW_HEIGHT);

            GetCategory("General");
        }

        private static void ValidateStyles()
        {
            if (windowStyle != null)
            {
                return;
            }

            windowStyle = new GUIStyle(GUI.skin.window)
            {
                padding = new RectOffset(10, 10, 25, 10)
            };

            categoryButtonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(12, 8, 6, 6)
            };

            categorySelectedStyle = new GUIStyle(categoryButtonStyle)
            {
                fontStyle = FontStyle.Bold
            };

            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 14,
                margin = new RectOffset(0, 5, 8, 5)
            };

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft
            };

            valueStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleRight
            };

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fixedHeight = 28f
            };
        }

        public static void Show() => isVisible = true;
        public static void Hide() => isVisible = false;
        public static void Toggle() => isVisible = !isVisible;
        public static void Clear(string category)
        {
            if (string.IsNullOrEmpty(category))
            {
                return;
            }

            if (categoryLookup.TryGetValue(category, out Category entry))
            {
                entry.Entries.Clear();
                entry.HasRemoved = false;
            }
        }
        private static Category GetCategory(string name)
        {
            if (categoryLookup.TryGetValue(name, out Category existing))
            {
                return existing;
            }

            Category category = new(name);

            categories.Add(category);
            categoryLookup.Add(name, category);

            return category;
        }

        public static void AddButton(string category, string label, Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action), "Add entry failed! action is null!?");
            }

            Add(category, new ButtonEntry(label, action));
        }
        public static void AddToggle(string category, string label, Func<bool> getter, Action<bool> setter)
        {
            if (getter == null)
            {
                throw new ArgumentNullException(nameof(getter), "Add command entry failed! getter is null!?");
            }

            if (setter == null)
            {
                throw new ArgumentNullException(nameof(setter), "Add command entry failed! setter is null!?");
            }

            Add(category, new ToggleEntry(label, getter, setter));
        }
        public static void AddSlider(string category, string label, Func<float> getter, Action<float> setter, float min, float max, bool isInt = false)
        {
            if (getter == null)
            {
                throw new ArgumentNullException(nameof(getter), "Add command entry failed! getter is null!?");
            }

            if (setter == null)
            {
                throw new ArgumentNullException(nameof(setter), "Add command entry failed! setter is null!?");
            }

            Add(category, new SliderEntry(label, getter, setter, min, max, isInt));
        }
        public static void AddNumberField(string category, string label, Func<float> getter, Action<float> setter, bool isInt = false)
        {
            if (getter == null)
            {
                throw new ArgumentNullException(nameof(getter), "Add command entry failed! getter is null!?");
            }

            if (setter == null)
            {
                throw new ArgumentNullException(nameof(setter), "Add command entry failed! setter is null!?");
            }

            Add(category, new NumberFieldEntry(label, getter, setter, isInt));
        }
        public static void AddTextField(string category, string label, Func<string> getter, Action<string> setter)
        {
            if (getter == null)
            {
                throw new ArgumentNullException(nameof(getter), "Add command entry failed! getter is null!?");
            }

            if (setter == null)
            {
                throw new ArgumentNullException(nameof(setter), "Add command entry failed! setter is null!?");
            }

            Add(category, new TextFieldEntry(label, getter, setter));
        }
        public static void AddEnum<T>(string category, string label, Func<T> getter, Action<T> setter) where T : struct, Enum
        {
            if (getter == null)
            {
                throw new ArgumentNullException(nameof(getter), "Add command entry failed! getter is null!?");
            }

            if (setter == null)
            {
                throw new ArgumentNullException(nameof(setter), "Add command entry failed! setter is null!?");
            }

            Add(category, new EnumEntry<T>(label, getter, setter));
        }
        public static void AddFloat(string category, string label, Func<float> getter)
        {
            if (getter == null)
            {
                throw new ArgumentNullException(nameof(getter), "Add command entry failed! getter is null!?");
            }

            Add(category, new FloatValueEntry(label, getter));
        }
        public static void AddInt(string category, string label, Func<int> getter)
        {
            if (getter == null)
            {
                throw new ArgumentNullException(nameof(getter), "Add command entry failed! getter is null!?");
            }

            Add(category, new IntValueEntry(label, getter));
        }
        public static void AddBool(string category, string label, Func<bool> getter)
        {
            if (getter == null)
            {
                throw new ArgumentNullException(nameof(getter), "Add command entry failed! getter is null!?");
            }

            Add(category, new BoolValueEntry(label, getter));
        }
        public static void AddString(string category, string label, Func<string> getter)
        {
            if (getter == null)
            {
                throw new ArgumentNullException(nameof(getter), "Add command entry failed! getter is null!?");
            }

            Add(category, new StringValueEntry(label, getter));
        }
        public static void AddVector2(string category, string label, Func<Vector2> getter)
        {
            if (getter == null)
            {
                throw new ArgumentNullException(nameof(getter), "Add command entry failed! getter is null!?");
            }

            Add(category, new Vector2ValueEntry(label, getter));
        }
        public static void AddVector3(string category, string label, Func<Vector3> getter)
        {
            if (getter == null)
            {
                throw new ArgumentNullException(nameof(getter), "Add command entry failed! getter is null!?");
            }

            Add(category, new Vector3ValueEntry(label, getter));
        }
        public static void AddLabel(string category, string text) => Add(category, new LabelEntry(text));
        public static void AddHeader(string category, string text) => Add(category, new HeaderEntry(text));
        public static void AddSeparator(string category) => Add(category, new SeparatorEntry());
        public static void AddSpace(string category, float amount = 8f) => Add(category, new SpaceEntry(amount));
        private static void Add(string category, Entry entry)
        {
            if (string.IsNullOrEmpty(category))
            {
                throw new ArgumentException("Command menu add failed! Category cannot be null or empty.", nameof(category));
            }

            Category target = GetCategory(category);

            if (!string.IsNullOrEmpty(entry.Label))
            {
                for (int i = 0; i < target.Entries.Count; i++)
                {
                    Entry existing = target.Entries[i];

                    if (!existing.Removed && existing.Label == entry.Label)
                    {
                        target.Entries[i] = entry;
                        return;
                    }
                }
            }

            target.Entries.Add(entry);
        }
        public static bool Remove(string category, string label)
        {
            if (string.IsNullOrEmpty(category) || string.IsNullOrEmpty(label))
            {
                return false;
            }

            if (!categoryLookup.TryGetValue(category, out Category entry))
            {
                return false;
            }

            for (int i = 0; i < entry.Entries.Count; i++)
            {
                Entry item = entry.Entries[i];

                if (!item.Removed && item.Label == label)
                {
                    item.Removed = true;
                    entry.HasRemoved = true;
                    return true;
                }
            }

            return false;
        }

        public static void Label(string text)
        {
            GUILayout.Label(text, labelStyle);
        }
        public static void Header(string text)
        {
            GUILayout.Space(4f);
            GUILayout.Label(text, headerStyle);
        }
        public static void Space(float amount = 8f)
        {
            GUILayout.Space(amount);
        }
        public static void Separator()
        {
            GUILayout.Space(4f);
            GUI.DrawTexture(GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, lineHeight), Texture2D.whiteTexture);
            GUILayout.Space(4f);
        }

        private static void Draw()
        {
            ValidateStyles();

            windowRect = GUI.Window(928374, windowRect, windowFunction, "DEBUG MENU", windowStyle);
        }
        private static void DrawWindow(int id)
        {
            GUILayout.BeginHorizontal();
            {
                DrawCategories();

                GUILayout.BeginVertical();
                {
                    if (categories.Count > 0 && selectedCategory >= 0 && selectedCategory < categories.Count)
                    {
                        DrawCategory(categories[selectedCategory]);
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6f);

            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Close", closeWidth))
                {
                    Hide();
                }
            }
            GUILayout.EndHorizontal();

            GUI.DragWindow(new(0f, 0f, 10000f, 24f));
        }
        private static void DrawCategory(Category category)
        {
            GUILayout.Label(category.Name, headerStyle);

            Separator();

            List<Entry> entries = category.Entries;

            for (int i = 0; i < entries.Count; i++)
            {
                Entry entry = entries[i];

                if (!entry.Removed)
                {
                    entry.Draw();
                }
            }

            if (category.HasRemoved && Event.current.type == EventType.Repaint)
            {
                for (int i = entries.Count - 1; i >= 0; i--)
                {
                    if (entries[i].Removed)
                    {
                        entries.RemoveAt(i);
                    }
                }

                category.HasRemoved = false;
            }
        }
        private static void DrawCategories()
        {
            GUILayout.BeginVertical(leftPanelWidth);
            {
                for (int i = 0; i < categories.Count; i++)
                {
                    Category category = categories[i];
                    GUIStyle style = i == selectedCategory ? categorySelectedStyle : categoryButtonStyle;

                    if (GUILayout.Button(category.Name, style))
                    {
                        selectedCategory = i;
                    }
                }
            }
            GUILayout.EndVertical();
        }
        private static void DrawValueRow(string label, string text)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(label, labelStyle);
                GUILayout.FlexibleSpace();
                GUILayout.Label(text, valueStyle, valueMinWidth);
            }
            GUILayout.EndHorizontal();
        }

        private sealed class Updater : MonoBehaviour
        {
            private void OnGUI()
            {
                if (!isVisible)
                {
                    return;
                }

                Draw();
            }
        }
        private sealed class Category
        {
            public readonly string Name;
            public readonly List<Entry> Entries = new();
            public bool HasRemoved;

            public Category(string name)
            {
                Name = name ?? throw new ArgumentNullException(nameof(name));
            }
        }
        private abstract class Entry
        {
            public readonly string Label;
            public bool Removed;

            protected Entry(string label)
            {
                Label = label;
            }
            public abstract void Draw();
        }
        private sealed class LabelEntry : Entry
        {
            public LabelEntry(string text) : base(text) { }
            public override void Draw() => GUILayout.Label(Label, labelStyle);
        }
        private sealed class HeaderEntry : Entry
        {
            public HeaderEntry(string text) : base(text) { }
            public override void Draw()
            {
                GUILayout.Space(4f);
                GUILayout.Label(Label, headerStyle);
            }
        }
        private sealed class SeparatorEntry : Entry
        {
            public SeparatorEntry() : base(null) { }
            public override void Draw() => Separator();
        }
        private sealed class SpaceEntry : Entry
        {
            private readonly float amount;

            public SpaceEntry(float amount) : base(null)
            {
                this.amount = amount;
            }
            public override void Draw() => GUILayout.Space(amount);
        }
        private sealed class ButtonEntry : Entry
        {
            private readonly Action action;

            public ButtonEntry(string label, Action action) : base(label)
            {
                this.action = action;
            }
            public override void Draw()
            {
                if (GUILayout.Button(Label, buttonStyle))
                {
                    action.Invoke();
                }
            }
        }
        private sealed class ToggleEntry : Entry
        {
            private readonly Func<bool> getter;
            private readonly Action<bool> setter;

            public ToggleEntry(string label, Func<bool> getter, Action<bool> setter) : base(label)
            {
                this.getter = getter;
                this.setter = setter;
            }
            public override void Draw()
            {
                bool value = getter();
                bool next = GUILayout.Toggle(value, Label);

                if (next != value)
                {
                    setter(next);
                }
            }
        }
        private sealed class SliderEntry : Entry
        {
            private readonly Func<float> getter;
            private readonly Action<float> setter;
            private readonly float min;
            private readonly float max;
            private readonly bool isInt;
            private float shownValue = float.NaN;
            private string shownText = string.Empty;

            public SliderEntry(string label, Func<float> getter, Action<float> setter, float min, float max, bool isInt) : base(label)
            {
                this.getter = getter;
                this.setter = setter;
                this.min = min;
                this.max = max;
                this.isInt = isInt;
            }
            public override void Draw()
            {
                float value = getter();
                float next;

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(Label, labelStyle, labelWidth);

                    next = GUILayout.HorizontalSlider(value, min, max);

                    if (isInt)
                    {
                        next = Mathf.RoundToInt(next);
                    }

                    // String sadece deðer deðiþince üretilir
                    if (next != shownValue)
                    {
                        shownValue = next;
                        shownText = next.ToString(isInt ? "0" : "0.00");
                    }

                    GUILayout.Label(shownText, valueStyle, valueWidth);
                }
                GUILayout.EndHorizontal();

                if (!Mathf.Approximately(value, next))
                {
                    setter(next);
                }
            }
        }
        private sealed class NumberFieldEntry : Entry
        {
            private readonly Func<float> getter;
            private readonly Action<float> setter;
            private readonly bool isInt;

            private float lastValue = float.NaN;
            private string buffer = string.Empty;

            public NumberFieldEntry(string label, Func<float> getter, Action<float> setter, bool isInt) : base(label)
            {
                this.getter = getter;
                this.setter = setter;
                this.isInt = isInt;
            }

            public override void Draw()
            {
                float value = getter();

                if (value != lastValue)
                {
                    lastValue = value;
                    buffer = value.ToString(isInt ? "0" : "0.00");
                }

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(Label, labelStyle, labelWidth);

                    string text = GUILayout.TextField(buffer);

                    if (text != buffer)
                    {
                        buffer = text;

                        if (isInt)
                        {
                            if (int.TryParse(text, out int parsedInt))
                            {
                                lastValue = parsedInt;
                                setter(parsedInt);
                            }
                        }
                        else if (float.TryParse(text, out float parsedFloat))
                        {
                            lastValue = parsedFloat;
                            setter(parsedFloat);
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }
        }
        private sealed class TextFieldEntry : Entry
        {
            private readonly Func<string> getter;
            private readonly Action<string> setter;

            public TextFieldEntry(string label, Func<string> getter, Action<string> setter) : base(label)
            {
                this.getter = getter;
                this.setter = setter;
            }
            public override void Draw()
            {
                string value = getter() ?? string.Empty;

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(Label, labelStyle, labelWidth);

                    string next = GUILayout.TextField(value);

                    if (next != value)
                    {
                        setter(next);
                    }
                }
                GUILayout.EndHorizontal();
            }
        }
        private sealed class EnumEntry<T> : Entry where T : struct, Enum
        {
            private static readonly T[] Values = (T[])System.Enum.GetValues(typeof(T));
            private static readonly string[] Names = System.Enum.GetNames(typeof(T));

            private readonly Func<T> getter;
            private readonly Action<T> setter;

            public EnumEntry(string label, Func<T> getter, Action<T> setter) : base(label)
            {
                this.getter = getter;
                this.setter = setter;
            }
            public override void Draw()
            {
                int current = Array.IndexOf(Values, getter());

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(Label, labelStyle, labelWidth);

                    int selected = GUILayout.SelectionGrid(current, Names, 1);

                    if (selected != current && selected >= 0 && selected < Values.Length)
                    {
                        setter(Values[selected]);
                    }
                }
                GUILayout.EndHorizontal();
            }
        }
        private sealed class FloatValueEntry : Entry
        {
            private readonly Func<float> getter;
            private float last = float.NaN;
            private string text = string.Empty;

            public FloatValueEntry(string label, Func<float> getter) : base(label)
            {
                this.getter = getter;
            }
            public override void Draw()
            {
                float value = getter();

                if (value != last)
                {
                    last = value;
                    text = value.ToString("0.00");
                }

                DrawValueRow(Label, text);
            }
        }
        private sealed class IntValueEntry : Entry
        {
            private readonly Func<int> getter;
            private int last;
            private bool hasLast;
            private string text = string.Empty;

            public IntValueEntry(string label, Func<int> getter) : base(label)
            {
                this.getter = getter;
            }
            public override void Draw()
            {
                int value = getter();

                if (!hasLast || value != last)
                {
                    hasLast = true;
                    last = value;
                    text = value.ToString();
                }

                DrawValueRow(Label, text);
            }
        }
        private sealed class BoolValueEntry : Entry
        {
            private readonly Func<bool> getter;

            public BoolValueEntry(string label, Func<bool> getter) : base(label)
            {
                this.getter = getter;
            }
            public override void Draw() => DrawValueRow(Label, getter() ? TRUE_TEXT : FALSE_TEXT);
        }
        private sealed class StringValueEntry : Entry
        {
            private readonly Func<string> getter;

            public StringValueEntry(string label, Func<string> getter) : base(label)
            {
                this.getter = getter;
            }
            public override void Draw() => DrawValueRow(Label, getter() ?? STRING_NULL);
        }
        private sealed class Vector2ValueEntry : Entry
        {
            private readonly Func<Vector2> getter;
            private Vector2 last = new(float.NaN, float.NaN);
            private string text = string.Empty;

            public Vector2ValueEntry(string label, Func<Vector2> getter) : base(label)
            {
                this.getter = getter;
            }
            public override void Draw()
            {
                Vector2 value = getter();

                if (value.x != last.x || value.y != last.y)
                {
                    last = value;
                    text = $"X {value.x:0.00}  Y {value.y:0.00}";
                }

                DrawValueRow(Label, text);
            }
        }
        private sealed class Vector3ValueEntry : Entry
        {
            private readonly Func<Vector3> getter;
            private Vector3 last = new(float.NaN, float.NaN, float.NaN);
            private string text = string.Empty;

            public Vector3ValueEntry(string label, Func<Vector3> getter) : base(label)
            {
                this.getter = getter;
            }
            public override void Draw()
            {
                Vector3 value = getter();

                if (value.x != last.x || value.y != last.y || value.z != last.z)
                {
                    last = value;
                    text = $"X {value.x:0.00}  Y {value.y:0.00}  Z {value.z:0.00}";
                }

                DrawValueRow(Label, text);
            }
        }
    }
}