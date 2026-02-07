using Codice.Client.BaseCommands;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{ 
    public static class CoreUtility
    {
        #region SCENE
        [Serializable]
        public class Scene
        {
            public string Path = string.Empty;
            public string Name = string.Empty;
            public int Index = 0;
        }
        #endregion

        #region MOTION
        public class Spring
        {
            [Serializable]
            public struct Config
            {
                public Vector3 Amplitude;
                [Min(0)] public float Frequency;
                [Range(0, 1)]public float Damping;

                public readonly static Config New = new(Vector3.zero, 8, 0.33f);
                public Config(Vector3 amplitude, float frequency, float damping) => (Amplitude, Frequency, Damping) = (amplitude, Mathf.Max(0, frequency), Mathf.Clamp01(damping));
                public Config(Config config) => (Amplitude, Frequency, Damping) = (config.Amplitude, Mathf.Max(0, config.Frequency), Mathf.Clamp01(config.Damping));
            }
            public class Instance
            {
                private readonly SwapBackArray<State> collection = default;
                public Instance(uint capacity) => collection = new(capacity);

                public void Start(Config config, float strength = 1f)
                {
                    if (collection.Count == collection.Capacity)
                    {
                        ref State s = ref collection.GetRef(collection.Count - 1);
                        s.Start(config, strength);
                        return;
                    }

                    State state = new();
                    state.Start(config, strength);
                    collection.Add(state);
                }
                public Vector3 Update(float deltaTime)
                {
                    Vector3 value = Vector3.zero;

                    for (int i = collection.Count - 1; i >= 0; i--)
                    {
                        ref State state = ref collection.GetRef(i);

                        if (!state.IsActive)
                        {
                            collection.RemoveAt(i);
                        }
                        else
                        {
                            value += state.Update(deltaTime);
                        }
                    }

                    return value;
                }
                public void Clear() => collection.Clear();
            }
            public struct State
            {
                public bool IsActive { get; private set; }

                private Vector3 currentValue;
                private Vector3 currentVelocity;
                private float frequency;
                private float damping;
                private const float EPS = 0.0001f;

                public void Start(Config config, float strength)
                {
                    Vector3 amplitude = config.Amplitude * strength;

                    if (amplitude.sqrMagnitude < EPS)
                    {
                        return;
                    }

                    currentVelocity += amplitude;
                    frequency = Mathf.Max(0, config.Frequency);
                    damping = Mathf.Clamp01(config.Damping);
                    IsActive = true;
                }
                public Vector3 Update(float deltaTime)
                {
                    if (!IsActive)
                    {
                        return currentValue;
                    }

                    Vector3 acceleration = -frequency * frequency * currentValue - 2f * damping * frequency * currentVelocity;
                    currentVelocity += acceleration * deltaTime;
                    currentValue += currentVelocity * deltaTime;

                    if (currentValue.sqrMagnitude < EPS && currentVelocity.sqrMagnitude < EPS)
                    {
                        currentValue = Vector3.zero;
                        currentVelocity = Vector3.zero;
                        IsActive = false;
                    }

                    return currentValue;
                }
            }
        }
        public class Shake
        {
            [Serializable]
            public struct Config
            {
                public Vector3 Influence;
                [Min(0)] public float Magnitude;
                [Min(0)] public float Roughness;
                [Min(0)] public float FadeInTime;
                [Min(0)] public float FadeOutTime;

                public readonly static Config New = new(1, 1, 0, 1, Vector3.zero);
                public Config(Config config) => (Magnitude, Roughness, FadeInTime, FadeOutTime, Influence) = (config.Magnitude, config.Roughness, config.FadeInTime, config.FadeOutTime, config.Influence);
                public Config(float magnitude, float roughness, float fadeInTime, float fadeOutTime, Vector3 influence) => (Magnitude, Roughness, FadeInTime, FadeOutTime, Influence) = (magnitude, roughness, fadeInTime, fadeOutTime, influence);
            }
            public class Instance
            {
                private readonly SwapBackArray<State> collection;
                public Instance(uint capacity) => collection = new(capacity);

                public void Start(Config config, float strength = 1f)
                {
                    if (collection.Count == collection.Capacity)
                    {
                        ref State s = ref collection.GetRef(collection.Count - 1);
                        s.Start(config, strength);
                        return;
                    }

                    State state = new();
                    state.Start(config, strength);
                    collection.Add(state);
                }
                public Vector3 Update(float deltaTime)
                {
                    Vector3 value = Vector3.zero;

                    for (int i = collection.Count - 1; i >= 0; i--)
                    {
                        ref State s = ref collection.GetRef(i);

                        if (!s.IsActive)
                        {
                            collection.RemoveAt(i);
                        }
                        else
                        {
                            value += s.Update(deltaTime);
                        }
                    }

                    return value;
                }
                public void Clear() => collection.Clear();
            }
            public struct State
            {
                public bool IsActive { get; private set; }

                private Vector3 influence;
                private Vector3 currentValue;
                private float magnitude;
                private float roughness;
                private float fadeInTime;
                private float fadeOutTime;
                private float fadeTimer;
                private float tickTimer;
                private bool isSustaining;

                public void Start(Config config, float strength)
                {
                    magnitude = config.Magnitude * strength;
                    roughness = config.Roughness;
                    fadeInTime = config.FadeInTime;
                    fadeOutTime = Mathf.Max(0.0001f, config.FadeOutTime);
                    influence = config.Influence;

                    fadeTimer = fadeInTime > 0 ? 0f : 1f;
                    tickTimer = UnityEngine.Random.value * 1000f;

                    isSustaining = fadeInTime > 0;
                    IsActive = true;
                }
                public Vector3 Update(float deltaTime)
                {
                    if (!IsActive)
                    {
                        return currentValue;
                    }

                    // Noise
                    float nx = Mathf.PerlinNoise(tickTimer, 0f) - 0.5f;
                    float ny = Mathf.PerlinNoise(0f, tickTimer) - 0.5f;
                    float nz = Mathf.PerlinNoise(tickTimer, tickTimer) - 0.5f;

                    // Fade in / out
                    if (isSustaining)
                    {
                        if (fadeInTime > 0)
                        {
                            fadeTimer += deltaTime / fadeInTime;
                            if (fadeTimer >= 1f)
                            {
                                fadeTimer = 1f;
                                isSustaining = false;
                            }
                        }
                    }
                    else
                    {
                        fadeTimer -= deltaTime / fadeOutTime;
                    }

                    if (fadeTimer <= 0f)
                    {
                        IsActive = false;
                        currentValue = Vector3.zero;
                        return currentValue;
                    }

                    tickTimer += deltaTime * roughness * fadeTimer;

                    currentValue.x = nx * magnitude * fadeTimer * influence.x;
                    currentValue.y = ny * magnitude * fadeTimer * influence.y;
                    currentValue.z = nz * magnitude * fadeTimer * influence.z;

                    return currentValue;
                }
            }
        }
        public class Sway
        {
            [Serializable]
            public class Config
            {
                [Min(0.01f)] public float Smoothness;
                public Vector3 Amplitude;
                public Vector3 Clamp;
                public bool3 IsOffset;

                public static Config New => new(0.15f, Vector3.one, Vector3.zero, new(false, false, false));
                public Config(float smoothness, Vector3 amplitude, Vector3 clamp, bool3 offset) => (Smoothness, Amplitude, Clamp, IsOffset) = (smoothness, amplitude, clamp, offset);
            }
            public class Instance
            {
                private Vector3 currentVelocity = Vector3.zero;
                private Vector3 currentValue = Vector3.zero;
                private Vector3 targetValue = Vector3.zero;

                public Vector3 Update(Config config, float inputX, float inputY, float inputZ, float strength = 1)
                {
                    if (config == null)
                    {
                        return Vector3.zero;
                    }

                    float x = Mathf.Clamp((inputX * config.Amplitude.x * strength) + (config.IsOffset.x ? currentValue.x : 0), -config.Clamp.x, config.Clamp.x);
                    float y = Mathf.Clamp((inputY * config.Amplitude.y * strength) + (config.IsOffset.y ? currentValue.y : 0), -config.Clamp.y, config.Clamp.y);
                    float z = Mathf.Clamp((inputZ * config.Amplitude.z * strength) + (config.IsOffset.z ? currentValue.z : 0), -config.Clamp.z, config.Clamp.z);

                    targetValue.x = float.IsNaN(x) ? 0 : x;
                    targetValue.y = float.IsNaN(y) ? 0 : y;
                    targetValue.z = float.IsNaN(z) ? 0 : z;

                    return currentValue = Vector3.SmoothDamp(currentValue, targetValue, ref currentVelocity, config.Smoothness * Time.deltaTime);
                }
            }
        }
        public class Cycle
        {
            [Serializable]
            public class Config
            {
                [Min(0.01f)] public float Roughness;
                public Vector3 Amplitude;
                public Vector3 Frequency;
                public Vector3 Clamp;

                public static Config New => new(5, Vector3.one * 0.1f, Vector3.one * 15.0f, Vector3.zero);
                public Config(float roughness, Vector3 amplitude, Vector3 frequency, Vector3 clamp) => (Roughness, Amplitude, Frequency, Clamp) = (roughness, amplitude, frequency, clamp);
            }
            public class Instance
            {
                private Vector3 currentValue = Vector3.zero;
                private Vector3 targetValue = Vector3.zero;

                public Vector3 Update(Config config, float deltaTime, float strength = 1, bool update = true)
                {
                    if (config == null)
                    {
                        return Vector3.zero;
                    }

                    float xTime = Mathf.Cos(Time.time * config.Frequency.x / 2.0f) * (update ? 1 : 0);
                    float yTime = Mathf.Sin(Time.time * config.Frequency.y) * (update ? 1 : 0);
                    float zTime = Mathf.Sin(Time.time * config.Frequency.z) * (update ? 1 : 0);

                    float x = config.Amplitude.x * xTime;
                    float y = config.Amplitude.y * yTime;
                    float z = config.Amplitude.z * zTime;

                    targetValue.x = Mathf.Clamp(x * strength, -config.Clamp.x, config.Clamp.x);
                    targetValue.y = Mathf.Clamp(y * strength, -config.Clamp.y, config.Clamp.y);
                    targetValue.z = Mathf.Clamp(z * strength, -config.Clamp.z, config.Clamp.z);

                    return currentValue = Vector3.Lerp(currentValue, targetValue, config.Roughness * deltaTime);
                }
            }
        }
        public class Transition
        {
            [Serializable]
            public class Config
            {
                [Min(0.01f)] public float Smoothness;
                public Vector3 Target;

                public static Config New => new(5, Vector3.zero);
                public Config(float smoothness, Vector3 target) => (Smoothness, Target) = (smoothness, target);
            }
            public class Instance
            {
                private Vector3 currentVelocity = Vector3.zero;
                public Vector3 Update(Vector3 from, Config target, float deltaTime)
                {
                    if (target == null)
                    {
                        return Vector3.SmoothDamp(from, Vector3.zero, ref currentVelocity, 2.5f, 100, deltaTime * 10);
                    }

                    return Vector3.SmoothDamp(from, target.Target, ref currentVelocity, target.Smoothness, 100, deltaTime * 10);
                }
            }
        }
        public class Momentum
        {
            [Serializable]
            public class Config
            {
                [Min(0)] public float MinThreshold = 1f;
                [Min(0)] public float MaxThreshold = 25f;
                [Min(0)] public float BaseMagnitude = 1f;
                [Min(0)] public float MaxMagnitude = 10f;
                [Min(0)] public float Smoothness = 0.25f;
                [Min(1)] public float Speed = 7.5f;

                public static Config New => new(1, 25, 1f, 10f, 0.25f, 7.5f);
                public Config(float minThreshold, float maxThreshold, float baseMagnitude, float maxMagnitude, float smoothness, float speed) => (MinThreshold, MaxThreshold, BaseMagnitude, MaxMagnitude, Smoothness, Speed) = (minThreshold, maxThreshold, baseMagnitude, maxMagnitude, smoothness, speed);
            }
            public class Instance
            {
                private float time = 0f;
                private float velocity = 0f;

                public Vector3 Update(Config config, float value)
                {
                    float t = Mathf.Clamp01(Mathf.InverseLerp(config.MinThreshold, config.MaxThreshold, value));

                    time = Mathf.SmoothDamp(time, t, ref velocity, config.Smoothness);

                    float magnitude = Mathf.Clamp(config.BaseMagnitude * time, 0f, config.MaxMagnitude);
                    float x = (Mathf.PerlinNoise(Time.time * config.Speed, 0f) - 0.5f) * 2f * magnitude;
                    float y = (Mathf.PerlinNoise(0f, Time.time * config.Speed) - 0.5f) * 2f * magnitude;
                    float z = (Mathf.PerlinNoise(Time.time * config.Speed, Time.time * config.Speed) - 0.5f) * 2f * magnitude;

                    return new(x, y, z);
                }
            }
        }
        #endregion

        #region MATH
        [Serializable]
        public readonly struct Float3 : IEquatable<Float3>
        {
            public bool IsZero => x == 0f && y == 0f && z == 0f;
            public bool IsValid => float.IsFinite(x) && float.IsFinite(y) && float.IsFinite(z);

            public readonly float x;
            public readonly float y;
            public readonly float z;

            public static readonly Float3 one = new(1, 1, 1);
            public static readonly Float3 zero = new(0, 0, 0);

            public Float3(float x, float y, float z) => (this.x, this.y, this.z) = (x, y, z);
            public static Float3 operator +(Float3 a, Float3 b) => new(a.x + b.x, a.y + b.y, a.z + b.z);
            public static Float3 operator -(Float3 a, Float3 b) => new(a.x - b.x, a.y - b.y, a.z - b.z);
            public static Float3 operator *(Float3 a, Float3 b) => new(a.x * b.x, a.y * b.y, a.z * b.z);
            public static Float3 operator /(Float3 a, Float3 b) => new(a.x / b.x, a.y / b.y, a.z / b.z);
            public static Float3 operator *(Float3 a, float s) => new(a.x * s, a.y * s, a.z * s);
            public static Float3 operator /(Float3 a, float s) => new(a.x / s, a.y / s, a.z / s);

            public static bool operator ==(Float3 a, Float3 b) => a.Equals(b);
            public static bool operator !=(Float3 a, Float3 b) => !a.Equals(b);

            public static implicit operator Vector3(Float3 b) => new(b.x, b.y, b.z);
            public static explicit operator Float3(Vector3 b) => new(b.x, b.y, b.z);
            public static explicit operator Int3(Float3 b) => new((int)b.x, (int)b.y, (int)b.z);

            public bool Equals(Float3 other) => x == other.x && y == other.y && z == other.z;
            public override bool Equals(object obj) => obj is Float3 other && Equals(other);
            public override readonly int GetHashCode() => x.GetHashCode() ^ (y.GetHashCode() << 2) ^ (z.GetHashCode() >> 2);
        }
        [Serializable]
        public readonly struct Float2 : IEquatable<Float2>
        {
            public bool IsZero => x == 0f && y == 0f;
            public bool IsValid => float.IsFinite(x) && float.IsFinite(y);

            public readonly float x;
            public readonly float y;

            public static readonly Float2 one = new(1, 1);
            public static readonly Float2 zero = new(0, 0);

            public Float2(float x, float y) => (this.x, this.y) = (x, y);
            public static Float2 operator +(Float2 a, Float2 b) => new(a.x + b.x, a.y + b.y);
            public static Float2 operator -(Float2 a, Float2 b) => new(a.x - b.x, a.y - b.y);
            public static Float2 operator *(Float2 a, Float2 b) => new(a.x * b.x, a.y * b.y);
            public static Float2 operator /(Float2 a, Float2 b) => new(a.x / b.x, a.y / b.y);
            public static Float2 operator *(Float2 a, float s) => new(a.x * s, a.y * s);
            public static Float2 operator /(Float2 a, float s) => new(a.x / s, a.y / s);

            public static bool operator ==(Float2 a, Float2 b) => a.Equals(b);
            public static bool operator !=(Float2 a, Float2 b) => !a.Equals(b);

            public static implicit operator Vector2(Float2 b) => new(b.x, b.y);
            public static explicit operator Float2(Vector2 b) => new(b.x, b.y);
            public static explicit operator Int2(Float2 b) => new((int)b.x, (int)b.y);

            public bool Equals(Float2 other) => x == other.x && y == other.y;
            public override bool Equals(object obj) => obj is Float2 other && Equals(other);
            public override readonly int GetHashCode() => x.GetHashCode() ^ (y.GetHashCode() << 2);
        }
        [Serializable]
        public readonly struct Int3 : IEquatable<Int3>
        {
            public readonly int x;
            public readonly int y;
            public readonly int z;

            public static readonly Int3 one = new(1, 1, 1);
            public static readonly Int3 zero = new(0, 0, 0);

            public Int3(int x, int y, int z) => (this.x, this.y, this.z) = (x, y, z);
            public static Int3 operator +(Int3 a, Int3 b) => new(a.x + b.x, a.y + b.y, a.z + b.z);
            public static Int3 operator -(Int3 a, Int3 b) => new(a.x - b.x, a.y - b.y, a.z - b.z);
            public static Int3 operator *(Int3 a, Int3 b) => new(a.x * b.x, a.y * b.y, a.z * b.z);
            public static Int3 operator *(Int3 a, int s) => new(a.x * s, a.y * s, a.z * s);
            public static Int3 operator /(Int3 a, int s) => new(a.x / s, a.y / s, a.z / s);

            public static bool operator ==(Int3 a, Int3 b) => a.Equals(b);
            public static bool operator !=(Int3 a, Int3 b) => !a.Equals(b);

            public static implicit operator Vector3Int(Int3 b) => new(b.x, b.y, b.z);
            public static explicit operator Int3(Vector3Int b) => new(b.x, b.y, b.z);
            public static implicit operator Float3(Int3 b) => new(b.x, b.y, b.z);

            public bool Equals(Int3 other) => x == other.x && y == other.y && z == other.z;
            public override bool Equals(object obj) => obj is Int3 other && Equals(other);
            public override int GetHashCode()
            {
                unchecked
                {
                    return x ^ (y << 2) ^ (z >> 2);
                }
            }
        }
        [Serializable]
        public readonly struct Int2 : IEquatable<Int2>
        {
            public readonly int x;
            public readonly int y;

            public static readonly Int2 one = new(1, 1);
            public static readonly Int2 zero = new(0, 0);

            public Int2(int x, int y) => (this.x, this.y) = (x, y);
            public static Int2 operator +(Int2 a, Int2 b) => new(a.x + b.x, a.y + b.y);
            public static Int2 operator -(Int2 a, Int2 b) => new(a.x - b.x, a.y - b.y);
            public static Int2 operator *(Int2 a, Int2 b) => new(a.x * b.x, a.y * b.y);
            public static Int2 operator /(Int2 a, Int2 b) => new(a.x / b.x, a.y / b.y);
            public static Int2 operator *(Int2 a, int s) => new(a.x * s, a.y * s);
            public static Int2 operator /(Int2 a, int s) => new(a.x / s, a.y / s);

            public static bool operator ==(Int2 a, Int2 b) => a.Equals(b);
            public static bool operator !=(Int2 a, Int2 b) => !a.Equals(b);

            public static implicit operator Vector2Int(Int2 b) => new(b.x, b.y);
            public static explicit operator Int2(Vector2Int b) => new(b.x, b.y);
            public static implicit operator Float2(Int2 b) => new(b.x, b.y);

            public bool Equals(Int2 other) => x == other.x && y == other.y;
            public override bool Equals(object obj) => obj is Int2 other && Equals(other);
            public override readonly int GetHashCode()
            {
                unchecked
                {
                    return x ^ (y << 16);
                }
            }
        }

        public static Vector3 Clamp(this Vector3 a, float length)
        {
            a.x = Mathf.Clamp(a.x, -length, length);
            a.y = Mathf.Clamp(a.y, -length, length);
            a.z = Mathf.Clamp(a.z, -length, length);

            return a;
        }
        public static Vector2 Clamp(this Vector2 a, float length)
        {
            a.x = Mathf.Clamp(a.x, -length, length);
            a.y = Mathf.Clamp(a.y, -length, length);

            return a;
        }
        public static Vector3 Multiply(this Vector3 a, Vector3 b) => new(a.x * b.x, a.y * b.y, a.z * b.z);
        public static Vector2 Multiply(this Vector2 a, Vector2 b) => new(a.x * b.x, a.y * b.y);
        public static Vector3 ClearX(this Vector3 a) { a.x = 0; return a; }
        public static Vector3 ClearY(this Vector3 a) { a.y = 0; return a; }
        public static Vector3 ClearZ(this Vector3 a) { a.z = 0; return a; }
        #endregion

        #region PHYSICS
        public readonly struct HitData
        {
            public readonly Collider Collider;
            public readonly Vector3 Point;
            public readonly Vector3 Normal;
            public readonly float Distance;

            public HitData(Collider collider, Vector3 point, Vector3 normal, float distance)
            {
                Collider = collider;
                Point = point;
                Normal = normal;
                Distance = distance;
            }
        }

        public delegate void HitProcessor(in HitData hitData);

        public static bool HitScan(Vector3 origin, Vector3 direction, float range, int mask, RaycastHit[] hitBuffer, HitData[] resultBuffer, QueryTriggerInteraction query, out int hits, HitProcessor processor = null)
        {
            int rayHits = Physics.RaycastNonAlloc(origin, direction, hitBuffer, range, mask, query);
            int maxHits = Mathf.Min(rayHits, resultBuffer.Length);
            hits = 0;

            for (int i = 0; i < maxHits; i++)
            {
                RaycastHit hit = hitBuffer[i];
                HitData data = new(hit.collider, hit.point, hit.normal, hit.distance);

                resultBuffer[hits++] = data;
                processor?.Invoke(in data);
            }

            return hits > 0;
        }
        public static bool HitCone(Vector3 origin, Vector3 forward, float angle, float radius, int mask, Collider[] overlapBuffer, HitData[] resultBuffer, QueryTriggerInteraction query, out int hits, HitProcessor processor = null)
        {            
            hits = 0;
            int overlapHits = Physics.OverlapSphereNonAlloc(origin, radius, overlapBuffer, mask, query);
            float cos = Mathf.Cos(angle * Mathf.Deg2Rad);
            
            for (int i = 0; i < overlapHits && hits < resultBuffer.Length; i++)
            {
                Collider collider = overlapBuffer[i];

                Vector3 point = collider.ClosestPoint(origin);
                Vector3 direction = point - origin;
                float distance = direction.magnitude;

                if (distance <= Mathf.Epsilon)
                {
                    resultBuffer[hits++] = new(collider, origin, Vector3.up, 0f);
                    continue;
                }

                direction /= distance;

                if (Vector3.Dot(forward, direction) < cos)
                {
                    continue;
                }

                HitData data = new(collider, point, -direction, distance);

                resultBuffer[hits++] = data;
                processor?.Invoke(in data);
            }

            return hits > 0;
        }
        public static bool HitSweep(Vector3 start, Vector3 end, float radius, int mask, RaycastHit[] hitBuffer, HitData[] resultBuffer, QueryTriggerInteraction query, out int hits, HitProcessor processor = null)
        {
            hits = 0;

            Vector3 direction = end - start;
            float distance = direction.magnitude;

            if (distance <= Mathf.Epsilon)
            {
                return false;
            }

            direction /= distance;

            int capsuleHits = Physics.CapsuleCastNonAlloc(start, end, radius, direction, hitBuffer, distance, mask, query);
            int maxHits = Mathf.Min(capsuleHits, resultBuffer.Length);

            for (int i = 0; i < maxHits; i++)
            {
                RaycastHit hit = hitBuffer[i];

                HitData data = new(hit.collider, hit.point, hit.normal, hit.distance);

                resultBuffer[hits++] = data;
                processor?.Invoke(in data);
            }

            return hits > 0;
        }
        public static bool HitArea(Vector3 origin, float radius, int overlapMask, int obstructionMask, Collider[] overlapBuffer, RaycastHit[] obstructionBuffer, HitData[] resultBuffer, QueryTriggerInteraction query, out int hits, HitProcessor processor = null)
        {
            int areaHits = Physics.OverlapSphereNonAlloc(origin, radius, overlapBuffer, overlapMask, query);
            hits = 0;

            for (int i = 0; i < areaHits && hits < resultBuffer.Length; i++)
            {
                Collider collider = overlapBuffer[i];
                Vector3 point = collider.ClosestPoint(origin);
                Vector3 direction = point - origin;
                float distance = direction.magnitude;

                if (distance <= Mathf.Epsilon)
                {
                    resultBuffer[hits++] = new(collider, point, Vector3.up, 0f);
                    continue;
                }

                direction /= distance;

                int obstructionHits = Physics.RaycastNonAlloc(origin, direction, obstructionBuffer, distance, obstructionMask, QueryTriggerInteraction.Ignore);

                bool blocked = false;

                for (int j = 0; j < obstructionHits; j++)
                {
                    RaycastHit hit = obstructionBuffer[j];

                    if (hit.collider == collider || hit.collider.transform.IsChildOf(collider.transform))
                    {
                        continue;
                    }

                    if (hit.distance < distance)
                    {
                        blocked = true;
                        break;
                    }
                }

                if (blocked)
                {
                    continue;
                }

                HitData data = new(collider, point, -direction, distance);

                resultBuffer[hits++] = data;
                processor?.Invoke(in data);
            }

            return hits > 0;
        }

        public static Vector3 GetRequiredLaunchVelocity(Vector3 targetPosition, Vector3 launchPosition, float forceMax)
        {
            Vector3 displacement = Vector3.zero;
            displacement.x = targetPosition.x - launchPosition.x;
            displacement.y = launchPosition.y - launchPosition.y;
            displacement.z = targetPosition.z - launchPosition.z;

            float displacementVertical = targetPosition.y - launchPosition.y;
            float displacementHorizontal = displacement.magnitude;
            float gravity = Mathf.Abs(UnityEngine.Physics.gravity.y);
            float force = Mathf.Sqrt(gravity * (displacementVertical + Mathf.Sqrt(Mathf.Pow(displacementVertical, 2) + Mathf.Pow(displacementHorizontal, 2))));

            if (force > forceMax)
            {
                //Debug.Log("Cant throw, velocity is not enough please move closer!");
                return Vector3.zero;
            }

            float angle = Mathf.PI / 2f - (0.5f * (Mathf.PI / 2 - (displacementVertical / displacementHorizontal)));

            if (float.IsNaN(angle))
            {
                //Debug.Log("Cant throw, angle is not feasible!");
                return Vector3.zero;
            }

            return Mathf.Cos(angle) * force * displacement.normalized + Mathf.Sin(angle) * force * Vector3.up;
        }
        public static Vector3 GetRequiredLaunchVelocity(Vector3 targetPosition, Vector3 launchPosition, float forceMax, float forceRatio)
        {
            Vector3 displacement = Vector3.zero;
            displacement.x = targetPosition.x - launchPosition.x;
            displacement.y = launchPosition.y - launchPosition.y;
            displacement.z = targetPosition.z - launchPosition.z;

            float displacementVertical = targetPosition.y - launchPosition.y;
            float displacementHorizontal = displacement.magnitude;
            float gravity = Mathf.Abs(UnityEngine.Physics.gravity.y);
            float force = Mathf.Sqrt(gravity * (displacementVertical + Mathf.Sqrt(Mathf.Pow(displacementVertical, 2) + Mathf.Pow(displacementHorizontal, 2))));

            if (force > forceMax)
            {
                //Debug.Log("Cant throw, velocity is not enough please move closer!");
                return Vector3.zero;
            }

            force = Mathf.Lerp(force, forceMax, forceRatio);

            float angle = Mathf.PI / 2f - (0.5f * (Mathf.PI / 2 - (displacementVertical / displacementHorizontal)));

            if (forceRatio > 0)
            {
                angle = Mathf.Atan((Mathf.Pow(force, 2) - Mathf.Sqrt(Mathf.Pow(force, 4) - gravity * (gravity * Mathf.Pow(displacementHorizontal, 2) + 2 * displacementVertical * Mathf.Pow(force, 2)))) / (gravity * displacementHorizontal));
            }

            if (float.IsNaN(angle))
            {
                //Debug.Log("Cant throw, angle is not feasible!");
                return Vector3.zero;
            }

            return Mathf.Cos(angle) * force * displacement.normalized + Mathf.Sin(angle) * force * Vector3.up;
        }
        public static Vector3 GetRequiredLaunchVelocity(Vector3 targetPosition, Vector3 targetVelocity, Vector3 launchPosition, float forceMax)
        {
            Vector3 displacement = Vector3.zero;
            displacement.x = targetPosition.x - launchPosition.x;
            displacement.y = launchPosition.y - launchPosition.y;
            displacement.z = targetPosition.z - launchPosition.z;

            Vector3 directVelocity = GetRequiredLaunchVelocity(targetPosition, launchPosition, forceMax);
            directVelocity.y = 0;

            float time = displacement.magnitude / directVelocity.magnitude;

            return Vector3.ClampMagnitude(GetRequiredLaunchVelocity(targetPosition + (targetVelocity * time), launchPosition, forceMax), forceMax);
        }
        public static Vector3 GetRequiredLaunchVelocity(Vector3 targetPosition, Vector3 targetVelocity, Vector3 launchPosition, float forceMax, float forceRatio)
        {
            Vector3 displacement = Vector3.zero;
            displacement.x = targetPosition.x - launchPosition.x;
            displacement.y = launchPosition.y - launchPosition.y;
            displacement.z = targetPosition.z - launchPosition.z;

            Vector3 directVelocity = GetRequiredLaunchVelocity(targetPosition, launchPosition, forceMax, forceRatio);
            directVelocity.y = 0;

            float time = displacement.magnitude / directVelocity.magnitude;

            return Vector3.ClampMagnitude(GetRequiredLaunchVelocity(targetPosition + (targetVelocity * time), launchPosition, forceMax, forceRatio), forceMax);
        }
        public static Vector3 GetPositionOnParabolic(Vector3 startPosition, Vector3 endPosition, float height, float time)
        {
            Vector3 currentPosition = Vector3.Lerp(startPosition, endPosition, time);

            float currentHeight = Mathf.Sin(time * Mathf.PI) * height;

            currentPosition.y += currentHeight;

            return currentPosition;
        }
        public static Vector3 GetPositionOnFieldofView(Vector3 targetPosition, Vector3 targetForward, float radius, float fov = 90)
        {
            float angle = UnityEngine.Random.Range(-fov / 2, fov / 2);

            Vector3 targetDirection = Quaternion.Euler(0, angle, 0) * targetForward;

            return targetPosition + targetDirection * radius;
        }
        #endregion

        #region GAMEOBJECT
        /// <summary> Returns true if gameobject layer in bitmask </summary>
        public static bool IsInBitMask(this GameObject gameObject, int bitMask) => (bitMask & (1 << gameObject.layer)) != 0;
        public static void SetName(this GameObject gameObject, string name) => gameObject.name = name;
        public static void SetLayer(this GameObject gameObject, int layer, bool includeChildren = false)
        {
            gameObject.layer = layer;

            if (!includeChildren)
            {
                return;
            }

            Transform[] transforms = gameObject.GetComponentsInChildren<Transform>(true);

            for (int i = 0; i < transforms.Length; i++)
            {
                transforms[i].gameObject.layer = layer;
            }
        }
        public static void SetTag(this GameObject gameObject, string tag, bool includeChildren = false)
        {
            gameObject.tag = tag;

            if (!includeChildren)
            {
                return;
            }

            Transform[] transforms = gameObject.GetComponentsInChildren<Transform>(true);

            for (int i = 0; i < transforms.Length; i++)
            {
                transforms[i].gameObject.tag = tag;
            }
        }
        #endregion

        #region MESH
        public static void StitchTo(this SkinnedMeshRenderer thisSkinnedMesh, SkinnedMeshRenderer targetSkinnedMesh)
        {
            thisSkinnedMesh.bones = targetSkinnedMesh.bones;
            thisSkinnedMesh.rootBone = targetSkinnedMesh.rootBone;
        }
        public static void SwapTo(this SkinnedMeshRenderer thisSkinnedMesh, SkinnedMeshRenderer targetSkinnedMesh)
        {
            StitchTo(thisSkinnedMesh, targetSkinnedMesh);
            thisSkinnedMesh.sharedMesh = targetSkinnedMesh.sharedMesh;
        }
        public static MeshRenderer BakeToDefault(this SkinnedMeshRenderer thisSkinnedMesh)
        {
            thisSkinnedMesh.enabled = true;

            GameObject meshHolder = thisSkinnedMesh.gameObject;

            Mesh bakedMesh = new() { name = thisSkinnedMesh.name };
            thisSkinnedMesh.BakeMesh(bakedMesh);

            bakedMesh.RecalculateBounds();

            MeshFilter meshFilter = meshHolder.GetComponent<MeshFilter>();
            MeshRenderer meshRenderer = meshHolder.GetComponent<MeshRenderer>();

            if (meshFilter != null)
            {
                // Cleanup for previous baked mesh
                GameObject.Destroy(meshFilter.sharedMesh);
            }

            if (meshFilter == null)
            {
                meshFilter = meshHolder.AddComponent<MeshFilter>();
            }

            if (meshRenderer == null)
            {
                meshRenderer = meshHolder.AddComponent<MeshRenderer>();
            }
           
            meshFilter.sharedMesh = bakedMesh;
            meshRenderer.sharedMaterial = thisSkinnedMesh.sharedMaterial;
            thisSkinnedMesh.enabled = false;

            return meshRenderer;
        }
        public static void BakeToCollider(this MeshRenderer thisMeshRenderer, MeshCollider targetCollider)
        {
            targetCollider.sharedMesh = null;
            targetCollider.sharedMesh = thisMeshRenderer.GetComponent<MeshFilter>().sharedMesh;
            targetCollider.convex = true;
        }

        public static Mesh CreateQuad()
        {
            Mesh mesh = new();

            Vector3[] vertices = new Vector3[4]
            {
                new(-0.5f, -0.5f, 0),
                new(0.5f, -0.5f, 0),
                new(-0.5f, 0.5f, 0),
                new(0.5f, 0.5f, 0)
            };

            int[] triangles = new int[6]
            {
                0, 2, 1,
                2, 3, 1
            };

            Vector2[] uv = new Vector2[4]
            {
                new(0,0),
                new(1,0),
                new(0,1),
                new(1,1)
            };

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }


        /// <summary> Set s bit to 0 or 1 at s specific bitIndex in s uint </summary>> /// 
        public static uint SetBit(uint value, int bitIndex, bool b)
        {
            if (bitIndex < 0 || bitIndex > 31)
            {
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be between 0 and 31");
            }

            if (b)
            {
                value |= (1u << bitIndex);
            }
            else
            {
                value &= ~(1u << bitIndex);
            }

            return value;
        }
        /// <summary> Encode "value" into X bitCount starting at bitOffset </summary>> /// 
        public static uint EncodeData(uint flags, int value, int bitOffset, int length)
        {
            if (length <= 0 || length > 32)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be between 1 and 32.");
            }                
            if (bitOffset < 0 || bitOffset > 31)
            {
                throw new ArgumentOutOfRangeException(nameof(bitOffset), "Offset must be between 0 and 31.");
            }
            if (bitOffset + length > 32)
            {
                throw new ArgumentOutOfRangeException("bitOffset + length exceeds 32 bits.");
            }

            // Extract and set individual bits into resultBuffer
            for (int i = 0; i < length; i++)
            {
                bool bit = ((value >> i) & 1) != 0;
                flags = SetBit(flags, bitOffset + i, bit);
            }

            return flags;
        }
        public static int DecodeData(uint value, int bitOffset, int length)
        {
            if (length <= 0 || length > 32)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be between 1 and 32.");
            }
            if (bitOffset < 0 || bitOffset > 31)
            {
                throw new ArgumentOutOfRangeException(nameof(bitOffset), "Offset must be between 0 and 31.");
            }
            if (bitOffset + length > 32)
            {
                throw new ArgumentOutOfRangeException("bitOffset + length exceeds 32 bits.");
            }

            // Shift right to remove lower irrelevant bits, then mask the desired field
            uint mask = (length == 32) ? uint.MaxValue : (1u << length) - 1;
            uint extracted = (value >> bitOffset) & mask;

            return (int)extracted;
        }
        public static uint EncodeColor(Color color, bool enabled)
        {
            byte r = (byte)(Mathf.Clamp01(color.linear.r) * 255f);
            byte g = (byte)(Mathf.Clamp01(color.linear.g) * 255f);
            byte b = (byte)(Mathf.Clamp01(color.linear.b) * 255f);
            byte a = (byte)(Mathf.Clamp01(color.linear.a) * 255f);

            uint data =
                ((uint)a << 24) |
                ((uint)r << 16) |
                ((uint)g << 8) |
                ((uint)b << 0);

            // bit0 = enable (blue LSB sacrifice)
            data &= 0xFFFFFFFE;
            data |= enabled ? 0x1u : 0x0u;

            return data;
        }
        public static Color DecodeColor(uint data, out bool enabled)
        {
            enabled = (data & 1u) != 0;

            byte a = (byte)((data >> 24) & 0xFF);
            byte r = (byte)((data >> 16) & 0xFF);
            byte g = (byte)((data >> 8) & 0xFF);
            byte b = (byte)(data & 0xFF);

            b = (byte)(b & 0xFE);

            return new Color32(r, g, b, a);
        }
        public static uint EncodeColor32(Color32 color, bool enabled)
        {
            uint data =
                ((uint)color.a << 24) |
                ((uint)color.r << 16) |
                ((uint)color.g << 8) |
                ((uint)color.b << 0);

            // bit0 = enable (blue LSB sacrifice)
            data &= 0xFFFFFFFE;
            data |= enabled ? 0x1u : 0x0u;

            return data;
        }
        public static Color32 DecodeColor32(uint data, out bool enabled)
        {
            enabled = (data & 1u) != 0;

            byte a = (byte)((data >> 24) & 0xFF);
            byte r = (byte)((data >> 16) & 0xFF);
            byte g = (byte)((data >> 8) & 0xFF);
            byte b = (byte)(data & 0xFF);

            // mirror shader: blue LSB is not color
            b = (byte)(b & 0xFE);

            return new Color32(r, g, b, a);
        }
        public static uint EncodeUV(Vector2 offset, float scale, bool enabled)
        {
            scale = Mathf.Clamp(scale, 0f, 8f);
            offset = Vector2.ClampMagnitude(offset, 4f);

            uint uScale = (uint)Mathf.RoundToInt(scale / 8f * 1023f);
            uint uOffX = (uint)Mathf.RoundToInt((offset.x + 4f) / 8f * 1023f);
            uint uOffY = (uint)Mathf.RoundToInt((offset.y + 4f) / 8f * 1023f);

            uint data = 0;

            if (enabled) data |= 1u; // bit 0      
            data |= (uScale & 0x3FFu) << 1;  // bit 1–10
            data |= (uOffX & 0x3FFu) << 11; // bit 11–20
            data |= (uOffY & 0x3FFu) << 21; // bit 21–30

            return data;
        }
        public static void DecodeUV(uint data, out Vector2 offset, out float scale,  out bool enabled)
        {
            enabled = (data & 1u) != 0;

            uint uScale = (data >> 1) & 0x3FFu;
            uint uOffX = (data >> 11) & 0x3FFu;
            uint uOffY = (data >> 21) & 0x3FFu;

            scale = (uScale / 1023f) * 8f;

            offset.x = (uOffX / 1023f) * 8f - 4f;
            offset.y = (uOffY / 1023f) * 8f - 4f;
        }
        #endregion

        #region UI
        public static void ScrollToView(this RectTransform thisTransform, ScrollRect scrollRect)
        {
            Rect rect = scrollRect.GetComponent<RectTransform>().rect;
            RectTransform content = scrollRect.content;

            // The position of the selected UI element is the absolute anchor position,
            // ie. the local position within the scroll rect + its height if we're
            // scrolling down. If we're scrolling up it's just the absolute anchor position.
            float selectedPositionY = Mathf.Abs(thisTransform.anchoredPosition.y) + thisTransform.rect.height;
            // The upper bound of the scroll view is the anchor position of the content we're scrolling.
            float scrollViewMinY = content.anchoredPosition.y;
            // The lower bound is the anchor position + the height of the scroll rect.
            float scrollViewMaxY = content.anchoredPosition.y + rect.height;
            // If the selected position is below the current lower bound of the scroll view we scroll down.
            if (selectedPositionY > scrollViewMaxY)
            {
                float newY = selectedPositionY - rect.height;
                content.anchoredPosition = new Vector2(content.anchoredPosition.x, newY);
            }
            // If the selected position is above the current upper bound of the scroll view we scroll up.
            else if (Mathf.Abs(thisTransform.anchoredPosition.y) < scrollViewMinY)
            {
                content.anchoredPosition = new Vector2(content.anchoredPosition.x, Mathf.Abs(thisTransform.anchoredPosition.y + thisTransform.sizeDelta.y / 2));
            }
        }
        public static void ClampToView(this RectTransform thisTransform, RectTransform rootCanvas)
        {
            ClampToView(thisTransform, rootCanvas, thisTransform.anchoredPosition);
        }
        public static void ClampToView(this RectTransform thisTransform, RectTransform rootCanvas, Vector2 screenPosition)
        {
            if (rootCanvas == null)
            {
                Debug.LogError("CoreUtility.ClampToView() rootCanvas == null!");
                return;
            }

            Vector2 pivot = thisTransform.pivot;

            if (pivot.x > 0 || pivot.y > 0)
            {
                Debug.LogWarning("CoreUtility.ClampToView() wrong pivot, pivot must be = (0,0)");
            }

            Vector2 position = screenPosition / rootCanvas.localScale.x; 
            Rect rootRect = rootCanvas.rect; Rect thisRect = thisTransform.rect; 

            if (position.x + thisRect.width > rootRect.width) position.x = rootRect.width - thisRect.width;
            if (position.y + thisRect.height > rootRect.height) position.y = rootRect.height - thisRect.height;

            thisTransform.anchoredPosition = position;
        }

        public static void BindSelectable(this Button thisButton, Selectable selectableUp, Selectable selectableDown, Selectable selectableLeft, Selectable selectableRight)
        {
            Navigation navigationData = thisButton.navigation;

            if (selectableUp != null)
            {
                navigationData.selectOnUp = selectableUp;
            }

            if (selectableDown != null)
            {
                navigationData.selectOnDown = selectableDown;
            }

            if (selectableLeft != null)
            {
                navigationData.selectOnLeft = selectableLeft;
            }

            if (selectableRight != null)
            {
                navigationData.selectOnRight = selectableRight;
            }

            thisButton.navigation = navigationData;
        }

        public static void AlignTopLeft(this RectTransform rectTransform, Vector2 offset = default)
        {
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new(0, 1);
            rectTransform.anchoredPosition = offset;
        }
        public static void AlignTopCenter(this RectTransform rectTransform, Vector2 offset = default)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 1);
            rectTransform.anchorMax = new Vector2(0.5f, 1);
            rectTransform.pivot = new Vector2(0.5f, 1);
            rectTransform.anchoredPosition = offset;
        }
        public static void AlignTopRight(this RectTransform rectTransform, Vector2 offset = default)
        {
            rectTransform.anchorMin = new Vector2(1, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(1, 1);
            rectTransform.anchoredPosition = offset;
        }
        public static void AlignMiddleLeft(this RectTransform rectTransform, Vector2 offset = default)
        {
            rectTransform.anchorMin = new Vector2(0, 0.5f);
            rectTransform.anchorMax = new Vector2(0, 0.5f);
            rectTransform.pivot = new Vector2(0, 0.5f);
            rectTransform.anchoredPosition = offset;
        }
        public static void AlignMiddleCenter(this RectTransform rectTransform, Vector2 offset = default)
        {
            rectTransform.pivot = new(0.5f, 0.5f);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = offset;
        }
        public static void AlignMiddleRight(this RectTransform rectTransform, Vector2 offset = default)
        {
            rectTransform.anchorMin = new Vector2(1, 0.5f);
            rectTransform.anchorMax = new Vector2(1, 0.5f);
            rectTransform.pivot = new Vector2(1, 0.5f);
            rectTransform.anchoredPosition = offset;
        }
        public static void AlignBottomLeft(this RectTransform rectTransform, Vector2 offset = default)
        {
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.pivot = new Vector2(0, 0);
            rectTransform.anchoredPosition = offset;
        }
        public static void AlignBottomCenter(this RectTransform rectTransform, Vector2 offset = default)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0);
            rectTransform.anchorMax = new Vector2(0.5f, 0);
            rectTransform.pivot = new Vector2(0.5f, 0);
            rectTransform.anchoredPosition = offset;
        }
        public static void AlignBottomRight(this RectTransform rectTransform, Vector2 offset = default)
        {
            rectTransform.anchorMin = new Vector2(1, 0);
            rectTransform.anchorMax = new Vector2(1, 0);
            rectTransform.pivot = new Vector2(1, 0);
            rectTransform.anchoredPosition = offset;
        }
        public static void AlignStretch(this RectTransform rectTransform, Vector2 offsetMin = default, Vector2 offsetMax = default)
        {
            rectTransform.pivot = new(0.5f, 0.5f);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
        }

        public static void Hide(this CanvasGroup canvasGroup)
        {
            if (canvasGroup == null)
            {
                return;
            }

            if (canvasGroup.alpha == 0 && !canvasGroup.interactable && !canvasGroup.blocksRaycasts)
            {
                return;
            }

            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        public static void Show(this CanvasGroup canvasGroup)
        {
            if (canvasGroup == null)
            {
                return;
            }

            if (canvasGroup.alpha == 1 && canvasGroup.interactable && canvasGroup.blocksRaycasts)
            {
                return;
            }

            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        public static void Show(this CanvasGroup canvasGroup, bool isInteractable, bool isBlockRaycast)
        {
            if (canvasGroup == null)
            {
                return;
            }

            if (canvasGroup.alpha == 1 && canvasGroup.interactable == isInteractable && canvasGroup.blocksRaycasts == isBlockRaycast)
            {
                return;
            }

            canvasGroup.alpha = 1;
            canvasGroup.interactable = isInteractable;
            canvasGroup.blocksRaycasts = isBlockRaycast;
        }
        public static void Hide(this Canvas canvas)
        {
            if (canvas == null)
            {
                return;
            }

            if (!canvas.enabled)
            {
                return;
            }

            canvas.enabled = false;
        }
        public static void Show(this Canvas canvas)
        {
            if (canvas == null)
            {
                return;
            }

            if (canvas.enabled)
            {
                return;
            }

            canvas.enabled = true;
        }

        /// <summary> It Does not cause rebuild mesh </summary> ///
        public static void SetCanvasColor(this Image image, Color color) => image.canvasRenderer.SetColor(color);
        /// <summary> It Does not cause rebuild mesh </summary> ///
        public static void SetCanvasAlpha(this Image image, float alpha) => image.canvasRenderer.SetAlpha(alpha);        
        #endregion

        #region STRING
        public const string STRING_FORMAT_0 = "0";
        public const string STRING_FORMAT_00 = "0.0";
        public const string STRING_FORMAT_000 = "0.00";
        public const string STRING_FORMAT_0000 = "0.000";
        public const string STRING_EMPTY = "";
        public const string STRING_LINE = "\n";
        public const string STRING_NULL = "NULL";
        public const string STRING_RED = "#e84f4f";
        public const string STRING_GREEN = "#50c878";
        public const string STRING_BLUE = "#77A0FF";
        public const string STRING_YELLOW = "#FFD891";
        public const string STRING_WHITE = "#ffffff";
        public const string STRING_GRAY = "#808080";
        public const string STRING_BLACK = "#000000";
        public const string STRING_GHOST = "#666666";

        public static readonly string OPEN_BOLD = "<b>";
        public static readonly string CLOSE_BOLD = "</b>";
        public static readonly string OPEN_RED = $"<color={STRING_RED}>";
        public static readonly string OPEN_GREEN = $"<color={STRING_GREEN}>";
        public static readonly string OPEN_BLUE = $"<color={STRING_BLUE}>";
        public static readonly string OPEN_YELLOW = $"<color={STRING_YELLOW}>";
        public static readonly string OPEN_WHITE = $"<color={STRING_WHITE}>";
        public static readonly string OPEN_GRAY = $"<color={STRING_GRAY}>";
        public static readonly string OPEN_BLACK = $"<color={STRING_BLACK}>";
        public static readonly string OPEN_GHOST = $"<color={STRING_GHOST}>";
        public static readonly string CLOSE_COLOR = "</color>";

        public static string GetSprite(int id, string color = STRING_WHITE) => $"<sprite={id} color={color}>";
        public static string ToBold(this string a) => OPEN_BOLD + a + CLOSE_BOLD;
        public static string ToRed(this string a) => OPEN_RED + a + CLOSE_COLOR;
        public static string ToGreen(this string a) => OPEN_GREEN + a + CLOSE_COLOR;
        public static string ToBlue(this string a) => OPEN_BLUE + a + CLOSE_COLOR;
        public static string ToYellow(this string a) => OPEN_YELLOW + a + CLOSE_COLOR;
        public static string ToWhite(this string a) => OPEN_WHITE + a + CLOSE_COLOR;
        public static string ToBlack(this string a) => OPEN_BLACK + a + CLOSE_COLOR;
        public static string ToGray(this string a) => OPEN_GRAY + a + CLOSE_COLOR;
        public static string ToGhost(this string a) => OPEN_GHOST + a + CLOSE_COLOR;
        public static string ToStyle(this string a, string id) => $"<style={id}>{a}</style>";
        #endregion

        #region COLOR
        public readonly static Color COLOR_TRANSPARENT = new(1, 1, 1, 0);
        public readonly static Color COLOR_BLACK = Color.black;
        public readonly static Color COLOR_WHITE = Color.white;
        public readonly static Color COLOR_GRAY = Color.gray;
        public readonly static Color COLOR_YELLOW = new(1f, 0.8465738f, 0.5686275f);
        public readonly static Color COLOR_BLUE = new(0.4666667F, 0.627451f, 1f);
        public readonly static Color COLOR_RED = new(0.909f, 0.309f, 0.309f);
        public readonly static Color COLOR_GREEN = new(0.313f, 0.784f, 0.470f);

        public static Color32 Randomize(this Color32 color, float threshold = 0f)
        {
            color.r = (byte)(Mathf.Max(threshold, (color.r * UnityEngine.Random.Range(0, 1f))));
            color.g = (byte)(Mathf.Max(threshold, (color.g * UnityEngine.Random.Range(0, 1f))));
            color.b = (byte)(Mathf.Max(threshold, (color.b * UnityEngine.Random.Range(0, 1f))));

            return color;
        }
        public static Color Randomize(this Color color, float threshold = 0f)
        {
            color.r = Mathf.Max(threshold, (color.r * UnityEngine.Random.Range(0, 1f)));
            color.g = Mathf.Max(threshold, (color.g * UnityEngine.Random.Range(0, 1f)));
            color.b = Mathf.Max(threshold, (color.b * UnityEngine.Random.Range(0, 1f)));

            return color;
        }
        public static Color Alpha(this Color color, float alpha) { color.a = alpha; return color; }

        #endregion

        #region C#
        public static void Shuffle<T>(this IList<T> collection)
        {
            for (int i = collection.Count - 1; i > 0; i--)
            {
                int randomIndex = UnityEngine.Random.Range(0, i + 1);

                (collection[randomIndex], collection[i]) = (collection[i], collection[randomIndex]);
            }
        }
        public class SwapBackArray<T> : IEnumerable<T>
        {
            public T this[int index]
            {
                get
                {
                    if (index < 0 || index >= Count) throw new IndexOutOfRangeException();
                    return items[index];
                }
                set
                {
                    if (index < 0 || index >= Count) throw new IndexOutOfRangeException();
                    items[index] = value;
                }
            } private readonly T[] items;
            public int Count { get; private set; } = 0;
            public int Capacity => items.Length;

            public SwapBackArray(uint capacity) => items = new T[capacity];
            public ref T GetRef(int index) => ref items[index];
            public void Truncate(int newCount)
            {
                if (newCount < 0 || newCount > Count)
                {
                    throw new ArgumentOutOfRangeException();
                }

                Count = newCount;
            }
            public void Add(T item)
            {
                if (Count >= items.Length)
                {
                    Debug.LogError("SwapBackArray.Add() capacity exceeded!");
                    return;
                }

                items[Count++] = item;
            }
            public void RemoveAt(int index)
            {
                if (index >= Count)
                {
                    throw new IndexOutOfRangeException();
                }

                items[index] = items[Count - 1];
                Count--;
            }
            public void RemoveAll(Predicate<T> match)
            {
                for (int i = 0; i < Count; i++)
                {
                    if (match(items[i]))
                    {
                        RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }
            }
            public void Clear() => Count = 0;

            public IEnumerator<T> GetEnumerator()
            {
                Debug.LogError("SwapBackArray.GetEnumerator() does not support for each loop!");
                throw new InvalidOperationException("SwapBackArray.GetEnumerator() does not support for each loop!");
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
        public class StackFloat
        {
            public float CurrentValue => currentValue;

            private float currentValue = 1;
            private readonly float baseValue = 1;
            private readonly float[] multipliers = null;
            private readonly bool[] used = null;

            public StackFloat(float baseValue, uint capacity)
            {
                this.baseValue = baseValue;
                this.currentValue = baseValue;
                this.multipliers = new float[capacity];
                this.used = new bool[capacity];
            }
            public void Apply(float multiplier, out int token)
            {
                token = -1;

                for (int i = 0; i < used.Length; i++)
                {
                    if (!used[i])
                    {
                        multipliers[i] = multiplier;
                        used[i] = true;
                        token = i;
                        Recalculate();
                        return;
                    }
                }

                Debug.LogWarning("StackFloat.Apply() not enough space!");
                return;
            }
            public void Revert(ref int token)
            {
                if (token < 0 || token >= used.Length)
                {
                    Debug.LogWarning("StackFloat.Revert() token out of range");
                    token = -1;
                    return;
                }

                if (!used[token])
                {
                    Debug.LogWarning("StackFloat.Revert() token already released");
                    token = -1;
                    return;
                }

                multipliers[token] = 0;
                used[token] = false;
                token = -1;
                Recalculate();
            }
            private void Recalculate()
            {
                currentValue = baseValue;

                for (int i = 0; i < used.Length; i++)
                {
                    if (used[i])
                    {
                        currentValue *= multipliers[i];
                    }
                }
            }
        }
        public class StackInt
        {
            public int CurrentValue => currentValue;

            private int currentValue = 1;
            private readonly int baseValue = 1;
            private readonly int[] values = null;
            private readonly bool[] used = null;

            public StackInt(int baseValue, uint capacity)
            {
                this.baseValue = baseValue;
                this.currentValue = baseValue;
                this.values = new int[capacity];
                this.used = new bool[capacity];
            }
            public void Add(int value, out int token)
            {
                token = -1;

                for (int i = 0; i < used.Length; i++)
                {
                    if (!used[i])
                    {
                        values[i] = value;
                        used[i] = true;
                        token = i;
                        Recalculate();
                        return;
                    }
                }

                Debug.LogWarning("StackInt.Add() not enough space!");
            }
            public void Remove(ref int token)
            {
                if (token < 0 || token >= used.Length)
                {
                    Debug.LogWarning("StackInt.Remove() token out of range");
                    token = -1;
                    return;
                }

                if (!used[token])
                {
                    Debug.LogWarning("StackInt.Remove() token already released");
                    token = -1;
                    return;
                }

                values[token] = 0;
                used[token] = false;
                token = -1;
                Recalculate();
            }
            private void Recalculate()
            {
                currentValue = baseValue;

                for (int i = 0; i < used.Length; i++)
                {
                    if (used[i])
                    {
                        currentValue += values[i];
                    }
                }
            }
        }
        public class StackBool
        {
            public bool IsEnabled => disableCount == 0;

            private int disableCount = 0;
            private readonly bool[] used = null;

            public StackBool(uint capacity) => used = new bool[capacity];
            public void Disable(out int token)
            {
                token = -1;

                for (int i = 0; i < used.Length; i++)
                {
                    if (!used[i])
                    {
                        used[i] = true;
                        disableCount++;
                        token = i;
                        return;
                    }
                }

                Debug.LogWarning("StackBool.Disable() not enough space!");
            }
            public void Enable(ref int token)
            {
                if (token < 0 || token >= used.Length)
                {
                    Debug.LogWarning("StackBool.Enable() token out of range");
                    token = -1;
                    return;
                }

                if (!used[token])
                {
                    Debug.LogWarning("StackBool.Enable() token already released");
                    token = -1;
                    return;
                }

                used[token] = false;
                token = -1;
                disableCount--;

                if (disableCount < 0)
                {
                    Debug.LogError("StackBool.Enable() internal counter underflow");
                    disableCount = 0;
                }
            }
        }
        #endregion

        #region ATTRIBUTES
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
        public class Required : PropertyAttribute { }

        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
        public class ReadOnly : PropertyAttribute { }

        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
        public class Tag : PropertyAttribute { }
        #endregion

        #region TRANSFORM
        public static void SetGlobalScale(this Transform transform, Transform parentTransform, Vector3 globalScale)
        {
            if (parentTransform == null)
            {
                transform.localScale = globalScale;
            }
            else
            {
                Vector3 parentScale = parentTransform.lossyScale;
                transform.localScale = new Vector3(globalScale.x / parentScale.x, globalScale.y / parentScale.y, globalScale.z / parentScale.z);
            }
        }
        public static Transform GetClosest(this Transform[] transform, Vector3 position, float threshold = -1)
        {
            Transform c = null;
            float d = threshold > 0 ? (threshold * threshold) : float.MaxValue;

            foreach (Transform p in transform)
            {
                float v = (p.position - position).sqrMagnitude;

                if (v < d)
                {
                    d = v;
                    c = p;
                }
            }

            return c;
        }
        public static Transform GetHighest(this Transform[] transform)
        {
            Transform c = null;
            float d = float.MinValue;

            foreach (Transform p in transform)
            {
                if (p.position.y > d)
                {
                    d = p.position.y;
                    c = p;
                }
            }

            return c;
        }
        public static Transform GetLowest(this Transform[] transform)
        {
            Transform c = null;
            float d = float.MaxValue;

            foreach (Transform p in transform)
            {
                if (p.position.y < d)
                {
                    d = p.position.y;
                    c = p;
                }
            }

            return c;
        }
        #endregion
    }
}