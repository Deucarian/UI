using Deucarian.Theming;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Deucarian.UI
{
    /// <summary>
    /// Public facade for the complete reusable control-island composition.
    /// Consumers provide feature behavior while this type owns layout, chrome,
    /// theming, icon placement, and scrubber presentation.
    /// </summary>
    public static class DeucarianControlIslandVisualStyle
    {
        public const string ToolbarClass =
            DeucarianControlIslandElementStyle.ToolbarClass;
        public const string GlassPanelClass =
            DeucarianControlIslandElementStyle.GlassPanelClass;
        public const string IconButtonClass =
            DeucarianControlIslandElementStyle.IconButtonClass;
        public const string IconClass =
            DeucarianControlIslandElementStyle.IconClass;
        public const string TimelineScrubberClass =
            DeucarianControlIslandElementStyle.ScrubberClass;
        public const string TimelineScrubberTrackClass =
            DeucarianControlIslandElementStyle.ScrubberTrackClass;
        public const string TimelineScrubberFillClass =
            DeucarianControlIslandElementStyle.ScrubberFillClass;
        public const string TimelineScrubberHandleClass =
            DeucarianControlIslandElementStyle.ScrubberHandleClass;
        public const string ActiveClass =
            DeucarianControlIslandElementStyle.ActiveClass;
        public const string InactiveClass =
            DeucarianControlIslandElementStyle.InactiveClass;
        public const string DisabledClass =
            DeucarianControlIslandElementStyle.DisabledClass;
        public const string FocusedClass =
            DeucarianControlIslandElementStyle.FocusedClass;

        public const float ControlIslandRowHeight =
            DeucarianControlIslandStyle.DefaultRowHeight;
        public const float ControlIslandButtonSize =
            DeucarianControlIslandStyle.DefaultButtonSize;
        public const float ControlIslandIconSize =
            DeucarianControlIslandStyle.DefaultIconSize;
        public const float ControlIslandButtonMargin =
            DeucarianControlIslandStyle.DefaultButtonMargin;
        public const float ControlIslandCornerRadius =
            DeucarianControlIslandStyle.DefaultPanelCornerRadius;
        public const float ControlIslandButtonCornerRadius =
            DeucarianControlIslandStyle.DefaultButtonCornerRadius;
        public const float ControlIslandHorizontalPadding =
            DeucarianControlIslandStyle.DefaultHorizontalPadding;
        public const float ControlIslandVerticalPadding =
            DeucarianControlIslandStyle.DefaultVerticalPadding;
        public const float ControlIslandRowGap =
            DeucarianControlIslandStyle.DefaultRowGap;
        public const float ControlIslandBottomOffset =
            DeucarianControlIslandStyle.DefaultBottomOffset;
        public const float ControlIslandStatusWidth =
            DeucarianControlIslandStyle.DefaultStatusWidth;
        public const float ControlIslandStatusHeight =
            DeucarianControlIslandStyle.DefaultStatusHeight;
        public const float ControlIslandStatusFontSize =
            DeucarianControlIslandStyle.DefaultStatusFontSize;
        public const float VideoScrubberWidth =
            DeucarianControlIslandStyle.DefaultCompactScrubberWidth;
        public const float VideoScrubberHorizontalMargin =
            DeucarianControlIslandStyle
                .DefaultCompactScrubberHorizontalMargin;
        public const float VideoScrubberChromeInset =
            DeucarianControlIslandStyle
                .DefaultCompactScrubberChromeInset;
        public const float VideoScrubberHeight =
            ControlIslandButtonSize - VideoScrubberChromeInset * 2f;

        public static DeucarianPanelChrome CompactPanel =>
            DeucarianControlIslandStyle.CompactPanel;
        public static DeucarianIconButtonChrome CompactIconButton =>
            DeucarianControlIslandStyle.RoundedSquareButton;
        public static DeucarianPanelChrome VideoControlsPanel =>
            CompactPanel;
        public static DeucarianIconButtonChrome VideoControlsButton =>
            CompactIconButton;

        public static DeucarianControlIslandPresentation
            ResolveControlIslandPresentation(
                DeucarianTheme theme,
                Object context = null)
        {
            return DeucarianControlIslandPresentation.Resolve(
                theme,
                context);
        }

        public static void ApplyControlIslandRoot(
            VisualElement root,
            DeucarianControlIslandRow row)
        {
            ApplyControlIslandRoot(
                root,
                row,
                DeucarianControlIslandPresentation.Resolve());
        }

        public static void ApplyControlIslandRoot(
            VisualElement root,
            DeucarianControlIslandRow row,
            DeucarianControlIslandPresentation presentation)
        {
            DeucarianControlIslandElementStyle.ApplyRoot(
                root,
                row,
                presentation);
        }

        public static float ResolveControlIslandBottomPadding(
            DeucarianControlIslandRow row)
        {
            return ResolveControlIslandBottomPadding(
                row,
                DeucarianControlIslandPresentation.Resolve());
        }

        public static float ResolveControlIslandBottomPadding(
            DeucarianControlIslandRow row,
            DeucarianControlIslandPresentation presentation)
        {
            return DeucarianControlIslandElementStyle.ResolveBottomPadding(
                row,
                presentation);
        }

        public static float ResolveControlIslandStatusBottomPadding(
            DeucarianControlIslandRow row)
        {
            return ResolveControlIslandStatusBottomPadding(
                row,
                DeucarianControlIslandPresentation.Resolve());
        }

        public static float ResolveControlIslandStatusBottomPadding(
            DeucarianControlIslandRow row,
            DeucarianControlIslandPresentation presentation)
        {
            return DeucarianControlIslandElementStyle
                .ResolveStatusBottomPadding(row, presentation);
        }

        public static float CalculateIslandPanelWidth(int buttonCount) =>
            DeucarianControlIslandStyle.CalculatePanelWidth(
                CompactPanel,
                CompactIconButton,
                buttonCount);

        public static float CalculateIslandPanelWidth(
            DeucarianControlIslandPresentation presentation,
            int buttonCount) =>
            presentation.Profile.CalculatePanelWidth(buttonCount);

        public static float CalculateVideoControlsPanelWidth() =>
            DeucarianControlIslandStyle.CalculateControlRowWidth(
                CompactPanel,
                CompactIconButton,
                2,
                VideoScrubberWidth,
                VideoScrubberHorizontalMargin);

        public static float CalculateVideoControlsPanelWidth(
            DeucarianControlIslandPresentation presentation) =>
            presentation.Profile.CalculatePanelWidth(2, 1);

        public static void AddToolbarClasses(VisualElement panel) =>
            DeucarianControlIslandElementStyle.AddToolbarClasses(panel);

        public static void AddIconButtonClasses(Button button) =>
            DeucarianControlIslandElementStyle
                .AddIconButtonClasses(button);

        public static void AddIconClasses(VisualElement icon) =>
            DeucarianControlIslandElementStyle.AddIconClasses(icon);

        public static void ApplyToolbarPanel(
            VisualElement panel,
            DeucarianTheme theme,
            DeucarianPanelChrome panelStyle,
            Object context = null)
        {
            DeucarianControlIslandElementStyle.ApplyPanel(
                panel,
                theme,
                panelStyle,
                context);
        }

        public static void ApplyToolbarPanel(
            VisualElement panel,
            DeucarianTheme theme,
            DeucarianPanelChrome panelStyle,
            DeucarianControlIslandPresentation presentation,
            Object context = null)
        {
            if (panel == null)
            {
                return;
            }

            DeucarianControlIslandElementStyle.AddToolbarClasses(panel);
            DeucarianControlIslandStyle.ApplyPanel(
                panel,
                panelStyle,
                presentation.Style);
            DeucarianGlassPanelStyle.ApplyPanel(
                panel,
                theme ?? presentation.Theme,
                presentation.Style,
                context);
            panel.style.scale = new Scale(Vector3.one);
        }

        public static float CalculatePanelWidth(
            DeucarianPanelChrome panelStyle,
            DeucarianIconButtonChrome buttonStyle,
            int buttonCount) =>
            DeucarianControlIslandStyle.CalculatePanelWidth(
                panelStyle,
                buttonStyle,
                buttonCount);

        public static void ApplyIconButtonLayout(
            Button button,
            DeucarianIconButtonChrome buttonStyle)
        {
            DeucarianControlIslandElementStyle.ApplyIconButtonLayout(
                button,
                buttonStyle);
        }

        public static void ApplyIconButtonLayout(
            Button button,
            DeucarianIconButtonChrome buttonStyle,
            DeucarianTheme theme,
            Object context = null)
        {
            DeucarianControlIslandElementStyle.ApplyIconButtonLayout(
                button,
                buttonStyle,
                DeucarianGlassPanelStyle.ResolveStyle(theme, context));
        }

        public static void ApplyIconButtonLayout(
            Button button,
            DeucarianIconButtonChrome buttonStyle,
            DeucarianControlIslandPresentation presentation)
        {
            DeucarianControlIslandElementStyle.ApplyIconButtonLayout(
                button,
                buttonStyle,
                presentation.Style,
                presentation.Profile.VerticalPadding);
        }

        public static void ApplyIconLayout(
            VisualElement icon,
            DeucarianIconButtonChrome buttonStyle) =>
            DeucarianControlIslandElementStyle.ApplyIconLayout(
                icon,
                buttonStyle);

        /// <summary>
        /// Canonical centered icon layout. Unlike the retired consumer helper,
        /// this uses Position.Absolute so sibling state icons overlap.
        /// </summary>
        public static void ApplyCenteredIconLayout(VisualElement icon)
        {
            DeucarianControlIslandPresentation presentation =
                DeucarianControlIslandPresentation.Resolve();
            ApplyCenteredOverlayIconLayout(icon, presentation);
        }

        public static void ApplyCenteredIconLayout(
            VisualElement icon,
            DeucarianIconButtonChrome buttonStyle)
        {
            var overlayStyle = new DeucarianIconButtonChrome(
                buttonStyle.Size,
                buttonStyle.CornerRadius,
                buttonStyle.IconSize,
                buttonStyle.HorizontalMargin,
                true);
            DeucarianControlIslandElementStyle.ApplyIconLayout(
                icon,
                overlayStyle);
        }

        public static void ApplyCenteredOverlayIconLayout(
            VisualElement icon,
            DeucarianControlIslandPresentation presentation) =>
            DeucarianControlIslandElementStyle
                .ApplyCenteredOverlayIconLayout(icon, presentation);

        public static void SetIconTexture(
            VisualElement icon,
            Texture2D texture) =>
            DeucarianControlIslandElementStyle.SetIconTexture(
                icon,
                texture);

        public static void ApplyIconButtonState(
            Button button,
            VisualElement icon,
            DeucarianTheme theme,
            DeucarianIconButtonVisualState state,
            Object context = null) =>
            DeucarianControlIslandTheme.ApplyIconButtonState(
                button,
                icon,
                theme,
                state,
                context);

        public static void ApplyIconButtonState(
            Button button,
            DeucarianTheme theme,
            DeucarianIconButtonVisualState state,
            Object context = null) =>
            DeucarianControlIslandTheme.ApplyIconButtonState(
                button,
                theme,
                state,
                context);

        public static void ApplyAnimatedIconButtonState(
            DeucarianAnimatedIconButton animation,
            Button button,
            DeucarianTheme theme,
            DeucarianIconButtonVisualState state,
            bool animate,
            Object context = null) =>
            DeucarianControlIslandTheme.ApplyAnimatedIconButtonState(
                animation,
                button,
                theme,
                state,
                animate,
                context);

        public static void ApplyTimelineScrubber(
            DeucarianTimelineScrubber scrubber,
            VisualElement track,
            VisualElement fill,
            VisualElement handle,
            DeucarianTheme theme,
            DeucarianScrubberVisualState state,
            Object context = null) =>
            DeucarianControlIslandTheme.ApplyScrubber(
                scrubber,
                track,
                fill,
                handle,
                theme,
                state,
                context);

        public static void ApplyCompactScrubber(
            DeucarianTimelineScrubber scrubber,
            DeucarianControlIslandPresentation presentation) =>
            DeucarianControlIslandElementStyle.ApplyCompactScrubber(
                scrubber,
                presentation);

        public static Color ResolvePanelBackground(
            DeucarianTheme theme,
            Object context = null) =>
            DeucarianControlIslandTheme.ResolvePanelBackground(
                theme,
                context);

        public static Color ResolveGlassPanelBackground(
            DeucarianTheme theme,
            Object context = null) =>
            DeucarianControlIslandTheme.ResolveGlassPanelBackground(
                theme,
                context);

        public static Color ResolveTextColor(
            DeucarianTheme theme,
            Object context = null) =>
            DeucarianControlIslandTheme.ResolveTextColor(theme, context);

        public static Color ResolveMutedTextColor(
            DeucarianTheme theme,
            Object context = null) =>
            DeucarianControlIslandTheme.ResolveMutedTextColor(
                theme,
                context);

        public static Color ResolveButtonBackground(
            DeucarianTheme theme,
            DeucarianIconButtonVisualState state,
            Object context = null) =>
            DeucarianControlIslandTheme.ResolveButtonBackground(
                theme,
                state,
                context);

        public static Color ResolveIconTint(
            DeucarianTheme theme,
            DeucarianIconButtonVisualState state,
            Object context = null) =>
            DeucarianControlIslandTheme.ResolveIconTint(
                theme,
                state,
                context);

        public static DeucarianIconButtonPalette ResolveButtonPalette(
            DeucarianTheme theme,
            Object context = null) =>
            DeucarianControlIslandTheme.ResolveButtonPalette(
                theme,
                context);

        public static DeucarianScrubberPalette ResolveScrubberPalette(
            DeucarianTheme theme,
            DeucarianScrubberVisualState state,
            Object context = null) =>
            DeucarianControlIslandTheme.ResolveScrubberPalette(
                theme,
                state,
                context);
    }
}
