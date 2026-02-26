using MRCH.Common.Tool;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace MRCH.Content.Common.Scripts.Tools
{
    public class MultiConditionProgressUIHelper : MonoBehaviour
    {
        [SerializeField, Required]
        private MultiConditionEventManager conditionEventManager;
        
        [SerializeField, Required]
        private TMP_Text progressText;

        [SerializeField, TextArea(1, 5)] 
        [InfoBox("Use placeholder <b>'{0}'</b> for the progress and <b>'{1}'</b> for the max progress")]
        private string progressFormat = "Progress: {0} / {1}";
        
        private const string ProgressPlaceholder = "{0}";
        private const string ProgressMaxProgressPlaceholder = "{1}";

        private void OnEnable()
        {
            conditionEventManager?.onProgressChanged.AddListener(UpdateProgress);
        }

        private void OnDisable()
        {
            conditionEventManager?.onProgressChanged.RemoveListener(UpdateProgress);
        }

        private void UpdateProgress(int progress, int maxProgress)
        {
            progressText.text 
                = progressFormat.Replace(ProgressPlaceholder, progress.ToString())
                .Replace(ProgressMaxProgressPlaceholder, maxProgress.ToString());
        }
    }
}
