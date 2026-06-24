using System;
using UnityEngine;

namespace Core.Graphics
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public sealed class LightEmitter : MonoBehaviour
    {
        public bool IsActive => isActive;
        public float Brightness => currentBrightness;

        [Header("_")]
        [SerializeField] private Light[] lights = new Light[0];
        [SerializeField] private MeshRenderer[] meshes = new MeshRenderer[0];

        [Header("_")]
        [SerializeField, ColorUsage(false, true)] private Color lightColor = COLOR_WHITE;
        [SerializeField, Min(0)] private float lightIntensity = 1;

        [Header("_")]
        [SerializeField] private bool updateAlways = false;
        [SerializeField] private LightAnimation animationStyle = LightAnimation.DEFAULT;
        [SerializeField, Range(10, 60)] private float animationRate = 1;

        private bool isActive = true;
        private float[] defaultIntensities;
        private float currentBrightness = 1;

        private void Awake()
        {
            InitializeLights();
            InitializeMeshes();
        }
        private void Update()
        {
            if (!updateAlways && animationStyle == LightAnimation.DEFAULT)
            {
                return;
            }

            currentBrightness = LightAnimator.Calculate(animationStyle, animationRate);

            UpdateLights();
            UpdateMeshes();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            InitializeLights();
            UpdateLights();
            UpdateMeshes();
        }
#endif

        public void Enable()
        {
            if (isActive)
            {
                return;
            }

            isActive = true;

            EnableLights();
            UpdateLights();

            UpdateMeshes();
        }
        public void Enable(LightAnimation id, float rate)
        {
            this.animationStyle = id;
            this.animationRate = Mathf.Clamp(rate, 10, 60);

            Enable();
        }
        public void Disable()
        {
            if (!isActive)
            {
                return;
            }

            isActive = false;

            DisableLights();
            UpdateMeshes();
        }    

        private void InitializeLights()
        {
            defaultIntensities = new float[lights.Length];

            for (int i = 0; i < lights.Length; i++)
            {
                defaultIntensities[i] = lights[i].intensity;
            }

            currentBrightness = 1;
        }
        private void EnableLights()
        {
            for (int i = 0; i < lights.Length; i++)
            {
                lights[i].enabled = true;
            }
        }
        private void DisableLights()
        {
            for (int i = 0; i < lights.Length; i++)
            {
                lights[i].enabled = false;
            }
        }
        private void UpdateLights()
        {
            for (int i = 0; i < lights.Length; i++)
            {
                UpdateLight(lights[i], defaultIntensities[i]);
            }
        }
        private void UpdateLight(Light light, float intensity)
        {
            if (light == null)
            {
#if UNITY_EDITOR
                Debug.LogError("Light emitter light update failed! light is null!");
#endif
                return;
            }

            if (light.isActiveAndEnabled)
            {
                light.intensity = intensity * lightIntensity * currentBrightness;
            }
        }

        private void InitializeMeshes()
        {
            if (meshes == null)
            {
                return;
            }

            UpdateMeshes();
        }
        private void UpdateMeshes()
        {
            for (int i = 0; i < meshes.Length; i++)
            {
                UpdateMesh(meshes[i]);
            }
        }
        private void UpdateMesh(MeshRenderer mesh)
        {
            if (mesh == null)
            {
#if UNITY_EDITOR
                Debug.LogError("Light emitter mesh update failed! mesh is null!");
#endif
                return;
            }

            Color targetColor = isActive ? lightColor * currentBrightness : COLOR_BLACK;

            mesh.SetShaderUserValue(EncodeColorWithFlag(targetColor, true));
        }
    }
}