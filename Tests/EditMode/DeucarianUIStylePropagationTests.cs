using Deucarian.Theming;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.UI.Tests
{
    public sealed class DeucarianUIStylePropagationTests
    {
        [Test]
        public void LegacyIconButtonStateRetainsFixedOutlineRules()
        {
            Button button = new Button();
            VisualElement icon = new VisualElement();
            DeucarianIconButtonPalette palette = CreateButtonPalette();

            DeucarianIconButtonStyle.ApplyState(
                button,
                icon,
                palette,
                new DeucarianIconButtonVisualState(true, true, false, false, false, false));
            AssertBorder(button, DeucarianIconButtonStyle.NoBorderWidth, palette.Border);

            DeucarianIconButtonVisualState selected =
                new DeucarianIconButtonVisualState(true, true, true, false, false, false);
            DeucarianIconButtonStyle.ApplyState(button, icon, palette, selected);
            AssertBorder(button, DeucarianIconButtonStyle.ActiveBorderWidth, palette.BorderActive);

            DeucarianIconButtonVisualState disabled =
                new DeucarianIconButtonVisualState(true, false, false, false, false, false);
            DeucarianIconButtonStyle.ApplyButtonState(button, palette, disabled);
            AssertBorder(button, DeucarianIconButtonStyle.DisabledBorderWidth, palette.Border);
        }

        [Test]
        public void StyleAwareIconButtonUsesStyleStrokeForEveryOutlinedState()
        {
            DeucarianThemeShapeProfile shape;
            DeucarianThemeStrokeProfile stroke;
            DeucarianThemeStyle style = CreateStyle(16f, 2.5f, out shape, out stroke);
            try
            {
                Button button = new Button();
                VisualElement icon = new VisualElement();
                DeucarianIconButtonPalette palette = CreateButtonPalette();
                DeucarianIconButtonVisualState[] outlinedStates =
                {
                    new DeucarianIconButtonVisualState(true, true, true, false, false, false),
                    new DeucarianIconButtonVisualState(true, true, false, false, false, true),
                    new DeucarianIconButtonVisualState(true, false, false, false, false, false)
                };

                for (int i = 0; i < outlinedStates.Length; i++)
                {
                    DeucarianIconButtonVisualState state = outlinedStates[i];
                    DeucarianIconButtonStyle.ApplyState(button, icon, palette, state, style);

                    AssertBorder(
                        button,
                        style.BorderWidth,
                        style.ResolveBorderColor(palette.ResolveBorder(state)));
                }

                DeucarianIconButtonVisualState inactive =
                    new DeucarianIconButtonVisualState(true, true, false, true, false, false);
                DeucarianIconButtonStyle.ApplyButtonState(button, palette, inactive, style);
                AssertBorder(
                    button,
                    DeucarianIconButtonStyle.NoBorderWidth,
                    style.ResolveBorderColor(palette.Border));
            }
            finally
            {
                DestroyStyle(style, shape, stroke);
            }
        }

        [Test]
        public void StyleAwareIconButtonHonorsBorderlessStroke()
        {
            DeucarianThemeShapeProfile shape;
            DeucarianThemeStrokeProfile stroke;
            DeucarianThemeStyle style = CreateStyle(8f, 0f, out shape, out stroke);
            try
            {
                Button button = new Button();
                DeucarianIconButtonPalette palette = CreateButtonPalette();
                DeucarianIconButtonVisualState selected =
                    new DeucarianIconButtonVisualState(true, true, true, false, false, false);

                DeucarianIconButtonStyle.ApplyButtonState(button, palette, selected, style);

                AssertBorder(
                    button,
                    DeucarianIconButtonStyle.NoBorderWidth,
                    style.ResolveBorderColor(palette.BorderActive));
            }
            finally
            {
                DestroyStyle(style, shape, stroke);
            }
        }

        [Test]
        public void LegacyScrubberApplyRetainsMetricStrokeAndFunctionalRadii()
        {
            VisualElement scrubber = new VisualElement();
            VisualElement track = new VisualElement();
            VisualElement fill = new VisualElement();
            VisualElement handle = new VisualElement();
            DeucarianScrubberMetrics metrics = DeucarianScrubberMetrics.Compact;
            DeucarianScrubberPalette palette = CreateScrubberPalette();

            DeucarianScrubberStyle.Apply(
                scrubber,
                track,
                fill,
                handle,
                metrics,
                palette,
                new DeucarianScrubberVisualState(true, false, false, false));

            AssertRadius(scrubber, metrics.CornerRadius);
            AssertBorder(scrubber, metrics.BorderWidth, palette.Border);
            AssertRadius(track, metrics.TrackRadius);
            AssertRadius(fill, metrics.TrackRadius);
            AssertRadius(handle, metrics.HandleSize * 0.5f);
            AssertBorder(handle, metrics.BorderWidth, palette.Border);
        }

        [Test]
        public void StyleAwareScrubberUsesStrokeAndConcentricContainerRadius()
        {
            DeucarianThemeShapeProfile shape;
            DeucarianThemeStrokeProfile stroke;
            DeucarianThemeStyle style = CreateStyle(16f, 2.5f, out shape, out stroke);
            try
            {
                VisualElement scrubber = new VisualElement();
                VisualElement track = new VisualElement();
                VisualElement fill = new VisualElement();
                VisualElement handle = new VisualElement();
                DeucarianScrubberMetrics metrics = DeucarianScrubberMetrics.Compact;
                DeucarianScrubberPalette palette = CreateScrubberPalette();
                Color expectedBorder = style.ResolveBorderColor(palette.Border);

                DeucarianScrubberStyle.Apply(
                    scrubber,
                    track,
                    fill,
                    handle,
                    metrics,
                    palette,
                    new DeucarianScrubberVisualState(true, true, false, false),
                    style);

                AssertRadius(
                    scrubber,
                    DeucarianControlIslandStyle.ResolveNestedCornerRadius(
                        style.CornerRadius,
                        DeucarianControlIslandStyle.DefaultVerticalPadding));
                AssertBorder(scrubber, style.BorderWidth, expectedBorder);
                AssertRadius(track, metrics.TrackRadius);
                AssertRadius(fill, metrics.TrackRadius);
                AssertRadius(handle, metrics.HandleSize * 0.5f);
                AssertBorder(handle, style.BorderWidth, expectedBorder);
            }
            finally
            {
                DestroyStyle(style, shape, stroke);
            }
        }

        [Test]
        public void StyleAwareScrubberSupportsExplicitContainerInset()
        {
            DeucarianThemeShapeProfile shape;
            DeucarianThemeStrokeProfile stroke;
            DeucarianThemeStyle style = CreateStyle(16f, 1.5f, out shape, out stroke);
            try
            {
                VisualElement scrubber = new VisualElement();
                DeucarianScrubberStyle.Apply(
                    scrubber,
                    null,
                    null,
                    null,
                    DeucarianScrubberMetrics.Compact,
                    CreateScrubberPalette(),
                    new DeucarianScrubberVisualState(true, false, false, false),
                    style,
                    6f);

                AssertRadius(scrubber, 10f);
            }
            finally
            {
                DestroyStyle(style, shape, stroke);
            }
        }

        [Test]
        public void StyleAwareScrubberHonorsBorderlessStroke()
        {
            DeucarianThemeShapeProfile shape;
            DeucarianThemeStrokeProfile stroke;
            DeucarianThemeStyle style = CreateStyle(4f, 0f, out shape, out stroke);
            try
            {
                VisualElement scrubber = new VisualElement();
                VisualElement handle = new VisualElement();

                DeucarianScrubberStyle.Apply(
                    scrubber,
                    null,
                    null,
                    handle,
                    DeucarianScrubberMetrics.Compact,
                    CreateScrubberPalette(),
                    new DeucarianScrubberVisualState(false, false, false, false),
                    style);

                AssertBorderWidth(scrubber, 0f);
                AssertBorderWidth(handle, 0f);
                AssertRadius(scrubber, 0f);
                AssertRadius(handle, DeucarianScrubberMetrics.Compact.HandleSize * 0.5f);
            }
            finally
            {
                DestroyStyle(style, shape, stroke);
            }
        }

        [Test]
        public void CompactScrubberGeometryAppliesStyleRadiusOnlyThroughNewOverload()
        {
            DeucarianThemeShapeProfile shape;
            DeucarianThemeStrokeProfile stroke;
            DeucarianThemeStyle style = CreateStyle(16f, 1f, out shape, out stroke);
            try
            {
                DeucarianControlIslandProfile profile = DeucarianControlIslandProfiles.Comfortable;
                VisualElement legacyScrubber = new VisualElement();
                SetRadius(legacyScrubber, 7f);
                DeucarianControlIslandStyle.ApplyCompactScrubber(legacyScrubber, profile);

                VisualElement styledScrubber = new VisualElement();
                DeucarianControlIslandStyle.ApplyCompactScrubber(styledScrubber, profile, style);

                AssertRadius(legacyScrubber, 7f);
                AssertRadius(
                    styledScrubber,
                    DeucarianControlIslandStyle.ResolveNestedCornerRadius(
                        style.CornerRadius,
                        profile.VerticalPadding));
            }
            finally
            {
                DestroyStyle(style, shape, stroke);
            }
        }

        private static DeucarianThemeStyle CreateStyle(
            float cornerRadius,
            float borderWidth,
            out DeucarianThemeShapeProfile shape,
            out DeucarianThemeStrokeProfile stroke)
        {
            shape = ScriptableObject.CreateInstance<DeucarianThemeShapeProfile>();
            shape.Configure("test.shape", "Test Shape", "Test shape.", cornerRadius);
            stroke = ScriptableObject.CreateInstance<DeucarianThemeStrokeProfile>();
            stroke.Configure(
                "test.stroke",
                "Test Stroke",
                "Test stroke.",
                new Color(0.9f, 0.65f, 0.25f, 1f),
                0.4f,
                0.72f,
                borderWidth);
            DeucarianThemeStyle style = ScriptableObject.CreateInstance<DeucarianThemeStyle>();
            style.SetComposition(
                null,
                shape,
                stroke,
                DeucarianThemeDensity.Comfortable,
                true);
            return style;
        }

        private static void DestroyStyle(
            DeucarianThemeStyle style,
            DeucarianThemeShapeProfile shape,
            DeucarianThemeStrokeProfile stroke)
        {
            Object.DestroyImmediate(style);
            Object.DestroyImmediate(shape);
            Object.DestroyImmediate(stroke);
        }

        private static DeucarianIconButtonPalette CreateButtonPalette()
        {
            return new DeucarianIconButtonPalette(
                new Color(0.1f, 0.1f, 0.1f, 1f),
                new Color(0.2f, 0.2f, 0.2f, 1f),
                new Color(0.3f, 0.3f, 0.3f, 1f),
                new Color(0.4f, 0.4f, 0.4f, 1f),
                new Color(0.05f, 0.05f, 0.05f, 0.5f),
                Color.white,
                Color.gray,
                Color.white,
                Color.cyan,
                Color.black,
                new Color(0.2f, 0.35f, 0.5f, 0.8f),
                new Color(0.4f, 0.75f, 1f, 1f));
        }

        private static DeucarianScrubberPalette CreateScrubberPalette()
        {
            return new DeucarianScrubberPalette(
                new Color(0.1f, 0.1f, 0.1f, 0.5f),
                new Color(0.25f, 0.25f, 0.25f, 0.5f),
                new Color(0.3f, 0.7f, 1f, 0.8f),
                Color.white,
                new Color(0.2f, 0.35f, 0.5f, 0.8f));
        }

        private static void AssertBorder(VisualElement element, float expectedWidth, Color expectedColor)
        {
            AssertBorderWidth(element, expectedWidth);
            Assert.AreEqual(expectedColor, element.style.borderLeftColor.value);
            Assert.AreEqual(expectedColor, element.style.borderRightColor.value);
            Assert.AreEqual(expectedColor, element.style.borderTopColor.value);
            Assert.AreEqual(expectedColor, element.style.borderBottomColor.value);
        }

        private static void AssertBorderWidth(VisualElement element, float expectedWidth)
        {
            Assert.That(element.style.borderLeftWidth.value, Is.EqualTo(expectedWidth).Within(0.0001f));
            Assert.That(element.style.borderRightWidth.value, Is.EqualTo(expectedWidth).Within(0.0001f));
            Assert.That(element.style.borderTopWidth.value, Is.EqualTo(expectedWidth).Within(0.0001f));
            Assert.That(element.style.borderBottomWidth.value, Is.EqualTo(expectedWidth).Within(0.0001f));
        }

        private static void AssertRadius(VisualElement element, float expectedRadius)
        {
            Assert.That(
                element.style.borderTopLeftRadius.value.value,
                Is.EqualTo(expectedRadius).Within(0.0001f));
            Assert.That(
                element.style.borderTopRightRadius.value.value,
                Is.EqualTo(expectedRadius).Within(0.0001f));
            Assert.That(
                element.style.borderBottomLeftRadius.value.value,
                Is.EqualTo(expectedRadius).Within(0.0001f));
            Assert.That(
                element.style.borderBottomRightRadius.value.value,
                Is.EqualTo(expectedRadius).Within(0.0001f));
        }

        private static void SetRadius(VisualElement element, float radius)
        {
            element.style.borderTopLeftRadius = radius;
            element.style.borderTopRightRadius = radius;
            element.style.borderBottomLeftRadius = radius;
            element.style.borderBottomRightRadius = radius;
        }
    }
}
