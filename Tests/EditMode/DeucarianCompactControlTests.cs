using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Deucarian.UI.Tests
{
    public sealed class DeucarianCompactControlTests
    {
        [TestCase(true)]
        [TestCase(false)]
        public void ControlIslandPressKeepsHoverDepth(bool hovered)
        {
            var button = new Button();
            DeucarianControlIslandElementStyle.AddIconButtonClasses(button);
            var icon = new DeucarianChevronIcon(true);
            var animation = new DeucarianAnimatedIconButton(null, button, icon, DeucarianMotionProfile.ControlState);
            var palette = DeucarianControlIslandTheme.ResolveButtonPalette(null);
            animation.SetState(palette, new DeucarianIconButtonVisualState(true, true, false, hovered, false, false), false);
            Vector3 buttonScale = button.style.scale.value.value;
            Vector3 iconScale = icon.style.scale.value.value;
            animation.SetState(palette, new DeucarianIconButtonVisualState(true, true, false, hovered, true, false), false);
            Assert.That(button.style.scale.value.value, Is.EqualTo(buttonScale));
            Assert.That(icon.style.scale.value.value, Is.EqualTo(iconScale));
        }

        [UnityTest]
        public IEnumerator CompactTooltipFitsNarrowAndShallowViewportsAndAvoidsTheControlStack()
        {
            var window = ScriptableObject.CreateInstance<TooltipTestWindow>();
            window.Show();
            try
            {
                foreach (var size in new[] { new Vector2(240, 320), new Vector2(320, 240), new Vector2(960, 240) })
                {
                    var root = window.rootVisualElement;
                    root.Clear();
                    root.style.width = size.x;
                    root.style.height = size.y;
                    var bottom = new VisualElement();
                    DeucarianControlIslandElementStyle.AddToolbarClasses(bottom);
                    bottom.style.position = Position.Absolute;
                    bottom.style.width = 200;
                    bottom.style.height = 40;
                    bottom.style.left = (size.x - 200) / 2;
                    bottom.style.top = size.y - 50;
                    var button = new Button { tooltip = "Next media" };
                    button.style.width = 32;
                    button.style.height = 32;
                    bottom.Add(button);
                    root.Add(bottom);
                    var upper = new VisualElement();
                    DeucarianControlIslandElementStyle.AddToolbarClasses(upper);
                    upper.style.position = Position.Absolute;
                    upper.style.left = (size.x - 200) / 2;
                    upper.style.top = size.y - 98;
                    upper.style.width = 200;
                    upper.style.height = 40;
                    root.Add(upper);
                    using (var tooltip = new DeucarianRuntimeTooltipPresenter(null, root))
                    {
                        tooltip.Bind(button);
                        for (int i = 0; i < 5; i++) yield return null;
                        typeof(DeucarianRuntimeTooltipPresenter).GetField("pendingTarget", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(tooltip, button);
                        typeof(DeucarianRuntimeTooltipPresenter).GetMethod("ShowPending", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(tooltip, null);
                        for (int i = 0; i < 5; i++) yield return null;
                        var bounds = tooltip.Bubble.worldBound;
                        Assert.That(bounds.width, Is.LessThan(180), "Short hints should fit their text.");
                        Assert.That(bounds.xMin, Is.GreaterThanOrEqualTo(root.worldBound.xMin));
                        Assert.That(bounds.xMax, Is.LessThanOrEqualTo(root.worldBound.xMin + size.x));
                        Assert.That(bounds.yMin, Is.GreaterThanOrEqualTo(root.worldBound.yMin));
                        Assert.False(bounds.Overlaps(bottom.worldBound));
                        Assert.False(bounds.Overlaps(upper.worldBound));
                        Assert.That(tooltip.Bubble.pickingMode, Is.EqualTo(PickingMode.Ignore));
                    }
                }
            }
            finally { window.Close(); Object.DestroyImmediate(window); }
        }

        private sealed class TooltipTestWindow : EditorWindow { }
    }
}
