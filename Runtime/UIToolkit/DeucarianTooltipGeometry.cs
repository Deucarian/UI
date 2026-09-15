using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.UI
{
    /// <summary>Measures tooltip content before positioning so screen position never changes wrapping.</summary>
    internal sealed class DeucarianTooltipGeometry
    {
        private readonly VisualElement bubble;
        private readonly Label label;
        private string measuredText;
        private float measuredWidth = -1f;
        private Vector2 size;

        internal DeucarianTooltipGeometry(VisualElement bubble, Label label)
        {
            this.bubble = bubble;
            this.label = label;
            label.style.marginLeft = label.style.marginRight = 0f;
            label.style.marginTop = label.style.marginBottom = 0f;
            label.style.paddingLeft = label.style.paddingRight = 0f;
            label.style.paddingTop = label.style.paddingBottom = 0f;
            label.style.flexShrink = 0f;
        }

        internal void Invalidate() => measuredWidth = -1f;

        internal Vector2 Measure(float viewportWidth)
        {
            float maximum = Mathf.Min(240f, Mathf.Max(24f, viewportWidth - 20f));
            if (measuredText == label.text && Mathf.Approximately(measuredWidth, maximum)) return size;
            float horizontal = bubble.style.paddingLeft.value.value + bubble.style.paddingRight.value.value +
                bubble.style.borderLeftWidth.value + bubble.style.borderRightWidth.value;
            float vertical = bubble.style.paddingTop.value.value + bubble.style.paddingBottom.value.value +
                bubble.style.borderTopWidth.value + bubble.style.borderBottomWidth.value;
            Vector2 natural = label.MeasureTextSize(label.text, 0f, VisualElement.MeasureMode.Undefined,
                0f, VisualElement.MeasureMode.Undefined);
            float width = Mathf.Clamp(Mathf.Ceil(natural.x) + horizontal + 1f, 24f, maximum);
            float contentWidth = Mathf.Max(1f, width - horizontal);
            Vector2 wrapped = label.MeasureTextSize(label.text, contentWidth, VisualElement.MeasureMode.AtMost,
                0f, VisualElement.MeasureMode.Undefined);
            float radius = bubble.style.borderTopLeftRadius.value.value;
            size = new Vector2(width, Mathf.Max(24f, radius * 2f, Mathf.Ceil(wrapped.y) + vertical));
            bubble.style.width = size.x;
            bubble.style.height = size.y;
            bubble.style.maxWidth = maximum;
            measuredText = label.text;
            measuredWidth = natural.x > 0f ? maximum : -1f;
            return size;
        }
    }
}
