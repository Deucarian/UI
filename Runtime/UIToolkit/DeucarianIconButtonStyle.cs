using Deucarian.Theming;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.UI
{
    public readonly struct DeucarianIconButtonVisualState
    {
        public DeucarianIconButtonVisualState(
            bool visible,
            bool enabled,
            bool selected,
            bool hovered,
            bool pressed,
            bool focused)
        {
            Visible = visible;
            Enabled = visible && enabled;
            Selected = Visible && Enabled && selected;
            Hovered = Visible && Enabled && hovered;
            Pressed = Visible && Enabled && pressed;
            Focused = Visible && Enabled && focused;
        }

        public bool Visible { get; }
        public bool Enabled { get; }
        public bool Selected { get; }
        public bool Hovered { get; }
        public bool Pressed { get; }
        public bool Focused { get; }
        public bool Active => Enabled && (Selected || Pressed);
        public bool Inactive => Enabled && !Active;
        public bool Disabled => Visible && !Enabled;
    }

    public readonly struct DeucarianIconButtonPalette
    {
        public DeucarianIconButtonPalette(
            Color background,
            Color backgroundHover,
            Color backgroundPressed,
            Color backgroundSelected,
            Color backgroundDisabled,
            Color text,
            Color icon,
            Color iconHover,
            Color iconActive,
            Color iconDisabled,
            Color border,
            Color borderActive,
            bool autoContrast = false,
            Color backingSurface = default)
        {
            Background = background;
            BackgroundHover = backgroundHover;
            BackgroundPressed = backgroundPressed;
            BackgroundSelected = backgroundSelected;
            BackgroundDisabled = backgroundDisabled;
            Text = text;
            Icon = icon;
            IconHover = iconHover;
            IconActive = iconActive;
            IconDisabled = iconDisabled;
            Border = border;
            BorderActive = borderActive;
            AutoContrast = autoContrast;
            BackingSurface = backingSurface;
        }

        public Color Background { get; }
        public Color BackgroundHover { get; }
        public Color BackgroundPressed { get; }
        public Color BackgroundSelected { get; }
        public Color BackgroundDisabled { get; }
        public Color Text { get; }
        public Color Icon { get; }
        public Color IconHover { get; }
        public Color IconActive { get; }
        public Color IconDisabled { get; }
        public Color Border { get; }
        public Color BorderActive { get; }
        public bool AutoContrast { get; }
        public Color BackingSurface { get; }

        public Color ResolveBackground(DeucarianIconButtonVisualState state)
        {
            Color background = ResolveStateBackground(state);
            return AutoContrast ? DeucarianForegroundContrast.Composite(background, BackingSurface) : background;
        }

        private Color ResolveStateBackground(DeucarianIconButtonVisualState state)
        {
            if (!state.Visible || !state.Enabled)
            {
                return BackgroundDisabled;
            }

            if (state.Pressed)
            {
                return BackgroundPressed;
            }

            if (state.Selected)
            {
                return BackgroundSelected;
            }

            return state.Hovered ? BackgroundHover : Background;
        }

        public Color ResolveIcon(DeucarianIconButtonVisualState state)
        {
            Color preferred = ResolveStateIcon(state);
            return AutoContrast && state.Enabled
                ? DeucarianForegroundContrast.Resolve(preferred, ResolveBackground(state), DeucarianForegroundContrast.IconMinimum)
                : preferred;
        }

        private Color ResolveStateIcon(DeucarianIconButtonVisualState state)
        {
            if (!state.Visible || !state.Enabled)
            {
                return IconDisabled;
            }

            if (state.Pressed || state.Selected)
            {
                return IconActive;
            }

            return state.Hovered ? IconHover : Icon;
        }

        public Color ResolveBorder(DeucarianIconButtonVisualState state)
        {
            return state.Active || state.Focused ? BorderActive : Border;
        }
    }

    public readonly struct DeucarianIconButtonPresentation
    {
        public DeucarianIconButtonPresentation(
            bool visible,
            float opacity,
            Color background,
            Color text,
            Color icon,
            Color border,
            float borderWidth,
            Vector3 buttonScale,
            Vector3 iconScale,
            bool autoContrast = false)
        {
            Visible = visible;
            Opacity = Mathf.Clamp01(opacity);
            Background = background;
            Text = autoContrast ? DeucarianForegroundContrast.Resolve(text, background) : text;
            Icon = autoContrast ? DeucarianForegroundContrast.Resolve(icon, background, DeucarianForegroundContrast.IconMinimum) : icon;
            AutoContrast = autoContrast;
            Border = border;
            BorderWidth = Mathf.Max(0f, borderWidth);
            ButtonScale = buttonScale;
            IconScale = iconScale;
        }

        public bool Visible { get; }
        public float Opacity { get; }
        public Color Background { get; }
        public Color Text { get; }
        public Color Icon { get; }
        public Color Border { get; }
        public float BorderWidth { get; }
        public Vector3 ButtonScale { get; }
        public Vector3 IconScale { get; }
        public bool AutoContrast { get; }

        public static DeucarianIconButtonPresentation Lerp(
            DeucarianIconButtonPresentation from,
            DeucarianIconButtonPresentation to,
            float progress)
        {
            float t = Mathf.Clamp01(progress);
            return new DeucarianIconButtonPresentation(
                to.Visible,
                Mathf.Lerp(from.Opacity, to.Opacity, t),
                Color.Lerp(from.Background, to.Background, t),
                to.AutoContrast ? to.Text : Color.Lerp(from.Text, to.Text, t),
                to.AutoContrast ? to.Icon : Color.Lerp(from.Icon, to.Icon, t),
                Color.Lerp(from.Border, to.Border, t),
                Mathf.Lerp(from.BorderWidth, to.BorderWidth, t),
                Vector3.Lerp(from.ButtonScale, to.ButtonScale, t),
                Vector3.Lerp(from.IconScale, to.IconScale, t),
                to.AutoContrast);
        }
    }

    public static class DeucarianIconButtonStyle
    {
        public const float ActiveBorderWidth = 1f;
        public const float DisabledBorderWidth = 1f;
        public const float NoBorderWidth = 0f;

        public static void ApplyState(
            Button button,
            VisualElement icon,
            DeucarianIconButtonPalette palette,
            DeucarianIconButtonVisualState state)
        {
            ApplyState(button, icon, palette, state, null);
        }

        /// <summary>
        /// Applies icon-button state while resolving outlined states from the supplied theme style.
        /// </summary>
        public static void ApplyState(
            Button button,
            VisualElement icon,
            DeucarianIconButtonPalette palette,
            DeucarianIconButtonVisualState state,
            DeucarianThemeStyle style)
        {
            ApplyButtonState(button, palette, state, style);
            ApplyIconState(icon, palette, state);
        }

        public static void ApplyButtonState(
            Button button,
            DeucarianIconButtonPalette palette,
            DeucarianIconButtonVisualState state)
        {
            ApplyButtonState(button, palette, state, null);
        }

        /// <summary>
        /// Applies button state while resolving active, focused, and disabled outlines from the supplied theme style.
        /// </summary>
        public static void ApplyButtonState(
            Button button,
            DeucarianIconButtonPalette palette,
            DeucarianIconButtonVisualState state,
            DeucarianThemeStyle style)
        {
            ApplyButtonPresentation(
                button,
                ResolvePresentation(palette, state, style));
        }

        public static void ApplyIconState(
            VisualElement icon,
            DeucarianIconButtonPalette palette,
            DeucarianIconButtonVisualState state)
        {
            ApplyIconPresentation(
                icon,
                ResolvePresentation(palette, state, null));
        }

        public static DeucarianIconButtonPresentation ResolvePresentation(
            DeucarianIconButtonPalette palette,
            DeucarianIconButtonVisualState state,
            DeucarianThemeStyle style = null)
        {
            return ResolvePresentation(palette, state, style, false);
        }

        public static DeucarianIconButtonPresentation ResolvePresentation(
            DeucarianIconButtonPalette palette,
            DeucarianIconButtonVisualState state,
            DeucarianThemeStyle style,
            bool preservePressedScale)
        {
            // Contained controls keep a fixed silhouette in every interaction state.
            var scaleState = state;
            bool outlined = state.Active || state.Focused || state.Disabled;
            float borderWidth = style != null
                ? outlined ? Mathf.Max(0f, style.BorderWidth) : NoBorderWidth
                : state.Active || state.Focused
                    ? ActiveBorderWidth
                    : state.Disabled ? DisabledBorderWidth : NoBorderWidth;
            Color paletteBorder = palette.ResolveBorder(state);
            Color borderColor = style != null
                ? style.ResolveBorderColor(paletteBorder)
                : paletteBorder;
            return new DeucarianIconButtonPresentation(
                state.Visible,
                state.Visible ? 1f : 0f,
                palette.ResolveBackground(state),
                palette.Text,
                palette.ResolveIcon(state),
                preservePressedScale && !state.Focused ? Color.clear : borderColor,
                preservePressedScale ? (style != null ? style.BorderWidth : ActiveBorderWidth) : borderWidth,
                preservePressedScale ? Vector3.one : ResolveButtonScale(scaleState),
                preservePressedScale ? Vector3.one : ResolveIconScale(scaleState),
                palette.AutoContrast && state.Enabled);
        }

        public static void ApplyPresentation(
            Button button,
            VisualElement icon,
            DeucarianIconButtonPresentation presentation,
            bool keepDisplayed = false,
            bool manageIconVisibility = true,
            bool manageButtonScale = true,
            bool manageIconScale = true)
        {
            ApplyButtonPresentation(
                button,
                presentation,
                keepDisplayed,
                manageButtonScale);
            ApplyIconPresentation(
                icon,
                presentation,
                keepDisplayed,
                manageIconVisibility,
                manageIconScale);
        }

        public static void ApplyButtonPresentation(
            Button button,
            DeucarianIconButtonPresentation presentation,
            bool keepDisplayed = false,
            bool manageScale = true)
        {
            if (button == null)
            {
                return;
            }

            button.style.display = presentation.Visible || keepDisplayed
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            button.style.opacity = presentation.Opacity;
            button.style.backgroundColor = presentation.Background;
            if (button.ClassListContains(DeucarianControlIslandElementStyle.IconButtonClass))
            {
                button.style.backgroundImage = StyleKeyword.None;
                button.style.translate = new Translate(0f, 0f, 0f);
            }
            button.style.color = presentation.Text;
            if (manageScale)
            {
                button.style.scale = new Scale(presentation.ButtonScale);
            }
            SetBorder(
                button,
                presentation.BorderWidth,
                presentation.Border);
        }

        public static void ApplyIconPresentation(
            VisualElement icon,
            DeucarianIconButtonPresentation presentation,
            bool keepDisplayed = false,
            bool manageVisibility = true,
            bool manageScale = true)
        {
            if (icon == null)
            {
                return;
            }

            icon.style.unityBackgroundImageTintColor = presentation.Icon;
            if (icon is DeucarianChevronIcon) icon.MarkDirtyRepaint();
            if (manageVisibility)
            {
                icon.style.opacity = presentation.Opacity;
                icon.style.display = presentation.Visible || keepDisplayed
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
            }

            if (manageScale)
            {
                icon.style.scale = new Scale(presentation.IconScale);
            }
        }

        public static Vector3 ResolveButtonScale(DeucarianIconButtonVisualState state)
        {
            float scale = 1f;
            if (!state.Visible)
            {
                scale = 0.92f;
            }
            else if (state.Pressed)
            {
                scale = 0.94f;
            }
            else if (state.Hovered)
            {
                scale = 1.045f;
            }
            else if (state.Selected)
            {
                scale = 1.02f;
            }
            else if (!state.Enabled)
            {
                scale = 0.96f;
            }

            return new Vector3(scale, scale, 1f);
        }

        public static Vector3 ResolveIconScale(DeucarianIconButtonVisualState state)
        {
            float scale = state.Pressed ? 0.96f : state.Hovered || state.Selected ? 1.04f : 1f;
            if (!state.Enabled)
            {
                scale = 0.94f;
            }

            return new Vector3(scale, scale, 1f);
        }

        private static void SetBorder(Button button, float width, Color color)
        {
            button.style.borderLeftWidth = width;
            button.style.borderRightWidth = width;
            button.style.borderTopWidth = width;
            button.style.borderBottomWidth = width;
            button.style.borderLeftColor = color;
            button.style.borderRightColor = color;
            button.style.borderTopColor = color;
            button.style.borderBottomColor = color;
        }
    }
}
