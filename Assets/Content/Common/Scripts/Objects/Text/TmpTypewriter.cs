using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;
using UnityEngine.SceneManagement;

namespace MRCH.Common.Interact
{
    public abstract class TmpTypewriter : MonoBehaviour
    {
        [SerializeField, ReadOnly] protected TextMeshProUGUI textUI;

        [SerializeField, ReadOnly] protected TextMeshPro text;

        [CanBeNull, AssetsOnly] protected AudioSource TypeAudioSource;

        [Title("Content to type", bold: false),
         InfoBox("Put the content you want to type in the text area, the content of the TMP component will be ignored",
             InfoMessageType.Info)]//, visibleIfMemberName: "contentToTypeExist")]
        [HideLabel]
        [MultiLineProperty(8), SerializeField]
        protected string contentToType;

        //private bool contentToTypeExist => contentToType.Length == 0;
        [SerializeField, Unit(Units.Second)] protected float typeSpeed = 0.1f;

        [Title("Setting"), DetailedInfoBox("It will start a new line in advance",
             "It will start a new line in advance if the text overflows when typing the next word. It is recommended to enable" +
             "especially if it is in English-like language. However, if the width is short, it might be fine to disable it.")]
        [SerializeField]
        protected bool startNewLineWhenOverflow = true;

        [SerializeField, Space] protected bool typeOnEnable = false;

        [SerializeField, ShowIf("typeOnEnable")]
        protected bool onlyTypeForTheFirstTime = false;

        [SerializeField, ShowIf("@typeOnEnable && onlyTypeForTheFirstTime")]
        protected bool saveCrossScene = false;

        [CanBeNull, SerializeField,
         InfoBox(
             "If you need to play a sound when typing, you need to have a AudioSource on this object and audioclip here"),
         Space]
        protected AudioClip typeSound;

        protected bool _isPlayed = false;
        protected bool _isPlaying = false;

        protected virtual void Awake()
        {
            TryGetComponent<TextMeshProUGUI>(out textUI);
            TryGetComponent<TextMeshPro>(out text);

            if (textUI == null == (text == null)) //It means both are null or both are not null, I write it for fun lol
            {
                Debug.LogWarning("Check the tmp/tmpUI component on " + gameObject.name);
            }

            TryGetComponent(out TypeAudioSource);
        }

        protected virtual void OnEnable()
        {
            if (typeOnEnable)
            {
                if ((PlayerPrefs.GetInt($"TextTypedOn{gameObject.name}On{SceneManager.GetActiveScene()}") == 1 ||
                     _isPlayed) && onlyTypeForTheFirstTime)
                {
                    FinishTyping();
                }
                else
                {
                    StartCoroutine(TypeText(contentToType));
                }
            }
        }

        protected virtual void OnDisable()
        {
            _isPlaying = false;
        }

        public virtual void StartTyping()
        {
            StartCoroutine(TypeText(contentToType));
        }

        public virtual void FinishTyping()
        {
            if (text) text.text = contentToType;
            if (textUI) textUI.text = contentToType;
        }

        protected virtual IEnumerator TypeText(string textToType)
        {
            if (_isPlaying)
            {
                Debug.LogWarning("Text has been typed on " + gameObject.name);
                yield break;
            }

            _isPlaying = true;

            if (text) text.text = "";
            if (textUI) textUI.text = "";

            if (startNewLineWhenOverflow)
            {
                var words = textToType.Split(' ');

                for (int wordIndex = 0; wordIndex < words.Length; wordIndex++)
                {
                    var word = words[wordIndex];

                    // Check if adding this word would cause overflow
                    bool shouldAddNewLine = false;

                    if (wordIndex > 0) // Don't check for the first word
                    {
                        string testText = GetCurrentText() + word;

                        if (textUI)
                        {
                            string originalText = textUI.text;
                            textUI.text = testText;
                            textUI.ForceMeshUpdate();

                            if (textUI.preferredWidth > textUI.rectTransform.rect.width)
                            {
                                shouldAddNewLine = true;
                            }

                            textUI.text = originalText;
                        }

                        if (text && !shouldAddNewLine)
                        {
                            string originalText = text.text;
                            text.text = testText;
                            text.ForceMeshUpdate();

                            // For TextMeshPro 3D, check against bounds
                            if (text.bounds.size.x > text.rectTransform.rect.width)
                            {
                                shouldAddNewLine = true;
                            }

                            text.text = originalText;
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
                PlayerPrefs.SetInt($"TextTypedOn{gameObject.name}On{SceneManager.GetActiveScene()}", 1);
            else
                _isPlayed = true;

            _isPlaying = false;
        }

        private string GetCurrentText()
        {
            if (textUI) return textUI.text;
            if (text) return text.text;
            return "";
        }
    }
}
