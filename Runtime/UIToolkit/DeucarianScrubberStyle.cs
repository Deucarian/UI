using Deucarian.Theming;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.UI
{
    public readonly struct DeucarianScrubberVisualState
    {
        public DeucarianScrubberVisualState(
            bool enabled,
            bool hovered,
            bool pressed,
            bool dragging)
        {
            Enabled = enabled;
            Hovered = enabled && hovered;
            Pressed = enabled && pressed;
            Dragging = enabled && dragging;
        }

        public bool Enabled { get; }
        public bool Hovered { get; }
        public bool Pressed { get; }
        public bool Dragging { get; }
        public bool Active => Pressed || Dragging;
    }

    public readonly struct DeucarianScrubberMetrics
    {
        public DeucarianScrubberMetrics(
            float cornerRadius,
            float horizontalPadding,
            float verticalPadding,
            float trackHeight,
            float trackRadius,
            float handleSize,
            float handleHoverScale,
            float handlePressedScale,
            float borderWidth,
            float enabledOpacity,
            float disabledOpacity)
        {
            CornerRadius = Mathf.Max(0f, cornerRadius);
            HorizontalPadding = Mathf.Max(0f, horizontalPadding);
            VerticalPadding = Mathf.Max(0f, verticalPadding);
            TrackHeight = Mathf.Max(0f, trackHeight);
            TrackRadius = Mathf.Max(0f, trackRadius);
            HandleSize = Mathf.Max(0f, handleSize);
            HandleHoverScale = Mathf.Max(0f, handleHoverScale);
            HandlePressedScale = Mathf.Max(0f, handlePressedScale);
            BorderWidth = Mathf.Max(0f, borderWidth);
            EnabledOpacity = Mathf.Clamp01(enabledOpacity);
            DisabledOpacity = Mathf.Clamp01(disabledOpacity);
        }

        public float CornerRadius { get; }
        public float HorizontalPadding { get; }
        public float VerticalPadding { get; }
        public float TrackHeight { get; }
        public float TrackRadius { get; }
        public float HandleSize { get; }
        public float HandleHoverScale { get; }
        public float HandlePressedScale { get; }
        public float BorderWidth { get; }
        public float EnabledOpacity { get; }
        public float DisabledOpacity { get; }

        public static DeucarianScrubberMetrics Compact => new DeucarianScrubberMetrics(
            9f,
            8f,
            2f,
            5f,
            3f,
            14f,
            1.06f,
            1.12f,
            1f,
            1f,
            0.72f);
    }

    public readonly struct DeucarianScrubberPalette
    {
        public DeucarianScrubberPalette(
            Color well,
            Color track,
            Color fill,
            Color handle,
            Color border)
        {
            Well = well;
            Track = track;
            Fill = fill;
            Handle = handle;
            Border = border;
        }

        public Color Well { get; }
        public Color Track { get; }
        public Color Fill { get; }
        public Color Handle { get; }
        public Color Border { get; }
    }

    public static class DeucarianScrubberStyle
    {
        public const float DefaultWellAlphaEnabled = 0.5f;
        public const float DefaultWellAlphaDisabled = 0.34f;
        public const float DefaultTrackAlphaEnabled = 0.5f;
        public const float DefaultTrackAlphaDisabled = 0.32f;
        public const float DefaultFillAlphaEnabled = 0.82f;
        public const float DefaultFillAlphaHover = 0.92f;
        public const float DefaultFillAlphaDisabled = 0.4f;
        public const float DefaultHandleAlphaEnabled = 0.98f;
        public const float DefaultHandleAlphaDisabled = 0.62f;

        public static float ResolveDefaultWellAlpha(float sourceAlpha, bool enabled)
        {
            return enabled
                ? Mathf.Clamp(sourceAlpha * DefaultWellAlphaEnabled, 0.3f, 0.42f)
                : Mathf.Clamp(sourceAlpha * DefaultWellAlphaDisabled, 0.2f, 0.32f);
        }

        public static void Apply(
            VisualElement scrubber,
            VisualElement track,
            VisualElement fill,
            VisualElement handle,
            DeucarianScrubberMetrics metrics,
            DeucarianScrubberPalette palette,
            DeucarianScrubberVisualState state)
        {
            Apply(scrubber, track, fill, handle, metrics, palette, state, null, 0f);
        }

        /// <summary>
        /// Applies scrubber chrome using the supplied theme stroke and a concentric direct-child radius.
        /// </summary>
        public static void Apply(
            VisualElement scrubber,
            VisualElement track,
            VisualElement fill,
            VisualElement handle,
            DeucarianScrubberMetrics metrics,
            DeucarianScrubberPalette palette,
            DeucarianScrubberVisualState state,
            DeucarianThemeStyle style)
        {
            Apply(
                scrubber,
                track,
                fill,
                handle,
                metrics,
                palette,
                state,
                style,
                DeucarianControlIslandStyle.DefaultVerticalPadding);
        }

        /// <summary>
        /// Applies scrubber chrome using the supplied theme stroke and explicit container inset.
        /// </summary>
        public static void Apply(
            VisualElement scrubber,
            VisualElement track,
            VisualElement fill,
            VisualElement handle,
            DeucarianScrubberMetrics metrics,
            DeucarianScrubberPalette palette,
            DeucarianScrubberVisualState state,
            DeucarianThemeStyle style,
            float containerInset)
        {
            if (scrubber == null)
            {
                return;
            }

            scrubber.style.display = DisplayStyle.Flex;
            scrubber.style.flexDirection = FlexDirection.Row;
            scrubber.style.alignItems = Align.Center;
            scrubber.style.justifyContent = Justify.Center;
            scrubber.style.backgroundColor = palette.Well;
            scrubber.style.opacity = state.Enabled ? metrics.EnabledOpacity : metrics.DisabledOpacity;
            scrubber.style.paddingLeft = metrics.HorizontalPadding;
            scrubber.style.paddingRight = metrics.HorizontalPadding;
            scrubber.style.paddingTop = metrics.VerticalPadding;
            scrubber.style.paddingBottom = metrics.VerticalPadding;
            scrubber.style.scale = new Scale(Vector3.one);
            scrubber.style.overflow = Overflow.Visible;
            float scrubberRadius = style != null
                ? DeucarianControlIslandStyle.ResolveNestedCornerRadius(
                    style.CornerRadius,
                    containerInset)
                : metrics.CornerRadius;
            float borderWidth = style != null
                ? Mathf.Max(0f, style.BorderWidth)
                : metrics.BorderWidth;
            Color borderColor = style != null
                ? style.ResolveBorderColor(palette.Border)
                : palette.Border;
            ApplyCornerRadius(scrubber, scrubberRadius);
            ApplyElementBorder(scrubber, borderWidth, borderColor);

            ApplyTrack(track, metrics, palette);
            ApplyFill(fill, metrics, palette);
            ApplyHandle(handle, metrics, palette, state, borderWidth, borderColor);
        }

        private static void ApplyTrack(
            VisualElement track,
            DeucarianScrubberMetrics metrics,
            DeucarianScrubberPalette palette)
        {
            if (track == null)
            {
                return;
            }

            track.pickingMode = PickingMode.Ignore;
            track.style.position = Position.Relative;
            track.style.flexGrow = 1f;
            track.style.flexShrink = 1f;
            track.style.width = Length.Percent(100f);
            track.style.height = metrics.TrackHeight;
            track.style.minHeight = metrics.TrackHeight;
            track.style.maxHeight = metrics.TrackHeight;
            track.style.backgroundColor = palette.Track;
            track.style.overflow = Overflow.Visible;
            ApplyCornerRadius(track, metrics.TrackRadius);
            ApplyElementBorder(track, 0f, Color.clear);
        }

        private static void ApplyFill(
            VisualElement fill,
            DeucarianScrubberMetrics metrics,
            DeucarianScrubberPalette palette)
        {
            if (fill == null)
            {
                return;
            }

            fill.pickingMode = PickingMode.Ignore;
            fill.style.position = Position.Absolute;
            fill.style.left = 0f;
            fill.style.top = 0f;
            fill.style.bottom = 0f;
            fill.style.height = metrics.TrackHeight;
            fill.style.minHeight = metrics.TrackHeight;
            fill.style.maxHeight = metrics.TrackHeight;
            fill.style.backgroundColor = palette.Fill;
            fill.style.overflow = Overflow.Visible;
            ApplyCornerRadius(fill, metrics.TrackRadius);
            ApplyElementBorder(fill, 0f, Color.clear);
        }

        private static void ApplyHandle(
            VisualElement handle,
            DeucarianScrubberMetrics metrics,
            DeucarianScrubberPalette palette,
            DeucarianScrubberVisualState state,
            float borderWidth,
            Color borderColor)
        {
            if (handle == null)
            {
                return;
            }

            handle.pickingMode = PickingMode.Ignore;
            handle.style.position = Position.Absolute;
            handle.style.right = -metrics.HandleSize * 0.5f;
            handle.style.top = -(metrics.HandleSize - metrics.TrackHeight) * 0.5f;
            handle.style.width = metrics.HandleSize;
            handle.style.height = metrics.HandleSize;
            handle.style.minWidth = metrics.HandleSize;
            handle.style.minHeight = metrics.HandleSize;
            handle.style.maxWidth = metrics.HandleSize;
            handle.style.maxHeight = metrics.HandleSize;
            handle.style.backgroundColor = palette.Handle;

            float handleScale = state.Active
                ? metrics.HandlePressedScale
                : state.Hovered ? metrics.HandleHoverScale : 1f;
            handle.style.scale = new Scale(new Vector3(handleScale, handleScale, 1f));
            ApplyCornerRadius(handle, metrics.HandleSize * 0.5f);
            ApplyElementBorder(handle, borderWidth, borderColor);
        }

        private static void ApplyCornerRadius(VisualElement element, float radius)
        {
            element.style.borderTopLeftRadius = radius;
            element.style.borderTopRightRadius = radius;
            element.style.borderBottomLeftRadius = radius;
            element.style.borderBottomRightRadius = radius;
        }

        private static void ApplyElementBorder(VisualElement element, float width, Color color)
        {
            if (element == null)
            {
                return;
            }

            element.style.borderLeftWidth = width;
            element.style.borderRightWidth = width;
            element.style.borderTopWidth = width;
            element.style.borderBottomWidth = width;
            element.style.borderLeftColor = color;
            element.style.borderRightColor = color;
            element.style.borderTopColor = color;
            element.style.borderBottomColor = color;
        }
    }
}
