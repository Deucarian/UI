using Deucarian.Theming;
using Deucarian.Theming.UIToolkit;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.UI
{
    public static class DeucarianUIToolkitGlassPanel
    {
        public const string ClassName = "deucarian-glass-panel";

        public static void AddClass(VisualElement element)
        {
            element?.AddToClassList(ClassName);
        }

        public static bool Apply(
            VisualElement element,
            DeucarianTheme theme,
            Color baseColor,
            DeucarianThemeStyle style = null)
        {
            AddClass(element);
            return DeucarianUIToolkitThemeStyleUtility.ApplyPanel(
                element,
                baseColor,
                DeucarianUIToolkitThemeStyleUtility.ResolveStyle(theme, style));
        }
    }
}
