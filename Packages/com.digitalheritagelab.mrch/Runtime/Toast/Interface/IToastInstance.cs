// Copyright (c) 2026 Digital Heritage Lab / Shengyang "Billiton" Peng
// Licensed under the MIT License. See LICENSE for details.

using UnityEngine;

namespace MRCH.Toast.Interface
{
    /// <summary>
    /// Interface for toast instance components.
    /// Implement this on a MonoBehaviour and attach it to your toast prefab's root.
    /// The ToastManager will find this component and use it to display content.
    /// <br/><br/>
    /// Students: Create your own MonoBehaviour that implements this interface,
    /// then replace DefaultToastInstance on the prefab to customize toast behavior.
    /// </summary>
    public interface IToastInstance
    {
        /// <summary>
        /// The RectTransform of this toast. Used by ToastManager for positioning and animation.
        /// </summary>
        RectTransform RectTransform { get; }

        /// <summary>
        /// The CanvasGroup of this toast. Used by ToastManager for alpha fading.
        /// </summary>
        CanvasGroup CanvasGroup { get; }

        /// <summary>
        /// The GameObject of this toast instance.
        /// </summary>
        GameObject GameObj { get; }

        /// <summary>
        /// Called once after instantiation. Cache your component references here.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Set the content to display on this toast.
        /// </summary>
        /// <param name="message">The text message to show.</param>
        /// <param name="icon">Optional icon sprite. Null means no icon.</param>
        void SetContent(string message, Sprite icon = null);

        /// <summary>
        /// Reset all content and visual state to a clean slate.
        /// Called before the toast is reused for a new message.
        /// </summary>
        void ResetContent();
    }
}