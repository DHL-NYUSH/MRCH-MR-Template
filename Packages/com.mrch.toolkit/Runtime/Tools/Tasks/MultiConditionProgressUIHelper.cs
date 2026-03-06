// Copyright (c) 2026 Digital Heritage Lab / Shengyang "Billiton" Peng
// Licensed under the MIT License. See LICENSE for details.

using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace MRCH.Tools.Tasks
{
    public abstract class MultiConditionProgressUIHelper : MonoBehaviour
    {
        [SerializeField, Required]
        protected MultiConditionEventManager conditionEventManager;
        
        [SerializeField, Required]
        protected TMP_Text progressText;

        [SerializeField, TextArea(1, 5)] 
        [InfoBox("Use placeholder <b>'{0}'</b> for the progress and <b>'{1}'</b> for the max progress")]
        protected string progressFormat = "Progress: {0} / {1}";
        
        protected const string ProgressPlaceholder = "{0}";
        protected const string ProgressMaxProgressPlaceholder = "{1}";

        protected void OnEnable()
        {
            conditionEventManager?.onProgressChanged.AddListener(UpdateProgress);
        }

        protected void OnDisable()
        {
            conditionEventManager?.onProgressChanged.RemoveListener(UpdateProgress);
        }

        protected void UpdateProgress(int progress, int maxProgress)
        {
            progressText.text 
                = progressFormat.Replace(ProgressPlaceholder, progress.ToString())
                .Replace(ProgressMaxProgressPlaceholder, maxProgress.ToString());
        }
    }
}
