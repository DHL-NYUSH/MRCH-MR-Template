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
            text.text = content;
        }
    }
}
