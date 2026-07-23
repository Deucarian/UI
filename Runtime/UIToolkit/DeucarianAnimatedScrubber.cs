using System.Collections;
using Deucarian.Theming;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.UI
{
    /// <summary>
    /// Animates UI Toolkit scrubber chrome while applying logical enabled
    /// state immediately.
    /// </summary>
    public sealed class DeucarianAnimatedScrubber
    {
        private const float ComparisonTolerance = 0.0001f;

        private readonly MonoBehaviour host;
        private readonly VisualElement scrubber;
        private readonly VisualElement track;
        private readonly VisualElement fill;
        private readonly VisualElement handle;
        private readonly DeucarianMotionProfile profile;
        private Coroutine routine;
        private bool initialized;
        private bool hasTarget;
        private DeucarianScrubberVisualState targetState;
        private DeucarianScrubberPresentation currentPresentation;
        private DeucarianScrubberPresentation targetPresentation;

        public DeucarianAnimatedScrubber(
            MonoBehaviour host,
            VisualElement scrubber,
            VisualElement track,
            VisualElement fill,
            VisualElement handle,
            DeucarianMotionProfile profile)
        {
            this.host = host;
            this.scrubber = scrubber;
            this.track = track;
            this.fill = fill;
            this.handle = handle;
            this.profile = profile;
        }

        public bool IsAnimating => routine != null;
        public DeucarianScrubberVisualState TargetState => targetState;

        public void SetState(
            DeucarianScrubberMetrics metrics,
            DeucarianScrubberPalette palette,
            DeucarianScrubberVisualState state,
            bool animate = true)
        {
            SetState(metrics, palette, state, null, 0f, animate);
        }

        public void SetState(
            DeucarianScrubberMetrics metrics,
            DeucarianScrubberPalette palette,
            DeucarianScrubberVisualState state,
            DeucarianThemeStyle style,
            bool animate = true)
        {
            SetState(
                metrics,
                palette,
                state,
                style,
                DeucarianControlIslandStyle.DefaultVerticalPadding,
                animate);
        }

        public void SetState(
            DeucarianScrubberMetrics metrics,
            DeucarianScrubberPalette palette,
            DeucarianScrubberVisualState state,
            DeucarianThemeStyle style,
            float containerInset,
            bool animate = true)
        {
            scrubber?.SetEnabled(state.Enabled);
            DeucarianScrubberStyle.ApplyLayout(
                scrubber,
                track,
                fill,
                handle,
                metrics,
                style,
                containerInset);
            DeucarianScrubberPresentation next =
                DeucarianScrubberStyle.ResolvePresentation(
                    metrics,
                    palette,
                    state,
                    style);
            if (hasTarget && PresentationsMatch(targetPresentation, next))
            {
                targetState = state;
                return;
            }

            DeucarianScrubberVisualState previousState = targetState;
            hasTarget = true;
            targetState = state;
            targetPresentation = next;

            if (!initialized ||
                !animate ||
                host == null ||
                !host.isActiveAndEnabled ||
                !Application.isPlaying)
            {
                Stop();
                ApplyImmediate(next);
                return;
            }

            Stop();
            bool entering = IsEntering(previousState, state);
            float duration = profile.Duration(entering);
            if (duration <= ComparisonTolerance)
            {
                ApplyImmediate(next);
                return;
            }

            routine = host.StartCoroutine(
                Animate(currentPresentation, next, entering, duration));
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
            DeucarianScrubberPresentation from,
            DeucarianScrubberPresentation to,
            bool entering,
            float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float linear = Mathf.Clamp01(elapsed / duration);
                float eased = profile.Evaluate(entering, linear);
                currentPresentation =
                    DeucarianScrubberPresentation.Lerp(from, to, eased);
                DeucarianScrubberStyle.ApplyPresentation(
                    scrubber,
                    track,
                    fill,
                    handle,
                    currentPresentation);
                elapsed += Time.deltaTime;
                yield return null;
            }

            ApplyImmediate(to);
            routine = null;
        }

        private void ApplyImmediate(DeucarianScrubberPresentation presentation)
        {
            currentPresentation = presentation;
            initialized = true;
            DeucarianScrubberStyle.ApplyPresentation(
                scrubber,
                track,
                fill,
                handle,
                presentation);
        }

        private static bool IsEntering(
            DeucarianScrubberVisualState from,
            DeucarianScrubberVisualState to)
        {
            if (from.Enabled != to.Enabled)
            {
                return to.Enabled;
            }

            return to.Hovered || to.Pressed || to.Dragging;
        }

        private static bool PresentationsMatch(
            DeucarianScrubberPresentation first,
            DeucarianScrubberPresentation second)
        {
            return ColorsMatch(first.Well, second.Well) &&
                   ColorsMatch(first.Track, second.Track) &&
                   ColorsMatch(first.Fill, second.Fill) &&
                   ColorsMatch(first.Handle, second.Handle) &&
                   ColorsMatch(first.Border, second.Border) &&
                   Mathf.Abs(first.Opacity - second.Opacity) <= ComparisonTolerance &&
                   Mathf.Abs(first.BorderWidth - second.BorderWidth) <= ComparisonTolerance &&
                   Vector3.Distance(first.HandleScale, second.HandleScale) <= ComparisonTolerance;
        }

        private static bool ColorsMatch(Color first, Color second)
        {
            return Mathf.Abs(first.r - second.r) <= ComparisonTolerance &&
                   Mathf.Abs(first.g - second.g) <= ComparisonTolerance &&
                   Mathf.Abs(first.b - second.b) <= ComparisonTolerance &&
                   Mathf.Abs(first.a - second.a) <= ComparisonTolerance;
        }
    }
}
