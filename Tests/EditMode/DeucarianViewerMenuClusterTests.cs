using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.UI.Tests
{
    public sealed class DeucarianViewerMenuClusterTests
    {
        private GameObject root;
        private DeucarianMorphingMenuTestHost host;
        private DeucarianViewerMenuCluster cluster;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("DeucarianViewerMenuClusterTests");
            host = root.AddComponent<DeucarianMorphingMenuTestHost>();
        }

        [TearDown]
        public void TearDown()
        {
            cluster?.Dispose();
            cluster = null;
            UnityEngine.Object.DestroyImmediate(root);
        }

        [Test]
        public void OwnsCanonicalBodiesGlyphsSlotsAndLayering()
        {
            var informationBody = new VisualElement
            {
                name = "InformationBody"
            };
            var settingsBody = new VisualElement
            {
                name = "SettingsBody"
            };
            cluster = new DeucarianViewerMenuCluster(
                host,
                informationBody,
                settingsBody);

            Assert.AreSame(informationBody, cluster.InformationMenu.Body);
            Assert.AreSame(settingsBody, cluster.SettingsMenu.Body);
            Assert.AreEqual(
                DeucarianMorphingMenuIcon.Information,
                cluster.InformationMenu.CollapsedIcon);
            Assert.AreEqual(
                DeucarianMorphingMenuIcon.Settings,
                cluster.SettingsMenu.CollapsedIcon);
            Assert.That(
                cluster.InformationMenu.RightInset,
                Is.EqualTo(76f).Within(0.0001f));
            Assert.That(
                cluster.SettingsMenu.RightInset,
                Is.EqualTo(24f).Within(0.0001f));
            Assert.IsTrue(cluster.InformationMenu.IsVisible);
            Assert.IsTrue(cluster.SettingsMenu.IsVisible);
            Assert.IsNull(cluster.ExpandedMenu);
            Assert.True(DeucarianUIRuntime.IsConfigured(
                cluster.InformationMenu.Document,
                DeucarianUISurfaceRole.Menu));
            Assert.True(DeucarianUIRuntime.IsConfigured(
                cluster.SettingsMenu.Document,
                DeucarianUISurfaceRole.Menu));
            Assert.AreSame(
                cluster.InformationMenu.RuntimeTooltip.OverlayDocument,
                cluster.SettingsMenu.RuntimeTooltip.OverlayDocument);
            Assert.True(DeucarianUIRuntime.IsConfigured(
                cluster.InformationMenu.RuntimeTooltip.OverlayDocument,
                DeucarianUISurfaceRole.Tooltip));
            Assert.That(
                cluster.InformationMenu.RuntimeTooltip.OverlayDocument
                    .sortingOrder,
                Is.GreaterThan(
                    cluster.InformationMenu.Document.sortingOrder));
            Assert.AreEqual(
                "Open information",
                cluster.InformationMenu.Button.tooltip);
            Assert.AreEqual(
                "Open settings",
                cluster.SettingsMenu.Button.tooltip);
        }

        [Test]
        public void InformationExpansionClaimsEdgeSlotAndCollapseRestoresBoth()
        {
            cluster = new DeucarianViewerMenuCluster(
                host,
                new VisualElement(),
                new VisualElement());

            cluster.SetExpanded(
                DeucarianViewerMenuKind.Information,
                true,
                animate: false);

            Assert.AreEqual(
                DeucarianViewerMenuKind.Information,
                cluster.ExpandedMenu);
            Assert.IsTrue(cluster.InformationMenu.IsExpanded);
            Assert.IsTrue(cluster.InformationMenu.IsVisible);
            Assert.IsFalse(cluster.SettingsMenu.IsVisible);
            Assert.That(
                cluster.InformationMenu.RightInset,
                Is.EqualTo(24f).Within(0.0001f));
            Assert.AreEqual(
                DisplayStyle.None,
                cluster.SettingsMenu.MenuRoot.style.display.value);

            cluster.SetExpanded(
                DeucarianViewerMenuKind.Information,
                false,
                animate: false);

            Assert.IsNull(cluster.ExpandedMenu);
            Assert.IsFalse(cluster.InformationMenu.IsExpanded);
            Assert.IsTrue(cluster.InformationMenu.IsVisible);
            Assert.IsTrue(cluster.SettingsMenu.IsVisible);
            Assert.That(
                cluster.InformationMenu.RightInset,
                Is.EqualTo(76f).Within(0.0001f));
        }

        [Test]
        public void DirectMenuRequestsStayMutuallyExclusiveAndPublishInOrder()
        {
            cluster = new DeucarianViewerMenuCluster(
                host,
                new VisualElement(),
                new VisualElement());
            var events = new List<string>();
            cluster.ExpandedChanged += (kind, value) =>
                events.Add(kind + ":" + value);

            cluster.InformationMenu.SetExpanded(true, animate: false);
            cluster.SettingsMenu.SetExpanded(true, animate: false);

            CollectionAssert.AreEqual(
                new[]
                {
                    "Information:True",
                    "Information:False",
                    "Settings:True"
                },
                events);
            Assert.IsFalse(cluster.InformationMenu.IsExpanded);
            Assert.IsFalse(cluster.InformationMenu.IsVisible);
            Assert.IsTrue(cluster.SettingsMenu.IsExpanded);
            Assert.IsTrue(cluster.SettingsMenu.IsVisible);
            Assert.AreEqual(
                DeucarianViewerMenuKind.Settings,
                cluster.ExpandedMenu);
        }

        [Test]
        public void ExpandedMenuUsesEdgeResponsiveWidth()
        {
            cluster = new DeucarianViewerMenuCluster(
                host,
                new VisualElement(),
                new VisualElement());
            cluster.SetExpanded(
                DeucarianViewerMenuKind.Information,
                true,
                animate: false);

            SendGeometry(cluster.InformationMenu.Root, 200f, 100f);

            Assert.That(
                cluster.InformationMenu.MenuRoot.style.width.value.value,
                Is.EqualTo(152f).Within(0.0001f));
        }

        [Test]
        public void PerMenuDimensionsOverrideSharedCompatibilityDefaults()
        {
            var layout = new DeucarianViewerMenuClusterLayout
            {
                MaximumWidth = 260f,
                ExpandedFallbackHeight = 120f,
                InformationMaximumWidth = 340f,
                InformationExpandedFallbackHeight = 80f
            };
            Assert.That(
                layout.SettingsMaximumWidth,
                Is.EqualTo(260f).Within(0.0001f));
            Assert.That(
                layout.SettingsExpandedFallbackHeight,
                Is.EqualTo(120f).Within(0.0001f));

            cluster = new DeucarianViewerMenuCluster(
                host,
                new VisualElement(),
                new VisualElement(),
                layout);

            Assert.That(
                cluster.InformationMenu.MenuRoot.style.width.value.value,
                Is.EqualTo(340f).Within(0.0001f));
            Assert.That(
                cluster.SettingsMenu.MenuRoot.style.width.value.value,
                Is.EqualTo(260f).Within(0.0001f));

            cluster.SetExpanded(
                DeucarianViewerMenuKind.Information,
                true,
                animate: false);
            Assert.That(
                cluster.InformationMenu.Chrome.style.minHeight.value.value,
                Is.EqualTo(80f).Within(0.0001f));

            cluster.SetExpanded(
                DeucarianViewerMenuKind.Settings,
                true,
                animate: false);
            Assert.That(
                cluster.SettingsMenu.Chrome.style.minHeight.value.value,
                Is.EqualTo(120f).Within(0.0001f));
        }

        [Test]
        public void SharedHooksApplyPerBodyAndDisposeExactlyOncePerMenu()
        {
            int informationThemes = 0;
            int settingsThemes = 0;
            int guardBindings = 0;
            int guardDisposals = 0;
            var layout = new DeucarianViewerMenuClusterLayout
            {
                ApplyInformationBodyTheme = _ => informationThemes++,
                ApplySettingsBodyTheme = _ => settingsThemes++,
                BindInputGuard = _ =>
                {
                    guardBindings++;
                    return new CallbackDisposable(
                        () => guardDisposals++);
                }
            };
            cluster = new DeucarianViewerMenuCluster(
                host,
                new VisualElement(),
                new VisualElement(),
                layout);

            Assert.That(informationThemes, Is.GreaterThan(0));
            Assert.That(settingsThemes, Is.GreaterThan(0));
            Assert.AreEqual(2, guardBindings);
            Assert.AreEqual(0, guardDisposals);

            int informationBefore = informationThemes;
            int settingsBefore = settingsThemes;
            cluster.RefreshPresentation();

            Assert.That(informationThemes, Is.GreaterThan(informationBefore));
            Assert.That(settingsThemes, Is.GreaterThan(settingsBefore));
            cluster.Dispose();
            cluster.Dispose();
            cluster = null;
            Assert.AreEqual(2, guardDisposals);
        }

        [Test]
        public void VisibilityAndLifecycleAreForwardedWithoutLosingState()
        {
            cluster = new DeucarianViewerMenuCluster(
                host,
                new VisualElement(),
                new VisualElement());
            cluster.SetExpanded(
                DeucarianViewerMenuKind.Settings,
                true,
                animate: false);

            cluster.SetVisible(false);

            Assert.IsFalse(cluster.IsVisible);
            Assert.IsFalse(cluster.InformationMenu.IsVisible);
            Assert.IsFalse(cluster.SettingsMenu.IsVisible);

            cluster.OnDisable();
            Assert.IsFalse(cluster.InformationMenu.Document.enabled);
            Assert.IsFalse(cluster.SettingsMenu.Document.enabled);
            cluster.OnEnable();
            cluster.SetVisible(true);

            Assert.IsTrue(cluster.SettingsMenu.Document.enabled);
            Assert.IsTrue(cluster.SettingsMenu.IsVisible);
            Assert.IsFalse(cluster.InformationMenu.IsVisible);
            Assert.AreEqual(
                DeucarianViewerMenuKind.Settings,
                cluster.ExpandedMenu);
        }

        private static void SendGeometry(
            VisualElement target,
            float width,
            float height)
        {
            using (GeometryChangedEvent geometry =
                   GeometryChangedEvent.GetPooled(
                       Rect.zero,
                       new Rect(0f, 0f, width, height)))
            {
                geometry.target = target;
                target.SendEvent(geometry);
            }
        }

        private sealed class CallbackDisposable : IDisposable
        {
            private readonly Action callback;

            public CallbackDisposable(Action callback)
            {
                this.callback = callback;
            }

            public void Dispose()
            {
                callback();
            }
        }
    }
}
