using Deucarian.Common;
using Deucarian.Theming;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.UI.Tests
{
    public sealed class DeucarianUITests
    {
        [Test]
        public void DefaultPanelMotionUsesSharedEasing()
        {
            DeucarianMotionProfile profile = DeucarianMotionProfile.UiPanel;

            Assert.AreEqual(DeucarianEasing.EaseOutSoftBack, profile.EnterEasing);
            Assert.AreEqual(DeucarianEasing.EaseInCubic, profile.ExitEasing);
            Assert.That(profile.EnterSeconds, Is.EqualTo(0.18f).Within(0.0001f));
            Assert.That(profile.ExitSeconds, Is.EqualTo(0.14f).Within(0.0001f));
        }

        [Test]
        public void AnimatedVisibilityImmediateProgressAppliesOpacityScaleAndTranslate()
        {
            VisualElement element = new VisualElement();

            DeucarianAnimatedVisibility.ApplyProgress(
                element,
                DeucarianMotionProfile.UiPanel,
                1f,
                true);

            Assert.That(element.style.opacity.value, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(element.style.scale.value.value.x, Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void IconSwapConfiguresIconsIntoSingleAbsoluteSlot()
        {
            VisualElement first = new VisualElement();
            VisualElement second = new VisualElement();

            DeucarianIconSwap.ConfigureIconSlot(first, 32f, 18f);
            DeucarianIconSwap.ConfigureIconSlot(second, 32f, 18f);

            Assert.AreEqual(Position.Absolute, first.style.position.value);
            Assert.AreEqual(Position.Absolute, second.style.position.value);
            Assert.That(first.style.left.value.value, Is.EqualTo(second.style.left.value.value).Within(0.0001f));
            Assert.That(first.style.top.value.value, Is.EqualTo(second.style.top.value.value).Within(0.0001f));
        }

        [Test]
        public void IconSwapImmediateShowsExactlyOneIcon()
        {
            VisualElement first = new VisualElement();
            VisualElement second = new VisualElement();

            DeucarianIconSwap.SetImmediate(first, second, true);

            Assert.AreEqual(DisplayStyle.Flex, first.style.display.value);
            Assert.AreEqual(DisplayStyle.None, second.style.display.value);
            Assert.That(first.style.opacity.value, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(second.style.opacity.value, Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void ControlIslandCalculatesCompactPanelWidth()
        {
            float width = DeucarianControlIslandStyle.CalculatePanelWidth(
                DeucarianControlIslandStyle.CompactPanel,
                DeucarianControlIslandStyle.RoundedSquareButton,
                4);

            Assert.That(width, Is.EqualTo(160f).Within(0.0001f));
        }

        [TestCase(DeucarianThemeStyleIds.FrostedGlass, 40f, 32f, 18f, 112f, 28f)]
        [TestCase(DeucarianThemeStyleIds.FluentAcrylic, 38f, 30f, 17f, 104f, 26f)]
        [TestCase(DeucarianThemeStyleIds.MaterialDark, 36f, 28f, 16f, 96f, 24f)]
        public void ControlIslandProfilesResolveBuiltInStyleDensity(
            string styleId,
            float rowHeight,
            float buttonSize,
            float iconSize,
            float scrubberWidth,
            float scrubberHeight)
        {
            DeucarianThemeStyle style = DeucarianThemeStylePresets.CreateRuntimeStyle(styleId);
            try
            {
                DeucarianControlIslandProfile profile = DeucarianControlIslandProfiles.Resolve(style);

                Assert.AreEqual(styleId, profile.StyleId);
                Assert.That(profile.RowHeight, Is.EqualTo(rowHeight).Within(0.0001f));
                Assert.That(profile.ButtonSize, Is.EqualTo(buttonSize).Within(0.0001f));
                Assert.That(profile.IconSize, Is.EqualTo(iconSize).Within(0.0001f));
                Assert.That(profile.CompactScrubberWidth, Is.EqualTo(scrubberWidth).Within(0.0001f));
                Assert.That(profile.CompactScrubberHeight, Is.EqualTo(scrubberHeight).Within(0.0001f));
                Assert.That(profile.ItemHorizontalMargin, Is.EqualTo(4f).Within(0.0001f));
                Assert.That(profile.VerticalPadding, Is.EqualTo(4f).Within(0.0001f));
            }
            finally
            {
                Object.DestroyImmediate(style);
            }
        }

        [Test]
        public void ControlIslandProfilesUseFrostedFallbackForUnknownOrMissingStyles()
        {
            Assert.AreEqual(
                DeucarianThemeStyleIds.FrostedGlass,
                DeucarianControlIslandProfiles.Resolve((DeucarianThemeStyle)null).StyleId);
            Assert.AreEqual(
                DeucarianThemeStyleIds.FrostedGlass,
                DeucarianControlIslandProfiles.Resolve("deucarian.style.unknown").StyleId);
        }

        [TestCase(DeucarianThemeDensity.Comfortable, 40f, 32f, 18f)]
        [TestCase(DeucarianThemeDensity.Standard, 38f, 30f, 17f)]
        [TestCase(DeucarianThemeDensity.Compact, 36f, 28f, 16f)]
        public void ControlIslandProfilesResolveExplicitSemanticDensity(
            DeucarianThemeDensity density,
            float rowHeight,
            float buttonSize,
            float iconSize)
        {
            DeucarianThemeStyle style = DeucarianThemeStylePresets.CreateRuntimeStyle(
                DeucarianThemeStyleIds.FrostedGlass);
            try
            {
                style.SetComposition(null, null, null, density, true);

                DeucarianControlIslandProfile profile = DeucarianControlIslandProfiles.Resolve(style);

                Assert.AreEqual(density, profile.Density);
                Assert.That(profile.RowHeight, Is.EqualTo(rowHeight).Within(0.0001f));
                Assert.That(profile.ButtonSize, Is.EqualTo(buttonSize).Within(0.0001f));
                Assert.That(profile.IconSize, Is.EqualTo(iconSize).Within(0.0001f));
            }
            finally
            {
                Object.DestroyImmediate(style);
            }
        }

        [Test]
        public void ControlIslandUsesShapeProfileWithIndependentDensity()
        {
            DeucarianThemeStyle style = DeucarianThemeStylePresets.CreateRuntimeStyle(
                DeucarianThemeStyleIds.FrostedGlass);
            DeucarianThemeShapeProfile square = ScriptableObject.CreateInstance<DeucarianThemeShapeProfile>();
            try
            {
                square.Configure(
                    DeucarianThemePresentationProfileIds.Shape.Square,
                    "Square",
                    "Square test shape.",
                    0f);
                style.SetComposition(null, square, null, DeucarianThemeDensity.Compact, true);
                DeucarianControlIslandProfile profile = DeucarianControlIslandProfiles.Resolve(style);
                VisualElement panel = new VisualElement();
                Button button = new Button();

                DeucarianControlIslandStyle.ApplyPanel(panel, profile.CreatePanelChrome(), style);
                DeucarianControlIslandStyle.ApplyIconButton(button, profile.CreateIconButtonChrome(), style);

                Assert.AreEqual(DeucarianThemeDensity.Compact, profile.Density);
                Assert.That(profile.RowHeight, Is.EqualTo(36f).Within(0.0001f));
                AssertCornerRadius(panel, 0f);
                AssertCornerRadius(button, 0f);
            }
            finally
            {
                Object.DestroyImmediate(square);
                Object.DestroyImmediate(style);
            }
        }

        [Test]
        public void ControlIslandProfileAppliesSharedScrubberSpacingAndCalculatesWidth()
        {
            DeucarianControlIslandProfile profile = DeucarianControlIslandProfiles.FluentAcrylic;
            VisualElement scrubber = new VisualElement();

            DeucarianControlIslandStyle.ApplyCompactScrubber(scrubber, profile);

            Assert.That(scrubber.style.width.value.value, Is.EqualTo(104f).Within(0.0001f));
            Assert.That(scrubber.style.height.value.value, Is.EqualTo(26f).Within(0.0001f));
            Assert.That(scrubber.style.marginLeft.value.value, Is.EqualTo(4f).Within(0.0001f));
            Assert.That(scrubber.style.marginRight.value.value, Is.EqualTo(4f).Within(0.0001f));
            Assert.That(profile.CalculatePanelWidth(2, 1), Is.EqualTo(188f).Within(0.0001f));
        }

        [Test]
        public void LegacyScrubberMarginAliasesSharedButtonMargin()
        {
            Assert.AreEqual(
                DeucarianControlIslandStyle.DefaultButtonMargin,
                DeucarianControlIslandStyle.DefaultCompactScrubberHorizontalMargin);
        }

        [TestCase(DeucarianThemeStyleIds.FrostedGlass, 16f, 12f)]
        [TestCase(DeucarianThemeStyleIds.FluentAcrylic, 8f, 4f)]
        [TestCase(DeucarianThemeStyleIds.MaterialDark, 4f, 0f)]
        public void ControlIslandUsesConcentricThemeStyleCornerRadii(
            string styleId,
            float expectedPanelRadius,
            float expectedButtonRadius)
        {
            DeucarianThemeStyle style = DeucarianThemeStylePresets.CreateRuntimeStyle(styleId);
            try
            {
                VisualElement panel = new VisualElement();
                Button button = new Button();
                DeucarianControlIslandProfile profile = DeucarianControlIslandProfiles.Resolve(style);

                DeucarianControlIslandStyle.ApplyPanel(
                    panel,
                    profile.CreatePanelChrome(),
                    style);
                DeucarianControlIslandStyle.ApplyIconButton(
                    button,
                    profile.CreateIconButtonChrome(),
                    style);

                AssertCornerRadius(panel, expectedPanelRadius);
                AssertCornerRadius(button, expectedButtonRadius);
                Assert.That(panel.style.height.value.value, Is.EqualTo(profile.RowHeight).Within(0.0001f));
                Assert.That(button.style.width.value.value, Is.EqualTo(profile.ButtonSize).Within(0.0001f));
            }
            finally
            {
                Object.DestroyImmediate(style);
            }
        }

        [Test]
        public void ControlIslandSupportsCustomNestedButtonInset()
        {
            DeucarianThemeStyle style = DeucarianThemeStylePresets.CreateRuntimeStyle(
                DeucarianThemeStyleIds.FrostedGlass);
            try
            {
                Button button = new Button();

                DeucarianControlIslandStyle.ApplyIconButton(
                    button,
                    DeucarianControlIslandStyle.RoundedSquareButton,
                    style,
                    6f);

                AssertCornerRadius(button, 10f);
            }
            finally
            {
                Object.DestroyImmediate(style);
            }
        }

        [Test]
        public void ControlIslandLegacyOverloadsKeepChromeCornerRadii()
        {
            VisualElement panel = new VisualElement();
            Button button = new Button();

            DeucarianControlIslandStyle.ApplyPanel(
                panel,
                DeucarianControlIslandStyle.CompactPanel);
            DeucarianControlIslandStyle.ApplyIconButton(
                button,
                DeucarianControlIslandStyle.RoundedSquareButton);

            AssertCornerRadius(panel, DeucarianControlIslandStyle.DefaultPanelCornerRadius);
            AssertCornerRadius(button, DeucarianControlIslandStyle.DefaultButtonCornerRadius);
        }

        [Test]
        public void IconButtonPaletteResolvesStatefulColors()
        {
            DeucarianIconButtonPalette palette = new DeucarianIconButtonPalette(
                Color.black,
                Color.gray,
                Color.red,
                Color.green,
                Color.blue,
                Color.white,
                Color.cyan,
                Color.magenta,
                Color.yellow,
                Color.clear,
                Color.grey,
                Color.white);

            DeucarianIconButtonVisualState selected =
                new DeucarianIconButtonVisualState(true, true, true, false, false, false);
            DeucarianIconButtonVisualState disabled =
                new DeucarianIconButtonVisualState(true, false, true, true, true, true);

            Assert.AreEqual(Color.green, palette.ResolveBackground(selected));
            Assert.AreEqual(Color.yellow, palette.ResolveIcon(selected));
            Assert.AreEqual(Color.blue, palette.ResolveBackground(disabled));
            Assert.AreEqual(Color.clear, palette.ResolveIcon(disabled));
        }

        [Test]
        public void ScrubberStyleAppliesStableCompactGeometry()
        {
            VisualElement scrubber = new VisualElement();
            VisualElement track = new VisualElement();
            VisualElement fill = new VisualElement();
            VisualElement handle = new VisualElement();
            DeucarianScrubberPalette palette = new DeucarianScrubberPalette(
                Color.black,
                Color.gray,
                Color.green,
                Color.white,
                Color.cyan);

            DeucarianScrubberStyle.Apply(
                scrubber,
                track,
                fill,
                handle,
                DeucarianScrubberMetrics.Compact,
                palette,
                new DeucarianScrubberVisualState(true, true, false, false));

            Assert.That(scrubber.style.paddingLeft.value.value, Is.EqualTo(8f).Within(0.0001f));
            Assert.That(track.style.height.value.value, Is.EqualTo(5f).Within(0.0001f));
            Assert.That(handle.style.width.value.value, Is.EqualTo(14f).Within(0.0001f));
            Assert.That(handle.style.scale.value.value.x, Is.EqualTo(1.06f).Within(0.0001f));
        }

        [Test]
        public void ControlIslandPresetExposesDefaultFrostedControlMetrics()
        {
            DeucarianControlIslandPreset preset = ScriptableObject.CreateInstance<DeucarianControlIslandPreset>();
            try
            {
                Assert.That(preset.RowHeight, Is.EqualTo(40f).Within(0.0001f));
                Assert.That(preset.ButtonSize, Is.EqualTo(32f).Within(0.0001f));
                Assert.That(preset.CompactScrubberHeight, Is.EqualTo(28f).Within(0.0001f));
                Assert.That(preset.CompactScrubberHorizontalMargin, Is.EqualTo(4f).Within(0.0001f));
                Assert.That(preset.ResolveBottomPadding(2), Is.EqualTo(152f).Within(0.0001f));
            }
            finally
            {
                Object.DestroyImmediate(preset);
            }
        }

        private static void AssertCornerRadius(VisualElement element, float expectedRadius)
        {
            Assert.That(element.style.borderTopLeftRadius.value.value, Is.EqualTo(expectedRadius).Within(0.0001f));
            Assert.That(element.style.borderTopRightRadius.value.value, Is.EqualTo(expectedRadius).Within(0.0001f));
            Assert.That(element.style.borderBottomLeftRadius.value.value, Is.EqualTo(expectedRadius).Within(0.0001f));
            Assert.That(element.style.borderBottomRightRadius.value.value, Is.EqualTo(expectedRadius).Within(0.0001f));
        }
    }
}
