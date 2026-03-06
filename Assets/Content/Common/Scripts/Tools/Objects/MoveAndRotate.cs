// Copyright (c) 2026 Digital Heritage Lab / Shengyang "Billiton" Peng
// Licensed under the MIT License. See LICENSE for details.

using DG.Tweening;
using MRCH.Bridge;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MRCH.Tools.Objects
{
    public enum MoveStartMode { None, ForOnceOnEnable, ForthAndBackOnEnable, ForOnceAfterLocalized, ForthAndBackAfterLocalized }
    public enum RotateStartMode { None, OnEnable, AfterLocalized }
    public enum StopBehavior { StopInPlace, ReturnToOrigin, SnapToOrigin }

    public abstract class MoveAndRotate : MonoBehaviour
    {
        [Title("Move Options")]
        [EnumToggleButtons]
        public MoveStartMode moveStartMode;

        [Required, ShowIf("@moveStartMode != MoveStartMode.None")]
        public Transform moveTarget;

        [Unit(Units.MetersPerSecond)]
        public float moveSpeed = 2f;

        [Space, SerializeField]
        protected Ease moveType = Ease.InOutSine;

        [EnumToggleButtons, LabelText("On Stop")]
        public StopBehavior moveStopBehavior = StopBehavior.StopInPlace;

        [Title("Rotate Options")]
        [EnumToggleButtons]
        public RotateStartMode rotateStartMode;

        public Vector3 rotationAxis = Vector3.up;

        [Unit(Units.Second)]
        public float rotateDuration = 10f;

        [Space, SerializeField]
        protected Ease rotateType = Ease.Linear;

        [EnumToggleButtons, LabelText("On Stop")]
        public StopBehavior rotateStopBehavior = StopBehavior.StopInPlace;

        protected Vector3 initialLocalPosition;
        protected Quaternion initialLocalRotation;

        protected Tween moveTween;
        protected Tween rotateTween;

        [Space, Title("Setting", bold: false), SerializeField]
        protected bool showGizmos = true;
        
        protected virtual void OnEnable()
        {
            initialLocalPosition = transform.localPosition;
            initialLocalRotation = transform.localRotation;

            if (EventBroadcaster.HasLocalized || Application.isEditor)
                Initialize();
            else
                EventBroadcaster.OnFirstLocalized += Initialize;

            if (moveStartMode == MoveStartMode.ForthAndBackOnEnable)
            {
                KillTween(ref moveTween);
                MoveForthAndBack();
            }
            else if (moveStartMode == MoveStartMode.ForOnceOnEnable)
            {
                KillTween(ref moveTween);
                MoveForOnce();
            }

            if (rotateStartMode == RotateStartMode.OnEnable)
            {
                KillTween(ref rotateTween);
                RotateObject();
            }
        }
        
        protected virtual void Initialize()
        {
            Debug.Log("Initialized in " + gameObject.name);

            if (moveStartMode == MoveStartMode.ForOnceAfterLocalized)
                MoveForOnce();
            else if (moveStartMode == MoveStartMode.ForthAndBackAfterLocalized)
                MoveForthAndBack();

            if (rotateStartMode == RotateStartMode.AfterLocalized)
                RotateObject();
        }

        public virtual void MoveForOnce()
        {
            if (!moveTarget)
            {
                Debug.LogError("Move target not set on MoveAndRotate " + gameObject.name);
                return;
            }

            KillTween(ref moveTween);
            var localTarget = GetMoveTargetLocalPosition();
            var duration = Vector3.Distance(transform.localPosition, localTarget) / moveSpeed;
            moveTween = transform.DOLocalMove(localTarget, duration)
                .SetEase(moveType);
        }

        public virtual void MoveBackForOnce()
        {
            KillTween(ref moveTween);
            var duration = Vector3.Distance(transform.localPosition, initialLocalPosition) / moveSpeed;
            moveTween = transform.DOLocalMove(initialLocalPosition, duration)
                .SetEase(moveType);
        }

        public virtual void JumpBackToInitialPosition()
        {
            KillTween(ref moveTween);
            transform.localPosition = initialLocalPosition;
        }

        public virtual void MoveForthAndBack()
        {
            if (!moveTarget)
            {
                Debug.LogError("Move target not set on MoveAndRotate " + gameObject.name);
                return;
            }

            KillTween(ref moveTween);
            var localTarget = GetMoveTargetLocalPosition();
            var duration = Vector3.Distance(transform.localPosition, localTarget) / moveSpeed;
            moveTween = transform.DOLocalMove(localTarget, duration)
                .SetEase(moveType)
                .SetLoops(-1, LoopType.Yoyo);
        }

        public virtual void RotateObject()
        {
            KillTween(ref rotateTween);
            var targetRotation = transform.localRotation * Quaternion.AngleAxis(360f, rotationAxis);
            rotateTween = transform.DOLocalRotateQuaternion(targetRotation, rotateDuration)
                .SetEase(rotateType)
                .SetLoops(-1, LoopType.Restart);
        }

        public virtual void RotateBackToOrigin()
        {
            KillTween(ref rotateTween);
            var angleDelta = Quaternion.Angle(transform.localRotation, initialLocalRotation);
            var duration = (angleDelta / 360f) * rotateDuration;
            rotateTween = transform.DOLocalRotateQuaternion(initialLocalRotation, duration)
                .SetEase(rotateType);
        }

        public virtual void JumpBackToInitialRotation()
        {
            KillTween(ref rotateTween);
            transform.localRotation = initialLocalRotation;
        }

        /// <summary>
        /// Stops movement using the configured <see cref="moveStopBehavior"/>.
        /// </summary>
        public virtual void StopMovement()
        {
            switch (moveStopBehavior)
            {
                case StopBehavior.StopInPlace:
                    KillTween(ref moveTween);
                    break;
                case StopBehavior.ReturnToOrigin:
                    MoveBackForOnce();
                    break;
                case StopBehavior.SnapToOrigin:
                    JumpBackToInitialPosition();
                    break;
            }
        }

        /// <summary>
        /// Stops rotation using the configured <see cref="rotateStopBehavior"/>.
        /// </summary>
        public virtual void StopRotation()
        {
            switch (rotateStopBehavior)
            {
                case StopBehavior.StopInPlace:
                    KillTween(ref rotateTween);
                    break;
                case StopBehavior.ReturnToOrigin:
                    RotateBackToOrigin();
                    break;
                case StopBehavior.SnapToOrigin:
                    JumpBackToInitialRotation();
                    break;
            }
        }

        protected virtual void KillTween(ref Tween tween)
        {
            if (tween != null && tween.IsActive())
                tween.Kill();
            tween = null;
        }

        protected virtual void OnDisable()
        {
            KillTween(ref moveTween);
            KillTween(ref rotateTween);
            EventBroadcaster.OnFirstLocalized -= Initialize;
        }

        protected void OnDrawGizmosSelected()
        {
            if (!enabled || !showGizmos) return;

            if (moveTarget)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, moveTarget.position);
            }
        }

        /// <summary>
        /// Converts moveTarget's world position into this object's local space,
        /// accounting for whether or not a parent exists.
        /// </summary>
        private Vector3 GetMoveTargetLocalPosition()
        {
            return transform.parent != null
                ? transform.parent.InverseTransformPoint(moveTarget.position)
                : moveTarget.position;
        }
    }
}