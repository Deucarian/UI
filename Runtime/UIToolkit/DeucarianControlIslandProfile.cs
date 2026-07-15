using System;
using Deucarian.Theming;
using UnityEngine;

namespace Deucarian.UI
{
    /// <summary>
    /// Immutable control-island density and spacing metrics for a Deucarian visual style.
    /// </summary>
    public readonly struct DeucarianControlIslandProfile
    {
        public DeucarianControlIslandProfile(
            string styleId,
            float rowHeight,
            float buttonSize,
            float iconSize,
            float compactScrubberWidth,
            float compactScrubberHeight,
            float itemHorizontalMargin,
            float horizontalPadding,
            float verticalPadding,
            float fallbackPanelCornerRadius)
        {
            StyleId = styleId ?? string.Empty;
            RowHeight = Mathf.Max(0f, rowHeight);
            ButtonSize = Mathf.Max(0f, buttonSize);
            IconSize = Mathf.Max(0f, iconSize);
            CompactScrubberWidth = Mathf.Max(0f, compactScrubberWidth);
            CompactScrubberHeight = Mathf.Max(0f, compactScrubberHeight);
            ItemHorizontalMargin = Mathf.Max(0f, itemHorizontalMargin);
            HorizontalPadding = Mathf.Max(0f, horizontalPadding);
            VerticalPadding = Mathf.Max(0f, verticalPadding);
            FallbackPanelCornerRadius = Mathf.Max(0f, fallbackPanelCornerRadius);
        }

        public string StyleId { get; }
        public float RowHeight { get; }
        public float ButtonSize { get; }
        public float IconSize { get; }
        public float CompactScrubberWidth { get; }
        public float CompactScrubberHeight { get; }
        public float ItemHorizontalMargin { get; }
        public float HorizontalPadding { get; }
        public float VerticalPadding { get; }
        public float FallbackPanelCornerRadius { get; }
        public float FallbackButtonCornerRadius =>
            DeucarianControlIslandStyle.ResolveNestedCornerRadius(
                FallbackPanelCornerRadius,
                VerticalPadding);

        public DeucarianPanelChrome CreatePanelChrome(float minWidth = 0f)
        {
            return new DeucarianPanelChrome(
                RowHeight,
                FallbackPanelCornerRadius,
                HorizontalPadding,
                VerticalPadding,
                minWidth);
        }

        public DeucarianIconButtonChrome CreateIconButtonChrome(bool iconAbsoluteCentered = false)
        {
            return new DeucarianIconButtonChrome(
                ButtonSize,
                FallbackButtonCornerRadius,
                IconSize,
                ItemHorizontalMargin,
                iconAbsoluteCentered);
        }

        public float CalculatePanelWidth(int buttonCount, int compactScrubberCount = 0)
        {
            int safeButtonCount = Mathf.Max(0, buttonCount);
            int safeScrubberCount = Mathf.Max(0, compactScrubberCount);
            float buttonWidth = safeButtonCount * (ButtonSize + ItemHorizontalMargin * 2f);
            float scrubberWidth = safeScrubberCount
                                  * (CompactScrubberWidth + ItemHorizontalMargin * 2f);
            return HorizontalPadding * 2f + buttonWidth + scrubberWidth;
        }
    }

    /// <summary>
    /// Resolves built-in control-island profiles from stable Theming style IDs.
    /// </summary>
    public static class DeucarianControlIslandProfiles
    {
        public const float SharedItemHorizontalMargin = 4f;
        public const float SharedHorizontalPadding = 0f;
        public const float SharedVerticalPadding = 4f;

        public static readonly DeucarianControlIslandProfile FrostedGlass =
            new DeucarianControlIslandProfile(
                DeucarianThemeStyleIds.FrostedGlass,
                40f,
                32f,
                18f,
                112f,
                28f,
                SharedItemHorizontalMargin,
                SharedHorizontalPadding,
                SharedVerticalPadding,
                16f);

        public static readonly DeucarianControlIslandProfile FluentAcrylic =
            new DeucarianControlIslandProfile(
                DeucarianThemeStyleIds.FluentAcrylic,
                38f,
                30f,
                17f,
                104f,
                26f,
                SharedItemHorizontalMargin,
                SharedHorizontalPadding,
                SharedVerticalPadding,
                8f);

        public static readonly DeucarianControlIslandProfile MaterialDark =
            new DeucarianControlIslandProfile(
                DeucarianThemeStyleIds.MaterialDark,
                36f,
                28f,
                16f,
                96f,
                24f,
                SharedItemHorizontalMargin,
                SharedHorizontalPadding,
                SharedVerticalPadding,
                4f);

        public static DeucarianControlIslandProfile Resolve(DeucarianThemeStyle style)
        {
            return Resolve(style != null ? style.StyleId : null);
        }

        public static DeucarianControlIslandProfile Resolve(string styleId)
        {
            if (string.Equals(
                    styleId,
                    DeucarianThemeStyleIds.FluentAcrylic,
                    StringComparison.OrdinalIgnoreCase))
            {
                return FluentAcrylic;
            }

            if (string.Equals(
                    styleId,
                    DeucarianThemeStyleIds.MaterialDark,
                    StringComparison.OrdinalIgnoreCase))
            {
                return MaterialDark;
            }

            return FrostedGlass;
        }
    }
}
