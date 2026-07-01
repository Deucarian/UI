using Deucarian.Common;
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
    }
}
