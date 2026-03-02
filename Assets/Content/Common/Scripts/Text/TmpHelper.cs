using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace MRCH.Text
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
