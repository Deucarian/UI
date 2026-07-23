using System;
using System.Collections;
using UnityEngine;

namespace Deucarian.UI
{
    /// <summary>
    /// Animates a normalized presentation value for custom UI controls while
    /// leaving logical state ownership with the caller.
    /// </summary>
    public sealed class DeucarianAnimatedProgress
    {
        private const float ProgressEpsilon = 0.0001f;

        private readonly MonoBehaviour host;
        private readonly DeucarianMotionProfile profile;
        private readonly Action<float> apply;
        private Coroutine routine;

        public DeucarianAnimatedProgress(
            MonoBehaviour host,
            float initialValue,
            DeucarianMotionProfile profile,
            Action<float> apply)
        {
            this.host = host;
            this.profile = profile;
            this.apply = apply;
            Value = Mathf.Clamp01(initialValue);
            Target = Value;
            apply?.Invoke(Value);
        }

        public bool IsAnimating => routine != null;
        public float Value { get; private set; }
        public float Target { get; private set; }

        public void SetTarget(float target, bool animate = true)
        {
            float normalizedTarget = Mathf.Clamp01(target);
            if (Mathf.Abs(Target - normalizedTarget) <= ProgressEpsilon &&
                routine != null)
            {
                return;
            }

            Stop();
            Target = normalizedTarget;
            float distance = Mathf.Abs(Target - Value);
            if (!animate ||
                distance <= ProgressEpsilon ||
                host == null ||
                !host.isActiveAndEnabled ||
                !Application.isPlaying)
            {
                Apply(Target);
                return;
            }

            bool entering = Target > Value;
            float duration = profile.Duration(entering) * distance;
            if (duration <= ProgressEpsilon)
            {
                Apply(Target);
                return;
            }

            routine = host.StartCoroutine(Animate(Value, Target, entering, duration));
        }

        public void Stop()
        {
            if (routine != null && host != null)
            {
                host.StopCoroutine(routine);
            }

            routine = null;
        }

        private IEnumerator Animate(
            float from,
            float to,
            bool entering,
            float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float linear = Mathf.Clamp01(elapsed / duration);
                Apply(Mathf.Lerp(from, to, profile.Evaluate(entering, linear)));
                elapsed += Time.deltaTime;
                yield return null;
            }

            Apply(to);
            routine = null;
        }

        private void Apply(float value)
        {
            Value = Mathf.Clamp01(value);
            apply?.Invoke(Value);
        }
    }
}
