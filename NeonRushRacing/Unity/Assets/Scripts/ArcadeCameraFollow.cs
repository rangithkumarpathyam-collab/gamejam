using UnityEngine;

namespace NeonRush
{
    /// <summary>
    /// Smooth chase camera for arcade racing.
    /// Follows the player car with smooth position dampening and look-ahead.
    /// </summary>
    public class ArcadeCameraFollow : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("Transform of the car to follow.")]
        [SerializeField] private Transform target;

        [Header("Follow Offsets")]
        [Tooltip("Distance behind the car.")]
        [SerializeField] private float distance = 7.0f;

        [Tooltip("Height above the car.")]
        [SerializeField] private float height = 2.6f;

        [Tooltip("Look target height offset above car pivot.")]
        [SerializeField] private float lookAtHeightOffset = 0.8f;

        [Header("Smoothness")]
        [Tooltip("Position follow speed (higher = tighter follow).")]
        [SerializeField] private float positionDamping = 10f;

        [Tooltip("Rotation follow speed.")]
        [SerializeField] private float rotationDamping = 8f;

        private void LateUpdate()
        {
            if (target == null) return;

            // Desired position behind the car's current heading
            Vector3 targetForward = target.forward;
            targetForward.y = 0f;
            if (targetForward.sqrMagnitude < 0.001f) targetForward = Vector3.forward;
            targetForward.Normalize();

            Vector3 desiredPosition = target.position - targetForward * distance + Vector3.up * height;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * positionDamping);

            // Look slightly ahead of the car
            Vector3 lookTarget = target.position + Vector3.up * lookAtHeightOffset + targetForward * 2f;
            Quaternion desiredRotation = Quaternion.LookRotation(lookTarget - transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * rotationDamping);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}

