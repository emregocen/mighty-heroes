using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace MightyHeroes.Gameplay.Puzzle.Extensions
{
    /// <summary>
    ///     Extension classes for the Transform class
    /// </summary>
    public static class TransformExtensions
    {
        /// <summary>
        ///     A coroutine scaling the transform
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="from">The original vector</param>
        /// <param name="to">The target vector</param>
        /// <param name="time">The duration of the animation</param>
        /// <param name="onComplete">Optional UnityAction, which is invoked when the animation is completed</param>
        /// <returns></returns>
        public static IEnumerator ScaleCoroutine(this Transform transform, Vector2 from, Vector2 to, float time,
            UnityAction onComplete = null)
        {
            var currentTime = Time.timeSinceLevelLoad;
            var elapsedTime = 0.0f;
            var lastTime = currentTime;

            while (time > 0 && elapsedTime < time)
            {
                //Update Time
                currentTime = Time.timeSinceLevelLoad;
                elapsedTime += currentTime - lastTime;
                lastTime = currentTime;

                transform.localScale = Vector3.Lerp(from, to, elapsedTime / time);

                yield return null;
            }

            transform.localScale = to;

            onComplete?.Invoke();

            yield break;
        }

        public static Tween DOScaleCustom(this Transform transform, Vector2 from, Vector2 to, float time,
            UnityAction onComplete = null)
        {
            transform.DOKill();
            // Önce başlangıç ölçeğini ayarla
            transform.localScale = from;

            // DOTween tween başlat
            Tween tween = transform.DOScale(to, time)
                .SetEase(Ease.Linear) // Coroutine’de Lerp linear çalışıyor
                .OnComplete(() => onComplete?.Invoke());

            return tween;
        }

        /// <summary>
        ///     A coroutine moving the transform
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="targetPos">The target position of the transform</param>
        /// <param name="time">The duration of the animation</param>
        /// <param name="onComplete">Optional UnityAction, which is invoked when the animation is completed</param>
        /// <returns></returns>
        public static IEnumerator MoveCoroutine(this Transform transform, Vector2 targetPos, float time,
            UnityAction onComplete = null)
        {
            Vector2 startPos = transform.position;

            var currentTime = Time.timeSinceLevelLoad;
            var elapsedTime = 0.0f;
            var lastTime = currentTime;

            while (time > 0 && elapsedTime < time)
            {
                //Update Time
                currentTime = Time.timeSinceLevelLoad;
                elapsedTime += currentTime - lastTime;
                lastTime = currentTime;

                transform.position = Vector2.Lerp(startPos, targetPos, elapsedTime / time);

                yield return null;
            }

            transform.position = targetPos;

            onComplete?.Invoke();

            yield break;
        }
    }
}