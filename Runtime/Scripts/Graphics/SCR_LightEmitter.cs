using System;
using UnityEngine;

namespace Core.Graphics
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public sealed class LightEmitter : MonoBehaviour
    {
        public bool IsActive => thisLight.isActiveAndEnabled;
        public float Brightness => currentBrightness;

        [Header("_")]
        [SerializeField] private Light thisLight = null;
        [SerializeField] private MeshRenderer thisMesh = null;

        [Header("_")]
        [SerializeField, ColorUsage(false, true)] private Color lightColor = COLOR_WHITE;
        [SerializeField, Min(0)] private float lightIntensity = 1;

        [Header("_")]
        [SerializeField] private bool updateAlways = false;
        [SerializeField] private LightAnimation animationStyle = LightAnimation.DEFAULT;
        [SerializeField, Range(10, 60)] private float animationRate = 1;

        private float defaultIntensity = 1;
        private float currentBrightness = 1;

        private void Awake()
        {
            InitializeLight();
            InitializeMesh();
        }
        private void Update()
        {
            if (!updateAlways && animationStyle == LightAnimation.DEFAULT)
            {
                return;
            }

            currentBrightness = LightAnimator.Calculate(animationStyle, animationRate);

            if (thisLight != null)
            {
                UpdateLight();
            }
           
            if (thisMesh != null)
            {
                UpdateMesh();
            }
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (thisLight != null)
            {
                thisLight.color = lightColor;
                thisLight.intensity = lightIntensity;
            }

            if (thisMesh != null)
            {
                UpdateMesh();
            }
        }
#endif
        public void Enable()
        {
            if (thisLight != null)
            {
                thisLight.enabled = true;
                UpdateLight();
            }

            if (thisMesh != null)
            {
                UpdateMesh();
            }
        }
        public void Enable(LightAnimation id, float rate)
        {
            this.animationStyle = id;
            this.animationRate = Mathf.Clamp(rate, 10, 60);

            Enable();
        }
        public void Disable()
        {
            if (thisLight != null)
            {
                thisLight.enabled = false;
            }

            if (thisMesh != null)
            {
                UpdateMesh();
            }
        }    

        private void InitializeLight()
        {
            if (thisLight == null)
            {
                return;
            }

            defaultIntensity = thisLight.intensity;
            currentBrightness = 1;
        }
        private void InitializeMesh()
        {
            if (thisMesh == null)
            {
                return;
            }

            UpdateMesh();
        }

        private void UpdateLight()
        {
            if (thisLight.isActiveAndEnabled)
            {
                thisLight.intensity = defaultIntensity * currentBrightness;
            }
        }
        private void UpdateMesh()
        {
            Color targetColor;

            if (thisLight != null)
            {
                targetColor = thisLight.enabled ? lightColor * defaultIntensity * currentBrightness : COLOR_BLACK;
            }
            else
            {
                targetColor = lightColor * defaultIntensity * currentBrightness;
            }

            thisMesh.SetShaderUserValue(EncodeColorWithFlag(targetColor, true));
        }
    }
}