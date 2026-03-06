// Copyright (c) 2026 Digital Heritage Lab / Shengyang "Billiton" Peng
// Licensed under the MIT License. See LICENSE for details.

using Sirenix.OdinInspector;
using UnityEngine;

namespace MRCH.Tools.Objects
{
    public abstract class ObjectToolset : MonoBehaviour
    {
        [InfoBox("Includes ToggleComponentEnabled for components and ToggleObjectEnabled for objects")]
        public virtual void ToggleComponentEnabled(Component component)
        {
            if (component is Behaviour behaviourComponent)
            {
                behaviourComponent.enabled = !behaviourComponent.enabled;
            }
            else
            {
                Debug.LogWarning("The provided component doesn't have an 'enabled' property.");
            }
        }

        public virtual void ToggleObjectEnabled(GameObject go)
        {
            if (go != null)
            {
                go.SetActive(!go.activeSelf);
            }
        }
    }
}