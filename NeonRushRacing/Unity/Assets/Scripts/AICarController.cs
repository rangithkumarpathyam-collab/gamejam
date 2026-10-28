using UnityEngine;

namespace NeonRush
{
    /// <summary>
    /// Autonomous racing AI that drives rival cars along the circuit.
    /// Uses real Rigidbody physics, cornering speed prediction, and obstacle avoidance.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class AICarController : MonoBehaviour
    {
        [Header("AI Identity")]
        public string driverName = "Rival AI";
        public Color carColor = Color.cyan;

        [Header("Engine & Speed")]
        [SerializeField] private float acceleration = 52f;
        [SerializeField] private float topSpeed = 42f; // ~150 km/h
        [SerializeField] private float corneringSpeed = 24f; // ~86 km/h on sharp bends
        [SerializeField] private float brakingForce = 70f;
        [SerializeField] private float lateralGrip = 0.94f;
        [SerializeField] private float downforce = 35f;

        [Header("Navigation & Avoidance")]
        [SerializeField] private float baseLookahead = 14f;
        [SerializeField] private float speedLookaheadMultiplier = 0.4f;
        [SerializeField] private float avoidanceDistance = 12f;
        [SerializeField] private LayerMask obstacleLayers = ~0;

        [Header("Visual Wheels")]
        [SerializeField] private Transform wheelFL;
        [SerializeField] private Transform wheelFR;
        [SerializeField] private Transform wheelRL;
        [SerializeField] private Transform wheelRR;

        // Internal State
        private Rigidbody rb;
        private int currentWaypointIndex;
        private float currentSteerAngle;
        private float wheelSpinAngle;
        private Quaternion baseFLRot, baseFRRot, baseRLRot, baseRRRot;
        private bool isGrounded;

        // Race tracking
        public bool ControlsEnabled { get; set; } = true;
        public int CurrentLap { get; set; } = 1;
        public float LapProgress { get; private set; }
        public float TotalRaceProgress => (CurrentLap - 1) + LapProgress;
        public float CurrentSpeedKmh => rb != null ? (Vector3.Dot(rb.linearVelocity, transform.forward) * 3.6f) : 0f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.mass = 1200f;
            rb.linearDamping = 0.05f;
            rb.angularDamping = 2.0f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            AutoHookWheels();
        }

        private void Start()
        {
            currentWaypointIndex = TrackWaypoints.GetClosestIndex(transform.position);
        }

        private void AutoHookWheels()
        {
            Transform[] children = GetComponentsInChildren<Transform>(true);
            foreach (var c in children)
            {
                string n = c.name.ToLower();
                if (n == "wheel_fl") wheelFL = c;
                else if (n == "wheel_fr") wheelFR = c;
                else if (n == "wheel_rl") wheelRL = c;
                else if (n == "wheel_rr") wheelRR = c;
            }

            if (wheelFL != null) baseFLRot = wheelFL.localRotation;
            if (wheelFR != null) baseFRRot = wheelFR.localRotation;
            if (wheelRL != null) baseRLRot = wheelRL.localRotation;
            if (wheelRR != null) baseRRRot = wheelRR.localRotation;
        }

        private void FixedUpdate()
        {
            PerformGroundCheck();
            UpdateNavigation();

            float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
            float lateralSpeed = Vector3.Dot(rb.linearVelocity, transform.right);

            if (isGrounded)
            {
                if (ControlsEnabled)
                {
                    ApplyAIDriving(forwardSpeed);
                }
                ApplyLateralGrip(lateralSpeed);
                ApplyDownforce(forwardSpeed);
            }

            ApplyStability();
            CheckOutOfBounds();
        }

        private void Update()
        {
            UpdateVisualWheels();
        }

        private void PerformGroundCheck()
        {
            RaycastHit hit;
            isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.5f, -Vector3.up, out hit, 1.3f, obstacleLayers, QueryTriggerInteraction.Ignore);
        }

        private void UpdateNavigation()
        {
            // Update current waypoint
            Vector3 myPos = transform.position;
            float currentDist = (TrackWaypoints.GetPoint(currentWaypointIndex) - myPos).sqrMagnitude;
            int nextIdx = (currentWaypointIndex + 1) % TrackWaypoints.Points.Length;
            float nextDist = (TrackWaypoints.GetPoint(nextIdx) - myPos).sqrMagnitude;

            if (nextDist < currentDist || currentDist < 64f) // within 8 meters
            {
                // Check if crossed finish line
                if (currentWaypointIndex > 160 && nextIdx < 20)
                {
                    CurrentLap++;
                }
                currentWaypointIndex = nextIdx;
            }

            LapProgress = (float)currentWaypointIndex / TrackWaypoints.Points.Length;
        }

        private void ApplyAIDriving(float forwardSpeed)
        {
            // 1. Calculate Target Point along track
            float lookahead = baseLookahead + Mathf.Max(0f, forwardSpeed) * speedLookaheadMultiplier;
            int lookaheadSteps = Mathf.Clamp(Mathf.RoundToInt(lookahead / 6f), 2, 8);
            int targetIdx = (currentWaypointIndex + lookaheadSteps) % TrackWaypoints.Points.Length;
            Vector3 targetPoint = TrackWaypoints.GetPoint(targetIdx);

            // 2. Obstacle & Car Avoidance
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;
            float avoidanceSteer = 0f;

            RaycastHit hit;
            // Ray left
            if (Physics.Raycast(transform.position + Vector3.up * 0.4f - right * 0.6f, forward + right * -0.2f, out hit, avoidanceDistance, obstacleLayers))
            {
                if (hit.collider != null && !hit.collider.transform.IsChildOf(transform))
                    avoidanceSteer += (1f - hit.distance / avoidanceDistance) * 0.8f;
            }
            // Ray right
            if (Physics.Raycast(transform.position + Vector3.up * 0.4f + right * 0.6f, forward + right * 0.2f, out hit, avoidanceDistance, obstacleLayers))
            {
                if (hit.collider != null && !hit.collider.transform.IsChildOf(transform))
                    avoidanceSteer -= (1f - hit.distance / avoidanceDistance) * 0.8f;
            }

            // 3. Compute Steering Angle
            Vector3 localTarget = transform.InverseTransformPoint(targetPoint);
            float steerFactor = Mathf.Clamp(localTarget.x / localTarget.magnitude + avoidanceSteer, -1f, 1f);

            // Apply yaw steering torque
            float targetYawRate = steerFactor * 65f * Mathf.Deg2Rad;
            float yawDamping = 4f;
            Vector3 currentAngVel = rb.angularVelocity;
            currentAngVel.y = Mathf.Lerp(currentAngVel.y, targetYawRate, Time.fixedDeltaTime * yawDamping);
            rb.angularVelocity = currentAngVel;

            // 4. Cornering Speed Prediction
            float turnSharpness = Mathf.Abs(steerFactor);
            float targetSpeed = Mathf.Lerp(topSpeed, corneringSpeed, turnSharpness);

            if (forwardSpeed < targetSpeed)
            {
                float accelForce = acceleration * (1f - forwardSpeed / topSpeed);
                rb.AddForce(forward * accelForce, ForceMode.Acceleration);
            }
            else if (forwardSpeed > targetSpeed + 2f)
            {
                rb.AddForce(-forward * brakingForce, ForceMode.Acceleration);
            }

            currentSteerAngle = Mathf.MoveTowards(currentSteerAngle, steerFactor * 26f, Time.fixedDeltaTime * 120f);
        }

        private void ApplyLateralGrip(float lateralSpeed)
        {
            Vector3 lateralImpulse = -transform.right * (lateralSpeed * lateralGrip);
            rb.AddForce(lateralImpulse, ForceMode.VelocityChange);
        }

        private void ApplyDownforce(float forwardSpeed)
        {
            float speedRatio = Mathf.Clamp01(Mathf.Abs(forwardSpeed) / topSpeed);
            rb.AddForce(-Vector3.up * (downforce * speedRatio * speedRatio + 15f), ForceMode.Acceleration);
        }

        private void ApplyStability()
        {
            // Upright stabilization
            Vector3 predictedUp = Quaternion.AngleAxis(rb.angularVelocity.magnitude * Mathf.Rad2Deg * 0.1f / 10f, rb.angularVelocity) * transform.up;
            Vector3 torque = Vector3.Cross(predictedUp, Vector3.up);
            rb.AddTorque(torque * 25f, ForceMode.Acceleration);
        }

        private void CheckOutOfBounds()
        {
            if (transform.position.y < -5f)
            {
                Vector3 safePoint = TrackWaypoints.GetPoint(currentWaypointIndex);
                transform.position = safePoint + Vector3.up * 0.5f;
                Vector3 dir = TrackWaypoints.GetDirection(currentWaypointIndex);
                transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
                rb.linearVelocity = dir * 15f;
                rb.angularVelocity = Vector3.zero;
            }
        }

        private void UpdateVisualWheels()
        {
            float speed = Vector3.Dot(rb.linearVelocity, transform.forward);
            float spinDelta = (speed * Time.deltaTime / 0.35f) * Mathf.Rad2Deg;
            wheelSpinAngle = (wheelSpinAngle + spinDelta) % 360f;

            if (wheelFL != null)
                wheelFL.localRotation = baseFLRot * Quaternion.Euler(0f, currentSteerAngle, 0f) * Quaternion.Euler(wheelSpinAngle, 0f, 0f);
            if (wheelFR != null)
                wheelFR.localRotation = baseFRRot * Quaternion.Euler(0f, currentSteerAngle, 0f) * Quaternion.Euler(wheelSpinAngle, 0f, 0f);
            if (wheelRL != null)
                wheelRL.localRotation = baseRLRot * Quaternion.Euler(wheelSpinAngle, 0f, 0f);
            if (wheelRR != null)
                wheelRR.localRotation = baseRRRot * Quaternion.Euler(wheelSpinAngle, 0f, 0f);
        }
    }
}
