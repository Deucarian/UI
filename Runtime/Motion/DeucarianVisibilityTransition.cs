using System;
using UnityEngine;

namespace Deucarian.UI
{
    public enum DeucarianVisibilityPhase
    {
        Hidden,
        Entering,
        Visible,
        Exiting
    }

    /// <summary>
    /// Renderer-independent state for a reversible enter/exit transition.
    /// The consumer chooses the time source and applies <see cref="Progress"/>
    /// to its own presentation.
    /// </summary>
    public sealed class DeucarianVisibilityTransition
    {
        private const float ProgressEpsilon = 0.0001f;

        private readonly DeucarianMotionProfile profile;
        private float startProgress;
        private float targetProgress;
        private float elapsedSeconds;
        private float durationSeconds;

        public DeucarianVisibilityTransition(DeucarianMotionProfile profile)
        {
            this.profile = profile;
            Reset(false);
        }

        public event Action<DeucarianVisibilityTransition> Completed;

        public DeucarianMotionProfile Profile => profile;
        public float Progress { get; private set; }
        public DeucarianVisibilityPhase Phase { get; private set; }
        public bool IsAnimating =>
            Phase == DeucarianVisibilityPhase.Entering ||
            Phase == DeucarianVisibilityPhase.Exiting;
        public bool IsHiding => Phase == DeucarianVisibilityPhase.Exiting;
        public float RemainingSeconds => IsAnimating
            ? Mathf.Max(0f, durationSeconds - elapsedSeconds)
            : 0f;

        public float Show(bool restartFromHidden = false)
        {
            if (restartFromHidden)
            {
                Reset(false);
            }

            return StartTransition(1f, profile.EnterSeconds);
        }

        public float Hide()
        {
            return StartTransition(0f, profile.ExitSeconds);
        }

        /// <summary>
        /// Advances the active transition by a caller-provided delta.
        /// Returns true when an active transition was evaluated.
        /// </summary>
        public bool Advance(float deltaSeconds)
        {
            if (!IsAnimating)
            {
                return false;
            }

            elapsedSeconds += Mathf.Max(0f, deltaSeconds);
            float linearProgress = durationSeconds <= ProgressEpsilon
                ? 1f
                : Mathf.Clamp01(elapsedSeconds / durationSeconds);
            bool entering = Phase == DeucarianVisibilityPhase.Entering;
            float easedProgress = profile.Evaluate(entering, linearProgress);
            Progress = Mathf.Clamp01(Mathf.Lerp(
                startProgress,
                targetProgress,
                easedProgress));

            if (linearProgress >= 1f)
            {
                SettleAt(targetProgress, true);
            }

            return true;
        }

        public void Complete()
        {
            if (IsAnimating)
            {
                SettleAt(targetProgress, true);
            }
        }

        public void Reset(bool visible)
        {
            SettleAt(visible ? 1f : 0f, false);
        }

        public void SetProgress(float progress)
        {
            SettleAt(progress, false);
        }

        private float StartTransition(float target, float fullDurationSeconds)
        {
            startProgress = Progress;
            targetProgress = Mathf.Clamp01(target);
            elapsedSeconds = 0f;

            float remainingDistance = Mathf.Abs(targetProgress - startProgress);
            if (remainingDistance <= ProgressEpsilon)
            {
                SettleAt(targetProgress, false);
                return 0f;
            }

            durationSeconds = Mathf.Max(0f, fullDurationSeconds) * remainingDistance;
            Phase = targetProgress > startProgress
                ? DeucarianVisibilityPhase.Entering
                : DeucarianVisibilityPhase.Exiting;

            if (durationSeconds <= ProgressEpsilon)
            {
                SettleAt(targetProgress, true);
                return 0f;
            }

            return durationSeconds;
        }

        private void SettleAt(float progress, bool notifyCompletion)
        {
            Progress = Mathf.Clamp01(progress);
            startProgress = Progress;
            targetProgress = Progress;
            elapsedSeconds = 0f;
            durationSeconds = 0f;
            Phase = Progress <= ProgressEpsilon
                ? DeucarianVisibilityPhase.Hidden
                : DeucarianVisibilityPhase.Visible;

            if (notifyCompletion)
            {
                Completed?.Invoke(this);
            }
        }
    }
}
