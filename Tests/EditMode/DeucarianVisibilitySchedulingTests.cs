using System;
using Deucarian.Common;
using Deucarian.Tweens;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.UI.Tests
{
    public sealed class DeucarianVisibilitySchedulingTests
    {
        private GameObject root;
        private DeucarianVisibilityTestHost host;
        private TweenScheduler scheduler;
        private VisualElement element;
        private DeucarianAnimatedVisibility visibility;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("Visibility host");
            host = root.AddComponent<DeucarianVisibilityTestHost>();
            scheduler = new TweenScheduler();
            element = new VisualElement();
            visibility = new DeucarianAnimatedVisibility(host, element,
                new DeucarianMotionProfile(1, 1, DeucarianEasing.Linear, DeucarianEasing.Linear, 0.8f, 1, -0.1f),
                scheduler);
        }

        [TearDown]
        public void TearDown()
        {
            scheduler.Dispose();
            UnityEngine.Object.DestroyImmediate(root);
        }

        [Test]
        public void RepeatedTargetUsesOneRegistrationAndCompletesEachCallbackOnce()
        {
            int completed = 0;
            Action callback = () => completed++;
            visibility.SetVisible(true, callback);
            scheduler.Advance(0.25f, 10);
            visibility.SetVisible(true, callback);
            visibility.SetVisible(true, () => completed++);
            Assert.That(scheduler.ActiveCount, Is.EqualTo(1));
            Assert.That(visibility.Progress, Is.EqualTo(0.25f).Within(0.0001f));
            Assert.That(element.style.opacity.value, Is.EqualTo(0.25f).Within(0.0001f));
            scheduler.Advance(0.75f, 0);
            Assert.That(completed, Is.EqualTo(2));
            Assert.That(scheduler.ActiveCount, Is.Zero);
            Assert.That(visibility.IsAnimating, Is.False);
        }

        [Test]
        public void ReversalStartsAtCurrentAppearanceAndCancelsSupersededCompletion()
        {
            int shown = 0, hidden = 0;
            visibility.SetVisible(true, () => shown++);
            scheduler.Advance(0.4f, 0.4f);
            visibility.SetVisible(false, () => hidden++);
            Assert.That(visibility.Progress, Is.EqualTo(0.4f).Within(0.0001f));
            Assert.That(element.style.display.value, Is.EqualTo(DisplayStyle.Flex));
            scheduler.Advance(0.4f, 0.4f);
            Assert.That(shown, Is.Zero);
            Assert.That(hidden, Is.EqualTo(1));
            Assert.That(element.style.display.value, Is.EqualTo(DisplayStyle.None));
            Assert.That(scheduler.ActiveCount, Is.Zero);
        }

        [Test]
        public void CompletionMayStartTheOppositeTransitionWithoutLosingItsRegistration()
        {
            int hidden = 0;
            visibility.SetVisible(true, () => visibility.SetVisible(false, () => hidden++));
            scheduler.Advance(1, 1);
            Assert.That(visibility.IsAnimating, Is.True);
            Assert.That(visibility.TargetVisible, Is.False);
            Assert.That(scheduler.ActiveCount, Is.EqualTo(1));
            scheduler.Advance(1, 1);
            Assert.That(hidden, Is.EqualTo(1));
            Assert.That(scheduler.ActiveCount, Is.Zero);
        }

        [Test]
        public void ImmediateRequestSettlesAnActiveTargetAndCancelsItsOldCallback()
        {
            int oldCompletion = 0, immediateCompletion = 0;
            visibility.SetVisible(true, () => oldCompletion++);
            scheduler.Advance(0.2f, 0.2f);
            visibility.SetVisible(true, false, () => immediateCompletion++);
            scheduler.Advance(1, 1);
            Assert.That(visibility.Progress, Is.EqualTo(1));
            Assert.That(oldCompletion, Is.Zero);
            Assert.That(immediateCompletion, Is.EqualTo(1));
            Assert.That(scheduler.ActiveCount, Is.Zero);
        }

        [TestCase("stop")]
        [TestCase("disabled")]
        [TestCase("inactive")]
        [TestCase("destroyed")]
        [TestCase("disposed")]
        public void CancellationAndTargetLossReleaseWorkWithoutCallingCompletion(string cause)
        {
            int completed = 0;
            visibility.SetVisible(true, () => completed++);
            scheduler.Advance(0.2f, 0.2f);
            switch (cause)
            {
                case "stop": visibility.Stop(); break;
                case "disabled": host.enabled = false; break;
                case "inactive": root.SetActive(false); break;
                case "destroyed": UnityEngine.Object.DestroyImmediate(root); break;
                case "disposed": scheduler.Dispose(); break;
            }
            if (cause != "disposed") scheduler.Advance(1, 1);
            Assert.That(completed, Is.Zero);
            Assert.That(visibility.IsAnimating, Is.False);
            Assert.That(scheduler.ActiveCount, Is.Zero);
        }

        [Test]
        public void ZeroDurationCompletesWithoutLeavingScheduledWork()
        {
            visibility = new DeucarianAnimatedVisibility(host, element, default, scheduler);
            int completed = 0;
            visibility.SetVisible(true, () => completed++);
            Assert.That(completed, Is.EqualTo(1));
            Assert.That(visibility.Progress, Is.EqualTo(1));
            Assert.That(scheduler.ActiveCount, Is.Zero);
        }
    }

    public sealed class DeucarianVisibilityTestHost : MonoBehaviour { }
}
