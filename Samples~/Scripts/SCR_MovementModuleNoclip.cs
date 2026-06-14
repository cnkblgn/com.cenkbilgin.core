using UnityEngine;
using Core.Input;
using Core.UI;

namespace Game
{
    using static InputDatabase;

    public class MovementModuleNoclip : MonoBehaviour, IMovementModule
    {
        public int Priority => 0;
        public bool IsActive => isActive;

        [Header("_")]
        [SerializeField, Min(1)] private int maxStep = 15;

        private MovementController controller = null;
        private float targetSpeed = 0;
        private int targetIndex = 7;
        private bool isActive = false;

        public void OnStateChanged(MovementContext ctx) { }
        public void Bind(MovementController controller)
        {
            this.controller = controller;
        }
        public void Unbind(MovementController controller)
        {

        }

        public void OnBeforeMove()
        {
            if (!isActive)
            {
                return;
            }

            Vector3 direction = controller.GetCameraOrigin().TransformVector(new(Move.GetAxis().x, 0f, Move.GetAxis().y));

            if (ManagerCoreInput.Instance.PointerScroll.y > 0)
            {
                targetIndex = Mathf.Clamp(targetIndex + 1, 1, maxStep);
                ManagerCoreUI.Instance.ShowNotification($"Noclip Speed: {targetIndex}", 3f);
            }
            else if (ManagerCoreInput.Instance.PointerScroll.y < 0)
            {
                targetIndex = Mathf.Clamp(targetIndex - 1, 1, maxStep);
                ManagerCoreUI.Instance.ShowNotification($"Noclip Speed: {targetIndex}", 3f);
            }

            targetSpeed = targetIndex * 0.5f;

            controller.SetVelocity(targetSpeed * direction);
        }
        public void OnBeforeLook() { }

        public void Toggle()
        {
            isActive = !isActive;

            if (isActive)
            {
                controller.DisableMovement(this);
                controller.SetCollisionMask(0);
                controller.SetVelocity(Vector3.zero);
            }
            else
            {

                controller.EnableMovement(this);
                controller.ResetCollisionMask();
            }
        }
    }
}
