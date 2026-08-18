using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.UI.Tests
{
    public sealed class DeucarianMorphingMenuVariantTests
    {
        private GameObject root;
        private DeucarianMorphingMenuTestHost host;
        private DeucarianMorphingMenu menu;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("DeucarianMorphingMenuVariantTests");
            host = root.AddComponent<DeucarianMorphingMenuTestHost>();
        }

        [TearDown]
        public void TearDown()
        {
            menu?.Dispose();
            menu = null;
            Object.DestroyImmediate(root);
        }

        [Test]
        public void DefaultsPreserveSettingsGlyphAndEdgeMarginContract()
        {
            var layout = new DeucarianMorphingMenuLayout
            {
                EdgeMargin = 31f
            };
            menu = new DeucarianMorphingMenu(
                host,
                new VisualElement(),
                layout);

            Assert.AreEqual(
                DeucarianMorphingMenuIcon.Settings,
                menu.CollapsedIcon);
            Assert.AreEqual(
                DeucarianMorphingMenu.MenuIconName,
                menu.MenuIcon.name);
            Assert.AreEqual(3, menu.MenuIcon.childCount);
            Assert.That(menu.RightInset, Is.EqualTo(31f).Within(0.0001f));
            Assert.That(
                menu.MenuRoot.style.right.value.value,
                Is.EqualTo(31f).Within(0.0001f));
            for (int i = 0; i < 3; i++)
            {
                VisualElement line = menu.MenuIcon.ElementAt(i);
                Assert.AreEqual(
                    DeucarianMorphingMenu.MenuIconLineNamePrefix + i,
                    line.name);
                Assert.AreEqual(1, line.childCount);
                Assert.AreEqual(
                    DeucarianMorphingMenu.MenuIconKnobNamePrefix + i,
                    line[0].name);
            }

            Assert.That(
                DeucarianMorphingMenu.ResolveExpandedWidth(200f, 24f, 300f),
                Is.EqualTo(152f).Within(0.0001f));
        }

        [Test]
        public void InformationGlyphOwnsExactPackagePrimitiveTree()
        {
            menu = new DeucarianMorphingMenu(
                host,
                new VisualElement(),
                new DeucarianMorphingMenuLayout
                {
                    CollapsedIcon =
                        DeucarianMorphingMenuIcon.Information
                });

            Assert.AreEqual(
                DeucarianMorphingMenuIcon.Information,
                menu.CollapsedIcon);
            Assert.AreEqual(
                DeucarianMorphingMenu.InformationIconName,
                menu.MenuIcon.name);
            Assert.AreEqual(PickingMode.Ignore, menu.MenuIcon.pickingMode);
            Assert.AreEqual(2, menu.MenuIcon.childCount);

            VisualElement dot = menu.MenuIcon[0];
            Assert.AreEqual(
                DeucarianMorphingMenu.InformationIconDotName,
                dot.name);
            Assert.AreEqual(PickingMode.Ignore, dot.pickingMode);
            AssertStyleRect(dot, 8f, 2f, 2f, 2f);

            VisualElement stem = menu.MenuIcon[1];
            Assert.AreEqual(
                DeucarianMorphingMenu.InformationIconStemName,
                stem.name);
            Assert.AreEqual(PickingMode.Ignore, stem.pickingMode);
            AssertStyleRect(stem, 8f, 7f, 2f, 9f);
            Assert.IsNull(menu.MenuIcon.Q<VisualElement>(
                DeucarianMorphingMenu.MenuIconLineNamePrefix + "0"));
        }

        [Test]
        public void RuntimeIconChangePreservesBodyDocumentAndMorphState()
        {
            var body = new VisualElement { name = "ConsumerBody" };
            menu = new DeucarianMorphingMenu(host, body);
            UIDocument document = menu.Document;

            menu.SetExpanded(true, animate: false);
            menu.SetCollapsedIcon(DeucarianMorphingMenuIcon.Information);

            Assert.AreSame(document, menu.Document);
            Assert.AreSame(body, menu.Body);
            Assert.AreEqual(
                DeucarianMorphingMenu.InformationIconName,
                menu.MenuIcon.name);
            Assert.That(
                menu.MenuIcon.style.opacity.value,
                Is.EqualTo(0f).Within(0.0001f));
            Assert.That(
                menu.CloseIcon.style.opacity.value,
                Is.EqualTo(1f).Within(0.0001f));
            Assert.AreSame(menu.MenuIcon, menu.Button[0]);
            Assert.AreSame(menu.CloseIcon, menu.Button[1]);

            menu.SetExpanded(false, animate: false);
            menu.SetCollapsedIcon(DeucarianMorphingMenuIcon.Settings);

            Assert.That(
                menu.MenuIcon.style.opacity.value,
                Is.EqualTo(1f).Within(0.0001f));
            Assert.AreEqual(
                DeucarianMorphingMenu.MenuIconName,
                menu.MenuIcon.name);
        }

        [Test]
        public void RightInsetUpdatesPositionAndResponsiveWidthAtRuntime()
        {
            var body = new VisualElement();
            menu = new DeucarianMorphingMenu(
                host,
                body,
                new DeucarianMorphingMenuLayout
                {
                    EdgeMargin = 24f,
                    RightInset = 72f
                });

            SendGeometry(menu.Root, 200f, 100f);

            Assert.That(menu.RightInset, Is.EqualTo(72f).Within(0.0001f));
            Assert.That(
                menu.MenuRoot.style.width.value.value,
                Is.EqualTo(104f).Within(0.0001f));
            Assert.That(
                DeucarianMorphingMenu.ResolveExpandedWidth(
                    200f,
                    24f,
                    72f,
                    300f),
                Is.EqualTo(104f).Within(0.0001f));

            menu.SetRightInset(44f);

            Assert.AreSame(body, menu.Body);
            Assert.That(menu.RightInset, Is.EqualTo(44f).Within(0.0001f));
            Assert.That(
                menu.MenuRoot.style.right.value.value,
                Is.EqualTo(44f).Within(0.0001f));
            Assert.That(
                menu.MenuRoot.style.width.value.value,
                Is.EqualTo(132f).Within(0.0001f));
        }

        [Test]
        public void InvalidIconIsRejectedBeforeCreatingRuntimeUi()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                new DeucarianMorphingMenu(
                    host,
                    new VisualElement(),
                    new DeucarianMorphingMenuLayout
                    {
                        CollapsedIcon =
                            (DeucarianMorphingMenuIcon)999
                    }));
            Assert.AreEqual(0, root.transform.childCount);
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

        private static void AssertStyleRect(
            VisualElement element,
            float left,
            float top,
            float width,
            float height)
        {
            Assert.AreEqual(Position.Absolute, element.style.position.value);
            Assert.That(
                element.style.left.value.value,
                Is.EqualTo(left).Within(0.0001f));
            Assert.That(
                element.style.top.value.value,
                Is.EqualTo(top).Within(0.0001f));
            Assert.That(
                element.style.width.value.value,
                Is.EqualTo(width).Within(0.0001f));
            Assert.That(
                element.style.height.value.value,
                Is.EqualTo(height).Within(0.0001f));
        }
    }
}
