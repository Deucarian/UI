using System.Collections;
using Deucarian.Theming;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.UI
{
    /// <summary>
    /// Animates UI Toolkit icon-button presentation while applying logical
    /// enabled state immediately.
    /// </summary>
    public sealed class DeucarianAnimatedIconButton
    {
        private const float ComparisonTolerance = 0.0001f;

        private readonly MonoBehaviour host;
        private readonly Button button;
        private readonly VisualElement icon;
        private readonly DeucarianMotionProfile profile;
        private readonly bool manageIconVisibility;
        private readonly bool manageButtonScale;
        private readonly bool manageIconScale;
        private Coroutine routine;
        private bool initialized;
        private bool hasTarget;
        private DeucarianIconButtonVisualState targetState;
        private DeucarianIconButtonPresentation currentPresentation;
        private DeucarianIconButtonPresentation targetPresentation;

        public DeucarianAnimatedIconButton(
            MonoBehaviour host,
            Button button,
            VisualElement icon,
            DeucarianMotionProfile profile,
            bool manageIconVisibility = true,
            bool manageButtonScale = true,
            bool manageIconScale = true)
        {
            this.host = host;
            this.button = button;
            this.icon = icon;
            this.profile = profile;
            this.manageIconVisibility = manageIconVisibility;
            this.manageButtonScale = manageButtonScale;
            this.manageIconScale = manageIconScale;
        }

        public bool IsAnimating => routine != null;
        public DeucarianIconButtonVisualState TargetState => targetState;

        public void SetState(
            DeucarianIconButtonPalette palette,
            DeucarianIconButtonVisualState state,
            bool animate = true)
        {
            SetState(palette, state, null, animate);
        }

        public void SetState(
            DeucarianIconButtonPalette palette,
            DeucarianIconButtonVisualState state,
            DeucarianThemeStyle style,
            bool animate = true)
        {
            button?.SetEnabled(state.Enabled);
            DeucarianIconButtonPresentation next =
                DeucarianIconButtonStyle.ResolvePresentation(palette, state, style);
            if (hasTarget && PresentationsMatch(targetPresentation, next))
            {
                targetState = state;
                return;
            }

            DeucarianIconButtonVisualState previousState = targetState;
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

        /// <summary>
        /// Invalidates the cached presentation after another style applier has
        /// touched the same elements. The next SetState call reapplies the full
        /// resolved state immediately.
        /// </summary>
        public void InvalidatePresentation()
        {
            Stop();
            initialized = false;
            hasTarget = false;
        }

        private IEnumerator Animate(
            DeucarianIconButtonPresentation from,
            DeucarianIconButtonPresentation to,
            bool entering,
            float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float linear = Mathf.Clamp01(elapsed / duration);
                float eased = profile.Evaluate(entering, linear);
                currentPresentation =
                    DeucarianIconButtonPresentation.Lerp(from, to, eased);
                DeucarianIconButtonStyle.ApplyPresentation(
                    button,
                    icon,
                    currentPresentation,
                    true,
                    manageIconVisibility,
                    manageButtonScale,
                    manageIconScale);
                elapsed += Time.deltaTime;
                yield return null;
            }

            ApplyImmediate(to);
            routine = null;
        }

        private void ApplyImmediate(DeucarianIconButtonPresentation presentation)
        {
            currentPresentation = presentation;
            initialized = true;
            DeucarianIconButtonStyle.ApplyPresentation(
                button,
                icon,
                presentation,
                false,
                manageIconVisibility,
                manageButtonScale,
                manageIconScale);
        }

        private static bool IsEntering(
            DeucarianIconButtonVisualState from,
            DeucarianIconButtonVisualState to)
        {
            if (from.Visible != to.Visible)
            {
                return to.Visible;
            }

            if (from.Enabled != to.Enabled)
            {
                return to.Enabled;
            }

            return to.Selected || to.Hovered || to.Pressed || to.Focused;
        }

        private static bool PresentationsMatch(
            DeucarianIconButtonPresentation first,
            DeucarianIconButtonPresentation second)
        {
            return first.Visible == second.Visible &&
                   Mathf.Abs(first.Opacity - second.Opacity) <= ComparisonTolerance &&
                   ColorsMatch(first.Background, second.Background) &&
                   ColorsMatch(first.Text, second.Text) &&
                   ColorsMatch(first.Icon, second.Icon) &&
                   ColorsMatch(first.Border, second.Border) &&
                   Mathf.Abs(first.BorderWidth - second.BorderWidth) <= ComparisonTolerance &&
                   Vector3.Distance(first.ButtonScale, second.ButtonScale) <= ComparisonTolerance &&
                   Vector3.Distance(first.IconScale, second.IconScale) <= ComparisonTolerance;
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
