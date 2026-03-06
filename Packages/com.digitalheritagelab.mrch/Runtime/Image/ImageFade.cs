// Copyright (c) 2026 Digital Heritage Lab / Shengyang "Billiton" Peng
// Licensed under the MIT License. See LICENSE for details.

using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace MRCH.ImageOp
{
    /// <summary>
    /// This class provides functionality to fade in `RawImage` or `Image` components over a specified duration.
    /// </summary>
    public abstract class ImageFade : MonoBehaviour
    {
        [ReadOnly, SerializeField] 
        protected RawImage rawImage;
        [ReadOnly, SerializeField] 
        protected Image image;
        [ReadOnly, SerializeField] 
        protected SpriteRenderer spriteRenderer;

        protected bool rawImageExists;
        protected bool imageExists;
        protected bool spriteRendererExists;
        
        [SerializeField, Unit(Units.Second)] 
        protected float secondsToFade = 0.5f;
        [Space(10), SerializeField] 
        protected bool fadeInOnAwake = true;
        [SerializeField] 
        private bool deactivateItAfterFading = true;

        protected virtual void Awake()
        {
            var activeCount = 0;
            rawImage = GetComponent<RawImage>();
            image = GetComponent<Image>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (rawImage)
            {
                rawImageExists = true;
                activeCount++;
            }

            if (image)
            {
                imageExists = true;
                activeCount++;
            }

            if (spriteRenderer)
            {
                spriteRendererExists = true;
                activeCount++;
            }

            switch (activeCount)
            {
                case 0:
                    Debug.LogWarning("No Image component found in " + gameObject.name);
                    break;
                case > 1:
                    Debug.LogWarning("Multiple Image components found in " + gameObject.name);
                    break;
            }
        }

        protected virtual void OnEnable()
        {
            if (fadeInOnAwake)
            {
                FadeIn();
            }
        }

        public virtual void SetTimeToFade(float time)
        {
            secondsToFade = time;
        }

        public virtual void FadeIn()
        {
            if (rawImageExists)
                rawImage.color = new Color(rawImage.color.r, rawImage.color.g, rawImage.color.b, 0f);
            if (imageExists)
                image.color = new Color(image.color.r, image.color.g, image.color.b, 0f);
            if (spriteRendererExists)
                spriteRenderer.color =
                    new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0f);
            StartCoroutine(Fade(true));
        }

        public virtual void Fadeout()
        {
            if (rawImageExists)
                rawImage.color = new Color(rawImage.color.r, rawImage.color.g, rawImage.color.b, 1f);
            if (imageExists)
                image.color = new Color(image.color.r, image.color.g, image.color.b, 1f);
            if (spriteRendererExists)
                spriteRenderer.color =
                    new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1f);
            StartCoroutine(Fade(false));
        }

        protected virtual IEnumerator Fade(bool target)
        {
            var t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / secondsToFade;
                Color color;
                if (rawImageExists)
                {
                    color = rawImage.color;
                    color.a = Mathf.Lerp(target ? 0f : 1f, target ? 1f : 0f, t);
                    rawImage.color = color;
                }
                else if (imageExists)
                {
                    color = image.color;
                    color.a = Mathf.Lerp(target ? 0f : 1f, target ? 1f : 0f, t);
                    image.color = color;
                }
                else if (spriteRendererExists)
                {
                    color = spriteRenderer.color;
                    color.a = Mathf.Lerp(target ? 0f : 1f, target ? 1f : 0f, t);
                    spriteRenderer.color = color;
                }
                else
                {
                    Debug.LogError("There is no Image related component on " + name);
                    color = new Color();
                }

                yield return null;
                if (deactivateItAfterFading && color.a <= 0f)
                    gameObject.SetActive(false);
            }
        }
    }
}