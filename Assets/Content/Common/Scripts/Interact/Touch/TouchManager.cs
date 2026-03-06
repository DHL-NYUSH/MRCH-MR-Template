// Copyright (c) 2026 Digital Heritage Lab / Shengyang "Billiton" Peng
// Licensed under the MIT License. See LICENSE for details.

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MRCH.Interact.Touch
{
    public abstract class TouchManager : MonoBehaviour
    {
        [InfoBox("Add Collider and TouchableObject.cs to the object you want to be touchable")]
        [Required,
         InfoBox("Assign this and all touchable Objects to a (special) layer", InfoMessageType.Error,
             "TouchableLayerAssigned")]
        public LayerMask touchableLayer = 6; // Assign this in the Inspector to include only the touchable objects

        private bool TouchableLayerAssigned => touchableLayer == 0;

        [Space(10), Header("Universal Touch Event"), SerializeField]
        protected UnityEvent universalTouchEvent;

        [Title("Setting"), PropertyRange(1f, 60f), SerializeField]
        private float touchRange = 10f;

        private Camera _mainCam;

        [Space, SerializeField,
         InfoBox("Enable this if you want other objects to be unable to interact after one is touched.\n" +
                 "Call Unlock() from any UnityEvent to re-enable interaction."),
         Tooltip("Enable this if you want other objects to be unable to interact after one is touched")]
        private bool disableTouchOfOtherObjects;

        private bool _isLocked;

        [Space] public float clickInterval = 0.5f;
        private float _timeCnt = float.MaxValue;
        [InfoBox("If your click interval is too long, try playing fail audio fx or some vfx here.")]
        public UnityEvent failedToClickEvent;

        // Input System actions
        protected static InputAction SharedTouchAction;
        private static int _refCount = 0;
        
        [Obsolete]
        protected InputAction touchAction => SharedTouchAction;

        [Space, SerializeField] protected bool showGizmos = true;

        private static readonly HashSet<uint> LayerUsed = new();
        
        protected virtual void Start()
        {
            for (var i = 0; i < 32; i++)
            {
                if ((touchableLayer.value & (1 << i)) != 0)
                {
                    if (!LayerUsed.Add((uint)i))
                        Debug.LogWarning($"Layer {LayerMask.LayerToName(i)} is already used by another TouchManager! " +
                                         $"This will cause duplicate touch events. ({gameObject.name})");
                }
            }
            if (Camera.main == null)
            {
                Debug.LogError("Main Camera not found!!!");
            }

            _mainCam = Camera.main;

            if (touchableLayer == 0)
                Debug.LogWarning("Please check if you forgot to assign the touchable layer on " + gameObject.name);
        }

        protected virtual void OnEnable()
        {
            if (SharedTouchAction == null)
            {
                SharedTouchAction = new InputAction(binding: "<Touchscreen>/press");
                SharedTouchAction.AddBinding("<Mouse>/leftButton");
            }
            _refCount++;
            SharedTouchAction.Enable();
        }

        protected virtual void OnDisable()
        {
            _refCount--;
            if (_refCount <= 0)
            {
                SharedTouchAction?.Disable();
                _refCount = 0;
            }
        }
        
        protected virtual void OnDestroy()
        {
            for (var i = 0; i < 32; i++)
            {
                if ((touchableLayer.value & (1 << i)) != 0)
                    LayerUsed.Remove((uint)i);
            }
        }

        protected virtual void Update()
        {
            _timeCnt += Time.deltaTime;

            if (!SharedTouchAction.WasPressedThisFrame()) return;
            
            Vector3 inputPosition;

            // Check if the input is from touchscreen or mouse
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                inputPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            }
            else if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                inputPosition = Mouse.current.position.ReadValue();
            }
            else
            {
                return;
            }

            var ray = _mainCam.ScreenPointToRay(inputPosition);
            
            if (!Physics.Raycast(ray, out var hit, touchRange, touchableLayer)) return;
            
            var touchable = hit.transform.GetComponent<TouchableObject>();
            if (touchable)
            {
                if(_isLocked) return;
                
                if (_timeCnt <= clickInterval)
                {
                    failedToClickEvent?.Invoke();
                    return;
                }

                _timeCnt = 0f;

                if (disableTouchOfOtherObjects)
                    _isLocked = true;

                Debug.Log("Universal Touch/Click Event triggered");
                universalTouchEvent?.Invoke();
                touchable.OnTouch();
            }
            else
            {
                Debug.LogWarning(hit.transform.name + " has no TouchableObject component");
            }
        }

        public void Lock()
        {
            _isLocked = true;
            Debug.Log("Locked Touch Object");
        }
        /// <summary>
        /// Re-enables interaction after it was locked by disableTouchOfOtherObjects.
        /// Wire this to any TouchableObject's onTouchEvent via UnityEvent in the Inspector.
        /// </summary>
        public void Unlock()
        {
            _isLocked = false;
            Debug.Log("Touch interaction unlocked");
        }

        protected void OnDrawGizmosSelected()
        {
            if (!enabled || !showGizmos) return;

            Gizmos.color = new Color(1, 0.5f, 0.5f, 0.75f);
            Gizmos.DrawWireSphere(transform.position, touchRange);
#if UNITY_EDITOR
            var labelPosition = transform.position + Vector3.forward * touchRange;
            Handles.Label(labelPosition, "Touch Range");
#endif
        }
    }
}