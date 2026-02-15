using UnityEngine;
using Core.Input;
using Core.UI;

namespace Game
{
    using static InputActionDatabase;

    public class MovementProcessorNoclip : MonoBehaviour, IMovementProcessor
    {
        public int Priority => 0;
        public bool IsActive => isActive;

        [Header("_")]
        [SerializeField, Min(1)] private int maxStep = 15;

        private MovementController movementController = null;
        private float targetSpeed = 0;
        private int targetIndex = 7;
        private int movementToken = 0;
        private bool isActive = false;

        private void Awake() => movementController = GetComponent<MovementController>();

        public void OnBeforeMove(MovementController movementController)
        {
            if (!isActive)
            {
                return;
            }

            Vector3 direction = this.movementController.GetCameraOrigin().TransformVector(new(Move.GetAxis().x, 0f, Move.GetAxis().y));

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

            movementController.SetVelocity(targetSpeed * direction);
        }
        public void OnBeforeLook(MovementController controller) { }

        public void Toggle()
        {
            isActive = !isActive;

            if (isActive)
            {
                movementController.DisableMovement(out movementToken);
                movementController.SetCollisionMask(0);
                movementController.SetVelocity(Vector3.zero);
            }
            else
            {

                movementController.EnableMovement(ref movementToken);
                movementController.ResetCollisionMask();
            }
        }
    }
}
