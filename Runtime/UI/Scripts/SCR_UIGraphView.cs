using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class UIGraphView : Graphic
    {
        [Header("_")]
        [Info("Maximum available points on screen")]
        [SerializeField, Range(2, 256)] private int resolution = 8;
        [SerializeField, Range(1, 30)] private float refreshRate = 15f;

        [Header("_")]
        [Info("Given point at any time system tries to normalize points based on min/max")]
        [SerializeField] private bool autoRange = true;
        [SerializeField] private float manualMin = 0f;
        [SerializeField] private float manualMax = 100f;
        [SerializeField, Range(0f, 0.5f)] private float rangePadding = 0.1f;

        [Header("_")]
        [SerializeField] private Color lineColor = new(0.2f, 1f, 0.9f);
        [SerializeField, Range(1f, 10f)] private float lineThickness = 3f;

        [Header("_")]
        [SerializeField] private bool fillGraph = true;
        [SerializeField] private Color fillColor = new(0.2f, 1f, 0.9f, 0.2f);

        [Header("_")]
        [SerializeField] private bool drawPoints = true;
        [SerializeField] private Color pointColor = new(1f, 1f, 1f, 1f);
        [SerializeField, Range(1f, 20f)] private float pointRadius = 5f;
        [SerializeField, Range(4, 16)] private int pointSegments = 8;

        [Header("_")]
        [SerializeField] private bool drawGrid = true;
        [SerializeField] private Color gridColor = new(0.3f, 0.9f, 1f, 0.15f);
        [SerializeField, Min(2)] private int gridLinesX = 4;
        [SerializeField, Min(2)] private int gridLinesY = 4;

        private Vector2[] cache;
        private float[] buffer;
        private float time;
        private int bufferHead;
        private int bufferCount;
        private bool isDirty;

        protected override void Awake()
        {
            base.Awake();

            EnsureBuffer();
        }
        private void Update()
        {
            if (!isDirty)
            {
                return;
            }

            time += Time.unscaledDeltaTime;

            float interval = refreshRate <= 0f ? 0f : 1f / refreshRate;

            if (time >= interval)
            {
                time = 0f;
                isDirty = false;

                SetVerticesDirty();
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            EnsureBuffer();
            isDirty = true;
        }
#endif
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            EnsureBuffer();

            Rect rect = GetPixelAdjustedRect();

            if (drawGrid)
            {
                DrawGrid(vh, rect);
            }

            if (bufferCount < 2)
            {
                return;
            }

            GetRange(out float min, out float max);

            float range = Mathf.Max(0.0001f, max - min);

            int n = bufferCount;
            float stepX = rect.width / Mathf.Max(1, buffer.Length - 1);
            float startX = rect.xMin + (buffer.Length - n) * stepX;

            for (int i = 0; i < n; i++)
            {
                float v = GetValue(i);
                float x = startX + i * stepX;
                float y = rect.yMin + (v - min) / range * rect.height;
                cache[i] = new Vector2(x, y);
            }

            if (fillGraph)
            {
                DrawFill(vh, rect, cache, n);
            }

            DrawLine(vh, cache, n, lineColor, lineThickness);

            if (drawPoints)
            {
                DrawPoints(vh, cache, n);
            }
        }

        private void EnsureBuffer()
        {
            if (buffer == null || buffer.Length != resolution)
            {
                buffer = new float[Mathf.Max(2, resolution)];
                cache = new Vector2[buffer.Length];
                bufferHead = 0;
                bufferCount = 0;
            }
        }

        public void Clear()
        {
            EnsureBuffer();

            bufferHead = 0;
            bufferCount = 0;
            isDirty = true;
        }
        public void SetRange(float min, float max)
        {
            autoRange = false;
            manualMin = min;
            manualMax = max;
            isDirty = true;
        }
        private void GetRange(out float min, out float max)
        {
            if (!autoRange)
            {
                min = manualMin;
                max = manualMax;
                return;
            }

            min = float.MaxValue;
            max = float.MinValue;

            for (int i = 0; i < bufferCount; i++)
            {
                float v = GetValue(i);

                if (v < min)
                {
                    min = v;
                }

                if (v > max)
                {
                    max = v;
                }
            }

            if (min == float.MaxValue)
            {
                min = 0f;
                max = 1f;
            }

            if (Mathf.Approximately(min, max))
            {
                min -= 1f;
                max += 1f;
            }

            float pad = (max - min) * rangePadding;

            min -= pad;
            max += pad;
        }
        public void PushValue(float value)
        {
            EnsureBuffer();

            buffer[bufferHead] = value;
            bufferHead = (bufferHead + 1) % buffer.Length;

            if (bufferCount < buffer.Length)
            {
                bufferCount++;
            }

            isDirty = true;
        }
        private float GetValue(int index)
        {
            int start = (bufferHead - bufferCount + buffer.Length) % buffer.Length;

            return buffer[(start + index) % buffer.Length];
        }

        private void DrawLine(VertexHelper vh, Vector2[] points, int count, Color color, float thickness)
        {
            for (int i = 1; i < count; i++)
            {
                AddLineQuad(vh, points[i - 1], points[i], color, thickness);
            }
        }
        private void DrawFill(VertexHelper vh, Rect r, Vector2[] points, int count)
        {
            float baseline = r.yMin;

            for (int i = 1; i < count; i++)
            {
                Vector2 a = points[i - 1];
                Vector2 b = points[i];

                int idx = vh.currentVertCount;
                UIVertex v = UIVertex.simpleVert;
                v.color = fillColor;

                v.position = new Vector2(a.x, baseline); vh.AddVert(v);
                v.position = a; vh.AddVert(v);
                v.position = b; vh.AddVert(v);
                v.position = new Vector2(b.x, baseline); vh.AddVert(v);

                vh.AddTriangle(idx, idx + 1, idx + 2);
                vh.AddTriangle(idx, idx + 2, idx + 3);
            }
        }
        private void DrawPoints(VertexHelper vh, Vector2[] points, int count)
        {
            for (int i = 0; i < count; i++)
            {
                AddCircle(vh, points[i], pointColor, pointRadius, pointSegments);
            }
        }
        private void AddCircle(VertexHelper vh, Vector2 center, Color color, float radius, int segments)
        {
            int idx = vh.currentVertCount;

            UIVertex v = UIVertex.simpleVert;
            v.color = color;
            v.position = center;
            vh.AddVert(v);

            for (int i = 0; i <= segments; i++)
            {
                float angle = (i / (float)segments) * Mathf.PI * 2f;
                v.position = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                vh.AddVert(v);
            }

            for (int i = 1; i <= segments; i++)
            {
                vh.AddTriangle(idx, idx + i, idx + i + 1);
            }
        }
        private void AddLineQuad(VertexHelper vh, Vector2 a, Vector2 b, Color color, float thickness)
        {
            Vector2 direction = b - a;
            float length = direction.magnitude;

            if (length < 0.0001f)
            {
                return;
            }

            direction /= length;
            Vector2 normal = new Vector2(-direction.y, direction.x) * (thickness * 0.5f);

            int idx = vh.currentVertCount;
            UIVertex v = UIVertex.simpleVert;
            v.color = color;

            v.position = a - normal; vh.AddVert(v);
            v.position = a + normal; vh.AddVert(v);
            v.position = b + normal; vh.AddVert(v);
            v.position = b - normal; vh.AddVert(v);

            vh.AddTriangle(idx, idx + 1, idx + 2);
            vh.AddTriangle(idx, idx + 2, idx + 3);
        }
        private void DrawGrid(VertexHelper vh, Rect r)
        {
            for (int i = 1; i < gridLinesX; i++)
            {
                float x = r.xMin + r.width * i / gridLinesX;
                AddLineQuad(vh, new(x, r.yMin), new(x, r.yMax), gridColor, 1f);
            }

            for (int i = 1; i < gridLinesY; i++)
            {
                float y = r.yMin + r.height * i / gridLinesY;
                AddLineQuad(vh, new(r.xMin, y), new(r.xMax, y), gridColor, 1f);
            }
        }
    }
}