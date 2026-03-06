// Copyright (c) 2026 Digital Heritage Lab / Shengyang "Billiton" Peng
// Licensed under the MIT License. See LICENSE for details.

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MRCH.Tools.Edit
{
    [HideMonoScript]
    [AddComponentMenu("MRCH/Edit/Editor Note")]
    public class EditorNote : MonoBehaviour
    {
#if UNITY_EDITOR
        [Title("Editor Note",titleAlignment:TitleAlignments.Centered)]
        [SerializeField, ListDrawerSettings(ShowFoldout = false, ShowIndexLabels =  true, DraggableItems = false)]
        private List<ReferenceObjectNote> notes = new();
        
        [Serializable]
        private class ReferenceObjectNote
        {
            [SerializeField]
            private Object reference;
            [SerializeField,TextArea(1,20)]
            private string note;
        }
#endif
    }
}
