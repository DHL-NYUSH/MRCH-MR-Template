// Copyright (c) 2026 Digital Heritage Lab / Shengyang "Billiton" Peng
// Licensed under the MIT License. See LICENSE for details.

using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

namespace MRCH.Tools.Objects
{
    /// <summary>
    /// Makes the GameObject this component is attached to follow a target with a delay and some other layout options.
    /// Drop-in replacement for XRI's LazyFollow, with no XR Interaction Toolkit dependency.
    /// </summary>
    [AddComponentMenu("MRCH/Objects/Lazy Follow")]
    [DisallowMultipleComponent]
    public class LazyFollow : MonoBehaviour
    {
        #region Enums

        public enum PositionFollowMode
        {
            None,
            Follow,
        }

        public enum RotationFollowMode
        {
            None,

            /// <summary> Rotate to face the target, free up axis. Best used with camera as target. </summary>
            LookAt,

            /// <summary> Rotate to face the target, world-up locked. Best used with camera as target. </summary>
            LookAtWithWorldUp,

            /// <summary> Match the target's rotation. </summary>
            Follow,
        }

        #endregion

        #region Target Config

        [TitleGroup("Target Config")]
        [SerializeField, Tooltip("(Optional) The object being followed. Defaults to main camera if not set.")]
        private Transform target;

        [TitleGroup("Target Config")] [SerializeField, Tooltip("Offset relative/local to the target object.")]
        private Vector3 targetOffset = new Vector3(0f, 0f, 0.5f);

        [TitleGroup("Target Config")]
        [SerializeField,
         Tooltip(
             "If true, read/write the local transform instead of world transform. Not supported with LookAt rotation modes.")]
        private bool followInLocalSpace;

        [TitleGroup("Target Config")]
        [SerializeField, Tooltip("If true, apply the final position/rotation in local space.")]
        private bool applyTargetInLocalSpace;

        #endregion

        #region General Follow Params

        [TitleGroup("General Follow Params")]
        [SerializeField, Tooltip("Movement speed for smoothing. Lower = more lag behind target.")]
        private float movementSpeed = 6f;

        [TitleGroup("General Follow Params")]
        [SerializeField, Range(0f, 0.999f)]
        [Tooltip("Speed variance based on distance. 0 = constant speed. e.g. 0.25 with speed 6 → range [4.5, 7.5].")]
        private float movementSpeedVariancePercentage = 0.25f;

        [TitleGroup("General Follow Params")]
        [SerializeField, Tooltip("Snap to target position/rotation immediately when enabled.")]
        private bool snapOnEnable = true;

        #endregion

        #region Position Follow Params

        [TitleGroup("Position Follow Params")] [SerializeField]
        private PositionFollowMode positionFollowMode = PositionFollowMode.Follow;

        [TitleGroup("Position Follow Params")]
        [SerializeField, Tooltip("Minimum distance from target before lazy follow starts.")]
        private float minDistanceAllowed = 0.01f;

        [TitleGroup("Position Follow Params")]
        [SerializeField, Tooltip("Maximum distance threshold (reached after time delay).")]
        private float maxDistanceAllowed = 0.3f;

        [TitleGroup("Position Follow Params")]
        [SerializeField, Tooltip("Seconds before threshold ramps from min to max distance.")]
        private float timeUntilThresholdReachesMaxDistance = 3f;

        #endregion

        #region Rotation Follow Params

        [TitleGroup("Rotation Follow Params")]
        [SerializeField, Tooltip("LookAt modes are best used with main camera as target.")]
        private RotationFollowMode rotationFollowMode = RotationFollowMode.LookAt;

        [TitleGroup("Rotation Follow Params")]
        [SerializeField, Tooltip("Minimum angle (degrees) before lazy follow starts.")]
        private float minAngleAllowed = 0.1f;

        [TitleGroup("Rotation Follow Params")]
        [SerializeField, Tooltip("Maximum angle threshold (reached after time delay).")]
        private float maxAngleAllowed = 5f;

        [TitleGroup("Rotation Follow Params")]
        [SerializeField, Tooltip("Seconds before threshold ramps from min to max angle.")]
        private float timeUntilThresholdReachesMaxAngle = 3f;

        #endregion

        #region Public Properties

        public Transform Target
        {
            get => target;
            set => target = value;
        }

        public Vector3 TargetOffset
        {
            get => targetOffset;
            set => targetOffset = value;
        }

        public bool FollowInLocalSpace
        {
            get => followInLocalSpace;
            set
            {
                followInLocalSpace = value;
                ValidateFollowMode();
            }
        }

        public bool ApplyTargetInLocalSpace
        {
            get => applyTargetInLocalSpace;
            set => applyTargetInLocalSpace = value;
        }

        public float MovementSpeed
        {
            get => movementSpeed;
            set
            {
                movementSpeed = value;
                UpdateSpeedBounds();
            }
        }

        public float MovementSpeedVariancePercentage
        {
            get => movementSpeedVariancePercentage;
            set
            {
                movementSpeedVariancePercentage = Mathf.Clamp(value, 0f, 0.999f);
                UpdateSpeedBounds();
            }
        }

        public bool SnapOnEnable
        {
            get => snapOnEnable;
            set => snapOnEnable = value;
        }

        public PositionFollowMode PositionMode
        {
            get => positionFollowMode;
            set => positionFollowMode = value;
        }

        public float MinDistanceAllowed
        {
            get => minDistanceAllowed;
            set => minDistanceAllowed = value;
        }

        public float MaxDistanceAllowed
        {
            get => maxDistanceAllowed;
            set => maxDistanceAllowed = value;
        }

        public RotationFollowMode RotationMode
        {
            get => rotationFollowMode;
            set
            {
                rotationFollowMode = value;
                ValidateFollowMode();
            }
        }

        public float MinAngleAllowed
        {
            get => minAngleAllowed;
            set => minAngleAllowed = value;
        }

        public float MaxAngleAllowed
        {
            get => maxAngleAllowed;
            set => maxAngleAllowed = value;
        }

        #endregion

        #region Runtime State

        // Current interpolated values
        private Vector3 _currentPosition;
        private Quaternion _currentRotation;

        // Targets
        private Vector3 _positionTarget;
        private Quaternion _rotationTarget;

        // Dynamic threshold timers (time since last target change)
        private float _positionIdleTime;
        private float _rotationIdleTime;

        // Cached previous targets for idle detection
        private Vector3 _prevPositionTarget;
        private Quaternion _prevRotationTarget;

        // Speed bounds
        private float _lowerMovementSpeed;
        private float _upperMovementSpeed;

        #endregion

        #region Lifecycle

        private void OnValidate()
        {
            UpdateSpeedBounds();
            ValidateFollowMode();
        }

        private void Awake()
        {
            UpdateSpeedBounds();
            ValidateFollowMode();
        }

        private void OnEnable()
        {
            if (target == null)
            {
                var mainCamera = Camera.main;
                if (mainCamera != null)
                    target = mainCamera.transform;
            }

            // Initialize current values from transform
            if (followInLocalSpace)
            {
                _currentPosition = transform.localPosition;
                _currentRotation = transform.localRotation;
            }
            else
            {
                _currentPosition = transform.position;
                _currentRotation = transform.rotation;
            }

            _positionTarget = _currentPosition;
            _rotationTarget = _currentRotation;
            _prevPositionTarget = _positionTarget;
            _prevRotationTarget = _rotationTarget;

            _positionIdleTime = 0f;
            _rotationIdleTime = 0f;

            if (snapOnEnable && target != null)
            {
                if (positionFollowMode != PositionFollowMode.None)
                {
                    _positionTarget = ComputeTargetPosition();
                    _currentPosition = _positionTarget;
                    ApplyPosition(_currentPosition);
                }

                if (rotationFollowMode != RotationFollowMode.None)
                {
                    _rotationTarget = ComputeTargetRotation();
                    _currentRotation = _rotationTarget;
                    ApplyRotation(_currentRotation);
                }
            }
        }

        private void LateUpdate()
        {
            if (!target)
                return;

            var dt = Time.unscaledDeltaTime;

            // --- Position ---
            if (positionFollowMode != PositionFollowMode.None)
            {
                var desiredPos = ComputeTargetPosition();

                if (IsPositionWithinThreshold(desiredPos))
                {
                    _positionTarget = desiredPos;
                }

                // Track idle time for dynamic threshold
                UpdatePositionIdleTime(dt);

                // Interpolate
                var speed = GetEffectiveSpeed(
                    Vector3.Distance(_currentPosition, _positionTarget));
                var t = Mathf.Clamp01(dt * speed);
                _currentPosition = Vector3.Lerp(_currentPosition, _positionTarget, t);
                ApplyPosition(_currentPosition);
            }

            // --- Rotation ---
            if (rotationFollowMode != RotationFollowMode.None)
            {
                var desiredRot = ComputeTargetRotation();

                if (IsRotationWithinThreshold(desiredRot))
                {
                    _rotationTarget = desiredRot;
                }

                UpdateRotationIdleTime(dt);

                var speed = GetEffectiveSpeed(
                    Quaternion.Angle(_currentRotation, _rotationTarget) / 180f);
                var t = Mathf.Clamp01(dt * speed);
                _currentRotation = Quaternion.Slerp(_currentRotation, _rotationTarget, t);
                ApplyRotation(_currentRotation);
            }
        }

        #endregion

        #region Target Computation

        private Vector3 ComputeTargetPosition()
        {
            if (followInLocalSpace)
                return target.localPosition + targetOffset;

            return target.position + target.TransformVector(targetOffset);
        }

        private Quaternion ComputeTargetRotation()
        {
            switch (rotationFollowMode)
            {
                case RotationFollowMode.LookAt:
                {
                    var forward = (transform.position - target.position).normalized;
                    if (forward.sqrMagnitude < 0.001f)
                        return _currentRotation;
                    return Quaternion.LookRotation(forward, Vector3.up);
                }

                case RotationFollowMode.LookAtWithWorldUp:
                {
                    var forward = (transform.position - target.position);
                    // Project onto horizontal plane (lock world up)
                    forward.y = 0f;
                    forward = forward.normalized;
                    if (forward.sqrMagnitude < 0.001f)
                        return _currentRotation;
                    return Quaternion.LookRotation(forward, Vector3.up);
                }

                case RotationFollowMode.Follow:
                    return followInLocalSpace ? target.localRotation : target.rotation;

                default:
                    return _currentRotation;
            }
        }

        #endregion

        #region Dynamic Threshold

        /// <summary>
        /// The allowed distance threshold ramps from min to max over time when the target is idle.
        /// This creates the "lazy" behavior: small movements are ignored until enough time has passed.
        /// </summary>
        private float GetCurrentDistanceThreshold()
        {
            if (timeUntilThresholdReachesMaxDistance <= 0f)
                return maxDistanceAllowed;

            var ratio = Mathf.Clamp01(_positionIdleTime / timeUntilThresholdReachesMaxDistance);
            return Mathf.Lerp(minDistanceAllowed, maxDistanceAllowed, ratio);
        }

        private float GetCurrentAngleThreshold()
        {
            if (timeUntilThresholdReachesMaxAngle <= 0f)
                return maxAngleAllowed;

            var ratio = Mathf.Clamp01(_rotationIdleTime / timeUntilThresholdReachesMaxAngle);
            return Mathf.Lerp(minAngleAllowed, maxAngleAllowed, ratio);
        }

        private bool IsPositionWithinThreshold(Vector3 newTarget)
        {
            var sqrDist = (newTarget - _positionTarget).sqrMagnitude;
            var threshold = GetCurrentDistanceThreshold();
            return sqrDist >= threshold * threshold;
        }

        private bool IsRotationWithinThreshold(Quaternion newTarget)
        {
            var angle = Quaternion.Angle(newTarget, _rotationTarget);
            return angle >= GetCurrentAngleThreshold();
        }

        private void UpdatePositionIdleTime(float dt)
        {
            // If the target hasn't moved, accumulate idle time; otherwise reset
            if ((_positionTarget - _prevPositionTarget).sqrMagnitude > 0.0001f)
                _positionIdleTime = 0f;
            else
                _positionIdleTime += dt;

            _prevPositionTarget = _positionTarget;
        }

        private void UpdateRotationIdleTime(float dt)
        {
            if (Quaternion.Angle(_rotationTarget, _prevRotationTarget) > 0.01f)
                _rotationIdleTime = 0f;
            else
                _rotationIdleTime += dt;

            _prevRotationTarget = _rotationTarget;
        }

        #endregion

        #region Speed

        /// <summary>
        /// Returns effective speed, optionally varied by normalized distance to target.
        /// When close to target → upper speed (converge faster).
        /// When far from target → lower speed (more lag).
        /// </summary>
        private float GetEffectiveSpeed(float normalizedDistance)
        {
            if (movementSpeedVariancePercentage <= 0f)
                return movementSpeed;

            // Invert: close = high speed, far = low speed
            var t = 1f - Mathf.Clamp01(normalizedDistance);
            return Mathf.Lerp(_lowerMovementSpeed, _upperMovementSpeed, t);
        }

        private void UpdateSpeedBounds()
        {
            if (movementSpeedVariancePercentage > 0f)
            {
                _lowerMovementSpeed = movementSpeed * (1f - movementSpeedVariancePercentage);
                _upperMovementSpeed = movementSpeed * (1f + movementSpeedVariancePercentage);
            }
            else
            {
                _lowerMovementSpeed = movementSpeed;
                _upperMovementSpeed = movementSpeed;
            }
        }

        #endregion

        #region Apply

        private void ApplyPosition(Vector3 position)
        {
            if (applyTargetInLocalSpace)
                transform.localPosition = position;
            else
                transform.position = position;
        }

        private void ApplyRotation(Quaternion rotation)
        {
            if (applyTargetInLocalSpace)
                transform.localRotation = rotation;
            else
                transform.rotation = rotation;
        }

        #endregion

        #region Validation

        private void ValidateFollowMode()
        {
            if (!followInLocalSpace)
                return;

            if (rotationFollowMode == RotationFollowMode.LookAt ||
                rotationFollowMode == RotationFollowMode.LookAtWithWorldUp)
            {
                followInLocalSpace = false;
                Debug.LogWarning(
                    $"[LazyFollow] Cannot follow in local space with {rotationFollowMode}. Disabling local space follow.",
                    this);
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Force snap to the current target immediately (no interpolation).
        /// </summary>
        [Button("Snap To Target Now"), HideInEditorMode]
        public void SnapToTarget()
        {
            if (!target) return;

            if (positionFollowMode != PositionFollowMode.None)
            {
                _positionTarget = ComputeTargetPosition();
                _currentPosition = _positionTarget;
                ApplyPosition(_currentPosition);
            }

            if (rotationFollowMode != RotationFollowMode.None)
            {
                _rotationTarget = ComputeTargetRotation();
                _currentRotation = _rotationTarget;
                ApplyRotation(_currentRotation);
            }
        }

        #endregion
    }
}