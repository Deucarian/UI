using System;
using Deucarian.Theming;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.UI
{
    /// <summary>
    /// Consumer-neutral layout, copy, input, and theme integration for the
    /// canonical viewer information/settings menu cluster.
    /// </summary>
    public sealed class DeucarianViewerMenuClusterLayout
    {
        public const float ReferenceHorizontalGap = 12f;

        private float? informationMaximumWidth;
        private float? settingsMaximumWidth;
        private float? informationExpandedFallbackHeight;
        private float? settingsExpandedFallbackHeight;

        public float EdgeMargin { get; set; } =
            DeucarianMorphingMenuLayout.ReferenceEdgeMargin;
        public float HorizontalGap { get; set; } = ReferenceHorizontalGap;
        public float MaximumWidth { get; set; } =
            DeucarianMorphingMenuLayout.ReferenceMaximumWidth;
        public float ExpandedFallbackHeight { get; set; } =
            DeucarianMorphingMenuLayout.ReferenceExpandedFallbackHeight;
        /// <summary>
        /// Information-menu width, falling back to
        /// <see cref="MaximumWidth"/> when not explicitly assigned.
        /// </summary>
        public float InformationMaximumWidth
        {
            get => informationMaximumWidth ?? MaximumWidth;
            set => informationMaximumWidth = value;
        }
        /// <summary>
        /// Settings-menu width, falling back to
        /// <see cref="MaximumWidth"/> when not explicitly assigned.
        /// </summary>
        public float SettingsMaximumWidth
        {
            get => settingsMaximumWidth ?? MaximumWidth;
            set => settingsMaximumWidth = value;
        }
        /// <summary>
        /// Information-menu fallback height, falling back to
        /// <see cref="ExpandedFallbackHeight"/> when not assigned.
        /// </summary>
        public float InformationExpandedFallbackHeight
        {
            get => informationExpandedFallbackHeight ??
                   ExpandedFallbackHeight;
            set => informationExpandedFallbackHeight = value;
        }
        /// <summary>
        /// Settings-menu fallback height, falling back to
        /// <see cref="ExpandedFallbackHeight"/> when not assigned.
        /// </summary>
        public float SettingsExpandedFallbackHeight
        {
            get => settingsExpandedFallbackHeight ??
                   ExpandedFallbackHeight;
            set => settingsExpandedFallbackHeight = value;
        }
        public string OpenInformationTooltip { get; set; } =
            "Open information";
        public string CloseInformationTooltip { get; set; } =
            "Close information";
        public string OpenSettingsTooltip { get; set; } = "Open settings";
        public string CloseSettingsTooltip { get; set; } =
            "Close settings";
        public Func<bool> ShouldAnimate { get; set; }
        public Func<VisualElement, IDisposable> BindInputGuard { get; set; }
        public DeucarianThemeProvider ThemeProvider { get; set; }
        public Action<DeucarianTheme> ApplyInformationBodyTheme { get; set; }
        public Action<DeucarianTheme> ApplySettingsBodyTheme { get; set; }
        public UnityEngine.Object ThemeContext { get; set; }

        /// <summary>
        /// Right inset of the collapsed information button, immediately to
        /// the left of the edge-slot settings button.
        /// </summary>
        public float InformationRightInset =>
            EdgeMargin +
            DeucarianMorphingMenuMotion.CollapsedSize +
            HorizontalGap;

        internal void Validate()
        {
            EdgeMargin = ResolveNonNegativeFinite(EdgeMargin);
            HorizontalGap = ResolveNonNegativeFinite(HorizontalGap);
            MaximumWidth = Mathf.Max(
                DeucarianMorphingMenuMotion.CollapsedSize,
                ResolveNonNegativeFinite(MaximumWidth));
            ExpandedFallbackHeight = Mathf.Max(
                DeucarianMorphingMenuMotion.CollapsedSize,
                ResolveNonNegativeFinite(ExpandedFallbackHeight));
            if (informationMaximumWidth.HasValue)
            {
                informationMaximumWidth = Mathf.Max(
                    DeucarianMorphingMenuMotion.CollapsedSize,
                    ResolveNonNegativeFinite(
                        informationMaximumWidth.Value));
            }

            if (settingsMaximumWidth.HasValue)
            {
                settingsMaximumWidth = Mathf.Max(
                    DeucarianMorphingMenuMotion.CollapsedSize,
                    ResolveNonNegativeFinite(settingsMaximumWidth.Value));
            }

            if (informationExpandedFallbackHeight.HasValue)
            {
                informationExpandedFallbackHeight = Mathf.Max(
                    DeucarianMorphingMenuMotion.CollapsedSize,
                    ResolveNonNegativeFinite(
                        informationExpandedFallbackHeight.Value));
            }

            if (settingsExpandedFallbackHeight.HasValue)
            {
                settingsExpandedFallbackHeight = Mathf.Max(
                    DeucarianMorphingMenuMotion.CollapsedSize,
                    ResolveNonNegativeFinite(
                        settingsExpandedFallbackHeight.Value));
            }
        }

        private static float ResolveNonNegativeFinite(float value)
        {
            return float.IsNaN(value) || float.IsInfinity(value)
                ? 0f
                : Mathf.Max(0f, value);
        }
    }
}
