using Deucarian.Common;
using UnityEngine;

namespace Deucarian.UI
{
    /// <summary>Retargetable presentation position; layout and collection ownership stay with the caller.</summary>
    public sealed class DeucarianLayoutTransition
    {
        public const float DefaultDurationSeconds = .18f;
        private Vector2 start;
        private float elapsed, duration;
        public Vector2 Current { get; private set; }
        public Vector2 Target { get; private set; }
        public bool IsInitialized { get; private set; }
        public bool IsAnimating { get; private set; }

        public void MoveTo(Vector2 target, float seconds = DefaultDurationSeconds, bool animate = true)
        {
            if (!Finite(target.x) || !Finite(target.y)) return;
            if (!IsInitialized || !animate || !Finite(seconds) || seconds <= 0)
            { Current = Target = target; IsInitialized = true; Complete(); return; }
            if (Target == target) return;
            start = Current;
            Target = target;
            elapsed = 0;
            duration = seconds;
            IsAnimating = Current != Target;
        }

        public void Advance(float deltaSeconds)
        {
            if (!IsAnimating || !Finite(deltaSeconds) || deltaSeconds <= 0) return;
            elapsed = Mathf.Min(duration, elapsed + deltaSeconds);
            float progress = DeucarianEasingUtility.Evaluate(DeucarianEasing.EaseOutCubic, elapsed / duration);
            Current = Vector2.LerpUnclamped(start, Target, progress);
            if (elapsed >= duration) Complete();
        }

        public void Complete() { Current = Target; IsAnimating = false; elapsed = duration = 0; }
        public void Reset() { start = Current = Target = Vector2.zero; IsInitialized = false; Complete(); }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
