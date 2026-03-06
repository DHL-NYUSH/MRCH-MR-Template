// Copyright (c) 2026 Digital Heritage Lab / Shengyang "Billiton" Peng
// Licensed under the MIT License. See LICENSE for details.

using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace MRCH.TextOp
{
    public abstract class TmpHelper : MonoBehaviour
    {
        [SerializeField, Required]
        private TMP_Text text;

        public virtual void ChangeContent(string content)
        {
            if (!text)
            {
                Debug.LogWarning($"TMP component not found: {gameObject.name}, abort ChangeContent to {content}");
                return;
            }
            text.text = content;
        }
    }
}
