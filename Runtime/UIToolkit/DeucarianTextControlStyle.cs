using Deucarian.Theming;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.UI
{
    /// <summary>Shared silhouette and content alignment for text buttons and toggles.</summary>
    public static class DeucarianTextControlStyle
    {
        public const string ControlClass = "deucarian-control-island-text-control";

        internal static bool IsContained(VisualElement element) => element != null &&
            (element.ClassListContains(ControlClass) ||
             element.ClassListContains(DeucarianControlIslandElementStyle.IconButtonClass));

        public static void Apply(VisualElement element, DeucarianControlIslandPresentation presentation)
        {
            if (element == null) return;
            element.AddToClassList(ControlClass);
            var profile = presentation.Profile;
            float radius = DeucarianControlIslandStyle.ResolveNestedCornerRadius(
                presentation.Style != null ? presentation.Style.CornerRadius : profile.FallbackPanelCornerRadius,
                profile.VerticalPadding);
            element.style.minHeight = profile.ButtonSize;
            element.style.flexShrink = 0f;
            element.style.paddingLeft = 10f;
            element.style.paddingRight = 10f;
            element.style.paddingTop = 4f;
            element.style.paddingBottom = 4f;
            element.style.fontSize = 12f;
            element.style.unityTextAlign = TextAnchor.MiddleCenter;
            element.style.borderTopLeftRadius = radius;
            element.style.borderTopRightRadius = radius;
            element.style.borderBottomLeftRadius = radius;
            element.style.borderBottomRightRadius = radius;
            element.style.backgroundImage = StyleKeyword.None;
            element.style.translate = new Translate(0f, 0f, 0f);
            element.style.scale = new Scale(Vector3.one);
        }

        /// <summary>Retains Toggle input and keyboard behavior with an aligned text state.</summary>
        public static void ConfigureToggle(Toggle toggle, Label state)
        {
            toggle.style.flexDirection = FlexDirection.Row;
            toggle.style.alignItems = Align.Center;
            toggle.labelElement.style.minWidth = 0f;
            toggle.labelElement.style.flexGrow = 1f;
            toggle.labelElement.style.flexShrink = 1f;
            toggle.labelElement.style.marginLeft = 0f;
            toggle.labelElement.style.marginRight = 8f;
            toggle.labelElement.style.color = StyleKeyword.Null;
            toggle.labelElement.style.whiteSpace = WhiteSpace.Normal;
            var input = toggle.Q<VisualElement>(className: Toggle.inputUssClassName);
            if (input == null) return;
            input.style.flexGrow = 0f;
            input.style.flexShrink = 0f;
            input.style.minWidth = 28f;
            input.style.backgroundImage = StyleKeyword.None;
            input.style.backgroundColor = Color.clear;
            var checkmark = input.Q<VisualElement>(className: Toggle.checkmarkUssClassName);
            if (checkmark != null) checkmark.style.display = DisplayStyle.None;
            state.style.color = StyleKeyword.Null;
            state.style.unityTextAlign = TextAnchor.MiddleRight;
            state.style.unityFontStyleAndWeight = FontStyle.Bold;
            state.style.marginLeft = 0f;
            state.style.marginRight = 0f;
            input.Add(state);
        }
    }
}
