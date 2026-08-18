using System;
using Deucarian.Theming;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.UI
{
    /// <summary>
    /// Consumer-neutral layout and integration options for the canonical
    /// top-right morphing menu.
    /// </summary>
    public sealed class DeucarianMorphingMenuLayout
    {
        public const float ReferenceEdgeMargin = 24f;
        public const float ReferenceMaximumWidth = 300f;
        public const float ReferenceExpandedFallbackHeight = 196f;

        private float? rightInset;

        public float EdgeMargin { get; set; } = ReferenceEdgeMargin;
        /// <summary>
        /// Distance from the viewport's right edge. When it is not explicitly
        /// assigned, it follows <see cref="EdgeMargin"/> for backward
        /// compatibility.
        /// </summary>
        public float RightInset
        {
            get => rightInset ?? EdgeMargin;
            set => rightInset = value;
        }
        public DeucarianMorphingMenuIcon CollapsedIcon { get; set; } =
            DeucarianMorphingMenuIcon.Settings;
        public float MaximumWidth { get; set; } = ReferenceMaximumWidth;
        public float ExpandedFallbackHeight { get; set; } =
            ReferenceExpandedFallbackHeight;
        [Obsolete(
            "Menu sorting is centrally owned. The morphing menu always uses " +
            "DeucarianUISurfaceRole.Menu.")]
        public int SortingOrder
        {
            get => DeucarianUIDepth.Resolve(
                DeucarianUISurfaceRole.Menu);
            set
            {
                int canonical = DeucarianUIDepth.Resolve(
                    DeucarianUISurfaceRole.Menu);
                if (value != canonical)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        value,
                        "The morphing menu sorting order is package-owned.");
                }
            }
        }
        public string OpenTooltip { get; set; } = "Open menu";
        public string CloseTooltip { get; set; } = "Close menu";
        public Func<bool> ShouldAnimate { get; set; }
        public Func<VisualElement, IDisposable> BindInputGuard { get; set; }
        public DeucarianThemeProvider ThemeProvider { get; set; }
        public Action<DeucarianTheme> ApplyBodyTheme { get; set; }
        public UnityEngine.Object ThemeContext { get; set; }

        internal bool ResolveShouldAnimate()
        {
            return ShouldAnimate == null || ShouldAnimate();
        }

        internal void Validate()
        {
            EdgeMargin = ResolveNonNegativeFinite(EdgeMargin);
            if (rightInset.HasValue)
            {
                rightInset = ResolveNonNegativeFinite(rightInset.Value);
            }

            if (CollapsedIcon != DeucarianMorphingMenuIcon.Settings &&
                CollapsedIcon != DeucarianMorphingMenuIcon.Information)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(CollapsedIcon),
                    CollapsedIcon,
                    "Unknown morphing-menu collapsed icon.");
            }

            MaximumWidth = Mathf.Max(
                DeucarianMorphingMenuMotion.CollapsedSize,
                ResolveNonNegativeFinite(MaximumWidth));
            ExpandedFallbackHeight = Mathf.Max(
                DeucarianMorphingMenuMotion.CollapsedSize,
                ResolveNonNegativeFinite(ExpandedFallbackHeight));
        }

        private static float ResolveNonNegativeFinite(float value)
        {
            return float.IsNaN(value) || float.IsInfinity(value)
                ? 0f
                : Mathf.Max(0f, value);
        }
    }
}
