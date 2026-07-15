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

        [TestCase(DeucarianThemeStyleIds.FrostedGlass, 16f)]
        [TestCase(DeucarianThemeStyleIds.FluentAcrylic, 8f)]
        [TestCase(DeucarianThemeStyleIds.MaterialDark, 4f)]
        public void ControlIslandUsesThemeStyleCornerRadius(string styleId, float expectedRadius)
        {
            DeucarianThemeStyle style = DeucarianThemeStylePresets.CreateRuntimeStyle(styleId);
            try
            {
                VisualElement panel = new VisualElement();
                Button button = new Button();

                DeucarianControlIslandStyle.ApplyPanel(
                    panel,
                    DeucarianControlIslandStyle.CompactPanel,
                    style);
                DeucarianControlIslandStyle.ApplyIconButton(
                    button,
                    DeucarianControlIslandStyle.RoundedSquareButton,
                    style);

                AssertCornerRadius(panel, expectedRadius);
                AssertCornerRadius(button, expectedRadius);
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
