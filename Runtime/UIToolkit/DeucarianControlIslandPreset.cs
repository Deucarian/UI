using UnityEngine;

namespace Deucarian.UI
{
    [CreateAssetMenu(
        fileName = "DeucarianControlIslandPreset",
        menuName = "Deucarian/UI/Control Island Preset")]
    public sealed class DeucarianControlIslandPreset : ScriptableObject
    {
        [SerializeField, Min(0f)] private float rowHeight = DeucarianControlIslandStyle.DefaultRowHeight;
        [SerializeField, Min(0f)] private float buttonSize = DeucarianControlIslandStyle.DefaultButtonSize;
        [SerializeField, Min(0f)] private float iconSize = DeucarianControlIslandStyle.DefaultIconSize;
        [SerializeField, Min(0f)] private float buttonMargin = DeucarianControlIslandStyle.DefaultButtonMargin;
        [SerializeField, Min(0f)] private float panelCornerRadius = DeucarianControlIslandStyle.DefaultPanelCornerRadius;
        [SerializeField, Min(0f)] private float buttonCornerRadius = DeucarianControlIslandStyle.DefaultButtonCornerRadius;
        [SerializeField, Min(0f)] private float horizontalPadding = DeucarianControlIslandStyle.DefaultHorizontalPadding;
        [SerializeField, Min(0f)] private float verticalPadding = DeucarianControlIslandStyle.DefaultVerticalPadding;
        [SerializeField, Min(0f)] private float rowGap = DeucarianControlIslandStyle.DefaultRowGap;
        [SerializeField, Min(0f)] private float bottomOffset = DeucarianControlIslandStyle.DefaultBottomOffset;
        [SerializeField, Min(0f)] private float statusWidth = DeucarianControlIslandStyle.DefaultStatusWidth;
        [SerializeField, Min(0f)] private float statusHeight = DeucarianControlIslandStyle.DefaultStatusHeight;
        [SerializeField, Min(0f)] private float statusFontSize = DeucarianControlIslandStyle.DefaultStatusFontSize;
        [SerializeField, Min(0f)] private float compactScrubberWidth =
            DeucarianControlIslandStyle.DefaultCompactScrubberWidth;
        [SerializeField, Min(0f)] private float compactScrubberHorizontalMargin =
            DeucarianControlIslandStyle.DefaultCompactScrubberHorizontalMargin;
        [SerializeField, Min(0f)] private float compactScrubberChromeInset =
            DeucarianControlIslandStyle.DefaultCompactScrubberChromeInset;

        public float RowHeight => rowHeight;
        public float ButtonSize => buttonSize;
        public float IconSize => iconSize;
        public float ButtonMargin => buttonMargin;
        public float PanelCornerRadius => panelCornerRadius;
        public float ButtonCornerRadius => buttonCornerRadius;
        public float HorizontalPadding => horizontalPadding;
        public float VerticalPadding => verticalPadding;
        public float RowGap => rowGap;
        public float BottomOffset => bottomOffset;
        public float StatusWidth => statusWidth;
        public float StatusHeight => statusHeight;
        public float StatusFontSize => statusFontSize;
        public float CompactScrubberWidth => compactScrubberWidth;
        public float CompactScrubberHorizontalMargin => compactScrubberHorizontalMargin;
        public float CompactScrubberHeight =>
            DeucarianControlIslandStyle.CalculateCompactScrubberHeight(buttonSize, compactScrubberChromeInset);

        public DeucarianPanelChrome CreatePanelChrome(float minWidth = 0f)
        {
            return new DeucarianPanelChrome(
                rowHeight,
                panelCornerRadius,
                horizontalPadding,
                verticalPadding,
                minWidth);
        }

        public DeucarianIconButtonChrome CreateIconButtonChrome()
        {
            return new DeucarianIconButtonChrome(
                buttonSize,
                buttonCornerRadius,
                iconSize,
                buttonMargin);
        }

        public DeucarianScrubberMetrics CreateCompactScrubberMetrics()
        {
            return DeucarianScrubberMetrics.Compact;
        }

        public float ResolveBottomPadding(int rowIndex)
        {
            return DeucarianControlIslandStyle.ResolveStackedBottomPadding(
                rowIndex,
                bottomOffset,
                rowHeight,
                rowGap);
        }

        public float ResolveStatusBottomPadding(int rowIndex)
        {
            return DeucarianControlIslandStyle.ResolveStackedStatusBottomPadding(
                rowIndex,
                bottomOffset,
                rowHeight,
                rowGap);
        }

        private void OnValidate()
        {
            rowHeight = Mathf.Max(0f, rowHeight);
            buttonSize = Mathf.Max(0f, buttonSize);
            iconSize = Mathf.Max(0f, iconSize);
            buttonMargin = Mathf.Max(0f, buttonMargin);
            panelCornerRadius = Mathf.Max(0f, panelCornerRadius);
            buttonCornerRadius = Mathf.Max(0f, buttonCornerRadius);
            horizontalPadding = Mathf.Max(0f, horizontalPadding);
            verticalPadding = Mathf.Max(0f, verticalPadding);
            rowGap = Mathf.Max(0f, rowGap);
            bottomOffset = Mathf.Max(0f, bottomOffset);
            statusWidth = Mathf.Max(0f, statusWidth);
            statusHeight = Mathf.Max(0f, statusHeight);
            statusFontSize = Mathf.Max(0f, statusFontSize);
            compactScrubberWidth = Mathf.Max(0f, compactScrubberWidth);
            compactScrubberHorizontalMargin = Mathf.Max(0f, compactScrubberHorizontalMargin);
            compactScrubberChromeInset = Mathf.Max(0f, compactScrubberChromeInset);
        }
    }
}
