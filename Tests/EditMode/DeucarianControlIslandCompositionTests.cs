using Deucarian.Theming;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.UI.Tests
{
    public sealed class DeucarianControlIslandCompositionTests
    {
        [Test]
        public void CenteredOverlayProfilePlacesSiblingIconsInOneAbsoluteSlot()
        {
            VisualElement first = new VisualElement();
            VisualElement second = new VisualElement();

            DeucarianControlIslandVisualStyle
                .ApplyCenteredIconLayout(first);
            DeucarianControlIslandVisualStyle
                .ApplyCenteredIconLayout(second);

            Assert.AreEqual(Position.Absolute, first.style.position.value);
            Assert.AreEqual(Position.Absolute, second.style.position.value);
            Assert.That(
                first.style.left.value.value,
                Is.EqualTo(second.style.left.value.value)
                    .Within(0.0001f));
            Assert.That(
                first.style.top.value.value,
                Is.EqualTo(second.style.top.value.value)
                    .Within(0.0001f));
            Assert.That(
                first.style.left.value.value,
                Is.EqualTo(7f).Within(0.0001f));
            Assert.That(
                first.style.top.value.value,
                Is.EqualTo(7f).Within(0.0001f));
        }

        [Test]
        public void GenericActiveRoleDrivesSelectedControlWithoutConsumerRoles()
        {
            Color selectedColor = new Color(0.21f, 0.54f, 0.87f, 1f);
            DeucarianColorRole role =
                ScriptableObject.CreateInstance<DeucarianColorRole>();
            DeucarianColorPalette palette =
                ScriptableObject.CreateInstance<DeucarianColorPalette>();
            DeucarianThemeStyle style =
                DeucarianThemeStylePresets.CreateRuntimeStyle(
                    DeucarianThemeStyleIds.FrostedGlass);
            DeucarianTheme theme =
                ScriptableObject.CreateInstance<DeucarianTheme>();
            try
            {
                role.Configure(
                    DeucarianControlIslandColorRoleIds.Active,
                    "Control Island Active",
                    DeucarianColorRoleCategories.UiState,
                    string.Empty,
                    selectedColor,
                    false);
                palette.SetColor(role, selectedColor);
                theme.Configure(
                    "deucarian.test.control-island",
                    "Control Island",
                    palette,
                    style);

                DeucarianIconButtonPalette resolved =
                    DeucarianControlIslandTheme
                        .ResolveButtonPalette(theme);
                var selected = new DeucarianIconButtonVisualState(
                    true,
                    true,
                    true,
                    false,
                    false,
                    false);

                Assert.AreEqual(
                    selectedColor,
                    resolved.ResolveBackground(selected));
            }
            finally
            {
                Object.DestroyImmediate(theme);
                Object.DestroyImmediate(style);
                Object.DestroyImmediate(palette);
                Object.DestroyImmediate(role);
            }
        }

        [Test]
        public void ReferencePaletteMapsCanonicalControlIslandRolesExactly()
        {
            DeucarianTheme theme = DeucarianViewerReferenceThemePreset
                .Resolve()
                .DefaultTheme;
            DeucarianIconButtonPalette palette =
                DeucarianControlIslandTheme.ResolveButtonPalette(theme);

            AssertRoleColor(
                theme,
                DeucarianBuiltinColorRoleIds.UiNormal,
                palette.BackgroundDisabled);
            AssertRoleColor(
                theme,
                DeucarianBuiltinColorRoleIds.Accent,
                palette.BackgroundSelected);
            AssertRoleColor(
                theme,
                DeucarianBuiltinColorRoleIds.TextMuted,
                palette.Icon);
            AssertRoleColor(
                theme,
                DeucarianBuiltinColorRoleIds.Primary,
                palette.IconHover);
            AssertRoleColor(
                theme,
                DeucarianBuiltinColorRoleIds.TextPrimary,
                palette.IconActive);
            AssertRoleColor(
                theme,
                DeucarianBuiltinColorRoleIds.TextDisabled,
                palette.IconDisabled);
            AssertRoleColor(
                theme,
                DeucarianBuiltinColorRoleIds.TextMuted,
                palette.Border);
            AssertRoleColor(
                theme,
                DeucarianBuiltinColorRoleIds.UiFocused,
                palette.BorderActive);
        }

        [Test]
        public void FacadeAppliesGenericClassesLayoutAndState()
        {
            VisualElement panel = new VisualElement();
            Button button = new Button();
            VisualElement icon = new VisualElement();
            var state = new DeucarianIconButtonVisualState(
                true,
                true,
                true,
                false,
                false,
                false);

            DeucarianControlIslandVisualStyle.AddToolbarClasses(panel);
            DeucarianControlIslandVisualStyle
                .ApplyIconButtonLayout(
                    button,
                    DeucarianControlIslandVisualStyle
                        .CompactIconButton);
            DeucarianControlIslandVisualStyle
                .ApplyCenteredIconLayout(icon);
            DeucarianControlIslandTheme.ApplyIconButtonState(
                button,
                icon,
                null,
                state);

            Assert.IsTrue(
                panel.ClassListContains(
                    DeucarianControlIslandElementStyle.ToolbarClass));
            Assert.IsTrue(
                button.ClassListContains(
                    DeucarianControlIslandElementStyle.ActiveClass));
            Assert.AreEqual(Position.Absolute, icon.style.position.value);
            Assert.That(
                button.style.width.value.value,
                Is.EqualTo(32f).Within(0.0001f));
        }

        [Test]
        public void RuntimeAssetsExposeCanonicalPackageResources()
        {
            Assert.AreEqual(
                "Deucarian/UI/DeucarianControlIsland",
                DeucarianUIRuntimeAssets.ControlIslandStyleSheet);
            Assert.AreEqual(
                "Deucarian/UI/DeucarianRuntimePanelSettings",
                DeucarianUIRuntimeAssets.RuntimePanelSettings);
            Assert.NotNull(
                DeucarianUIRuntimeAssets.LoadControlIslandStyleSheet());
            Assert.NotNull(
                DeucarianUIRuntimeAssets.LoadRuntimePanelSettings());
        }

        [TestCase(0.35f, 0f)]
        [TestCase(0.675f, 0.5f)]
        [TestCase(1f, 1f)]
        public void MorphingMenuMotionUsesCanonicalRevealCurve(
            float progress,
            float expectedOpacity)
        {
            Assert.That(
                DeucarianMorphingMenuMotion
                    .ResolveBodyOpacity(progress),
                Is.EqualTo(expectedOpacity).Within(0.0001f));
            Assert.That(
                DeucarianMorphingMenuMotion.ResolveDuration(true),
                Is.GreaterThan(
                    DeucarianMorphingMenuMotion
                        .ResolveDuration(false)));
        }

        private static void AssertRoleColor(
            DeucarianTheme theme,
            string roleId,
            Color actual)
        {
            Assert.IsTrue(theme.TryGetColorById(roleId, out Color expected));
            Assert.AreEqual(expected, actual, roleId);
        }
    }
}
