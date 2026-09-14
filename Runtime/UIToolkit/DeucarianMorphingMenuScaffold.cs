using System;
using UnityEngine;
using UnityEngine.UIElements;
using static Deucarian.UI.DeucarianMorphingMenu;

namespace Deucarian.UI
{
    internal static class DeucarianMorphingMenuScaffold
    {
        internal static Button CreateScrim()
        {
            Button scrim = new Button
            {
                name = ScrimName,
                tabIndex = -1,
                text = string.Empty,
                pickingMode = PickingMode.Ignore
            };
            scrim.style.display = DisplayStyle.None;
            ApplyFullScreen(scrim);
            scrim.style.backgroundColor = Color.clear;
            scrim.style.backgroundImage = StyleKeyword.Null;
            SetBorder(scrim, Color.clear, 0f);
            return scrim;
        }

        internal static VisualElement CreateMenuRoot(float rightInset, float edgeMargin, float maximumWidth)
        {
            VisualElement root = new VisualElement
            {
                name = MenuRootName,
                pickingMode = PickingMode.Ignore
            };
            root.style.position = Position.Absolute;
            root.style.right = rightInset;
            root.style.top = edgeMargin;
            root.style.width = maximumWidth;
            root.style.alignItems = Align.FlexEnd;
            return root;
        }

        internal static VisualElement CreateChrome()
        {
            VisualElement chrome = new VisualElement
            {
                name = ChromeName,
                pickingMode = PickingMode.Position
            };
            SetFixedSize(
                chrome,
                DeucarianMorphingMenuMotion.CollapsedSize,
                DeucarianMorphingMenuMotion.CollapsedSize);
            chrome.style.alignItems = Align.Center;
            chrome.style.flexDirection = FlexDirection.Column;
            chrome.style.overflow = Overflow.Hidden;
            chrome.style.paddingLeft = 0f;
            chrome.style.paddingRight = 0f;
            chrome.style.paddingTop = 0f;
            chrome.style.paddingBottom = 0f;
            chrome.style.flexGrow = 0f;
            chrome.style.flexShrink = 0f;
            return chrome;
        }

        internal static VisualElement CreateButtonHost()
        {
            VisualElement buttonHost = new VisualElement
            {
                name = ButtonHostName,
                pickingMode = PickingMode.Ignore
            };
            buttonHost.style.alignSelf = Align.FlexEnd;
            SetFixedSize(
                buttonHost,
                DeucarianMorphingMenuMotion.CollapsedSize,
                DeucarianMorphingMenuMotion.CollapsedSize);
            buttonHost.style.alignItems = Align.Center;
            buttonHost.style.justifyContent = Justify.Center;
            buttonHost.style.flexGrow = 0f;
            buttonHost.style.flexShrink = 0f;
            return buttonHost;
        }

        internal static Button CreateButton(string tooltip)
        {
            Button button = new Button
            {
                name = ButtonName,
                text = string.Empty,
                tooltip = tooltip,
                pickingMode = PickingMode.Position
            };
            DeucarianControlIslandVisualStyle.AddIconButtonClasses(button);
            DeucarianControlIslandVisualStyle.ApplyIconButtonLayout(
                button,
                DeucarianControlIslandVisualStyle.CompactIconButton);
            return button;
        }

        internal static VisualElement CreatePanel()
        {
            ScrollView panel = new ScrollView(ScrollViewMode.Vertical)
            {
                name = PanelName,
                pickingMode = PickingMode.Ignore,
                horizontalScrollerVisibility = ScrollerVisibility.Hidden,
                verticalScrollerVisibility = ScrollerVisibility.Auto
            };
            panel.style.flexShrink = 1f;
            panel.style.minHeight = 0f;
            panel.contentContainer.style.flexShrink = 0f;
            panel.style.display = DisplayStyle.None;
            panel.style.visibility = Visibility.Hidden;
            panel.style.opacity = 0f;
            panel.style.translate = new Translate(
                0f,
                DeucarianMorphingMenuMotion.BodyHiddenOffset,
                0f);
            panel.style.alignSelf = Align.Stretch;
            panel.contentContainer.style.paddingLeft = 12f;
            panel.contentContainer.style.paddingRight = 12f;
            panel.contentContainer.style.paddingTop = 8f;
            panel.contentContainer.style.paddingBottom = 12f;
            panel.style.flexDirection = FlexDirection.Column;
            return panel;
        }

        internal static VisualElement BuildSettingsIcon()
        {
            VisualElement icon = CreateIcon(MenuIconName);
            for (int i = 0; i < 3; i++)
            {
                VisualElement line = new VisualElement
                {
                    name = MenuIconLineNamePrefix + i,
                    pickingMode = PickingMode.Ignore
                };
                line.style.position = Position.Absolute;
                line.style.left = 1f;
                line.style.right = 1f;
                line.style.top = 2f + i * 6f;
                line.style.height = 2f;
                ApplyRadius(line, 1f);
                VisualElement knob = new VisualElement
                {
                    name = MenuIconKnobNamePrefix + i,
                    pickingMode = PickingMode.Ignore
                };
                knob.style.position = Position.Absolute;
                knob.style.width = 6f;
                knob.style.height = 6f;
                knob.style.top = -2f;
                knob.style.left = i == 1 ? 0f : 10f;
                ApplyRadius(knob, 3f);
                line.Add(knob);
                icon.Add(line);
            }

            return icon;
        }

        internal static VisualElement BuildInformationIcon()
        {
            VisualElement icon = CreateIcon(InformationIconName);
            VisualElement dot = new VisualElement
            {
                name = InformationIconDotName,
                pickingMode = PickingMode.Ignore
            };
            dot.style.position = Position.Absolute;
            dot.style.left = 8f;
            dot.style.top = 2f;
            dot.style.width = 2f;
            dot.style.height = 2f;
            ApplyRadius(dot, 1f);
            icon.Add(dot);

            VisualElement stem = new VisualElement
            {
                name = InformationIconStemName,
                pickingMode = PickingMode.Ignore
            };
            stem.style.position = Position.Absolute;
            stem.style.left = 8f;
            stem.style.top = 7f;
            stem.style.width = 2f;
            stem.style.height = 9f;
            ApplyRadius(stem, 1f);
            icon.Add(stem);
            return icon;
        }

        internal static VisualElement BuildCollapsedIcon(
            DeucarianMorphingMenuIcon value)
        {
            if (value != DeucarianMorphingMenuIcon.Settings && value != DeucarianMorphingMenuIcon.Information)
                throw new ArgumentOutOfRangeException(nameof(value));
            return value == DeucarianMorphingMenuIcon.Information
                ? BuildInformationIcon()
                : BuildSettingsIcon();
        }

        internal static VisualElement BuildCloseIcon()
        {
            VisualElement icon = CreateIcon(CloseIconName);
            for (int i = 0; i < 2; i++)
            {
                VisualElement bar = new VisualElement
                {
                    name = CloseIconBarNamePrefix + i,
                    pickingMode = PickingMode.Ignore
                };
                bar.style.position = Position.Absolute;
                bar.style.left = 3f;
                bar.style.top = 8f;
                bar.style.width = 12f;
                bar.style.height = 2f;
                bar.style.rotate = new Rotate(new Angle(
                    i == 0 ? 45f : -45f,
                    AngleUnit.Degree));
                ApplyRadius(bar, 1f);
                icon.Add(bar);
            }

            return icon;
        }

        internal static VisualElement CreateIcon(string name)
        {
            VisualElement icon = new VisualElement
            {
                name = name,
                pickingMode = PickingMode.Ignore
            };
            DeucarianControlIslandVisualStyle.ApplyCenteredIconLayout(icon);
            return icon;
        }

        internal static void ApplyFullScreen(VisualElement element)
        {
            element.style.position = Position.Absolute;
            element.style.left = 0f;
            element.style.right = 0f;
            element.style.top = 0f;
            element.style.bottom = 0f;
            element.style.width = Length.Percent(100f);
            element.style.height = Length.Percent(100f);
            element.style.backgroundColor = StyleKeyword.Null;
        }

        internal static void SetFixedSize(
            VisualElement element,
            float width,
            float height)
        {
            element.style.width = width;
            element.style.minWidth = width;
            element.style.maxWidth = width;
            element.style.height = height;
            element.style.minHeight = height;
            element.style.maxHeight = height;
        }

        internal static void ApplyRadius(
            VisualElement element,
            float radius)
        {
            element.style.borderTopLeftRadius = radius;
            element.style.borderTopRightRadius = radius;
            element.style.borderBottomLeftRadius = radius;
            element.style.borderBottomRightRadius = radius;
        }

        internal static void SetBorder(
            VisualElement element,
            Color color,
            float width)
        {
            element.style.borderLeftWidth = width;
            element.style.borderRightWidth = width;
            element.style.borderTopWidth = width;
            element.style.borderBottomWidth = width;
            element.style.borderLeftColor = color;
            element.style.borderRightColor = color;
            element.style.borderTopColor = color;
            element.style.borderBottomColor = color;
        }

    }
}
