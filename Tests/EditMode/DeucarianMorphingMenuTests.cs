using System;
using Deucarian.Theming;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.UI.Tests
{
    public sealed class DeucarianMorphingMenuTests
    {
        private GameObject root;
        private DeucarianMorphingMenuTestHost host;
        private DeucarianMorphingMenu menu;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("DeucarianMorphingMenuTests");
            host = root.AddComponent<DeucarianMorphingMenuTestHost>();
        }

        [TearDown]
        public void TearDown()
        {
            menu?.Dispose();
            menu = null;
            UnityEngine.Object.DestroyImmediate(root);
        }

        [Test]
        public void OwnsCanonicalScaffoldAndOverlaysBothStateIcons()
        {
            VisualElement body = new VisualElement
            {
                name = "ConsumerBody"
            };
            var bodyTooltipControl = new Button
            {
                tooltip = "Consumer tooltip"
            };
            body.Add(bodyTooltipControl);

            menu = new DeucarianMorphingMenu(host, body);

            Assert.AreEqual(
                DeucarianMorphingMenu.DocumentObjectName,
                menu.Document.gameObject.name);
            Assert.AreEqual(host.transform, menu.Document.transform.parent);
            Assert.AreSame(
                menu.Root,
                menu.Document.rootVisualElement.Q<VisualElement>(
                    DeucarianMorphingMenu.RootName));
            Assert.AreSame(
                menu.Chrome,
                menu.Root.Q<VisualElement>(
                    DeucarianMorphingMenu.ChromeName));
            Assert.AreSame(
                menu.Button,
                menu.Root.Q<Button>(DeucarianMorphingMenu.ButtonName));
            Assert.AreSame(body, menu.Panel[0]);
            Assert.NotNull(menu.RuntimeTooltip);
            Assert.IsTrue(menu.RuntimeTooltip.IsBound(menu.Button));
            Assert.IsTrue(
                menu.RuntimeTooltip.IsBound(bodyTooltipControl));
            Assert.That(
                menu.RuntimeTooltip.OverlayDocument.sortingOrder,
                Is.EqualTo(DeucarianUIDepth.Tooltip));
            Assert.AreEqual(PickingMode.Ignore, menu.Root.pickingMode);
            Assert.AreEqual(PickingMode.Ignore, menu.MenuRoot.pickingMode);
            Assert.AreEqual(PickingMode.Position, menu.Chrome.pickingMode);
            Assert.AreEqual(PickingMode.Ignore, menu.ButtonHost.pickingMode);
            Assert.AreEqual(PickingMode.Position, menu.Button.pickingMode);
            Assert.AreEqual(DisplayStyle.None, menu.Scrim.style.display.value);
            Assert.AreEqual(PickingMode.Ignore, menu.Scrim.pickingMode);
            Assert.That(
                menu.MenuRoot.style.right.value.value,
                Is.EqualTo(
                    DeucarianMorphingMenuLayout.ReferenceEdgeMargin)
                    .Within(0.0001f));
            Assert.That(
                menu.MenuRoot.style.top.value.value,
                Is.EqualTo(
                    DeucarianMorphingMenuLayout.ReferenceEdgeMargin)
                    .Within(0.0001f));
            Assert.That(
                menu.Chrome.style.width.value.value,
                Is.EqualTo(DeucarianMorphingMenuMotion.CollapsedSize)
                    .Within(0.0001f));
            Assert.That(
                menu.Chrome.style.height.value.value,
                Is.EqualTo(DeucarianMorphingMenuMotion.CollapsedSize)
                    .Within(0.0001f));
            Assert.That(
                menu.Button.style.width.value.value,
                Is.EqualTo(
                    DeucarianControlIslandVisualStyle
                        .CompactIconButton.Size)
                    .Within(0.0001f));
            Assert.That(
                menu.MenuIcon.style.width.value.value,
                Is.EqualTo(
                    DeucarianControlIslandVisualStyle
                        .ControlIslandIconSize)
                    .Within(0.0001f));
            Assert.AreEqual(
                Position.Absolute,
                menu.MenuIcon.style.position.value);
            Assert.AreEqual(
                Position.Absolute,
                menu.CloseIcon.style.position.value);
            Assert.That(
                menu.MenuIcon.style.left.value.value,
                Is.EqualTo(menu.CloseIcon.style.left.value.value)
                    .Within(0.0001f));
            Assert.That(
                menu.MenuIcon.style.top.value.value,
                Is.EqualTo(menu.CloseIcon.style.top.value.value)
                    .Within(0.0001f));
            Assert.AreEqual(3, menu.MenuIcon.childCount);
            Assert.AreEqual(2, menu.CloseIcon.childCount);
            for (int i = 0; i < 3; i++)
            {
                VisualElement line = menu.MenuIcon.Q<VisualElement>(
                    DeucarianMorphingMenu.MenuIconLineNamePrefix + i);
                VisualElement knob = menu.MenuIcon.Q<VisualElement>(
                    DeucarianMorphingMenu.MenuIconKnobNamePrefix + i);
                Assert.NotNull(line);
                Assert.NotNull(knob);
                Assert.That(
                    line.style.top.value.value,
                    Is.EqualTo(2f + i * 6f).Within(0.0001f));
                Assert.That(
                    knob.style.left.value.value,
                    Is.EqualTo(i == 1 ? 0f : 10f).Within(0.0001f));
            }
        }

        [Test]
        public void ImmediateStateOwnsDimensionsPickingOpacityAndCopy()
        {
            menu = new DeucarianMorphingMenu(
                host,
                new VisualElement(),
                new DeucarianMorphingMenuLayout
                {
                    OpenTooltip = "Settings",
                    CloseTooltip = "Close settings"
                });

            Assert.IsFalse(menu.IsExpanded);
            Assert.AreEqual("Settings", menu.Button.tooltip);
            Assert.AreEqual(
                DisplayStyle.None,
                menu.Panel.style.display.value);
            Assert.That(
                menu.Chrome.style.width.value.value,
                Is.EqualTo(DeucarianMorphingMenuMotion.CollapsedSize)
                    .Within(0.0001f));

            menu.SetExpanded(true, animate: false);

            Assert.IsTrue(menu.IsExpanded);
            Assert.AreEqual("Close settings", menu.Button.tooltip);
            Assert.AreEqual(
                DisplayStyle.Flex,
                menu.Panel.style.display.value);
            Assert.AreEqual(
                Visibility.Visible,
                menu.Panel.style.visibility.value);
            Assert.AreEqual(PickingMode.Position, menu.Panel.pickingMode);
            Assert.That(
                menu.MenuIcon.style.opacity.value,
                Is.EqualTo(0f).Within(0.0001f));
            Assert.That(
                menu.CloseIcon.style.opacity.value,
                Is.EqualTo(1f).Within(0.0001f));

            menu.SetExpanded(false, animate: false);

            Assert.AreEqual(
                DisplayStyle.None,
                menu.Panel.style.display.value);
            Assert.AreEqual(PickingMode.Ignore, menu.Panel.pickingMode);
        }

        [Test]
        public void IntegrationHooksStayConsumerNeutralAndAreDisposed()
        {
            int themeApplications = 0;
            int guardBindings = 0;
            int guardDisposals = 0;
            var layout = new DeucarianMorphingMenuLayout
            {
                ShouldAnimate = () => false,
                ApplyBodyTheme = _ => themeApplications++,
                BindInputGuard = _ =>
                {
                    guardBindings++;
                    return new CallbackDisposable(
                        () => guardDisposals++);
                }
            };
            menu = new DeucarianMorphingMenu(
                host,
                new VisualElement(),
                layout);

            Assert.That(themeApplications, Is.GreaterThan(0));
            Assert.AreEqual(1, guardBindings);
            Assert.AreEqual(0, guardDisposals);

            menu.Dispose();
            menu.Dispose();
            menu = null;

            Assert.AreEqual(1, guardDisposals);
        }

        [Test]
        public void HoverSelectionAndEscapeUseCanonicalPackageBehavior()
        {
            DeucarianTheme theme = DeucarianViewerReferenceThemePreset
                .Resolve()
                .DefaultTheme;
            menu = new DeucarianMorphingMenu(
                host,
                new VisualElement());
            var normal = new DeucarianIconButtonVisualState(
                true,
                true,
                false,
                false,
                false,
                false);
            AssertColor(
                DeucarianControlIslandVisualStyle.ResolveButtonBackground(
                    theme,
                    normal,
                    host),
                menu.Button.style.backgroundColor.value);

            using (MouseEnterEvent mouseEnter = MouseEnterEvent.GetPooled(
                       Vector2.zero,
                       0,
                       0,
                       Vector2.zero,
                       EventModifiers.None))
            {
                mouseEnter.target = menu.Button;
                menu.Button.SendEvent(mouseEnter);
            }

            var hovered = new DeucarianIconButtonVisualState(
                true,
                true,
                false,
                true,
                false,
                false);
            AssertColor(
                DeucarianControlIslandVisualStyle.ResolveButtonBackground(
                    theme,
                    hovered,
                    host),
                menu.Button.style.backgroundColor.value);
            Assert.That(
                menu.Button.style.scale.value.value.x,
                Is.EqualTo(
                    DeucarianIconButtonStyle.ResolveButtonScale(hovered).x)
                    .Within(0.0001f));

            menu.SetExpanded(true, animate: false);
            using (KeyDownEvent keyDown = KeyDownEvent.GetPooled(
                       '\0',
                       KeyCode.Escape,
                       EventModifiers.None))
            {
                keyDown.target = menu.Root;
                menu.Root.SendEvent(keyDown);
            }

            Assert.IsFalse(menu.IsExpanded);
            Assert.AreEqual(
                DisplayStyle.None,
                menu.Panel.style.display.value);
        }

        [Test]
        public void GeometryChangesResolveTheReferenceTopRightWidth()
        {
            menu = new DeucarianMorphingMenu(
                host,
                new VisualElement());
            using (GeometryChangedEvent geometry =
                   GeometryChangedEvent.GetPooled(
                       Rect.zero,
                       new Rect(0f, 0f, 200f, 100f)))
            {
                geometry.target = menu.Root;
                menu.Root.SendEvent(geometry);
            }

            Assert.That(
                menu.MenuRoot.style.width.value.value,
                Is.EqualTo(152f).Within(0.0001f));
        }

        [Test]
        public void DedicatedDocumentPreservesConsumerDocumentAndContent()
        {
            UIDocument existingDocument = root.AddComponent<UIDocument>();
            PanelSettings existingSettings =
                ScriptableObject.CreateInstance<PanelSettings>();
            existingDocument.panelSettings = existingSettings;
            existingDocument.sortingOrder = 77;
            VisualElement existingContent = new VisualElement
            {
                name = "ExistingConsumerContent"
            };
            existingDocument.rootVisualElement.Add(existingContent);
            try
            {
                menu = new DeucarianMorphingMenu(
                    host,
                    new VisualElement());

                Assert.AreNotSame(existingDocument, menu.Document);
                Assert.AreNotSame(
                    existingSettings,
                    menu.Document.panelSettings);
                Assert.IsNull(menu.Document.transform.parent);
                Assert.AreSame(
                    existingContent,
                    existingDocument.rootVisualElement.Q<VisualElement>(
                        "ExistingConsumerContent"));
                Assert.AreSame(
                    existingSettings,
                    existingDocument.panelSettings);
                Assert.AreEqual(77, existingDocument.sortingOrder);

                menu.OnDisable();
                Assert.IsFalse(menu.Document.enabled);
                menu.OnEnable();
                Assert.IsTrue(menu.Document.enabled);

                menu.Dispose();
                menu = null;

                Assert.AreSame(
                    existingContent,
                    existingDocument.rootVisualElement.Q<VisualElement>(
                        "ExistingConsumerContent"));
                Assert.AreSame(
                    existingSettings,
                    existingDocument.panelSettings);
                Assert.AreEqual(77, existingDocument.sortingOrder);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(existingSettings);
            }
        }

        [Test]
        public void DedicatedDocumentFindsConflictUnderInactiveHost()
        {
            UIDocument existingDocument = root.AddComponent<UIDocument>();
            PanelSettings existingSettings =
                ScriptableObject.CreateInstance<PanelSettings>();
            existingDocument.panelSettings = existingSettings;
            root.SetActive(false);
            try
            {
                menu = new DeucarianMorphingMenu(
                    host,
                    new VisualElement());

                Assert.AreNotSame(
                    existingSettings,
                    menu.Document.panelSettings);
                Assert.IsNull(menu.Document.transform.parent);
                Assert.IsFalse(menu.Document.enabled);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(existingSettings);
            }
        }

        [Test]
        public void ThemeProviderCanAppearAfterConstructionAndUnsubscribes()
        {
            int themeApplications = 0;
            menu = new DeucarianMorphingMenu(
                host,
                new VisualElement(),
                new DeucarianMorphingMenuLayout
                {
                    ApplyBodyTheme = _ => themeApplications++
                });
            DeucarianThemeProvider provider =
                root.AddComponent<DeucarianThemeProvider>();
            DeucarianTheme theme = DeucarianViewerReferenceThemePreset
                .Resolve()
                .DefaultTheme;

            menu.RefreshPresentation();
            int afterDiscovery = themeApplications;
            provider.SetTheme(theme);

            Assert.That(themeApplications, Is.GreaterThan(afterDiscovery));
            menu.Dispose();
            menu = null;
            int afterDispose = themeApplications;
            provider.SetTheme(theme);
            Assert.AreEqual(afterDispose, themeApplications);
        }

        [Test]
        public void VisibilityAndReferenceWidthArePackageOwned()
        {
            menu = new DeucarianMorphingMenu(
                host,
                new VisualElement());

            menu.SetVisible(false);

            Assert.IsFalse(menu.IsVisible);
            Assert.AreEqual(
                DisplayStyle.None,
                menu.MenuRoot.style.display.value);
            Assert.That(
                DeucarianMorphingMenu.ResolveExpandedWidth(1920f),
                Is.EqualTo(
                    DeucarianMorphingMenuLayout.ReferenceMaximumWidth)
                    .Within(0.0001f));
            Assert.That(
                DeucarianMorphingMenu.ResolveExpandedWidth(70f),
                Is.EqualTo(DeucarianMorphingMenuMotion.CollapsedSize)
                    .Within(0.0001f));
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

        private static void AssertColor(Color expected, Color actual)
        {
            Assert.That(actual.r, Is.EqualTo(expected.r).Within(0.0001f));
            Assert.That(actual.g, Is.EqualTo(expected.g).Within(0.0001f));
            Assert.That(actual.b, Is.EqualTo(expected.b).Within(0.0001f));
            Assert.That(actual.a, Is.EqualTo(expected.a).Within(0.0001f));
        }
    }

    public sealed class DeucarianMorphingMenuTestHost : MonoBehaviour
    {
    }
}
