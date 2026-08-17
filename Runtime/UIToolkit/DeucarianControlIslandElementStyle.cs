using Deucarian.Theming;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Deucarian.UI
{
    /// <summary>
    /// Canonical UI Toolkit classes and layout normalization for Deucarian
    /// control islands.
    /// </summary>
    public static class DeucarianControlIslandElementStyle
    {
        public const string ToolbarClass = "deucarian-control-island";
        public const string GlassPanelClass = "deucarian-control-island-glass";
        public const string IconButtonClass = "deucarian-control-island-icon-button";
        public const string IconClass = "deucarian-control-island-icon";
        public const string ScrubberClass = "deucarian-control-island-scrubber";
        public const string ScrubberTrackClass = "deucarian-control-island-scrubber-track";
        public const string ScrubberFillClass = "deucarian-control-island-scrubber-fill";
        public const string ScrubberHandleClass = "deucarian-control-island-scrubber-handle";
        public const string ActiveClass = "deucarian-control-island-button-active";
        public const string InactiveClass = "deucarian-control-island-button-inactive";
        public const string DisabledClass = "deucarian-control-island-button-disabled";
        public const string FocusedClass = "deucarian-control-island-button-focused";

        public static void AddToolbarClasses(VisualElement panel)
        {
            if (panel == null)
            {
                return;
            }

            panel.AddToClassList(ToolbarClass);
            panel.AddToClassList(GlassPanelClass);
            DeucarianGlassPanelStyle.AddClass(panel);
        }

        public static void AddIconButtonClasses(Button button)
        {
            if (button == null)
            {
                return;
            }

            button.AddToClassList(IconButtonClass);
            button.AddToClassList(InactiveClass);
        }

        public static void AddIconClasses(VisualElement icon)
        {
            icon?.AddToClassList(IconClass);
        }

        public static void AddScrubberClasses(
            VisualElement scrubber,
            VisualElement track,
            VisualElement fill,
            VisualElement handle)
        {
            scrubber?.AddToClassList(ScrubberClass);
            track?.AddToClassList(ScrubberTrackClass);
            fill?.AddToClassList(ScrubberFillClass);
            handle?.AddToClassList(ScrubberHandleClass);
        }

        public static void ApplyRoot(
            VisualElement root,
            DeucarianControlIslandRow row,
            DeucarianControlIslandPresentation presentation)
        {
            if (root == null)
            {
                return;
            }

            root.style.backgroundColor = StyleKeyword.Null;
            root.style.justifyContent = Justify.FlexEnd;
            root.style.alignItems = Align.Center;
            root.style.paddingBottom = ResolveBottomPadding(
                row,
                presentation);
        }

        public static float ResolveBottomPadding(
            DeucarianControlIslandRow row,
            DeucarianControlIslandPresentation presentation)
        {
            return DeucarianControlIslandStyle.ResolveStackedBottomPadding(
                Mathf.Max(0, (int)row),
                DeucarianControlIslandStyle.DefaultBottomOffset,
                presentation.Profile.RowHeight,
                DeucarianControlIslandStyle.DefaultRowGap);
        }

        public static float ResolveStatusBottomPadding(
            DeucarianControlIslandRow row,
            DeucarianControlIslandPresentation presentation)
        {
            return DeucarianControlIslandStyle.ResolveStackedStatusBottomPadding(
                Mathf.Max(0, (int)row),
                DeucarianControlIslandStyle.DefaultBottomOffset,
                presentation.Profile.RowHeight,
                DeucarianControlIslandStyle.DefaultRowGap);
        }

        public static void ApplyPanel(
            VisualElement panel,
            DeucarianControlIslandPresentation presentation)
        {
            if (panel == null)
            {
                return;
            }

            AddToolbarClasses(panel);
            DeucarianControlIslandStyle.ApplyPanel(
                panel,
                presentation.CompactPanel,
                presentation.Style);
            DeucarianGlassPanelStyle.ApplyPanel(
                panel,
                presentation.Theme,
                presentation.Style);
            panel.style.scale = new Scale(Vector3.one);
        }

        public static void ApplyPanel(
            VisualElement panel,
            DeucarianTheme theme,
            DeucarianPanelChrome chrome,
            Object context = null)
        {
            DeucarianControlIslandPresentation presentation =
                DeucarianControlIslandPresentation.Resolve(
                    theme,
                    context);
            if (panel == null)
            {
                return;
            }

            AddToolbarClasses(panel);
            DeucarianControlIslandStyle.ApplyPanel(
                panel,
                chrome,
                presentation.Style);
            DeucarianGlassPanelStyle.ApplyPanel(
                panel,
                presentation.Theme,
                presentation.Style,
                context);
            panel.style.scale = new Scale(Vector3.one);
        }

        public static void ApplyIconButtonLayout(
            Button button,
            DeucarianIconButtonChrome chrome,
            DeucarianThemeStyle style = null,
            float containerInset =
                DeucarianControlIslandStyle.DefaultVerticalPadding)
        {
            if (button == null)
            {
                return;
            }

            AddIconButtonClasses(button);
            DeucarianControlIslandStyle.ApplyIconButton(
                button,
                chrome,
                style,
                containerInset);
            button.style.visibility = Visibility.Visible;
            button.style.opacity = 1f;
            button.style.flexDirection = FlexDirection.Row;
            button.style.backgroundImage = StyleKeyword.Null;
            button.style.fontSize = 0f;
            button.style.whiteSpace = WhiteSpace.NoWrap;
            button.style.unityTextAlign = TextAnchor.MiddleCenter;
            button.style.scale = new Scale(Vector3.one);
            SetBorderWidth(button, DeucarianIconButtonStyle.NoBorderWidth);
        }

        public static void ApplyIconButtonLayout(
            Button button,
            DeucarianControlIslandPresentation presentation)
        {
            ApplyIconButtonLayout(
                button,
                presentation.CompactIconButton,
                presentation.Style,
                presentation.Profile.VerticalPadding);
        }

        public static void ApplyIconLayout(
            VisualElement icon,
            DeucarianIconButtonChrome chrome)
        {
            if (icon == null)
            {
                return;
            }

            AddIconClasses(icon);
            icon.style.display = DisplayStyle.Flex;
            icon.style.visibility = Visibility.Visible;
            icon.style.opacity = 1f;
            DeucarianControlIslandStyle.ApplyIcon(
                icon,
                chrome,
                chrome.IconAbsoluteCentered);
            icon.style.marginLeft = 0f;
            icon.style.marginRight = 0f;
            icon.style.scale = new Scale(Vector3.one);
        }

        /// <summary>
        /// Places an icon in the package-owned absolute centered overlay slot.
        /// Calling this for two sibling icons guarantees they overlap exactly.
        /// </summary>
        public static void ApplyCenteredOverlayIconLayout(
            VisualElement icon,
            DeucarianControlIslandPresentation presentation)
        {
            ApplyIconLayout(
                icon,
                presentation.CenteredOverlayIconButton);
        }

        public static void ApplyCompactScrubber(
            VisualElement scrubber,
            DeucarianControlIslandPresentation presentation)
        {
            DeucarianControlIslandStyle.ApplyCompactScrubber(
                scrubber,
                presentation.Profile,
                presentation.Style);
        }

        public static float CalculatePanelWidth(
            DeucarianControlIslandPresentation presentation,
            int buttonCount,
            int compactScrubberCount = 0)
        {
            return presentation.Profile.CalculatePanelWidth(
                buttonCount,
                compactScrubberCount);
        }

        public static void SetIconTexture(
            VisualElement icon,
            Texture2D texture)
        {
            if (icon != null)
            {
                icon.style.backgroundImage = texture != null
                    ? new StyleBackground(texture)
                    : StyleKeyword.Null;
            }
        }

        public static void ApplyStateClasses(
            Button button,
            DeucarianIconButtonVisualState state)
        {
            if (button == null)
            {
                return;
            }

            button.EnableInClassList(ActiveClass, state.Active);
            button.EnableInClassList(InactiveClass, state.Inactive);
            button.EnableInClassList(DisabledClass, state.Disabled);
            button.EnableInClassList(FocusedClass, state.Focused);
        }

        private static void SetBorderWidth(
            VisualElement element,
            float width)
        {
            element.style.borderLeftWidth = width;
            element.style.borderRightWidth = width;
            element.style.borderTopWidth = width;
            element.style.borderBottomWidth = width;
        }
    }
}
