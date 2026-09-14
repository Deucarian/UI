using Deucarian.Theming;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.UI.Tests
{
    public sealed class DeucarianControlReadabilityTests
    {
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
