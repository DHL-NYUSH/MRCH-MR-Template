// Copyright (c) 2026 Digital Heritage Lab / Shengyang "Billiton" Peng
// Licensed under the MIT License. See LICENSE for details.

using Sirenix.OdinInspector;
using UnityEngine;

namespace MRCH.Tools.Objects
{
    [DisallowMultipleComponent]
    public abstract class KeepFacingToCam : MonoBehaviour
    {
        [DetailedInfoBox("This script is not obsolete but legacy", "You are recommended to use LazyFollow with: Follow => None, Rotate => Look At (w/ or w/o World Up) to reach the same function as this scripts with extra settings.")]
        protected Camera MainCam;

        protected bool FaceToCam;

        [Title("Setting")] [SerializeField] protected bool lockYAxis = false;
        [SerializeField] protected bool faceToCamOnEnable = true;

        protected virtual void Start()
        {
            MainCam = Camera.main;

            if (GetComponent(typeof(MoveAndRotate)) != null)
            {
                Debug.LogWarning($"{gameObject.name} has both 'TextFaceToCam' and 'Move and Rotate' component!");
            }

            FaceToCam = faceToCamOnEnable;
        }

        protected virtual void Update()
        {
            if (!MainCam || !FaceToCam) return;

            var directionToCamera = MainCam.transform.position - transform.position;
            if (lockYAxis)
                directionToCamera.y = 0;
            transform.rotation = Quaternion.LookRotation(-directionToCamera);
        }

        public virtual void SetFaceToCam(bool target)
        {
            FaceToCam = target;
        }
    }
}