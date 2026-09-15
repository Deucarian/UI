using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.UI
{
    /// <summary>A resolution-independent navigation icon using the shared icon tint.</summary>
    public sealed class DeucarianChevronIcon : VisualElement
    {
        public DeucarianChevronIcon(bool pointsRight)
        {
            PointsRight = pointsRight;
            pickingMode = PickingMode.Ignore;
            DeucarianControlIslandElementStyle.AddIconClasses(this);
            generateVisualContent += Draw;
        }

        public bool PointsRight { get; }

        private void Draw(MeshGenerationContext context)
        {
            var painter = context.painter2D;
            painter.strokeColor = resolvedStyle.unityBackgroundImageTintColor;
            painter.lineWidth = Mathf.Max(1.5f, contentRect.width * 0.1f);
            painter.lineCap = LineCap.Round;
            painter.lineJoin = LineJoin.Round;
            float edge = PointsRight ? 0.38f : 0.62f;
            float tip = 1f - edge;
            painter.BeginPath();
            painter.MoveTo(new Vector2(contentRect.width * edge, contentRect.height * 0.22f));
            painter.LineTo(new Vector2(contentRect.width * tip, contentRect.height * 0.5f));
            painter.LineTo(new Vector2(contentRect.width * edge, contentRect.height * 0.78f));
            painter.Stroke();
        }
    }
}
