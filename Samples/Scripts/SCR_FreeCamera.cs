using UnityEngine;
using Core.Input;

namespace Core.Misc
{
    using static InputActionDatabase;

    [DisallowMultipleComponent]   
    public class FreeCamera : MonoBehaviour
    {
        [Header("_")]
        [SerializeField, Min(0.01f)] private float lookSpeed = 1.0f;
        [SerializeField, Min(1.0f)] private float moveSpeed = 10.0f;

        private Transform thisTransform = null;
        private Vector2 inputMove = Vector2.zero;
        private Vector2 inputLook = Vector2.zero;
        private Vector3 velocityMove = Vector3.zero;
        private Vector3 velocityLook = Vector3.zero;
        private float inputScroll = 0.0f;
        private float inputSpeed = 1.0f;

        private void Awake() => thisTransform = GetComponent<Transform>();
        private void Update()
        {
            inputScroll = ManagerCoreInput.Instance.PointerScroll.x;
            inputLook = ManagerCoreInput.Instance.GetVector(Look);
            inputMove = ManagerCoreInput.Instance.GetVector(Move);
            inputSpeed = Mathf.Max(0.1f, inputSpeed + (inputScroll > 0 ? 0.1f : inputScroll < 0 ? -0.1f : 0));
            velocityMove = (inputSpeed * moveSpeed * thisTransform.TransformVector(inputMove.x, 0, inputMove.y));
            transform.position += velocityMove * Time.deltaTime;
        }
        private void LateUpdate()
        {
            velocityLook = new(inputLook.x * lookSpeed, -inputLook.y * lookSpeed);
            thisTransform.rotation *= Quaternion.AngleAxis(velocityLook.y, Vector3.right);
            thisTransform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + velocityLook.x, transform.eulerAngles.z);
        }
    }
}