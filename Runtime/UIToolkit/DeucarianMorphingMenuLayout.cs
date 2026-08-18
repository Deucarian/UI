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

        public float EdgeMargin { get; set; } = ReferenceEdgeMargin;
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
            EdgeMargin = Mathf.Max(0f, EdgeMargin);
            MaximumWidth = Mathf.Max(
                DeucarianMorphingMenuMotion.CollapsedSize,
                MaximumWidth);
            ExpandedFallbackHeight = Mathf.Max(
                DeucarianMorphingMenuMotion.CollapsedSize,
                ExpandedFallbackHeight);
        }
    }
}
