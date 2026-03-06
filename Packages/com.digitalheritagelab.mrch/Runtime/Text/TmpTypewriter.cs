// Copyright (c) 2026 Digital Heritage Lab / Shengyang "Billiton" Peng
// Licensed under the MIT License. See LICENSE for details.

using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;
using UnityEngine.SceneManagement;

namespace MRCH.TextOp
{
    public abstract class TmpTypewriter : MonoBehaviour
    {
        [SerializeField, ReadOnly] protected TextMeshProUGUI textUI;

        [SerializeField, ReadOnly] protected TextMeshPro text;

        [CanBeNull, AssetsOnly] protected AudioSource TypeAudioSource;

        [Title("Content to type", bold: false),
         InfoBox("Put the content you want to type in the text area, " +
                 "the content of the TMP component will be ignored")]
        [HideLabel]
        [MultiLineProperty(8), SerializeField]
        protected string contentToType;

        [SerializeField, Unit(Units.Second)] protected float typeSpeed = 0.1f;

        [Title("Setting"), DetailedInfoBox("It will start a new line in advance",
             "It will start a new line in advance if the text overflows when typing the next word. It is recommended to enable " +
             "especially if it is in English-like language. However, if the width is short, it might be fine to disable it.")]
        [SerializeField]
        protected bool startNewLineWhenOverflow = true;

        [SerializeField, Space] protected bool typeOnEnable;

        [SerializeField, ShowIf("typeOnEnable")]
        protected bool onlyTypeForTheFirstTime;

        [SerializeField, ShowIf("@typeOnEnable && onlyTypeForTheFirstTime")]
        protected bool saveCrossScene;

        [CanBeNull, SerializeField,
         InfoBox(
             "If you need to play a sound when typing, you need to have a AudioSource on this object and audioclip here"),
         Space]
        protected AudioClip typeSound;

        protected bool IsPlayed;
        protected bool IsPlaying;
        private Coroutine _typingCoroutine;

        protected virtual void Awake()
        {
            TryGetComponent(out textUI);
            TryGetComponent(out text);

            if (!textUI && !text)
            {
                Debug.LogWarning($"No TMP component found on {gameObject.name}");
            }
            else if (textUI && text)
            {
                Debug.LogWarning($"Both TMP and TMPUGUI found on {gameObject.name}, pick one.");
            }

            TryGetComponent(out TypeAudioSource);
        }

        protected virtual void OnEnable()
        {
            if (typeOnEnable)
            {
                // Reset partial state from a previous disable mid-type
                IsPlaying = false;

                var prefsKey = GetPlayerPrefsKey();

                if ((PlayerPrefs.GetInt(prefsKey) == 1 || IsPlayed) && onlyTypeForTheFirstTime)
                {
                    FinishTyping();
                }
                else
                {
                    StartTyping();
                }
            }
        }

        protected virtual void OnDisable()
        {
            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
                _typingCoroutine = null;
            }

            IsPlaying = false;
        }

        [Button, HideInEditorMode]
        public virtual void StartTyping()
        {
            StartTyping(contentToType);
        }

        public virtual void StartTyping(string content)
        {
            // Stop any existing typing coroutine before starting a new one
            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
                _typingCoroutine = null;
                IsPlaying = false;
            }

            _typingCoroutine = StartCoroutine(TypeText(content));
        }

        public virtual void FinishTyping()
        {
            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
                _typingCoroutine = null;
            }

            IsPlaying = false;

            if (text) text.text = contentToType;
            if (textUI) textUI.text = contentToType;
        }

        protected virtual IEnumerator TypeText(string textToType)
        {
            if (IsPlaying)
            {
                Debug.LogWarning("Text is already being typed on " + gameObject.name);
                yield break;
            }

            IsPlaying = true;

            if (text) text.text = "";
            if (textUI) textUI.text = "";

            if (startNewLineWhenOverflow)
            {
                var words = textToType.Split(' ');

                for (var wordIndex = 0; wordIndex < words.Length; wordIndex++)
                {
                    var word = words[wordIndex];

                    var shouldAddNewLine = false;

                    if (wordIndex > 0)
                    {
                        // Only measure the current line, not the full multi-line text
                        var currentLine = GetCurrentLineText();
                        var testLine = currentLine + word;

                        if (textUI)
                        {
                            // GetPreferredValues measures without mutating the component
                            var preferredSize = textUI.GetPreferredValues(testLine);
                            if (preferredSize.x > textUI.rectTransform.rect.width)
                            {
                                shouldAddNewLine = true;
                            }
                        }

                        if (text && !shouldAddNewLine)
                        {
                            var preferredSize = text.GetPreferredValues(testLine);
                            if (preferredSize.x > text.rectTransform.rect.width)
                            {
                                shouldAddNewLine = true;
                            }
                        }
                    }

                    // Add newline if needed (before typing the word)
                    if (shouldAddNewLine)
                    {
                        if (text) text.text += "\n";
                        if (textUI) textUI.text += "\n";
                        yield return new WaitForSeconds(typeSpeed);
                    }

                    // Type each character of the word
                    foreach (var c in word)
                    {
                        if (text) text.text += c;
                        if (textUI) textUI.text += c;

                        if (typeSound && TypeAudioSource)
                            TypeAudioSource.PlayOneShot(typeSound);

                        yield return new WaitForSeconds(typeSpeed);
                    }

                    // Add space after word (except for the last word)
                    if (wordIndex < words.Length - 1)
                    {
                        if (text) text.text += " ";
                        if (textUI) textUI.text += " ";
                        yield return new WaitForSeconds(typeSpeed);
                    }
                }
            }
            else
            {
                foreach (var c in textToType)
                {
                    if (text) text.text += c;
                    if (textUI) textUI.text += c;
                    if (typeSound && TypeAudioSource)
                        TypeAudioSource.PlayOneShot(typeSound);
                    yield return new WaitForSeconds(typeSpeed);
                }
            }

            if (saveCrossScene)
                PlayerPrefs.SetInt(GetPlayerPrefsKey(), 1);
            else
                IsPlayed = true;

            IsPlaying = false;
            _typingCoroutine = null;
        }

        private string GetCurrentText()
        {
            if (textUI) return textUI.text;
            if (text) return text.text;
            return "";
        }

        /// <summary>
        /// Returns only the text on the current (last) line, used for accurate overflow measurement.
        /// </summary>
        private string GetCurrentLineText()
        {
            var full = GetCurrentText();
            var lastNewLine = full.LastIndexOf('\n');
            return lastNewLine >= 0 ? full.Substring(lastNewLine + 1) : full;
        }

        private string GetPlayerPrefsKey()
        {
            return $"TextTypedOn{gameObject.name}On{SceneManager.GetActiveScene().name}";
        }
    }
}