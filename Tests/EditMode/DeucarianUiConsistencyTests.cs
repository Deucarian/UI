using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Deucarian.UI.Tests
{
    public sealed class DeucarianUiConsistencyTests
    {
        [Test]
        public void MenuOverlaysKeepIndependentVisibilityAndLifetime()
        {
            var root = new GameObject("MenuLifetimeTest");
            var host = root.AddComponent<DeucarianMorphingMenuTestHost>();
            var document = root.AddComponent<UIDocument>();
            DeucarianUIRuntime.Configure(document, DeucarianUISurfaceRole.PrimaryControls);
            DeucarianMorphingMenu first = null, second = null;
            try
            {
                first = new DeucarianMorphingMenu(host, new Label("First"));
                second = new DeucarianMorphingMenu(host, new Label("Second"));
                Assert.AreSame(first.Document, second.Document);
                Assert.IsNull(first.Document.transform.parent);
                Assert.Greater(first.Document.panelSettings.sortingOrder,
                    DeucarianUIDepth.Resolve(DeucarianUISurfaceRole.MediaControls));
                first.OnDisable();
                Assert.AreEqual(DisplayStyle.None, first.Root.parent.style.display.value);
                Assert.AreNotEqual(DisplayStyle.None, second.Root.parent.style.display.value);
                Assert.IsTrue(second.Document.enabled);
                first.Dispose(); first = null;
                Assert.IsTrue(second.Document != null);
                second.SetExpanded(true, animate: false);
                Assert.IsTrue(second.IsExpanded);
                Assert.IsInstanceOf<ScrollView>(second.Panel);
                Assert.IsTrue(second.RuntimeTooltip.IsBound(second.Button));
                using (var resize = GeometryChangedEvent.GetPooled(Rect.zero, new Rect(0, 0, 600, 180)))
                {
                    resize.target = second.Root;
                    second.Root.SendEvent(resize);
                }
                Assert.LessOrEqual(second.Chrome.style.maxHeight.value.value, 132f,
                    "The expanded menu must leave the configured edge margins in shallow viewports.");
            }
            finally { first?.Dispose(); second?.Dispose(); Object.DestroyImmediate(root); }
        }

        [UnityTest]
        public IEnumerator ShortTooltipDoesNotWrapAndLongTextKeepsItsCorners()
        {
            var window = ScriptableObject.CreateInstance<MeasurementWindow>();
            window.Show();
            try
            {
                var bubble = new VisualElement();
                bubble.style.position = Position.Absolute;
                bubble.style.paddingLeft = bubble.style.paddingRight = 8f;
                bubble.style.paddingTop = bubble.style.paddingBottom = 5f;
                bubble.style.borderTopLeftRadius = 12f;
                var label = new Label("Next media");
                label.style.fontSize = 11f;
                label.style.whiteSpace = WhiteSpace.Normal;
                bubble.Add(label); window.rootVisualElement.Add(bubble);
                for (int i = 0; i < 5; i++) yield return null;
                var geometry = new DeucarianTooltipGeometry(bubble, label);
                var shortSize = geometry.Measure(600);
                for (int i = 0; i < 5; i++) yield return null;
                var singleLine = label.MeasureTextSize(label.text, 0, VisualElement.MeasureMode.Undefined,
                    0, VisualElement.MeasureMode.Undefined);
                Assert.GreaterOrEqual(label.resolvedStyle.width, singleLine.x);
                Assert.LessOrEqual(label.resolvedStyle.height, singleLine.y + 1f);
                label.text = "Keep the key light aligned to the camera with a small local offset";
                var longSize = geometry.Measure(200);
                Assert.Greater(longSize.y, shortSize.y);
                Assert.LessOrEqual(longSize.x, 180f);
                Assert.AreEqual(12f, bubble.style.borderTopLeftRadius.value.value);
            }
            finally { window.Close(); Object.DestroyImmediate(window); }
        }

        [Test]
        public void BlockedTooltipStaysBesideItsOriginatingRow()
        {
            var target = new Rect(242, 220, 32, 32);
            var rows = new[] { new Rect(196, 216, 208, 40), new Rect(196, 168, 208, 40) };
            var size = new Vector2(84, 26);
            var result = DeucarianTooltipPlacementResolver.AvoidObstacles(
                new Vector2(216, 185), target, new Vector2(600, 360), size, rows);
            Assert.AreEqual(target.center.y, result.y + size.y / 2f);
            foreach (var row in rows) Assert.False(new Rect(result, size).Overlaps(row));
            Assert.Less(result.x, rows[0].xMin);
        }

        [Test]
        public void TextControlsUsePaletteContrastWithoutPressedMovementOrExtraBorder()
        {
            var button = new Button { text = "Screenshots 4" };
            using (var feedback = new DeucarianControlFeedback(null, button))
            {
                feedback.ApplyTheme(null);
                feedback.SetSelected(true);
                var background = button.style.backgroundColor.value;
                var foreground = button.style.color.value;
                var palette = DeucarianControlIslandTheme.ResolveButtonPalette(null).ForegroundPalette;
                Assert.AreEqual(palette.Dark, foreground);
                Assert.GreaterOrEqual(Deucarian.Theming.DeucarianForegroundContrast.Ratio(
                    foreground, background), Deucarian.Theming.DeucarianForegroundContrast.Ratio(
                    palette.Light, background));
                var radius = button.style.borderTopLeftRadius;
                using (var press = MouseDownEvent.GetPooled(new Event { type = EventType.MouseDown, button = 0 }))
                    button.SendEvent(press);
                Assert.AreEqual(Vector3.one, button.style.scale.value.value);
                Assert.AreEqual(0f, button.style.borderTopColor.value.a);
                Assert.AreEqual(radius, button.style.borderTopLeftRadius);
            }
        }

        private sealed class MeasurementWindow : EditorWindow { }
    }
}
