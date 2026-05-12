using Codice.CM.Common;
using Core;
using System.Drawing;
using Unity.Mathematics;
using UnityEngine;

namespace Game
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public class ScalingEntity : MonoBehaviour
    {
        [Header("_")]
        [SerializeField, ReadOnly] private bool isActive = false;
        [SerializeField] private bool startOnAwake = true;

        [Header("_")]
        [SerializeField] private Vector3 axis = Vector3.up;
        [SerializeField] private float speed = 2f;

        private Transform thisTransform = null;
        private Vector3 startScale = Vector3.one;

        private void Awake()
        {
            thisTransform = GetComponent<Transform>();

            startScale = thisTransform.localScale;

            Activate(startOnAwake);
        }
        private void Update()
        {
            if (!isActive)
            {
                return;
            }

            float scaleOffset = math.remap(-1, 1, 0, 1, Mathf.Sin(Time.time * speed));
            transform.localScale = startScale + (axis * scaleOffset);
        }

        public void Activate(bool state) => isActive = state;
    }
}
