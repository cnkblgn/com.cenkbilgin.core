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
        [SerializeField] private bool disableOnAwake = false; 

        [Header("_")]
        [SerializeField] private Light[] lights = new Light[0];
        [SerializeField] private MeshRenderer[] meshes = new MeshRenderer[0];

        [Header("_")]
        [SerializeField, ColorUsage(false, true)] private Color meshColor = COLOR_WHITE;
        [SerializeField, ColorUsage(false, false)] private Color lightColor = COLOR_WHITE;
        [SerializeField, Min(0)] private float lightIntensity = 1;

        [Header("_")]
        [SerializeField] private bool updateAlways = false;
        [SerializeField] private LightAnimation animationStyle = LightAnimation.DEFAULT;
        [SerializeField, Range(10, 60)] private float animationRate = 10;

        private bool isActive = true;
        private float currentBrightness = 1;

        private void Awake()
        {
            UpdateLights();
            UpdateMeshes();

            if (disableOnAwake)
            {
                Disable();
            }
        }
        private void Update()
        {
            if (!updateAlways && animationStyle == LightAnimation.DEFAULT)
            {
                return;
            }

            Refresh();
        }

#if UNITY_EDITOR
        [ContextMenu("Enable All Lights")]
        private void EnableAll() => EnableLights();

        [ContextMenu("Disable All Lights")]
        private void DisableAll() => DisableLights();

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                return;
            }

            UpdateLights();
        }
#endif

        private void Refresh()
        {
            currentBrightness = LightAnimator.Calculate(animationStyle, animationRate);
            UpdateLights();
            UpdateMeshes();
        }

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
            SetAnimationID(id);
            SetAnimationRate(rate);

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

        public LightAnimation GetAnimationID() => animationStyle;
        public void SetAnimationID(LightAnimation id) => animationStyle = id;
        public void SetAnimationID(int id)
        {
            animationStyle = (LightAnimation)id;
            Refresh();
        }

        public float GetAnimationRate() => animationRate;
        public void SetAnimationRate(float rate)
        {
            animationRate = Mathf.Clamp(rate, 10, 60);
            Refresh();
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
                UpdateLight(lights[i]);
            }
        }
        private void UpdateLight(Light light)
        {
            if (light == null)
            {
#if UNITY_EDITOR
                Debug.LogError("Light emitter light update failed! light is null!", gameObject);
#endif
                return;
            }

            if (light.isActiveAndEnabled)
            {
                light.color = lightColor;
                light.intensity = lightIntensity * currentBrightness;
            }
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
                Debug.LogError("Light emitter mesh update failed! mesh is null!", gameObject);
#endif
                return;
            }

            Color targetColor = isActive ? meshColor * currentBrightness : COLOR_BLACK;

            mesh.SetShaderUserValue(EncodeColorWithFlag(targetColor, true));
        }
    }
}