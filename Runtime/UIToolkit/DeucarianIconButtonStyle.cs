using System;
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
            Color borderActive)
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

        public Color ResolveBackground(DeucarianIconButtonVisualState state)
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

    public sealed class DeucarianIconButtonInteraction : IDisposable
    {
        private Button button;
        private Action changed;
        private EventCallback<MouseEnterEvent> mouseEnter;
        private EventCallback<MouseLeaveEvent> mouseLeave;
        private EventCallback<MouseDownEvent> mouseDown;
        private EventCallback<MouseUpEvent> mouseUp;
        private EventCallback<FocusInEvent> focusIn;
        private EventCallback<FocusOutEvent> focusOut;

        public bool Hovered { get; private set; }
        public bool Pressed { get; private set; }
        public bool Focused { get; private set; }

        public void Bind(Button targetButton, Action changedCallback)
        {
            Unbind();
            if (targetButton == null)
            {
                return;
            }

            button = targetButton;
            changed = changedCallback;
            mouseEnter = _ =>
            {
                Hovered = true;
                NotifyChanged();
            };
            mouseLeave = _ =>
            {
                Hovered = false;
                Pressed = false;
                NotifyChanged();
            };
            mouseDown = evt =>
            {
                if (evt.button != 0)
                {
                    return;
                }

                Pressed = true;
                NotifyChanged();
            };
            mouseUp = evt =>
            {
                if (evt.button != 0)
                {
                    return;
                }

                Pressed = false;
                NotifyChanged();
            };
            focusIn = _ =>
            {
                Focused = true;
                NotifyChanged();
            };
            focusOut = _ =>
            {
                Focused = false;
                NotifyChanged();
            };

            button.RegisterCallback(mouseEnter);
            button.RegisterCallback(mouseLeave);
            button.RegisterCallback(mouseDown);
            button.RegisterCallback(mouseUp);
            button.RegisterCallback(focusIn);
            button.RegisterCallback(focusOut);
        }

        public void Reset()
        {
            Hovered = false;
            Pressed = false;
            Focused = false;
        }

        public void Unbind()
        {
            if (button != null)
            {
                if (mouseEnter != null)
                {
                    button.UnregisterCallback(mouseEnter);
                }

                if (mouseLeave != null)
                {
                    button.UnregisterCallback(mouseLeave);
                }

                if (mouseDown != null)
                {
                    button.UnregisterCallback(mouseDown);
                }

                if (mouseUp != null)
                {
                    button.UnregisterCallback(mouseUp);
                }

                if (focusIn != null)
                {
                    button.UnregisterCallback(focusIn);
                }

                if (focusOut != null)
                {
                    button.UnregisterCallback(focusOut);
                }
            }

            button = null;
            changed = null;
            mouseEnter = null;
            mouseLeave = null;
            mouseDown = null;
            mouseUp = null;
            focusIn = null;
            focusOut = null;
            Reset();
        }

        public void Dispose()
        {
            Unbind();
        }

        private void NotifyChanged()
        {
            changed?.Invoke();
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
            if (button == null)
            {
                return;
            }

            button.style.display = state.Visible ? DisplayStyle.Flex : DisplayStyle.None;
            button.style.opacity = state.Visible ? 1f : 0f;
            button.style.backgroundColor = palette.ResolveBackground(state);
            button.style.color = palette.Text;
            button.style.scale = new Scale(ResolveButtonScale(state));
            ApplyBorder(button, palette, state, style);
        }

        public static void ApplyIconState(
            VisualElement icon,
            DeucarianIconButtonPalette palette,
            DeucarianIconButtonVisualState state)
        {
            if (icon == null)
            {
                return;
            }

            icon.style.unityBackgroundImageTintColor = palette.ResolveIcon(state);
            icon.style.opacity = state.Visible ? 1f : 0f;
            icon.style.display = state.Visible ? DisplayStyle.Flex : DisplayStyle.None;
            icon.style.scale = new Scale(ResolveIconScale(state));
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

        private static void ApplyBorder(
            Button button,
            DeucarianIconButtonPalette palette,
            DeucarianIconButtonVisualState state,
            DeucarianThemeStyle style)
        {
            bool outlined = state.Active || state.Focused || state.Disabled;
            float width = style != null
                ? outlined ? Mathf.Max(0f, style.BorderWidth) : NoBorderWidth
                : state.Active || state.Focused
                    ? ActiveBorderWidth
                    : state.Disabled ? DisabledBorderWidth : NoBorderWidth;
            button.style.borderLeftWidth = width;
            button.style.borderRightWidth = width;
            button.style.borderTopWidth = width;
            button.style.borderBottomWidth = width;

            Color paletteBorder = palette.ResolveBorder(state);
            Color borderColor = style != null
                ? style.ResolveBorderColor(paletteBorder)
                : paletteBorder;
            button.style.borderLeftColor = borderColor;
            button.style.borderRightColor = borderColor;
            button.style.borderTopColor = borderColor;
            button.style.borderBottomColor = borderColor;
        }
    }
}
