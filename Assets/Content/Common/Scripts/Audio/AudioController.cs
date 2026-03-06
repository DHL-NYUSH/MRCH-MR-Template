// Copyright (c) 2026 Digital Heritage Lab / Shengyang "Billiton" Peng
// Licensed under the MIT License. See LICENSE for details.

using System;
using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

namespace MRCH.AudioOp
{
    // Scripted by Prof. Zhang, modified by Shengyang

    [RequireComponent(typeof(AudioSource))]
    public abstract class AudioController : MonoBehaviour
    {
        [SerializeField, ReadOnly, Title("Component", bold: false)]
        protected AudioSource audioSource;

        [Title("Setting", bold: false), Unit(Units.Second)]
        public float fadeDuration = 1.0f;

        [SerializeField, PropertyRange(0, 1f)]
        protected float targetVolume = 1.0f;

        private Coroutine _activeFade;

        protected virtual void Reset()
        {
            audioSource = GetComponent<AudioSource>();
        }

        protected virtual void Start()
        {
            if (!audioSource && !TryGetComponent(out audioSource))
            {
                Debug.LogWarning($"There is no audio source attached to the AudioController of {name}, " +
                                 $"none of operations on it will be done");
            }
        }

        /// <summary>
        /// Plays the attached AudioSource with a fade-in effect.
        /// </summary>
        public virtual void FadeInAudioToTargetVolume()
        {
            if (audioSource)
                StartFade(FadeRoutine(targetVolume, fadeDuration, onStart: () =>
                {
                    if (!audioSource.isPlaying)
                        audioSource.Play();
                }));
        }

        /// <summary>
        /// Stops the attached AudioSource with a fade-out effect.
        /// </summary>
        public virtual void FadeOutAudio()
        {
            if (audioSource)
                StartFade(FadeRoutine(0f, fadeDuration, onComplete: () =>
                {
                    audioSource.Stop();
                }));
        }

        /// <summary>
        /// Sets the volume of the attached AudioSource with a fade effect.
        /// </summary>
        /// <param name="volume">Target volume value between 0.0 and 1.0</param>
        public virtual void SetVolumeTo(float volume)
        {
            if (audioSource)
                StartFade(FadeRoutine(volume, fadeDuration));
        }

        /// <summary>
        /// Immediately stops any active fade and the audio source.
        /// </summary>
        public virtual void StopImmediate()
        {
            StopActiveFade();
            if (audioSource)
            {
                audioSource.Stop();
                audioSource.volume = 0f;
            }
        }

        /// <summary>
        /// Unified coroutine to fade the audio volume from its current value to a target.
        /// </summary>
        /// <param name="toVolume">Target volume</param>
        /// <param name="duration">Time taken to change the volume</param>
        /// <param name="onStart">Optional callback invoked before the fade begins</param>
        /// <param name="onComplete">Optional callback invoked after the fade finishes</param>
        protected virtual IEnumerator FadeRoutine(float toVolume, float duration,
            Action onStart = null, Action onComplete = null)
        {
            onStart?.Invoke();

            var startVolume = audioSource.volume;
            var timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                var t = Mathf.Clamp01(timer / duration);
                audioSource.volume = Mathf.Lerp(startVolume, toVolume, t);
                yield return null;
            }

            audioSource.volume = toVolume;
            _activeFade = null;

            onComplete?.Invoke();
        }

        /// <summary>
        /// Starts a new fade, cancelling any currently running fade first.
        /// </summary>
        protected virtual void StartFade(IEnumerator routine)
        {
            StopActiveFade();
            _activeFade = StartCoroutine(routine);
        }

        /// <summary>
        /// Stops the currently active fade coroutine if one is running.
        /// </summary>
        private void StopActiveFade()
        {
            if (_activeFade != null)
            {
                StopCoroutine(_activeFade);
                _activeFade = null;
            }
        }
    }
}