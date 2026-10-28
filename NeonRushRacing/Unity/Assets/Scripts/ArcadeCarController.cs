using UnityEngine;

namespace NeonRush
{
    /// <summary>
    /// Arcade Car Controller for Neon Rush Racing.
    /// Uses Unity Rigidbody physics tuned for fast, stable, arcade-style racing:
    /// - Instant, responsive WASD & Arrow Keys input.
    /// - Razor-sharp lateral grip prevents sliding out of track on high-speed corners.
    /// - Road adhesion & downforce keeps car glued over crests and steep drops (no flying off).
    /// - Automatic out-of-bounds recovery: respawns safely on track if falling below Y threshold.
    /// - Manual respawn with 'R' key.
    /// - Auto-unflip recovery if rolled over for > 1.5 seconds.
    /// - Nitro Boost on Left Shift with meter drain and auto-recharge.
    /// - Anti-flip stabilization: lowered center of mass, upright torque, and pitch/roll damping.
    /// - Compatible with Unity 6 (linearVelocity) and older versions.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [SelectionBase]
    public class ArcadeCarController : MonoBehaviour
    {
        [Header("Engine & Speed Settings")]
        [Tooltip("Forward driving acceleration force.")]
        [SerializeField] private float acceleration = 60f;

        [Tooltip("Top forward speed in meters per second (45 m/s ~ 162 km/h).")]
        [SerializeField] private float maxSpeed = 45f;

        [Tooltip("Reverse engine acceleration force.")]
        [SerializeField] private float reverseAcceleration = 30f;

        [Tooltip("Top reverse speed in meters per second.")]
        [SerializeField] private float maxReverseSpeed = 18f;

        [Tooltip("Braking force applied when reversing direction or using handbrake.")]
        [SerializeField] private float brakingForce = 85f;

        [Tooltip("Natural rolling resistance when neither gas nor brake is pressed.")]
        [SerializeField] private float coastDamping = 2.5f;

        [Header("Steering & Handling")]
        [Tooltip("Turn rate in degrees per second.")]
        [SerializeField] private float steerSensitivity = 105f;

        [Tooltip("Minimum steering factor when stationary so you can always turn out of corners (0.1 to 1.0).")]
        [Range(0.1f, 1f)]
        [SerializeField] private float stationarySteerFactor = 0.5f;

        [Tooltip("Steering reduction at top speed to maintain high-speed stability.")]
        [Range(0f, 0.7f)]
        [SerializeField] private float highSpeedSteerReduction = 0.25f;

        [Tooltip("Lateral tire grip (0 to 1). High = tight arcade lines; Low = looser slide.")]
        [Range(0.3f, 1f)]
        [SerializeField] private float lateralGrip = 0.96f;

        [Tooltip("Aerodynamic downforce pressing the car to the road at speed.")]
        [SerializeField] private float downforce = 45f;

        [Tooltip("Constant base road adhesion force pulling the car to the track surface.")]
        [SerializeField] private float baseDownforce = 15f;

        [Header("Nitro Boost (Left Shift)")]
        [Tooltip("Key used to engage temporary boost.")]
        [SerializeField] private KeyCode boostKey = KeyCode.LeftShift;

        [Tooltip("Acceleration multiplier while boosting.")]
        [SerializeField] private float boostMultiplier = 1.85f;

        [Tooltip("Top forward speed while boosting (65 m/s ~ 234 km/h).")]
        [SerializeField] private float boostMaxSpeed = 65f;

        [Tooltip("Maximum boost capacity.")]
        [SerializeField] private float maxBoostCapacity = 100f;

        [Tooltip("Boost consumed per second.")]
        [SerializeField] private float boostDrainRate = 25f;

        [Tooltip("Boost recharged per second.")]
        [SerializeField] private float boostRechargeRate = 18f;

        [Tooltip("Cooldown delay before boost begins recharging.")]
        [SerializeField] private float boostRechargeDelay = 0.6f;

        [Header("Stability & Ground Check")]
        [Tooltip("Center of mass offset relative to transform position. Keep slightly above floor for stability.")]
        [SerializeField] private Vector3 centerOfMassOffset = new Vector3(0f, 0.15f, 0f);

        [Tooltip("Torque applied to keep the vehicle upright when tilted sideways.")]
        [SerializeField] private float uprightStabilityForce = 30f;

        [Tooltip("Damping applied to roll and pitch angular velocities.")]
        [SerializeField] private float rollPitchDamping = 16f;

        [Tooltip("Downward distance from bottom of chassis to detect ground.")]
        [SerializeField] private float groundCheckDistance = 1.2f;

        [Tooltip("Layer mask for track / ground surfaces.")]
        [SerializeField] private LayerMask groundLayers = ~0;

        [Header("Respawn & Recovery")]
        [Tooltip("If the car's Y position drops below this threshold, it automatically respawns on the track.")]
        [SerializeField] private float fallOutOfBoundsY = -3.0f;

        [Tooltip("Key to manually reset / respawn on the track.")]
        [SerializeField] private KeyCode resetKey = KeyCode.R;

        [Header("Visual Wheel Models (Optional)")]
        [SerializeField] private Transform frontLeftWheel;
        [SerializeField] private Transform frontRightWheel;
        [SerializeField] private Transform rearLeftWheel;
        [SerializeField] private Transform rearRightWheel;
        [SerializeField] private float maxVisualSteerAngle = 28f;
        [SerializeField] private float wheelRadius = 0.35f;

        // Internal State
        private Rigidbody rb;
        private float moveInput;
        private float steerInput;
        private bool isBoosting;
        private bool handbrakeInput;
        private float currentBoost;
        private float boostCooldownTimer;
        private bool isGrounded;
        private Vector3 groundNormal = Vector3.up;
        private float currentSteerAngle;
        private float wheelSpinAngle;
        private Quaternion baseFLRot = Quaternion.identity;
        private Quaternion baseFRRot = Quaternion.identity;
        private Quaternion baseRLRot = Quaternion.identity;
        private Quaternion baseRRRot = Quaternion.identity;

        // Recovery checkpoints
        private Vector3 lastSafePosition;
        private Quaternion lastSafeRotation;
        private float safePositionTimer;
        private float upsideDownTimer;

        // Public Properties for UI / HUD / Audio
        public float CurrentSpeedMps => Vector3.Dot(GetLinearVelocity(), transform.forward);
        public float CurrentSpeedKmh => Mathf.Abs(CurrentSpeedMps) * 3.6f;
        public float MaxSpeedKmh => (isBoosting ? boostMaxSpeed : maxSpeed) * 3.6f;
        public float SpeedRatio => Mathf.Clamp01(Mathf.Abs(CurrentSpeedMps) / (isBoosting ? boostMaxSpeed : maxSpeed));
        public float BoostNormalized => maxBoostCapacity > 0f ? Mathf.Clamp01(currentBoost / maxBoostCapacity) : 0f;
        public bool IsBoosting => isBoosting;
        public bool IsGrounded => isGrounded;
        public bool IsBraking { get; private set; }
        public bool IsReversing { get; private set; }
        public bool ControlsEnabled { get; set; } = true;

        private void Awake()
        {
            if (rb == null) rb = GetComponent<Rigidbody>();
            SetupRigidbody();
            currentBoost = maxBoostCapacity;

            SanitizeWheels();

            lastSafePosition = transform.position;
            lastSafeRotation = transform.rotation;
        }

        public void SanitizeWheels()
        {
            // Helper to check if a transform is actually a wheel or an invalid body part (doors, axles, etc.)
            bool IsInvalidWheel(Transform t)
            {
                if (t == null) return false;
                string n = t.name.ToLower();
                return n.Contains("door") || n.Contains("handle") || n.Contains("mirror") ||
                       n.Contains("axle") || n.Contains("panel") || n.Contains("hood") ||
                       n.Contains("interior") || n.Contains("windshield") || n.Contains("pillar");
            }

            if (IsInvalidWheel(frontLeftWheel)) frontLeftWheel = null;
            if (IsInvalidWheel(frontRightWheel)) frontRightWheel = null;
            if (IsInvalidWheel(rearLeftWheel)) rearLeftWheel = null;
            if (IsInvalidWheel(rearRightWheel)) rearRightWheel = null;

            // Auto-discover genuine wheel transforms if missing
            Transform[] children = GetComponentsInChildren<Transform>(true);
            foreach (var c in children)
            {
                string n = c.name.ToLower();
                if (n == "wheel_fl" || n == "wheelfrontl" || n == "proto_wheel_fl") frontLeftWheel = c;
                else if (n == "wheel_fr" || n == "wheelfrontr" || n == "proto_wheel_fr") frontRightWheel = c;
                else if (n == "wheel_rl" || n == "wheelrearl" || n == "proto_wheel_rl") rearLeftWheel = c;
                else if (n == "wheel_rr" || n == "wheelrearr" || n == "proto_wheel_rr") rearRightWheel = c;
            }

            if (frontLeftWheel != null) baseFLRot = frontLeftWheel.localRotation;
            if (frontRightWheel != null) baseFRRot = frontRightWheel.localRotation;
            if (rearLeftWheel != null) baseRLRot = rearLeftWheel.localRotation;
            if (rearRightWheel != null) baseRRRot = rearRightWheel.localRotation;
        }

        private void SetupRigidbody()
        {
            if (rb == null) rb = GetComponent<Rigidbody>();
            if (rb == null) return;

            rb.centerOfMass = centerOfMassOffset;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

#if UNITY_6000_0_OR_NEWER
            rb.linearDamping = 0.05f;
            rb.angularDamping = 0.5f;
#else
            rb.drag = 0.05f;
            rb.angularDrag = 0.5f;
#endif
        }

        private void Update()
        {
            ReadInput();
            UpdateBoostState();
            UpdateVisualWheels();

            // Manual respawn
            if (Input.GetKeyDown(resetKey))
            {
                RespawnAtSafePosition();
            }
        }

        private void FixedUpdate()
        {
            PerformGroundCheck();

            Vector3 velocity = GetLinearVelocity();
            float forwardSpeed = Vector3.Dot(velocity, transform.forward);
            float lateralSpeed = Vector3.Dot(velocity, transform.right);

            if (isGrounded)
            {
                ApplyDriveForces(forwardSpeed);
                ApplyLateralGrip(lateralSpeed);
                ApplySteering(forwardSpeed);
            }

            ApplyDownforce(forwardSpeed);
            ApplyAntiFlipStability();
            UpdateRecoveryTracking(forwardSpeed);
        }

        #region Input & Boost

        private void ReadInput()
        {
            if (!ControlsEnabled)
            {
                moveInput = 0f;
                steerInput = 0f;
                handbrakeInput = true;
                return;
            }

            float v = 0f;
            float h = 0f;

            // Direct keyboard polling ensures 100% responsiveness on any setup
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) v += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) v -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h += 1f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h -= 1f;

            // Fallback to InputManager axes if no direct keys pressed (e.g. gamepads)
            if (Mathf.Abs(v) < 0.01f)
            {
                try { v = Input.GetAxisRaw("Vertical"); } catch { }
            }
            if (Mathf.Abs(h) < 0.01f)
            {
                try { h = Input.GetAxisRaw("Horizontal"); } catch { }
            }

            moveInput = v;
            steerInput = h;
            handbrakeInput = Input.GetKey(KeyCode.Space);
        }

        private void UpdateBoostState()
        {
            bool boostRequested = Input.GetKey(boostKey);
            bool wantsToDrive = moveInput > 0.05f;

            if (boostRequested && wantsToDrive && currentBoost > 0.5f)
            {
                isBoosting = true;
                currentBoost = Mathf.Max(0f, currentBoost - boostDrainRate * Time.deltaTime);
                boostCooldownTimer = boostRechargeDelay;
            }
            else
            {
                isBoosting = false;

                if (boostCooldownTimer > 0f)
                {
                    boostCooldownTimer -= Time.deltaTime;
                }
                else if (currentBoost < maxBoostCapacity)
                {
                    currentBoost = Mathf.Min(maxBoostCapacity, currentBoost + boostRechargeRate * Time.deltaTime);
                }
            }
        }

        #endregion

        #region Physics

        private void PerformGroundCheck()
        {
            Vector3 rayOrigin = transform.position + transform.up * 0.5f;
            float checkDist = groundCheckDistance + 0.5f;

            RaycastHit[] hits = Physics.RaycastAll(rayOrigin, -transform.up, checkDist, groundLayers, QueryTriggerInteraction.Ignore);
            isGrounded = false;
            groundNormal = Vector3.up;

            float closestDist = float.MaxValue;
            foreach (var h in hits)
            {
                if (h.collider != null && !h.collider.transform.IsChildOf(transform))
                {
                    if (h.distance < closestDist)
                    {
                        closestDist = h.distance;
                        isGrounded = true;
                        groundNormal = h.normal;
                    }
                }
            }
        }

        private void ApplyDriveForces(float forwardSpeed)
        {
            float targetMaxSpeed = isBoosting ? boostMaxSpeed : maxSpeed;
            float currentAccel = isBoosting ? (acceleration * boostMultiplier) : acceleration;

            IsBraking = false;
            IsReversing = false;

            // Forward Drive
            if (moveInput > 0.05f)
            {
                if (forwardSpeed < -0.5f)
                {
                    // Moving backward but pressing forward: BRAKE
                    IsBraking = true;
                    rb.AddForce(transform.forward * (brakingForce * Mathf.Abs(moveInput)), ForceMode.Acceleration);
                }
                else if (forwardSpeed < targetMaxSpeed)
                {
                    float headRoom = 1f - Mathf.Clamp01(forwardSpeed / targetMaxSpeed);
                    Vector3 force = transform.forward * (moveInput * currentAccel * (0.35f + 0.65f * headRoom));
                    rb.AddForce(force, ForceMode.Acceleration);
                }
            }
            // Reverse or Braking
            else if (moveInput < -0.05f)
            {
                if (forwardSpeed > 0.5f)
                {
                    // Moving forward but pressing backward: BRAKE
                    IsBraking = true;
                    rb.AddForce(-transform.forward * (brakingForce * Mathf.Abs(moveInput)), ForceMode.Acceleration);
                }
                else
                {
                    // Reverse gear
                    IsReversing = true;
                    if (forwardSpeed > -maxReverseSpeed)
                    {
                        float headRoom = 1f - Mathf.Clamp01(Mathf.Abs(forwardSpeed) / maxReverseSpeed);
                        Vector3 force = transform.forward * (moveInput * reverseAcceleration * (0.35f + 0.65f * headRoom));
                        rb.AddForce(force, ForceMode.Acceleration);
                    }
                }
            }
            // Coasting
            else
            {
                if (Mathf.Abs(forwardSpeed) > 0.1f)
                {
                    rb.AddForce(-transform.forward * (Mathf.Sign(forwardSpeed) * coastDamping), ForceMode.Acceleration);
                }
            }

            // Handbrake
            if (handbrakeInput && Mathf.Abs(forwardSpeed) > 0.2f)
            {
                IsBraking = true;
                rb.AddForce(-transform.forward * (Mathf.Sign(forwardSpeed) * brakingForce), ForceMode.Acceleration);
            }
        }

        private void ApplyLateralGrip(float lateralSpeed)
        {
            // Cancels sideways sliding to give crisp arcade response and prevent wall smash
            float effectiveGrip = handbrakeInput ? (lateralGrip * 0.35f) : lateralGrip;
            Vector3 counterForce = -transform.right * (lateralSpeed * effectiveGrip * 40f);
            rb.AddForce(counterForce, ForceMode.Acceleration);
        }

        private void ApplySteering(float forwardSpeed)
        {
            if (rb == null) return;

            Vector3 localAng = transform.InverseTransformDirection(rb.angularVelocity);

            if (Mathf.Abs(steerInput) < 0.01f)
            {
                // Auto-center yaw angular velocity when steering is released
                localAng.y = Mathf.MoveTowards(localAng.y, 0f, 25f * Time.fixedDeltaTime);
                SetAngularVelocity(transform.TransformDirection(localAng));
                return;
            }

            // Invert steering when reversing
            float steerDirection = forwardSpeed < -0.3f ? -steerInput : steerInput;

            // Speed scaling: allow stationary turning (stationarySteerFactor) so player never gets stuck
            float speedFactor = Mathf.Lerp(stationarySteerFactor, 1.0f, Mathf.Clamp01(Mathf.Abs(forwardSpeed) / 4f));
            float highSpeedDamp = Mathf.Lerp(1f, 1f - highSpeedSteerReduction, Mathf.Clamp01(Mathf.Abs(forwardSpeed) / maxSpeed));

            float turnRateDegrees = steerDirection * steerSensitivity * speedFactor * highSpeedDamp;
            float targetYawRad = turnRateDegrees * Mathf.Deg2Rad;

            // Snappy arcade yaw angular velocity
            localAng.y = Mathf.MoveTowards(localAng.y, targetYawRad, 35f * Time.fixedDeltaTime);
            SetAngularVelocity(transform.TransformDirection(localAng));
        }

        private void ApplyDownforce(float forwardSpeed)
        {
            if (rb == null) return;

            // Ground adhesion downforce: keeps car glued to track over crests and steep drops
            float speedRatio = Mathf.Clamp01(Mathf.Abs(forwardSpeed) / maxSpeed);
            float totalDownforce = baseDownforce + (downforce * speedRatio * speedRatio);
            rb.AddForce(-transform.up * (totalDownforce * 12f), ForceMode.Acceleration);

            // Extra downward gravity if airborne so car doesn't launch into space
            if (!isGrounded)
            {
                rb.AddForce(Vector3.down * 30f, ForceMode.Acceleration);
            }
        }

        private void ApplyAntiFlipStability()
        {
            if (rb == null) return;

            // 1. Upright torque
            Vector3 targetUp = isGrounded ? groundNormal : Vector3.up;
            Vector3 tiltAxis = Vector3.Cross(transform.up, targetUp);
            float tiltAngle = Vector3.Angle(transform.up, targetUp);

            if (tiltAngle > 2f)
            {
                Vector3 correctiveTorque = tiltAxis.normalized * (tiltAngle * uprightStabilityForce);
                rb.AddTorque(correctiveTorque, ForceMode.Acceleration);
            }

            // 2. Dampen roll and pitch tumble
            Vector3 localAng = transform.InverseTransformDirection(rb.angularVelocity);
            localAng.x = Mathf.MoveTowards(localAng.x, 0f, rollPitchDamping * Time.fixedDeltaTime);
            localAng.z = Mathf.MoveTowards(localAng.z, 0f, rollPitchDamping * Time.fixedDeltaTime);
            SetAngularVelocity(transform.TransformDirection(localAng));
        }

        #endregion

        #region Recovery & Respawn

        private void UpdateRecoveryTracking(float forwardSpeed)
        {
            // Record safe checkpoint position every 0.5s while grounded and upright
            if (isGrounded && forwardSpeed > 2.0f && transform.up.y > 0.75f)
            {
                safePositionTimer += Time.fixedDeltaTime;
                if (safePositionTimer >= 0.5f)
                {
                    safePositionTimer = 0f;
                    lastSafePosition = transform.position;
                    lastSafeRotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
                }
            }

            // Auto-respawn if car falls below out-of-bounds Y threshold
            if (transform.position.y < fallOutOfBoundsY)
            {
                RespawnAtSafePosition();
            }

            // Auto-unflip if upside down for > 1.5 seconds
            if (transform.up.y < 0.25f)
            {
                upsideDownTimer += Time.fixedDeltaTime;
                if (upsideDownTimer > 1.5f)
                {
                    RespawnAtSafePosition();
                }
            }
            else
            {
                upsideDownTimer = 0f;
            }
        }

        public void RespawnAtSafePosition()
        {
            if (lastSafePosition == Vector3.zero || lastSafePosition.y < -1f)
            {
                lastSafePosition = new Vector3(-75f, 0.35f, -90f);
                lastSafeRotation = Quaternion.Euler(0f, 90f, 0f);
            }

            transform.position = lastSafePosition + Vector3.up * 0.8f;
            transform.rotation = lastSafeRotation;

            if (rb != null)
            {
#if UNITY_6000_0_OR_NEWER
                rb.linearVelocity = Vector3.zero;
#else
                rb.velocity = Vector3.zero;
#endif
                rb.angularVelocity = Vector3.zero;
            }

            upsideDownTimer = 0f;
        }

        #endregion

        #region Visual Wheels

        private void UpdateVisualWheels()
        {
            float speed = CurrentSpeedMps;
            float spinDelta = (speed * Time.deltaTime / Mathf.Max(0.1f, wheelRadius)) * Mathf.Rad2Deg;
            wheelSpinAngle = (wheelSpinAngle + spinDelta) % 360f;

            float targetSteer = steerInput * maxVisualSteerAngle;
            currentSteerAngle = Mathf.MoveTowards(currentSteerAngle, targetSteer, Time.deltaTime * 180f);

            if (frontLeftWheel != null)
                frontLeftWheel.localRotation = baseFLRot * Quaternion.Euler(0f, currentSteerAngle, 0f) * Quaternion.Euler(wheelSpinAngle, 0f, 0f);

            if (frontRightWheel != null)
                frontRightWheel.localRotation = baseFRRot * Quaternion.Euler(0f, currentSteerAngle, 0f) * Quaternion.Euler(wheelSpinAngle, 0f, 0f);

            if (rearLeftWheel != null)
                rearLeftWheel.localRotation = baseRLRot * Quaternion.Euler(wheelSpinAngle, 0f, 0f);

            if (rearRightWheel != null)
                rearRightWheel.localRotation = baseRRRot * Quaternion.Euler(wheelSpinAngle, 0f, 0f);
        }

        #endregion

        #region Helpers & Gizmos

        private Vector3 GetLinearVelocity()
        {
            if (rb == null) rb = GetComponent<Rigidbody>();
            if (rb == null) return Vector3.zero;

#if UNITY_6000_0_OR_NEWER
            return rb.linearVelocity;
#else
            return rb.velocity;
#endif
        }

        private void SetAngularVelocity(Vector3 value)
        {
            if (rb == null) rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.angularVelocity = value;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Vector3 rayStart = transform.position + transform.up * 0.5f;
            Gizmos.DrawLine(rayStart, rayStart - transform.up * (groundCheckDistance + 0.5f));

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.TransformPoint(centerOfMassOffset), 0.15f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position + Vector3.up * 0.3f, transform.forward * 2f);
        }

        #endregion
    }
}
