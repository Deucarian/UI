using Deucarian.Theming;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.UI.Tests
{
    public sealed class DeucarianControlReadabilityTests
    {
        [Test]
        public void SelectedAndAnimatedControlsUseTheSameTintedSurfaceAndTextFamily()
        {
            var dark = new Color(0.2f, 0.27f, 0.32f);
            var light = new Color(0.91f, 0.94f, 0.96f);
            var selected = new Color(0.77f, 0.63f, 0.98f);
            var palette = new DeucarianIconButtonPalette(dark, selected, selected,
                selected, dark, light, light, light, light, light, dark, selected, true, dark);
            var idle = DeucarianIconButtonStyle.ResolvePresentation(palette,
                new DeucarianIconButtonVisualState(true, true, false, false, false, false), null, true);
            var active = DeucarianIconButtonStyle.ResolvePresentation(palette,
                new DeucarianIconButtonVisualState(true, true, true, false, false, false), null, true);
            Assert.That(active.Text, Is.EqualTo(dark));
            Assert.That(active.Icon, Is.EqualTo(dark));
            Assert.That(idle.Text, Is.EqualTo(light));
            for (int frame = 0; frame <= 100; frame++)
            {
                var value = DeucarianIconButtonPresentation.Lerp(idle, active, frame / 100f);
                Assert.That(value.Text, Is.EqualTo(dark).Or.EqualTo(light));
                Assert.That(value.Icon, Is.EqualTo(dark).Or.EqualTo(light));
            }
        }

        [Test]
        public void LightControlsUseTheExistingLightColourOnADarkSelection()
        {
            var dark = new Color(0.13f, 0.22f, 0.28f);
            var light = new Color(0.88f, 0.93f, 0.97f);
            var palette = new DeucarianIconButtonPalette(light, dark, dark, dark, light,
                dark, dark, dark, dark, dark, light, dark, true, light);
            var selected = DeucarianIconButtonStyle.ResolvePresentation(palette,
                new DeucarianIconButtonVisualState(true, true, true, false, false, false), null, true);
            Assert.That(selected.Text, Is.EqualTo(light));
            Assert.That(selected.Icon, Is.EqualTo(light));
        }

        [Test]
        public void ViewerThemeSelectionUsesTheNormalControlSurfaceForDarkForeground()
        {
            var theme = DeucarianViewerReferenceThemePreset.Resolve().DefaultTheme;
            var palette = DeucarianControlIslandTheme.ResolveButtonPalette(theme);
            var idle = new DeucarianIconButtonVisualState(true, true, false, false, false, false);
            var selected = new DeucarianIconButtonVisualState(true, true, true, false, false, false);
            var value = DeucarianIconButtonStyle.ResolvePresentation(palette, selected, null, true);
            Assert.That(value.Text, Is.EqualTo(palette.ResolveBackground(idle)));
            Assert.That(value.Icon, Is.EqualTo(palette.ResolveBackground(idle)));
        }

        [Test]
        public void EveryAnimatedFrameKeepsTextReadableWithoutChangingContainedGeometry()
        {
            var palette = new DeucarianIconButtonPalette(Color.black, Color.gray, Color.white,
                Color.white, Color.black, Color.white, Color.white, Color.white, Color.white,
                Color.gray, Color.gray, Color.white, true, Color.black);
            var idle = DeucarianIconButtonStyle.ResolvePresentation(palette,
                new DeucarianIconButtonVisualState(true, true, false, false, false, false), null, true);
            for (int flags = 0; flags < 16; flags++)
            {
                var state = new DeucarianIconButtonVisualState(true, true,
                    (flags & 1) != 0, (flags & 2) != 0, (flags & 4) != 0, (flags & 8) != 0);
                var target = DeucarianIconButtonStyle.ResolvePresentation(palette, state, null, true);
                for (int frame = 0; frame <= 100; frame++)
                {
                    var value = DeucarianIconButtonPresentation.Lerp(idle, target, frame / 100f);
                    Assert.That(DeucarianForegroundContrast.Ratio(value.Text, value.Background), Is.GreaterThanOrEqualTo(4.5f));
                    Assert.That(DeucarianForegroundContrast.Ratio(value.Icon, value.Background), Is.GreaterThanOrEqualTo(3f));
                    Assert.That(value.ButtonScale, Is.EqualTo(Vector3.one));
                    Assert.That(value.IconScale, Is.EqualTo(Vector3.one));
                    Assert.That(value.BorderWidth, Is.EqualTo(idle.BorderWidth));
                }
            }
        }

        [Test]
        public void SelectedPointerStateAddsNoInnerOutlineAndClearsDefaultButtonImage()
        {
            var button = new Button();
            DeucarianControlIslandElementStyle.AddIconButtonClasses(button);
            var palette = DeucarianControlIslandTheme.ResolveButtonPalette(null);
            var state = new DeucarianIconButtonVisualState(true, true, true, true, true, false);
            var value = DeucarianIconButtonStyle.ResolvePresentation(palette, state, null, true);
            DeucarianIconButtonStyle.ApplyButtonPresentation(button, value);
            Assert.That(value.Border.a, Is.Zero);
            Assert.That(button.style.backgroundImage.value.texture, Is.Null);
            Assert.That(button.style.translate.value.x.value, Is.Zero);
            Assert.That(button.style.translate.value.y.value, Is.Zero);
        }
    }
}
