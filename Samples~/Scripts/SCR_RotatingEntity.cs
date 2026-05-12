using UnityEngine;
using Core;

namespace Game
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public class RotatingEntity : MonoBehaviour
    {
        [Header("_")]
        [SerializeField, ReadOnly] private bool isActive = false;
        [SerializeField] private bool startOnAwake = true;

        [Header("_")]
        [SerializeField] private Transform point = null;
        [SerializeField] private Space space = Space.Self;
        [SerializeField] private Vector3 axis = Vector3.up;
        [SerializeField] private float speed = 40f;

        [Header("_")]
        [SerializeField] private bool useCurve = false;
        [SerializeField] private AnimationCurve curveSpeed = AnimationCurve.Linear(0, 1, 1, 1);
        [SerializeField, Min(0)] private float curveDuration = 1;

        private Transform thisTransform = null;
        private float curveTime = 0f;

        private void Awake()
        {
            thisTransform = GetComponent<Transform>();

            Activate(startOnAwake);
        }
        private void Update()
        {
            if (!isActive)
            {
                return;
            }

            float t = speed * Time.deltaTime;

            if (useCurve)
            {
                curveTime += Time.deltaTime / curveDuration;

                if (curveTime > 1f)
                {
                    curveTime -= 1f;
                }

                t *= curveSpeed.Evaluate(curveTime);
            }

            if (point != null)
            {
                thisTransform.RotateAround(point.position, axis, t);
            }
            else
            {
                thisTransform.Rotate(axis, t, space);
            }
        }

        public void Activate(bool state) => isActive = state;
    }
}