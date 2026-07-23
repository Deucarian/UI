using System;
using UnityEngine.UIElements;

namespace Deucarian.UI
{
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
}
