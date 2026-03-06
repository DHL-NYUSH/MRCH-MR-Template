// Copyright (c) 2026 Digital Heritage Lab / Shengyang "Billiton" Peng
// Licensed under the MIT License. See LICENSE for details.

using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace MRCH.Interact.Touch
{
    public abstract class TouchableObject : MonoBehaviour
    {
        [Title("Touch Event")]
        [SerializeField]
        [InfoBox("<color=yellow>This touchable object is on the Default Layer, do you need to assign it to a specific touchable layer?</color>", VisibleIf = "OnDefaultLayer", Icon=SdfIconType.Alarm, IconColor ="red")]
        private UnityEvent onTouchEvent;
        
        private bool OnDefaultLayer => gameObject.layer == LayerMask.NameToLayer("Default");

        protected virtual void Start()
        {
            if (gameObject.layer == 0)
                Debug.LogError(
                    $"{gameObject.name} is on the Default Layer, do you need to assign it to a specific touchable layer?");
        }

        public virtual void OnTouch()
        {
            Debug.Log($"Touch event on {gameObject.name} triggered");
            onTouchEvent?.Invoke();
        }
    }
}