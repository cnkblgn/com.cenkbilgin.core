using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace Core.Editor
{
    [InitializeOnLoad]
    internal static partial class Placer
    {
        private enum Mode { NONE, GRAB, ROTATE, SCALE, SIMULATE }
        private enum Axis { NONE, X, Y, Z }

        private const KeyCode KEY_GRAB = KeyCode.G;
        private const KeyCode KEY_ROTATE = KeyCode.E;
        private const KeyCode KEY_SCALE = KeyCode.R;
        private const KeyCode KEY_SIMULATE = KeyCode.L;

        private const int MAX_SIMULATION_STEPS = 500;
        private const int MIN_SIMULATION_STEPS = 10;      
        private const float SIMULATION_STEP_SECONDS = 0.02f;

        private static SceneView sceneView;

        private static Mode placerMode = Mode.NONE;
        private static Axis placerAxis = Axis.NONE;
        private static Transform[] placerTargets;
        private static Vector3[] startPositions;
        private static Quaternion[] startRotations;
        private static Vector3[] startScales;
        private static Vector3 pivotWorld;
        private static Vector2 pivotScreen;

        private static Plane grabberDragPlane;
        private static Vector3 grabberStartPlaneHit;
        private static Ray grabberStartRay;

        private static float rotaterStartDist;
        private static float rotaterLastAngle;
        private static float rotaterAccumAngle;

        private static string numberBuffer = "";
        private static bool HasNumber => numberBuffer.Length > 0;

        private static bool alignToNormal = true;
        private static bool isSurfaceActive;

        private static Vector3 surfaceHit;
        private static Vector3 surfaceNormal;

        private static Vector2 lastMouse;

        private static string description = "";

        static Placer()
        {
            isEnabled = EditorPrefs.GetBool(PREFS_KEY, false);
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            Placer.sceneView = sceneView;

            if (!isEnabled)
            {
                return;
            }

            Event @event = Event.current;

            if (placerMode == Mode.NONE)
            {
                if (@event.type == EventType.KeyDown
                    && !@event.alt && !@event.control && !@event.shift && !@event.command
                    && !EditorGUIUtility.editingTextField
                    && Tools.viewTool != ViewTool.FPS
                    && Selection.activeTransform != null)
                {
                    Mode mode = GetMode(@event.keyCode);

                    if (mode == Mode.SIMULATE)
                    {
                        Simulate();
                        @event.Use();
                    }
                    else if (mode != Mode.NONE)
                    {
                        if (Begin(@event, mode))
                        {
                            @event.Use();
                        }
                    }
                }

                if (@event.type == EventType.Repaint && Selection.activeTransform != null)
                {
                    DrawHudIdle();
                }
                return;
            }

            if (@event.type == EventType.Layout)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            }

            // Hedeflerden biri silindiyse güvenli çýk
            foreach (Transform target in placerTargets)
            {
                if (target == null) 
                { 
                    End(false); 
                    return; 
                }
            }

            switch (@event.type)
            {
                case EventType.MouseMove:
                case EventType.MouseDrag:
                    lastMouse = @event.mousePosition;

                    if (!HasNumber)
                    {
                        Apply(@event);
                    }

                    @event.Use();
                    sceneView.Repaint();
                    break;
                case EventType.MouseDown:
                    if (@event.button == 0)
                    {
                        End(true);
                    }
                    else if (@event.button == 1)
                    {
                        End(false);
                    }

                    @event.Use();
                    break;
                case EventType.KeyDown:
                    HandleKey(@event);
                    sceneView.Repaint();
                    break;
                case EventType.KeyUp:
                    if (@event.keyCode == KeyCode.LeftShift || @event.keyCode == KeyCode.RightShift)
                    {
                        Apply(@event);
                        sceneView.Repaint();
                    }
                    break;
                case EventType.Repaint:
                    DrawHudActive();
                    break;
            }
        }
        private static bool Begin(Event @event, Mode mode)
        {
            placerTargets = Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.Editable);

            if (placerTargets == null || placerTargets.Length == 0)
            {
                return false;
            }

            int totalTargets = placerTargets.Length;
            startPositions = new Vector3[totalTargets];
            startRotations = new Quaternion[totalTargets];
            startScales = new Vector3[totalTargets];
            pivotWorld = Vector3.zero;

            for (int i = 0; i < totalTargets; i++)
            {
                placerTargets[i].GetPositionRotationScale(out Vector3 position, out Quaternion rotation, out Vector3 scale);
                startPositions[i] = position;
                startRotations[i] = rotation;
                startScales[i] = scale;
                pivotWorld += startPositions[i];
            }

            pivotWorld /= totalTargets;

            placerMode = mode;
            placerAxis = Axis.NONE;
            numberBuffer = "";
            isSurfaceActive = false;

            if (!Rebaseline(@event))
            {
                placerMode = Mode.NONE;
                placerTargets = null;
                return false;
            }

            Undo.RecordObjects(placerTargets, "Placer Transform");
            sceneView.Repaint();
            return true;
        }
        private static void End(bool confirm)
        {
            if (!confirm && placerTargets != null)
            {
                RestoreTargets();
            }
            else
            {
                Undo.SetCurrentGroupName("Placer Transform");
            }

            placerMode = Mode.NONE;
            placerAxis = Axis.NONE;

            numberBuffer = "";

            isSurfaceActive = false;

            placerTargets = null;

            startPositions = null;
            startRotations = null;
            startScales = null;

            SceneView.RepaintAll();
        }
        private static void RestoreTargets()
        {
            for (int i = 0; i < placerTargets.Length; i++)
            {
                if (placerTargets[i] == null)
                {
                    continue;
                }

                placerTargets[i].SetPositionRotationScale(startPositions[i], startRotations[i], startScales[i]);
            }
        }

        private static bool Rebaseline(Event @event)
        {
            Vector2 mouse = @event.mousePosition;
            lastMouse = mouse;

            pivotScreen = HandleUtility.WorldToGUIPoint(pivotWorld);
            grabberStartRay = HandleUtility.GUIPointToWorldRay(mouse);

            Vector3 forward = sceneView.camera.transform.forward;
            grabberDragPlane = new Plane(-forward, pivotWorld);

            if (placerMode == Mode.GRAB)
            {
                if (!grabberDragPlane.Raycast(grabberStartRay, out float enter))
                {
                    return false;
                }

                grabberStartPlaneHit = grabberStartRay.GetPoint(enter);
            }

            Vector2 offset = mouse - pivotScreen;

            rotaterStartDist = Mathf.Max(offset.magnitude, 5f);
            rotaterLastAngle = GetAngle(offset);
            rotaterAccumAngle = 0f;

            description = "";
            return true;
        }
        private static void HandleKey(Event @event)
        {
            // Sayýsal giriþ
            if (TryHandleNumberKey(@event))
            {
                return;
            }

            switch (@event.keyCode)
            {
                case KeyCode.Escape:
                    End(false);
                    @event.Use();
                    break;
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    End(true);
                    @event.Use();
                    break;
                case KeyCode.LeftShift:
                case KeyCode.RightShift:
                    Apply(@event);
                    break;
                case KEY_GRAB:
                case KEY_ROTATE:
                case KEY_SCALE:
                {
                    Mode mode = GetMode(@event.keyCode);

                    if (mode == placerMode)
                    {
                        End(true);
                    }
                    else
                    {
                        RestoreTargets();
                        placerMode = mode;
                        placerAxis = Axis.NONE;
                        numberBuffer = "";

                        if (Rebaseline(@event))
                        {
                            Apply(@event);
                        }
                        else
                        {
                            End(false);
                        }
                    }

                    @event.Use();
                    break;
                }
                case KeyCode.N:
                    alignToNormal = !alignToNormal;
                    Apply(@event);
                    @event.Use();
                    break;
                case KeyCode.X: 
                    ToggleAxis(Axis.X, @event); 
                    break;
                case KeyCode.Y: 
                    ToggleAxis(Axis.Y, @event); 
                    break;
                case KeyCode.Z: 
                    ToggleAxis(Axis.Z, @event);
                    break;
            }
        }
        private static bool TryHandleNumberKey(Event @event)
        {
            const int MAX_LENGTH = 12;
            KeyCode key = @event.keyCode;

            int digit = -1;

            if (key >= KeyCode.Alpha0 && key <= KeyCode.Alpha9)
            {
                digit = (int)key - (int)KeyCode.Alpha0;
            }
            else if (key >= KeyCode.Keypad0 && key <= KeyCode.Keypad9)
            {
                digit = (int)key - (int)KeyCode.Keypad0;
            }

            if (digit >= 0)
            {
                if (numberBuffer.Length < MAX_LENGTH)
                {
                    numberBuffer += digit.ToString();
                }

                ApplyNumericAndUse(@event);
                return true;
            }

            if (key == KeyCode.Period || key == KeyCode.KeypadPeriod || key == KeyCode.Comma)
            {
                if (numberBuffer.IndexOf('.') < 0 && numberBuffer.Length < MAX_LENGTH)
                {
                    numberBuffer += numberBuffer.Length == 0 || numberBuffer == "-" ? "0." : ".";
                }

                ApplyNumericAndUse(@event);
                return true;
            }

            if (key == KeyCode.Minus || key == KeyCode.KeypadMinus)
            {
                numberBuffer = numberBuffer.StartsWith("-") ? numberBuffer[1..] : "-" + numberBuffer;

                ApplyNumericAndUse(@event);
                return true;
            }

            if (key == KeyCode.Backspace)
            {
                if (HasNumber)
                {
                    numberBuffer = numberBuffer[..^1];

                    if (!HasNumber)
                    {
                        RestoreTargets();
                        Rebaseline(@event);
                        Apply(@event);
                        @event.Use();

                        return true;
                    }
                }

                ApplyNumericAndUse(@event);
                return true;
            }

            return false;
        }
        private static void ApplyNumericAndUse(Event @event)
        {
            lastMouse = @event.mousePosition;

            if (HasNumber)
            {
                ApplyNumeric();
            }

            @event.Use();
        }
        private static void ToggleAxis(Axis axis, Event @event)
        {
            placerAxis = placerAxis == axis ? Axis.NONE : axis;

            Apply(@event);

            @event.Use();
        }

        private static float ParseNumber()
        {
            if (float.TryParse(numberBuffer, NumberStyles.Float, CultureInfo.InvariantCulture, out float number))
            {
                return number;
            }

            return 0f;
        }
        
        private static void Apply(Event @event)
        {
            lastMouse = @event.mousePosition;
            isSurfaceActive = false;

            if (HasNumber)
            {
                ApplyNumeric();
                return;
            }

            switch (placerMode)
            {
                case Mode.GRAB: ApplyGrab(@event); break;
                case Mode.ROTATE: ApplyRotate(@event); break;
                case Mode.SCALE: ApplyScale(@event); break;
            }
        }
        private static void ApplyGrab(Event @event)
        {
            if (@event.shift && placerAxis == Axis.NONE && TrySnap(@event.mousePosition))
            {
                return;
            }

            Ray ray = HandleUtility.GUIPointToWorldRay(@event.mousePosition);
            Vector3 delta;

            if (placerAxis == Axis.NONE)
            {
                if (!grabberDragPlane.Raycast(ray, out float enter))
                {
                    return;
                }

                delta = ray.GetPoint(enter) - grabberStartPlaneHit;
            }
            else
            {
                Vector3 direction = GetDirection(placerAxis);

                if (!TryGetClosestParam(pivotWorld, direction, ray, out float t0))
                {
                    return;
                }

                TryGetClosestParam(pivotWorld, direction, grabberStartRay, out float t1);

                delta = direction * (t0 - t1);
            }

            if (@event.control)
            {
                Vector3 step = EditorSnapSettings.move;

                delta = new Vector3(Snap(delta.x, step.x), Snap(delta.y, step.y), Snap(delta.z, step.z));
            }

            SetPosition(delta);

            description = "D " + delta.x.ToString("0.###") + ", " + delta.y.ToString("0.###") + ", " + delta.z.ToString("0.###");
        }
        private static void ApplyRotate(Event @event)
        {
            float currentAngle = GetAngle(@event.mousePosition - pivotScreen);

            rotaterAccumAngle += Mathf.DeltaAngle(rotaterLastAngle, currentAngle);
            rotaterLastAngle = currentAngle;

            float accumAngle = rotaterAccumAngle;

            if (@event.control)
            {
                accumAngle = Snap(accumAngle, EditorSnapSettings.rotate);
            }

            Vector3 forward = sceneView.camera.transform.forward;
            Vector3 axis = placerAxis == Axis.NONE ? forward : GetDirection(placerAxis);
            float applied = Vector3.Dot(axis, forward) >= 0f ? -accumAngle : accumAngle;

            SetRotation(applied, axis);

            description = "R " + accumAngle.ToString("0.#") + "°";
        }
        private static void ApplyScale(Event @event)
        {
            float distance = (@event.mousePosition - pivotScreen).magnitude;
            float factor = distance / rotaterStartDist;

            if (@event.control)
            {
                factor = Snap(factor, 0.1f);
            }

            SetScale(factor);

            description = "S " + factor.ToString("0.###");
        }
        private static void ApplyNumeric()
        {
            float input = ParseNumber();
            Vector3 forward = sceneView.camera.transform.forward;

            switch (placerMode)
            {
                case Mode.GRAB:
                    SetPosition(placerAxis == Axis.NONE ? sceneView.camera.transform.right : GetDirection(placerAxis) * input);
                    break;
                case Mode.ROTATE:
                    SetRotation(input, placerAxis == Axis.NONE ? -forward : GetDirection(placerAxis));
                    break;
                case Mode.SCALE:
                    SetScale(input);
                    break;
            }

            description = numberBuffer + "_";
        }

        private static void SetPosition(Vector3 delta)
        {
            for (int i = 0; i < placerTargets.Length; i++)
            {
                placerTargets[i].SetPositionAndRotation(startPositions[i] + delta, startRotations[i]);
            }
        }
        private static void SetRotation(float angle, Vector3 axis)
        {
            Quaternion rotation = Quaternion.AngleAxis(angle, axis);

            for (int i = 0; i < placerTargets.Length; i++)
            {
                placerTargets[i].SetPositionAndRotation(pivotWorld + rotation * (startPositions[i] - pivotWorld), rotation * startRotations[i]);
            }
        }
        private static void SetScale(float factor)
        {
            Vector3 direction = placerAxis == Axis.NONE ? Vector3.zero : GetDirection(placerAxis);

            for (int i = 0; i < placerTargets.Length; i++)
            {
                Vector3 offset = startPositions[i] - pivotWorld;

                if (placerAxis == Axis.NONE)
                {
                    offset *= factor;
                    placerTargets[i].localScale = startScales[i] * factor;
                }
                else
                {
                    offset += (factor - 1f) * Vector3.Dot(offset, direction) * direction;
                    placerTargets[i].localScale = Scale(startRotations[i], startScales[i], direction, factor);
                }

                placerTargets[i].position = pivotWorld + offset;
            }
        }

        private static void Simulate()
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("Simulate physics only works in edit mode.");
                return;
            }

            Transform[] selected = Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.Editable);

            if (selected == null || selected.Length == 0)
            {
                Debug.LogWarning("Simulate physics failed! No selected game object.");
                return;
            }

            Undo.RecordObjects(selected, "Simulate Physics");

            List<Rigidbody> tempRigidbodies = new();
            List<Collider> tempColliders = new();

            List<Rigidbody> madeDynamic = new();
            List<Rigidbody> madeKinematic = new();

            List<MeshCollider> madeConvex = new();
            List<Rigidbody> simulated = new();

            SimulationMode previousMode = Physics.simulationMode;
            int steps = 0;

            try
            {
                foreach (Rigidbody rigidbody in Object.FindObjectsByType<Rigidbody>(FindObjectsSortMode.None))
                {
                    if (IsInSelection(rigidbody.transform, selected))
                    {
                        continue;
                    }

                    if (!rigidbody.isKinematic)
                    {
                        rigidbody.isKinematic = true;

                        madeKinematic.Add(rigidbody);
                    }
                }

                foreach (Transform root in selected)
                {
                    if (!root.TryGetComponent(out Rigidbody rigidbody))
                    {
                        rigidbody = root.gameObject.AddComponent<Rigidbody>();
                        tempRigidbodies.Add(rigidbody);
                    }
                    else if (rigidbody.isKinematic)
                    {
                        rigidbody.isKinematic = false;
                        madeDynamic.Add(rigidbody);
                    }

                    if (root.GetComponentInChildren<Collider>() == null)
                    {
                        tempColliders.Add(root.gameObject.AddComponent<BoxCollider>());
                    }

                    MeshCollider[] colliders = root.GetComponentsInChildren<MeshCollider>();

                    for (int i = 0; i < colliders.Length; i++)
                    {
                        MeshCollider meshCollider = colliders[i];

                        if (!meshCollider.convex)
                        {
                            meshCollider.convex = true;
                            madeConvex.Add(meshCollider);
                        }
                    }

                    Rigidbody[] bodies = root.GetComponentsInChildren<Rigidbody>();

                    for (int i = 0; i < bodies.Length; i++)
                    {
                        rigidbody = bodies[i];

                        if (rigidbody.isKinematic)
                        {
                            continue;
                        }

                        rigidbody.linearVelocity = Vector3.zero;
                        rigidbody.angularVelocity = Vector3.zero;
                        simulated.Add(rigidbody);
                    }
                }

                Physics.simulationMode = SimulationMode.Script;
                Physics.SyncTransforms();

                for (; steps < MAX_SIMULATION_STEPS; steps++)
                {
                    Physics.Simulate(SIMULATION_STEP_SECONDS);

                    if (steps >= MIN_SIMULATION_STEPS && IsSleeping(simulated))
                    {
                        steps++;
                        break;
                    }
                }
            }
            finally
            {
                Physics.simulationMode = previousMode;
                foreach (Collider col in tempColliders) if (col != null) Object.DestroyImmediate(col);
                foreach (Rigidbody rb in tempRigidbodies) if (rb != null) Object.DestroyImmediate(rb);
                foreach (Rigidbody rb in madeDynamic) if (rb != null) rb.isKinematic = true;
                foreach (Rigidbody rb in madeKinematic) if (rb != null) rb.isKinematic = false;
                foreach (MeshCollider mc in madeConvex) if (mc != null) mc.convex = false;
            }

            SceneView.RepaintAll();

            Debug.Log("Simulate physics done. Objects: " + selected.Length + ", steps: " + steps);
        }

        private static bool TrySnap(Vector2 mouse)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(mouse);
            RaycastHit[] hits = Physics.RaycastAll(ray, 100000f, ~0, QueryTriggerInteraction.Ignore);

            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            RaycastHit surfaceHit = default;
            bool surfaceFound = false;

            foreach (var h in hits)
            {
                if (IsTarget(h.transform))
                {
                    continue;
                }

                surfaceHit = h;
                surfaceFound = true;

                break;
            }

            if (!surfaceFound)
            {
                return false;
            }

            Vector3 normal = surfaceHit.normal;
            Vector3 delta = surfaceHit.point - pivotWorld;

            for (int i = 0; i < placerTargets.Length; i++)
            {
                placerTargets[i].SetPositionAndRotation(startPositions[i] + delta, startRotations[i]);
            }

            if (alignToNormal && placerTargets.Length == 1)
            {
                placerTargets[0].rotation = Quaternion.FromToRotation(startRotations[0] * Vector3.up, normal) * startRotations[0];
            }

            if (TryGetMinDistance(surfaceHit.point, normal, out float minDist))
            {
                Vector3 shift = -minDist * normal;

                for (int i = 0; i < placerTargets.Length; i++)
                {
                    placerTargets[i].position += shift;
                }
            }

            isSurfaceActive = true;

            Placer.surfaceHit = surfaceHit.point;
            surfaceNormal = normal;

            description = "Surface: " + surfaceHit.transform.name + (alignToNormal && placerTargets.Length == 1 ? " (aligned)" : "");

            return true;
        }

        private static bool IsInSelection(Transform transform, Transform[] selected)
        {
            foreach (Transform root in selected)
            {
                if (transform == root || transform.IsChildOf(root))
                {
                    return true;
                }
            }

            return false;
        }
        private static bool IsTarget(Transform transform)
        {
            foreach (Transform target in placerTargets)
            {
                if (transform == target || transform.IsChildOf(target))
                {
                    return true;
                }
            }

            return false;
        }
        private static bool IsSleeping(List<Rigidbody> bodies)
        {
            foreach (Rigidbody body in bodies)
            {
                if (body != null && !body.IsSleeping())
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TryGetMinDistance(Vector3 point, Vector3 normal, out float distance)
        {
            distance = float.MaxValue;

            bool isValid = false;

            foreach (Transform target in placerTargets)
            {
                foreach (Renderer renderer in target.GetComponentsInChildren<Renderer>())
                {
                    Mesh mesh = null;
                    SkinnedMeshRenderer skinnedMesh = renderer as SkinnedMeshRenderer;

                    if (skinnedMesh != null)
                    {
                        mesh = skinnedMesh.sharedMesh;
                    }
                    else
                    {
                        if (renderer.TryGetComponent(out MeshFilter meshFilter))
                        {
                            mesh = meshFilter.sharedMesh;
                        }
                    }

                    if (mesh == null)
                    {
                        continue;
                    }

                    Bounds bounds = mesh.bounds;
                    Matrix4x4 matrix = renderer.transform.localToWorldMatrix;
                    Vector3 center = bounds.center;
                    Vector3 extents = bounds.extents;

                    for (int i = 0; i < 8; i++)
                    {
                        Vector3 corner = center + new Vector3
                        (
                            (i & 1) == 0 ? -extents.x : extents.x,
                            (i & 2) == 0 ? -extents.y : extents.y,
                            (i & 4) == 0 ? -extents.z : extents.z
                        );

                        float d = Vector3.Dot(matrix.MultiplyPoint3x4(corner) - point, normal);

                        if (d < distance)
                        {
                            distance = d;
                        }

                        isValid = true;
                    }
                }
            }

            return isValid;
        }
        private static bool TryGetClosestParam(Vector3 origin, Vector3 direction, Ray ray, out float value)
        {
            Vector3 w0 = origin - ray.origin;

            float b = Vector3.Dot(direction, ray.direction);
            float d = Vector3.Dot(direction, w0);
            float eDot = Vector3.Dot(ray.direction, w0);
            float denom = 1f - b * b;

            if (denom < 1e-4f)
            {
                value = 0f;
                return false;
            }

            value = (b * eDot - d) / denom;
            return true;
        }

        private static Mode GetMode(KeyCode key)
        {
            return key switch
            {
                KEY_GRAB => Mode.GRAB,
                KEY_ROTATE => Mode.ROTATE,
                KEY_SCALE => Mode.SCALE,
                KEY_SIMULATE => Mode.SIMULATE,
                _ => Mode.NONE,
            };
        }
        private static Vector3 GetDirection(Axis axis)
        {
            return axis switch
            {
                Axis.X => Vector3.right,
                Axis.Y => Vector3.up,
                _ => Vector3.forward,
            };
        }
        private static float GetAngle(Vector2 vector) => Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;

        private static float Snap(float value, float step) => step > 0f ? Mathf.Round(value / step) * step : value;
        private static Vector3 Scale(Quaternion rotation, Vector3 startScale, Vector3 direction, float factor)
        {
            Vector3[] localAxes = { rotation * Vector3.right, rotation * Vector3.up, rotation * Vector3.forward };

            int best = 0;
            float bestDot = -1f;

            for (int i = 0; i < 3; i++)
            {
                float d = Mathf.Abs(Vector3.Dot(localAxes[i], direction));

                if (d > bestDot) 
                { 
                    bestDot = d;
                    best = i; 
                }
            }

            Vector3 s = startScale;
            s[best] *= factor;

            return s;
        } 
    }
}
