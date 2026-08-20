using MyAPI.Data;
using System.Collections;
using UnityEngine;

namespace MyAPI.Additions
{
    public class CustomAnimatorScript : MonoBehaviour
    {
        public Sprite defaultSprite;
        public SpriteRenderer[] _renderers { get; private set; }
        public AnimationData currentAnimation { get; private set; }
        public bool isPlaying => currentAnimation != null;

#pragma warning disable IDE0051
        private void Awake()
        {
            _renderers = GetComponentsInChildren<SpriteRenderer>();

            if (defaultSprite != null)
            {
                SetSprite(defaultSprite);
            }
        }
#pragma warning restore IDE0051

        private void SetSprite(Sprite sprite)
        {
            foreach (var _renderer in _renderers)
            {
                if (_renderer != null)
                {
                    _renderer.sprite = sprite;
                }
            }
        }

        public void StopAnimation()
        {
            if (currentAnimation == null) return;

            currentAnimation.loop = false;
            if (defaultSprite != null)
            {
                SetSprite(defaultSprite);
            }
            StopAllCoroutines();

            currentAnimation = null;
        }

        public void PlayAnimation(AnimationData animationData)
        {
            if (animationData == null || animationData.spriteData == null)
            {
                Debug.LogError("AnimationData or spriteData is null!");
                return;
            }

            StopAnimation();

            currentAnimation = animationData;

            StartCoroutine(PlayAnimationRoutine());
        }

        private IEnumerator PlayAnimationRoutine()
        {
            if (currentAnimation.spriteData == null || currentAnimation.spriteData.Count == 0)
            {
                Debug.LogError("Current Animation's sprite data is null or empty!");
                yield break;
            }

            do
            {
                int startIndex = currentAnimation.reverse ? currentAnimation.spriteData.Count - 1 : 0;
                int endIndex = currentAnimation.reverse ? -1 : currentAnimation.spriteData.Count;
                int step = currentAnimation.reverse ? -1 : 1;

                for (int i = startIndex; currentAnimation.reverse ? i > endIndex : i < endIndex; i += step)
                {
                    if (i < 0 || i >= currentAnimation.spriteData.Count)
                    {
                        break;
                    }

                    var sprite = currentAnimation.spriteData[i];
                    if (sprite == null)
                    {
                        Debug.LogWarning($"Sprite at index {i} is null!");
                        continue;
                    }

                    if (currentAnimation.delay != null)
                    {
                        yield return currentAnimation.delay;
                    }
                    else
                    {
                        yield return null;
                    }

                    SetSprite(sprite);
                }

            } while (currentAnimation.loop);
        }
    }
}