using Deucarian.Theming;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.UI.Tests
{
    public sealed class DeucarianToolbarSurfaceTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void RestingAndDisabledControlsKeepTheToolbarVisible(bool enabled)
        {
            var palette = DeucarianControlIslandTheme.ResolveButtonPalette(null);
            var state = new DeucarianIconButtonVisualState(true, enabled, false, false, false, false);
            var presentation = DeucarianIconButtonStyle.ResolvePresentation(palette, state, null, true);
            var button = new Button();
            DeucarianIconButtonStyle.ApplyButtonPresentation(button, presentation);
            Assert.AreEqual(0f, button.style.backgroundColor.value.a,
                "Contrast calculation must not paint an extra opaque icon surface.");
            Assert.AreEqual(palette.BackingSurface, presentation.BackingSurface);
        }

        [Test]
        public void AnimatedStateUsesCompositedContrastWithoutPaintingTheBackingSurfaceTwice()
        {
            var palette = DeucarianControlIslandTheme.ResolveButtonPalette(null);
            var resting = DeucarianIconButtonStyle.ResolvePresentation(palette,
                new DeucarianIconButtonVisualState(true, true, false, false, false, false));
            var selected = DeucarianIconButtonStyle.ResolvePresentation(palette,
                new DeucarianIconButtonVisualState(true, true, true, false, false, false));
            for (int i = 0; i <= 20; i++)
            {
                float progress = i / 20f;
                var frame = DeucarianIconButtonPresentation.Lerp(resting, selected, progress);
                Assert.AreEqual(Mathf.Lerp(resting.Background.a, selected.Background.a, progress),
                    frame.Background.a, 0.00001f);
                var visible = DeucarianForegroundContrast.Composite(frame.Background, palette.BackingSurface);
                var expected = DeucarianForegroundContrast.Resolve(selected.Text, visible, palette.ForegroundPalette);
                Assert.AreEqual(expected, frame.Text);
                Assert.AreEqual(palette.ForegroundPalette.Dark, frame.ForegroundPalette.Dark);
                Assert.AreEqual(palette.ForegroundPalette.Light, frame.ForegroundPalette.Light);
            }
        }
    }
}
