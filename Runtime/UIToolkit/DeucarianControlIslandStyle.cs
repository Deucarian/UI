using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.UI
{
    public readonly struct DeucarianPanelChrome
    {
        public DeucarianPanelChrome(
            float height,
            float cornerRadius,
            float horizontalPadding,
            float verticalPadding,
            float minWidth = 0f)
        {
            Height = height;
            CornerRadius = cornerRadius;
            HorizontalPadding = horizontalPadding;
            VerticalPadding = verticalPadding;
            MinWidth = minWidth;
        }

        public float Height { get; }
        public float CornerRadius { get; }
        public float HorizontalPadding { get; }
        public float VerticalPadding { get; }
        public float MinWidth { get; }
    }

    public readonly struct DeucarianIconButtonChrome
    {
        public DeucarianIconButtonChrome(
            float size,
            float cornerRadius,
            float iconSize,
            float horizontalMargin,
            bool iconAbsoluteCentered = false)
        {
            Size = size;
            CornerRadius = cornerRadius;
            IconSize = iconSize;
            HorizontalMargin = horizontalMargin;
            IconAbsoluteCentered = iconAbsoluteCentered;
        }

        public float Size { get; }
        public float CornerRadius { get; }
        public float IconSize { get; }
        public float HorizontalMargin { get; }
        public bool IconAbsoluteCentered { get; }
    }

    public static class DeucarianControlIslandStyle
    {
        public const float DefaultRowHeight = 40f;
        public const float DefaultButtonSize = 32f;
        public const float DefaultIconSize = 18f;
        public const float DefaultButtonMargin = 4f;
        public const float DefaultPanelCornerRadius = 16f;
        public const float DefaultButtonCornerRadius = 11f;
        public const float DefaultHorizontalPadding = 0f;
        public const float DefaultVerticalPadding = 4f;
        public const float DefaultRowGap = 8f;
        public const float DefaultBottomOffset = 56f;
        public const float DefaultStatusWidth = 180f;
        public const float DefaultStatusHeight = 20f;
        public const float DefaultStatusFontSize = 11f;
        public const float DefaultCompactScrubberWidth = 112f;
        public const float DefaultCompactScrubberHorizontalMargin = 5f;
        public const float DefaultCompactScrubberChromeInset = 2f;

        public static readonly DeucarianPanelChrome CompactPanel =
            new DeucarianPanelChrome(
                DefaultRowHeight,
                DefaultPanelCornerRadius,
                DefaultHorizontalPadding,
                DefaultVerticalPadding);

        public static readonly DeucarianIconButtonChrome RoundedSquareButton =
            new DeucarianIconButtonChrome(
                DefaultButtonSize,
                DefaultButtonCornerRadius,
                DefaultIconSize,
                DefaultButtonMargin);

        public static float ResolveStackedBottomPadding(
            int rowIndex,
            float bottomOffset = DefaultBottomOffset,
            float rowHeight = DefaultRowHeight,
            float rowGap = DefaultRowGap)
        {
            int safeRowIndex = Mathf.Max(0, rowIndex);
            return bottomOffset + safeRowIndex * (rowHeight + rowGap);
        }

        public static float ResolveStackedStatusBottomPadding(
            int rowIndex,
            float bottomOffset = DefaultBottomOffset,
            float rowHeight = DefaultRowHeight,
            float rowGap = DefaultRowGap)
        {
            return ResolveStackedBottomPadding(rowIndex, bottomOffset, rowHeight, rowGap)
                   + rowHeight
                   + rowGap;
        }

        public static float CalculateCompactScrubberHeight(
            float buttonSize = DefaultButtonSize,
            float chromeInset = DefaultCompactScrubberChromeInset)
        {
            return Mathf.Max(0f, buttonSize - chromeInset * 2f);
        }

        public static float CalculateControlRowWidth(
            DeucarianPanelChrome panel,
            DeucarianIconButtonChrome button,
            int buttonCount,
            float extraContentWidth = 0f,
            float extraHorizontalMargin = 0f)
        {
            return CalculatePanelWidth(panel, button, buttonCount)
                   + Mathf.Max(0f, extraContentWidth)
                   + Mathf.Max(0f, extraHorizontalMargin) * 2f;
        }

        public static void ApplyPanel(VisualElement panel, DeucarianPanelChrome chrome)
        {
            if (panel == null)
            {
                return;
            }

            panel.style.display = DisplayStyle.Flex;
            panel.style.flexDirection = FlexDirection.Row;
            panel.style.alignItems = Align.Center;
            panel.style.justifyContent = Justify.Center;
            panel.style.height = chrome.Height;
            panel.style.minHeight = chrome.Height;
            panel.style.maxHeight = chrome.Height;
            panel.style.paddingLeft = chrome.HorizontalPadding;
            panel.style.paddingRight = chrome.HorizontalPadding;
            panel.style.paddingTop = chrome.VerticalPadding;
            panel.style.paddingBottom = chrome.VerticalPadding;
            panel.style.flexGrow = 0f;
            panel.style.flexShrink = 0f;
            if (chrome.MinWidth > 0f)
            {
                panel.style.minWidth = chrome.MinWidth;
            }

            ApplyRadius(panel, chrome.CornerRadius);
        }

        public static void ApplyIconButton(Button button, DeucarianIconButtonChrome chrome)
        {
            if (button == null)
            {
                return;
            }

            button.style.display = DisplayStyle.Flex;
            button.style.position = Position.Relative;
            button.style.alignItems = Align.Center;
            button.style.justifyContent = Justify.Center;
            button.style.width = chrome.Size;
            button.style.height = chrome.Size;
            button.style.minWidth = chrome.Size;
            button.style.minHeight = chrome.Size;
            button.style.maxWidth = chrome.Size;
            button.style.maxHeight = chrome.Size;
            button.style.marginLeft = chrome.HorizontalMargin;
            button.style.marginRight = chrome.HorizontalMargin;
            button.style.paddingLeft = 0f;
            button.style.paddingRight = 0f;
            button.style.paddingTop = 0f;
            button.style.paddingBottom = 0f;
            button.style.flexGrow = 0f;
            button.style.flexShrink = 0f;
            ApplyRadius(button, chrome.CornerRadius);
        }

        public static void ApplyIcon(VisualElement icon, DeucarianIconButtonChrome chrome, bool absoluteCentered)
        {
            if (icon == null)
            {
                return;
            }

            icon.style.width = chrome.IconSize;
            icon.style.height = chrome.IconSize;
            icon.style.minWidth = chrome.IconSize;
            icon.style.minHeight = chrome.IconSize;
            icon.style.maxWidth = chrome.IconSize;
            icon.style.maxHeight = chrome.IconSize;
            icon.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
            icon.style.backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat);
            icon.style.flexGrow = 0f;
            icon.style.flexShrink = 0f;
            icon.pickingMode = PickingMode.Ignore;
            if (absoluteCentered)
            {
                DeucarianIconSwap.ConfigureIconSlot(icon, chrome.Size, chrome.IconSize);
            }
            else
            {
                icon.style.position = Position.Relative;
                icon.style.left = StyleKeyword.Null;
                icon.style.top = StyleKeyword.Null;
            }
        }

        public static float CalculatePanelWidth(DeucarianPanelChrome panel, DeucarianIconButtonChrome button, int count)
        {
            int safeCount = Mathf.Max(0, count);
            return panel.HorizontalPadding * 2f + safeCount * (button.Size + button.HorizontalMargin * 2f);
        }

        private static void ApplyRadius(VisualElement element, float radius)
        {
            element.style.borderTopLeftRadius = radius;
            element.style.borderTopRightRadius = radius;
            element.style.borderBottomLeftRadius = radius;
            element.style.borderBottomRightRadius = radius;
        }
    }
}
