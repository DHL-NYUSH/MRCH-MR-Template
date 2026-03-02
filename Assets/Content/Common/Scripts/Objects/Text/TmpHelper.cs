using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace MRCH.Content.Common.Scripts.Objects.Text
{
    public class TmpHelper : MonoBehaviour
    {
        [SerializeField, Required]
        private TMP_Text text;

        public void ChangeContent(string content)
        {
            text.text = content;
        }
    }
}
