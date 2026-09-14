using System;
using Deucarian.Theming;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.UI
{
    /// <summary>Reusable theme, contrast and tween feedback for text controls.</summary>
    public sealed class DeucarianControlFeedback : IDisposable
    {
        private readonly VisualElement control;
        private readonly Func<bool> shouldAnimate;
        private readonly DeucarianIconButtonInteraction interaction = new DeucarianIconButtonInteraction();
        private readonly DeucarianAnimatedIconButton animation;
        private DeucarianControlIslandPresentation presentation;
        private DeucarianIconButtonPalette palette;
        private bool selected;
        private bool themed;

        public DeucarianControlFeedback(MonoBehaviour host, VisualElement control, Func<bool> shouldAnimate = null)
        {
            this.control = control ?? throw new ArgumentNullException(nameof(control));
            this.shouldAnimate = shouldAnimate;
            control.AddToClassList(DeucarianTextControlStyle.ControlClass);
            animation = new DeucarianAnimatedIconButton(host, control, null,
                DeucarianMotionProfile.ControlState, manageButtonScale: false);
            interaction.Bind(control, Refresh);
            control.RegisterCallback<DetachFromPanelEvent>(OnDetached);
        }

        public void ApplyTheme(DeucarianTheme theme, Component context = null)
        {
            presentation = DeucarianControlIslandPresentation.Resolve(theme, context);
            palette = DeucarianControlIslandTheme.ResolveButtonPalette(presentation.Theme, context);
            DeucarianTextControlStyle.Apply(control, presentation);
            themed = true;
            Refresh();
        }

        public void SetSelected(bool value)
        {
            selected = value;
            Refresh();
        }

        public void Refresh()
        {
            if (!themed) return;
            animation.SetState(palette, new DeucarianIconButtonVisualState(true,
                control.enabledSelf, selected, interaction.Hovered, interaction.Pressed,
                interaction.Focused), presentation.Style, shouldAnimate?.Invoke() ?? true);
        }

        private void OnDetached(DetachFromPanelEvent evt)
        {
            animation.Stop();
            animation.InvalidatePresentation();
            interaction.Reset();
        }

        public void Dispose()
        {
            control.UnregisterCallback<DetachFromPanelEvent>(OnDetached);
            interaction.Dispose();
            animation.Stop();
        }
    }
}
