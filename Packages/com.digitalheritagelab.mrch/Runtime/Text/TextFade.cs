// Copyright (c) 2026 Digital Heritage Lab / Shengyang "Billiton" Peng
// Licensed under the MIT License. See LICENSE for details.

using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Serialization;

namespace MRCH.TextOp
{
    public abstract class TextFade : MonoBehaviour
    {
        public float fadeDuration;
        [ReadOnly, SerializeField] protected TMP_Text text;

        protected virtual void Awake()
        {
            TryGetComponent(out text);
            if (!text)
                Debug.LogError("TextMeshPro not found in " + gameObject.name + ", disable TextFade component.");
        }


        public virtual void FadeIn(float fadeDurationParam = 0)
        {
            Debug.Log("FadeIn on FadeTextWorldSpace of " + gameObject.name + " with duration " + fadeDurationParam);
            var fadeDurationToRun = fadeDurationParam == 0 ? fadeDuration : fadeDurationParam;
            if (text)
                StartCoroutine(TextFadeHelper(text, 1f, fadeDurationToRun));
            else
                Debug.LogWarning("TextMeshPro not found in " + gameObject.name + ", abort fade in.");
        }

        public virtual void FadeOut(float fadeDurationParam = 0)
        {
            Debug.Log("FadeOut on FadeTextWorldSpace of " + gameObject.name + " with duration " + fadeDurationParam);
            var fadeDurationToRun = fadeDurationParam == 0 ? fadeDuration : fadeDurationParam;
            if (text)
                StartCoroutine(TextFadeHelper(text, 0f, fadeDurationToRun));
            else
                Debug.LogWarning("TextMeshPro not found in " + gameObject.name + ", abort fade out.");
        }

        protected static IEnumerator TextFadeHelper(TMP_Text text, float targetAlpha, float duration)
        {
            if (!text)
            {
                Debug.LogWarning("Null TextMeshPro, abort fade text.");
                yield break; 
            }
            var originalAlpha = text.color.a;
            var elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var alpha = Mathf.Lerp(originalAlpha, targetAlpha, elapsed / duration);
                text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
                yield return null;
            }
        }
    }
}