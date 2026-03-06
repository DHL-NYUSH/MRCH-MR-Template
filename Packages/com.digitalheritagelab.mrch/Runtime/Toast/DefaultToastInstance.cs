// Copyright (c) 2026 Digital Heritage Lab / Shengyang "Billiton" Peng
// Licensed under the MIT License. See LICENSE for details.

using MRCH.Toast.Interface;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MRCH.Toast
{
    /// <summary>
    /// Default toast instance with TextMeshPro text and an optional icon Image.
    /// Attach this to your toast prefab root. The prefab root must also have a CanvasGroup.
    /// <br/><br/>
    /// Students: You can use this as-is, or create your own MonoBehaviour implementing
    /// IToastInstance and replace this component on the prefab.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("MRCH/Toast/Default Toast Instance")]
    public class DefaultToastInstance : MonoBehaviour, IToastInstance
    {
        #region References

        [TitleGroup("References")]
        [SerializeField, Required, Tooltip("The TextMeshProUGUI component for the toast message.")]
        private TextMeshProUGUI messageText;

        [TitleGroup("References")]
        [SerializeField, Tooltip("Optional icon Image. Leave empty if your toast has no icon.")]
        private Image iconImage;

        #endregion

        #region Events

        [FoldoutGroup("Instance Events")]
        [Tooltip("Fired when SetContent is called on this instance.")]
        public UnityEvent<string> onContentSet;

        [FoldoutGroup("Instance Events")]
        [Tooltip("Fired when ResetContent is called on this instance.")]
        public UnityEvent onContentReset;

        #endregion

        #region Cached

        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;

        #endregion

        #region IToastInstance

        public RectTransform RectTransform
        {
            get
            {
                if (!_rectTransform) _rectTransform = GetComponent<RectTransform>();
                return _rectTransform;
            }
        }

        public CanvasGroup CanvasGroup
        {
            get
            {
                if (_canvasGroup) return _canvasGroup;
                
                // Guard against accessing destroyed object
                if (!this) return null;
                
                if(!TryGetComponent(out _canvasGroup))
                    Debug.LogError($"No CanvasGroup found on {gameObject.name}. Ignore it when quit or destroying");
                return _canvasGroup;
            }
        }

        public GameObject GameObj => gameObject;

        public virtual void Initialize()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();

            // Start hidden
            _canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        public virtual void SetContent(string message, Sprite icon = null)
        {
            if (messageText)
            {
                messageText.text = message;
            }

            if (iconImage && iconImage.sprite && !icon)
            {
                iconImage.enabled = true;
            }
            //If you want to switch logo according the parameter passed:
            /*
            if (iconImage)
            {
                if (icon)
                {
                    iconImage.sprite = icon;
                    iconImage.enabled = true;
                    iconImage.preserveAspect = true;
                }
                else
                {
                    iconImage.enabled = false;
                }
            }*/

            onContentSet?.Invoke(message);
        }

        public virtual void ResetContent()
        {
            if (messageText)
            {
                messageText.text = string.Empty;
            }

            if (iconImage)
            {
                //iconImage.sprite = null;
                iconImage.enabled = false;
            }

            onContentReset?.Invoke();
        }

        #endregion
    }
}