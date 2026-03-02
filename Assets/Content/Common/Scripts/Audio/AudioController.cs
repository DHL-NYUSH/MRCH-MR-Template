using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

namespace MRCH.Audio
{
//Scripted by Prof. Zhang, modified by Shengyang
    public abstract class AudioController : MonoBehaviour
    {
        [SerializeField, ReadOnly,Title("Component",bold:false)] protected AudioSource audioSource;

        [Title("Setting",bold:false),Unit(Units.Second)]public float fadeDuration = 1.0f;

        [SerializeField,PropertyRange(0,1f)] protected float targetVolume = 1.0f;

        protected virtual void Start()
        {
            TryGetComponent(out audioSource);

            if (audioSource == null)
            {
                Debug.LogWarning(
                    "No AudioSource found on this GameObject. AudioController requires an AudioSource component.");
            }
        }

        /// <summary>
        /// Plays the attached AudioSource with a fade-in effect.
        /// </summary>
        public virtual void FadeInAudioToTargetVolume()
        {
            if (audioSource)
                StartCoroutine(FadeInRoutine(fadeDuration, targetVolume));
        }

        /// <summary>
        /// Stops the attached AudioSource with a fade-out effect.
        /// </summary>
        public virtual void FadeOutAudio()
        {
            if (audioSource)
                StartCoroutine(FadeOutRoutine(fadeDuration));
            
        }

        /// <summary>
        /// Sets the volume of the attached AudioSource with a fade effect.
        /// </summary>
        /// <param name="volume">Target volume value between 0.0 and 1.0</param>
        public virtual void SetVolumeTo(float volume)
        {
            if (audioSource != null)
            {
                // Start fading to the new target volume
                StartCoroutine(FadeToVolumeRoutine(volume, fadeDuration));
            }
        }

        /// <summary>
        /// Coroutine to fade in the audio to the target volume.
        /// </summary>
        protected virtual IEnumerator FadeInRoutine(float duration, float targetVolume)
        {
            audioSource.volume = 0f; // Start from volume 0
            audioSource.Play(); // Play the audio

            float startVolume = 0f;
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, targetVolume, timer / duration);
                yield return null; // Wait until the next frame
            }

            audioSource.volume = targetVolume; // Ensure we set to target volume at the end
        }

        /// <summary>
        /// Coroutine to fade out the audio and stop it.
        /// </summary>
        protected virtual IEnumerator FadeOutRoutine(float duration)
        {
            float startVolume = audioSource.volume;
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
                yield return null; // Wait until the next frame
            }

            audioSource.Stop(); // Stop the audio once faded out
            audioSource.volume = startVolume; // Reset volume for next play
        }

        /// <summary>
        /// Coroutine to gradually change the volume to a target value.
        /// </summary>
        /// <param name="volume">Target volume</param>
        /// <param name="duration">Time taken to change the volume</param>
        protected virtual IEnumerator FadeToVolumeRoutine(float volume, float duration)
        {
            float startVolume = audioSource.volume;
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, volume, timer / duration);
                yield return null; // Wait until the next frame
            }

            audioSource.volume = volume; // Ensure it reaches the target volume at the end
        }
    }
}
