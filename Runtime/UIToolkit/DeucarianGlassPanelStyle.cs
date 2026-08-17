using Deucarian.Theming;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Deucarian.UI
{
    /// <summary>
    /// Resolves and applies the shared Deucarian glass treatment without
    /// requiring application-specific theme utilities.
    /// </summary>
    public static class DeucarianGlassPanelStyle
    {
        public const float FallbackBorderWidth = 1f;

        private static DeucarianThemeStyle fallbackStyle;

        public static void AddClass(VisualElement element) =>
            DeucarianUIToolkitGlassPanel.AddClass(element);

        public static void ApplyPanel(
            VisualElement element,
            DeucarianTheme theme = null,
            Object context = null,
            string surfaceRoleId = DeucarianBuiltinColorRoleIds.SurfaceRaised)
        {
            ApplyPanel(
                element,
                theme,
                ResolveStyle(theme, context),
                context,
                surfaceRoleId);
        }

        public static void ApplyPanel(
            VisualElement element,
            DeucarianTheme theme,
            DeucarianThemeStyle style,
            Object context = null,
            string surfaceRoleId = DeucarianBuiltinColorRoleIds.SurfaceRaised)
        {
            if (element == null)
            {
                return;
            }

            DeucarianTheme resolvedTheme = ResolveTheme(theme, context);
            DeucarianThemeStyle resolvedStyle =
                style ?? ResolveStyle(resolvedTheme, context);
            Color baseColor = ResolveColor(
                resolvedTheme,
                surfaceRoleId,
                new Color(0.11f, 0.14f, 0.18f, 0.94f));
            if (!DeucarianUIToolkitGlassPanel.Apply(
                    element,
                    resolvedTheme,
                    baseColor,
                    resolvedStyle))
            {
                element.style.backgroundColor = baseColor;
                ApplyBorder(
                    element,
                    ResolveBorder(baseColor, resolvedStyle),
                    resolvedStyle != null
                        ? resolvedStyle.BorderWidth
                        : FallbackBorderWidth);
            }
        }

        public static Color ResolveUIToolkitBackground(
            DeucarianTheme theme = null,
            Object context = null,
            string surfaceRoleId = DeucarianBuiltinColorRoleIds.SurfaceRaised)
        {
            DeucarianTheme resolvedTheme = ResolveTheme(theme, context);
            Color baseColor = ResolveColor(
                resolvedTheme,
                surfaceRoleId,
                new Color(0.11f, 0.14f, 0.18f, 0.94f));
            DeucarianThemeStyle style = ResolveStyle(resolvedTheme, context);
            return style != null
                ? style.ResolveSurfaceColor(baseColor)
                : baseColor;
        }

        public static Color ResolveCanvasBackground(
            Color semanticColor,
            DeucarianTheme theme = null,
            Object context = null)
        {
            DeucarianThemeStyle style = ResolveStyle(theme, context);
            return style != null
                ? style.ResolveSurfaceColor(semanticColor)
                : semanticColor;
        }

        public static Color ResolveBorder(
            DeucarianTheme theme = null,
            Object context = null,
            string surfaceRoleId = DeucarianBuiltinColorRoleIds.SurfaceRaised)
        {
            Color background = ResolveUIToolkitBackground(
                theme,
                context,
                surfaceRoleId);
            return ResolveBorder(
                background,
                ResolveStyle(theme, context));
        }

        public static Color ResolveBorder(
            Color background,
            DeucarianTheme theme = null,
            Object context = null)
        {
            return ResolveBorder(
                background,
                ResolveStyle(theme, context));
        }

        public static void ApplyCanvasRim(
            Outline outline,
            Color background,
            DeucarianTheme theme = null,
            Object context = null)
        {
            if (outline == null)
            {
                return;
            }

            DeucarianTheme resolvedTheme = ResolveTheme(theme, context);
            DeucarianThemeStyle style =
                ResolveStyle(resolvedTheme, context);
            if (!DeucarianUGUIGlassPanel.ApplyOutline(
                    outline,
                    background,
                    resolvedTheme,
                    style))
            {
                outline.effectColor = ResolveBorder(background, style);
                float width = style != null
                    ? style.BorderWidth
                    : FallbackBorderWidth;
                outline.effectDistance = new Vector2(width, -width);
                outline.useGraphicAlpha = false;
            }
        }

        public static DeucarianTheme ResolveTheme(
            DeucarianTheme theme,
            Object context = null)
        {
            if (theme != null)
            {
                return theme;
            }

            if (context is Component component)
            {
                DeucarianThemeProvider provider =
                    DeucarianThemeRuntimeResolver.FindProvider(component);
                if (provider != null && provider.CurrentTheme != null)
                {
                    return provider.CurrentTheme;
                }
            }

            return null;
        }

        public static DeucarianThemeStyle ResolveStyle(
            DeucarianTheme theme,
            Object context = null)
        {
            if (context is Component component)
            {
                DeucarianThemeProvider provider =
                    DeucarianThemeRuntimeResolver.FindProvider(component);
                if (provider != null && provider.CurrentStyle != null)
                {
                    return provider.CurrentStyle;
                }
            }

            DeucarianTheme resolvedTheme = ResolveTheme(theme, context);
            if (resolvedTheme != null && resolvedTheme.VisualStyle != null)
            {
                return resolvedTheme.VisualStyle;
            }

            if (fallbackStyle == null)
            {
                fallbackStyle = DeucarianThemeStylePresets.CreateRuntimeStyle(
                    DeucarianThemeStyleIds.FrostedGlass);
            }

            return fallbackStyle;
        }

        private static Color ResolveColor(
            DeucarianTheme theme,
            string roleId,
            Color fallback)
        {
            return theme != null &&
                   theme.TryGetColorById(roleId, out Color color)
                ? color
                : fallback;
        }

        private static Color ResolveBorder(
            Color background,
            DeucarianThemeStyle style)
        {
            return style != null
                ? style.ResolveBorderColor(background)
                : background;
        }

        private static void ApplyBorder(
            VisualElement element,
            Color color,
            float borderWidth)
        {
            float width = Mathf.Max(0f, borderWidth);
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
