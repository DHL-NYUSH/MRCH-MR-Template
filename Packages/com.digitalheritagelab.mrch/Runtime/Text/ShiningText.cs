// Copyright (c) 2026 Digital Heritage Lab / Shengyang "Billiton" Peng
// Licensed under the MIT License. See LICENSE for details.

using TMPro;
using UnityEngine;

namespace MRCH.TextOp
{
    public abstract class ShiningText : MonoBehaviour
    {
        [SerializeField, ReadOnly] protected TextMeshProUGUI textUI;
        [SerializeField, ReadOnly] protected TextMeshPro text;
        public float cycleTime = 2.0f;

        private float _elapsedTime = 0.0f;

        protected bool play = false;

        [SerializeField] protected bool playOnAwake = false;

        protected virtual void Awake()
        {
            TryGetComponent(out textUI);
            TryGetComponent(out text);

            if (textUI == null == (text == null)) //It means both are null or both are not null, I write it for fun lol
            {
                Debug.LogWarning("Check the tmp/tmpUI component on " + gameObject.name);
            }

            play = playOnAwake;
        }

        protected virtual void Update()
        {
            if (!textUI && !text) return;

            if (!play) return;

            _elapsedTime += Time.deltaTime;

            var cycleProgress = _elapsedTime % cycleTime / cycleTime;

            var alpha = Mathf.Abs(Mathf.Cos(cycleProgress * Mathf.PI));

            if (textUI)
            {
                Color color = textUI.color;
                color.a = alpha;
                textUI.color = color;
            }

            else if (text)
            {
                Color color = text.color;
                color.a = alpha;
                text.color = color;
            }
        }

        public virtual void SetShiningText(bool target)
        {
            play = target;
        }
    }
}