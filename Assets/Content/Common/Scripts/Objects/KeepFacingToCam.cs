using Sirenix.OdinInspector;
using UnityEngine;

namespace MRCH.Common.Interact
{
    public abstract class KeepFacingToCam : MonoBehaviour
    {
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