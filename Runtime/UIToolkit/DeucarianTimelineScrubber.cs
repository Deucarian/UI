using System;
using Deucarian.Theming;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Deucarian.UI
{
    /// <summary>
    /// Reusable pointer-driven timeline scrubber with preview and committed
    /// seek events. Presentation is resolved by the shared control-island theme.
    /// </summary>
    public sealed class DeucarianTimelineScrubber : VisualElement
    {
        public const string TrackName = "DeucarianTimelineTrack";
        public const string FillName = "DeucarianTimelineProgress";
        public const string HandleName = "DeucarianTimelineHandle";

        private readonly VisualElement track;
        private readonly VisualElement fill;
        private readonly VisualElement handle;
        private DeucarianTheme theme;
        private Object themeContext;
        private DeucarianAnimatedScrubber stateAnimation;
        private Func<bool> animationPolicy;
        private int activePointerId = -1;
        private bool hovered;
        private bool pressed;
        private bool dragging;
        private float normalizedProgress;

        public DeucarianTimelineScrubber()
        {
            pickingMode = PickingMode.Position;
            focusable = true;

            track = new VisualElement { name = TrackName };
            fill = new VisualElement { name = FillName };
            handle = new VisualElement { name = HandleName };
            DeucarianControlIslandElementStyle.AddScrubberClasses(
                this,
                track,
                fill,
                handle);

            fill.Add(handle);
            track.Add(fill);
            Add(track);

            RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<PointerCancelEvent>(OnPointerCancel);
            UpdateProgressVisual();
        }

        public event Action<float> PreviewStarted;
        public event Action<float> PreviewChanged;
        public event Action<float> SeekRequested;
        public event Action PreviewCanceled;

        public float NormalizedProgress => normalizedProgress;
        public bool IsScrubEnabled { get; private set; }
        public bool IsDragging => dragging;
        public VisualElement TrackElement => track;
        public VisualElement FillElement => fill;
        public VisualElement HandleElement => handle;

        public void ConfigureMotion(
            MonoBehaviour host,
            Func<bool> shouldAnimate = null)
        {
            stateAnimation?.Stop();
            animationPolicy = shouldAnimate;
            stateAnimation = host != null
                ? new DeucarianAnimatedScrubber(
                    host,
                    this,
                    track,
                    fill,
                    handle,
                    DeucarianMotionProfile.ControlState)
                : null;
        }

        public void StopMotion()
        {
            stateAnimation?.Stop();
            stateAnimation = null;
            animationPolicy = null;
        }

        public void ApplyTheme(
            DeucarianTheme currentTheme,
            Object context = null)
        {
            theme = currentTheme;
            themeContext = context;
            ApplyVisualState();
        }

        public void SetScrubEnabled(bool enabled)
        {
            IsScrubEnabled = enabled;
            if (stateAnimation == null)
            {
                SetEnabled(enabled);
            }

            if (!enabled)
            {
                CancelInteraction();
            }

            ApplyVisualState();
        }

        public void SetProgress(float progress)
        {
            if (!dragging)
            {
                SetProgressImmediate(progress, false);
            }
        }

        public bool BeginScrubPreview(float progress)
        {
            if (!IsScrubEnabled)
            {
                return false;
            }

            dragging = true;
            pressed = true;
            float clamped = Mathf.Clamp01(progress);
            PreviewStarted?.Invoke(clamped);
            SetProgressImmediate(clamped, true);
            ApplyVisualState();
            return true;
        }

        public bool UpdateScrubPreview(float progress)
        {
            if (!IsScrubEnabled || !dragging)
            {
                return false;
            }

            SetProgressImmediate(progress, true);
            return true;
        }

        public bool EndScrubPreview(bool seek)
        {
            if (!dragging)
            {
                return false;
            }

            float targetProgress = normalizedProgress;
            dragging = false;
            pressed = false;
            activePointerId = -1;
            ApplyVisualState();
            if (seek && IsScrubEnabled)
            {
                SeekRequested?.Invoke(targetProgress);
            }

            return true;
        }

        public void CancelInteraction()
        {
            bool changed = dragging || pressed || activePointerId >= 0;
            dragging = false;
            pressed = false;
            activePointerId = -1;
            if (changed)
            {
                ApplyVisualState();
                PreviewCanceled?.Invoke();
            }
        }

        public bool SeekToNormalizedProgress(float progress)
        {
            return BeginScrubPreview(progress) &&
                   EndScrubPreview(true);
        }

        private void SetProgressImmediate(
            float progress,
            bool notifyPreview)
        {
            float clamped = Mathf.Clamp01(progress);
            if (Mathf.Approximately(normalizedProgress, clamped))
            {
                return;
            }

            normalizedProgress = clamped;
            UpdateProgressVisual();
            if (notifyPreview)
            {
                PreviewChanged?.Invoke(normalizedProgress);
            }
        }

        private void UpdateProgressVisual()
        {
            fill.style.width = Length.Percent(
                normalizedProgress * 100f);
        }

        private void ApplyVisualState()
        {
            var state = new DeucarianScrubberVisualState(
                IsScrubEnabled,
                hovered,
                pressed,
                dragging);
            if (stateAnimation == null)
            {
                DeucarianControlIslandTheme.ApplyScrubber(
                    this,
                    track,
                    fill,
                    handle,
                    theme,
                    state,
                    themeContext);
                return;
            }

            stateAnimation.SetState(
                DeucarianScrubberMetrics.Compact,
                DeucarianControlIslandTheme.ResolveScrubberPalette(
                    theme,
                    state,
                    themeContext),
                state,
                DeucarianGlassPanelStyle.ResolveStyle(
                    theme,
                    themeContext),
                DeucarianControlIslandStyle.DefaultVerticalPadding,
                animationPolicy != null
                    ? animationPolicy()
                    : Application.isPlaying);
        }

        private void OnPointerEnter(PointerEnterEvent _)
        {
            hovered = true;
            ApplyVisualState();
        }

        private void OnPointerLeave(PointerLeaveEvent _)
        {
            hovered = false;
            if (dragging)
            {
                CancelInteraction();
            }
            else
            {
                pressed = false;
            }

            ApplyVisualState();
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0 ||
                !BeginScrubPreview(
                    ResolveNormalizedProgress(
                        evt.localPosition.x)))
            {
                return;
            }

            activePointerId = evt.pointerId;
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!dragging || evt.pointerId != activePointerId)
            {
                return;
            }

            UpdateScrubPreview(
                ResolveNormalizedProgress(evt.localPosition.x));
            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!dragging || evt.pointerId != activePointerId)
            {
                return;
            }

            UpdateScrubPreview(
                ResolveNormalizedProgress(evt.localPosition.x));
            EndScrubPreview(true);
            evt.StopPropagation();
        }

        private void OnPointerCancel(PointerCancelEvent evt)
        {
            if (evt.pointerId == activePointerId)
            {
                CancelInteraction();
                evt.StopPropagation();
            }
        }

        private float ResolveNormalizedProgress(float localX)
        {
            float trackLeft = track.layout.x;
            float trackWidth = track.resolvedStyle.width;
            if (!(trackWidth > 0.001f))
            {
                trackWidth = track.layout.width;
            }

            if (!(trackWidth > 0.001f))
            {
                trackWidth = Mathf.Max(1f, resolvedStyle.width);
            }

            return Mathf.Clamp01(
                (localX - trackLeft) / trackWidth);
        }
    }
}
