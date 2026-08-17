using Deucarian.Theming;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Deucarian.UI
{
    /// <summary>
    /// Resolves canonical control-island presentation exclusively from built-in
    /// Deucarian theme roles.
    /// </summary>
    public static class DeucarianControlIslandTheme
    {
        public static void ApplyIconButtonState(
            Button button,
            VisualElement icon,
            DeucarianTheme theme,
            DeucarianIconButtonVisualState state,
            Object context = null)
        {
            DeucarianControlIslandElementStyle.ApplyStateClasses(
                button,
                state);
            DeucarianIconButtonStyle.ApplyState(
                button,
                icon,
                ResolveButtonPalette(theme, context),
                state,
                DeucarianGlassPanelStyle.ResolveStyle(theme, context));
        }

        public static void ApplyIconButtonState(
            Button button,
            DeucarianTheme theme,
            DeucarianIconButtonVisualState state,
            Object context = null)
        {
            DeucarianControlIslandElementStyle.ApplyStateClasses(
                button,
                state);
            DeucarianIconButtonStyle.ApplyButtonState(
                button,
                ResolveButtonPalette(theme, context),
                state,
                DeucarianGlassPanelStyle.ResolveStyle(theme, context));
        }

        public static void ApplyAnimatedIconButtonState(
            DeucarianAnimatedIconButton animation,
            Button button,
            DeucarianTheme theme,
            DeucarianIconButtonVisualState state,
            bool animate,
            Object context = null)
        {
            DeucarianControlIslandElementStyle.ApplyStateClasses(
                button,
                state);
            animation?.SetState(
                ResolveButtonPalette(theme, context),
                state,
                DeucarianGlassPanelStyle.ResolveStyle(theme, context),
                animate);
        }

        public static void ApplyScrubber(
            VisualElement scrubber,
            VisualElement track,
            VisualElement fill,
            VisualElement handle,
            DeucarianTheme theme,
            DeucarianScrubberVisualState state,
            Object context = null)
        {
            DeucarianControlIslandElementStyle.AddScrubberClasses(
                scrubber,
                track,
                fill,
                handle);
            DeucarianScrubberStyle.Apply(
                scrubber,
                track,
                fill,
                handle,
                DeucarianScrubberMetrics.Compact,
                ResolveScrubberPalette(theme, state, context),
                state,
                DeucarianGlassPanelStyle.ResolveStyle(theme, context),
                DeucarianControlIslandStyle.DefaultVerticalPadding);
        }

        public static Color ResolvePanelBackground(
            DeucarianTheme theme,
            Object context = null)
        {
            return ResolveColor(
                ResolveTheme(theme, context),
                DeucarianBuiltinColorRoleIds.SurfaceRaised,
                new Color(0.11f, 0.14f, 0.18f, 0.94f));
        }

        public static Color ResolveGlassPanelBackground(
            DeucarianTheme theme,
            Object context = null)
        {
            return DeucarianGlassPanelStyle.ResolveUIToolkitBackground(
                theme,
                context);
        }

        public static Color ResolveTextColor(
            DeucarianTheme theme,
            Object context = null)
        {
            return ResolveColor(
                ResolveTheme(theme, context),
                DeucarianBuiltinColorRoleIds.TextPrimary,
                Color.white);
        }

        public static Color ResolveMutedTextColor(
            DeucarianTheme theme,
            Object context = null)
        {
            return ResolveInactiveColor(
                ResolveTheme(theme, context),
                new Color(0.4f, 0.4f, 0.4f, 1f));
        }

        public static Color ResolveButtonBackground(
            DeucarianTheme theme,
            DeucarianIconButtonVisualState state,
            Object context = null)
        {
            return ResolveButtonPalette(theme, context)
                .ResolveBackground(state);
        }

        public static Color ResolveIconTint(
            DeucarianTheme theme,
            DeucarianIconButtonVisualState state,
            Object context = null)
        {
            return ResolveButtonPalette(theme, context)
                .ResolveIcon(state);
        }

        public static DeucarianIconButtonPalette ResolveButtonPalette(
            DeucarianTheme theme,
            Object context = null)
        {
            DeucarianTheme resolvedTheme = ResolveTheme(theme, context);
            Color normal = ResolveColor(
                resolvedTheme,
                DeucarianBuiltinColorRoleIds.UiNormal,
                Color.clear);
            Color secondary = ResolveInactiveColor(
                resolvedTheme,
                new Color(0.4f, 0.4f, 0.4f, 1f));
            Color selected = ResolveActiveColor(
                resolvedTheme,
                new Color(0.769f, 0.631f, 0.976f, 1f));
            Color text = ResolveColor(
                resolvedTheme,
                DeucarianBuiltinColorRoleIds.TextPrimary,
                Color.white);
            return new DeucarianIconButtonPalette(
                normal,
                ResolveColor(
                    resolvedTheme,
                    DeucarianBuiltinColorRoleIds.UiHighlighted,
                    new Color(1f, 1f, 1f, 0.12f)),
                ResolveColor(
                    resolvedTheme,
                    DeucarianBuiltinColorRoleIds.UiPressed,
                    new Color(1f, 1f, 1f, 0.2f)),
                selected,
                normal,
                text,
                secondary,
                ResolveColor(
                    resolvedTheme,
                    DeucarianBuiltinColorRoleIds.Primary,
                    text),
                text,
                ResolveColor(
                    resolvedTheme,
                    DeucarianBuiltinColorRoleIds.TextDisabled,
                    new Color(0.6f, 0.6f, 0.6f, 1f)),
                secondary,
                ResolveColor(
                    resolvedTheme,
                    DeucarianBuiltinColorRoleIds.UiFocused,
                    selected));
        }

        public static DeucarianScrubberPalette ResolveScrubberPalette(
            DeucarianTheme theme,
            DeucarianScrubberVisualState state,
            Object context = null)
        {
            DeucarianTheme resolvedTheme = ResolveTheme(theme, context);
            Color well = ResolveGlassPanelBackground(
                resolvedTheme,
                context);
            well.a = DeucarianScrubberStyle.ResolveDefaultWellAlpha(
                well.a,
                state.Enabled);

            Color track = ResolveColor(
                resolvedTheme,
                DeucarianBuiltinColorRoleIds.UiNormal,
                Color.clear);
            track.a = state.Enabled
                ? DeucarianScrubberStyle.DefaultTrackAlphaEnabled
                : DeucarianScrubberStyle.DefaultTrackAlphaDisabled;

            Color fill = state.Enabled
                ? ResolveActiveColor(
                    resolvedTheme,
                    new Color(0.769f, 0.631f, 0.976f, 1f))
                : ResolveColor(
                    resolvedTheme,
                    DeucarianBuiltinColorRoleIds.UiNormal,
                    Color.clear);
            fill.a = !state.Enabled
                ? DeucarianScrubberStyle.DefaultFillAlphaDisabled
                : state.Active || state.Hovered
                    ? DeucarianScrubberStyle.DefaultFillAlphaHover
                    : DeucarianScrubberStyle.DefaultFillAlphaEnabled;

            Color handle = ResolveColor(
                resolvedTheme,
                state.Enabled
                    ? state.Hovered || state.Active
                        ? DeucarianBuiltinColorRoleIds.Primary
                        : DeucarianBuiltinColorRoleIds.TextPrimary
                    : DeucarianBuiltinColorRoleIds.TextDisabled,
                Color.white);
            handle.a = state.Enabled
                ? DeucarianScrubberStyle.DefaultHandleAlphaEnabled
                : DeucarianScrubberStyle.DefaultHandleAlphaDisabled;

            Color border = state.Enabled &&
                           (state.Hovered || state.Active)
                ? ResolveColor(
                    resolvedTheme,
                    DeucarianBuiltinColorRoleIds.UiFocused,
                    new Color(0.769f, 0.631f, 0.976f, 1f))
                : ResolveInactiveColor(
                    resolvedTheme,
                    new Color(0.4f, 0.4f, 0.4f, 1f));
            return new DeucarianScrubberPalette(
                well,
                track,
                fill,
                handle,
                border);
        }

        public static Color ResolveColor(
            DeucarianTheme theme,
            string roleId,
            Color fallback)
        {
            return theme != null &&
                   theme.TryGetColorById(roleId, out Color color)
                ? color
                : fallback;
        }

        private static Color ResolveActiveColor(
            DeucarianTheme theme,
            Color fallback)
        {
            if (theme != null &&
                theme.TryGetColorById(
                    DeucarianControlIslandColorRoleIds.Active,
                    out Color color))
            {
                return color;
            }

            return ResolveColor(
                theme,
                IsLightTheme(theme)
                    ? DeucarianBuiltinColorRoleIds.UiSelected
                    : DeucarianBuiltinColorRoleIds.Accent,
                fallback);
        }

        private static Color ResolveInactiveColor(
            DeucarianTheme theme,
            Color fallback)
        {
            if (theme != null &&
                theme.TryGetColorById(
                    DeucarianControlIslandColorRoleIds.Inactive,
                    out Color color))
            {
                return color;
            }

            return ResolveColor(
                theme,
                IsLightTheme(theme)
                    ? DeucarianBuiltinColorRoleIds.TextSecondary
                    : DeucarianBuiltinColorRoleIds.TextMuted,
                fallback);
        }

        private static bool IsLightTheme(DeucarianTheme theme)
        {
            return theme != null &&
                   theme.ColorPalette != null &&
                   theme.ColorPalette.HasThemeMode &&
                   theme.ColorPalette.ThemeMode ==
                   DeucarianThemeMode.Light;
        }

        private static DeucarianTheme ResolveTheme(
            DeucarianTheme theme,
            Object context)
        {
            return DeucarianGlassPanelStyle.ResolveTheme(theme, context);
        }
    }
}
