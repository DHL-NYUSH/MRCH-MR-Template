// Copyright (c) 2026 Digital Heritage Lab / Shengyang "Billiton" Peng
// Licensed under the MIT License. See LICENSE for details.

using Sirenix.OdinInspector;
using UnityEngine;

namespace MRCH.Tools.Edit
{
    /// <summary>
    /// Marker component that flags this GameObject as an editor-only map reference model.
    /// The GameObject will be automatically stripped before build by <see cref="MRCH.Editor.MapModelBuildPreprocessor"/>.
    /// </summary>
    [AddComponentMenu("MRCH/Edit/Map Model"), HideMonoScript]
    public class MapModel : MonoBehaviour
    {
    }
}