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
            DeucarianThemeDensity density,
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
            Density = density;
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

        /// <summary>Backward-compatible constructor for callers that supplied only a style ID.</summary>
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
            : this(
                styleId,
                DeucarianThemeDensity.Unspecified,
                rowHeight,
                buttonSize,
                iconSize,
                compactScrubberWidth,
                compactScrubberHeight,
                itemHorizontalMargin,
                horizontalPadding,
                verticalPadding,
                fallbackPanelCornerRadius)
        {
        }

        public string StyleId { get; }
        public DeucarianThemeDensity Density { get; }
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
    /// Resolves built-in control-island profiles from semantic Theming density intents.
    /// Stable style IDs remain a fallback for legacy inline styles.
    /// </summary>
    public static class DeucarianControlIslandProfiles
    {
        public const float SharedItemHorizontalMargin = 4f;
        public const float SharedHorizontalPadding = 0f;
        public const float SharedVerticalPadding = 4f;

        public static readonly DeucarianControlIslandProfile Comfortable =
            new DeucarianControlIslandProfile(
                DeucarianThemeStyleIds.FrostedGlass,
                DeucarianThemeDensity.Comfortable,
                40f,
                32f,
                18f,
                112f,
                28f,
                SharedItemHorizontalMargin,
                SharedHorizontalPadding,
                SharedVerticalPadding,
                16f);

        public static readonly DeucarianControlIslandProfile Standard =
            new DeucarianControlIslandProfile(
                DeucarianThemeStyleIds.FluentAcrylic,
                DeucarianThemeDensity.Standard,
                38f,
                30f,
                17f,
                104f,
                26f,
                SharedItemHorizontalMargin,
                SharedHorizontalPadding,
                SharedVerticalPadding,
                8f);

        public static readonly DeucarianControlIslandProfile Compact =
            new DeucarianControlIslandProfile(
                DeucarianThemeStyleIds.MaterialDark,
                DeucarianThemeDensity.Compact,
                36f,
                28f,
                16f,
                96f,
                24f,
                SharedItemHorizontalMargin,
                SharedHorizontalPadding,
                SharedVerticalPadding,
                4f);

        /// <summary>Backward-compatible alias for the Comfortable density profile.</summary>
        public static readonly DeucarianControlIslandProfile FrostedGlass = Comfortable;

        /// <summary>Backward-compatible alias for the Standard density profile.</summary>
        public static readonly DeucarianControlIslandProfile FluentAcrylic = Standard;

        /// <summary>Backward-compatible alias for the Compact density profile.</summary>
        public static readonly DeucarianControlIslandProfile MaterialDark = Compact;

        public static DeucarianControlIslandProfile Resolve(DeucarianThemeStyle style)
        {
            if (style != null && style.Density != DeucarianThemeDensity.Unspecified)
            {
                return Resolve(style.Density);
            }

            return Resolve(style != null ? style.StyleId : null);
        }

        public static DeucarianControlIslandProfile Resolve(DeucarianThemeDensity density)
        {
            switch (density)
            {
                case DeucarianThemeDensity.Standard:
                    return Standard;
                case DeucarianThemeDensity.Compact:
                    return Compact;
                case DeucarianThemeDensity.Comfortable:
                default:
                    return Comfortable;
            }
        }

        public static DeucarianControlIslandProfile Resolve(string styleId)
        {
            if (string.Equals(
                    styleId,
                    DeucarianThemeStyleIds.FluentAcrylic,
                    StringComparison.OrdinalIgnoreCase))
            {
                return Standard;
            }

            if (string.Equals(
                    styleId,
                    DeucarianThemeStyleIds.MaterialDark,
                    StringComparison.OrdinalIgnoreCase))
            {
                return Compact;
            }

            return Comfortable;
        }
    }
}
