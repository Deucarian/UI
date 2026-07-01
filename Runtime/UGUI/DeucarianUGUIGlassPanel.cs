using Deucarian.Theming;
using UnityEngine;
using UnityEngine.UI;

namespace Deucarian.UI
{
    public static class DeucarianUGUIGlassPanel
    {
        public static bool Apply(Graphic graphic, DeucarianTheme theme, Color baseColor, DeucarianThemeStyle style = null)
        {
            return DeucarianUGUIThemeStyleUtility.ApplyPanel(graphic, baseColor, ResolveStyle(theme, style));
        }

        public static bool ApplyImage(Image image, DeucarianTheme theme, Color baseColor, DeucarianThemeStyle style = null)
        {
            return DeucarianUGUIThemeStyleUtility.ApplyPanelImage(image, baseColor, ResolveStyle(theme, style));
        }

        public static bool ApplyOutline(Outline outline, Color surfaceColor, DeucarianTheme theme, DeucarianThemeStyle style = null)
        {
            return DeucarianUGUIThemeStyleUtility.ApplyOutline(outline, surfaceColor, ResolveStyle(theme, style));
        }

        private static DeucarianThemeStyle ResolveStyle(DeucarianTheme theme, DeucarianThemeStyle style)
        {
            return style != null ? style : theme != null ? theme.VisualStyle : null;
        }
    }
}
