// Copyright (c) 2026 Digital Heritage Lab / Shengyang "Billiton" Peng
// Licensed under the MIT License. See LICENSE for details.

using Sirenix.OdinInspector;
using UnityEngine;

namespace MRCH.Tools.Edit
{
    /// <summary>
    /// Marker component that flags this GameObject as an editor-only map reference model.
    /// Automatically assigns the "EditorOnly" tag, which tells Unity to strip this GameObject from builds.
    /// The object remains in the scene during editing and is excluded from built packages without any build callbacks.
    /// </summary>
    [AddComponentMenu("MRCH/Edit/Map Model"), HideMonoScript]
    public class MapModel : MonoBehaviour
    {
        private void OnValidate()
        {
            if (!gameObject.CompareTag("EditorOnly"))
                gameObject.tag = "EditorOnly";
        }
    }
}