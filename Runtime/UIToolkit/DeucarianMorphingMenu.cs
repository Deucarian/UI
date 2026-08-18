using System;
using System.Collections;
using Deucarian.Common;
using Deucarian.Theming;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.UI
{
    /// <summary>
    /// Canonical package-owned top-right menu shell. Consumers supply only the
    /// feature-specific body and callbacks; this type owns document setup,
    /// chrome, glyphs, layout, motion, input surface, and theme presentation.
    /// </summary>
    public sealed class DeucarianMorphingMenu : IDisposable
    {
        public const string RootName = "DeucarianMorphingMenuRoot";
        public const string DocumentObjectName =
            "DeucarianMorphingMenuDocument";
        public const string MenuRootName = "DeucarianMorphingMenuAnchor";
        public const string ScrimName = "DeucarianMorphingMenuScrim";
        public const string ButtonName = "DeucarianMorphingMenuButton";
        public const string ChromeName = "DeucarianMorphingMenuChrome";
        public const string ButtonHostName = "DeucarianMorphingMenuButtonHost";
        public const string MenuIconName = "DeucarianMorphingMenuSettingsIcon";
        public const string CloseIconName = "DeucarianMorphingMenuCloseIcon";
        public const string MenuIconLineNamePrefix =
            "DeucarianMorphingMenuSettingsLine";
        public const string MenuIconKnobNamePrefix =
            "DeucarianMorphingMenuSettingsKnob";
        public const string CloseIconBarNamePrefix =
            "DeucarianMorphingMenuCloseBar";
        public const string PanelName = "DeucarianMorphingMenuPanel";

        private readonly MonoBehaviour host;
        private readonly DeucarianMorphingMenuLayout layout;
        private readonly DeucarianIconButtonInteraction buttonInteraction =
            new DeucarianIconButtonInteraction();
        private DeucarianRuntimeTooltipPresenter runtimeTooltip;
        private DeucarianThemeProvider themeProvider;
        private GameObject documentObject;
        private Coroutine morphRoutine;
        private IDisposable inputGuard;
        private bool expanded;
        private bool visible = true;
        private float currentWidth =
            DeucarianMorphingMenuMotion.CollapsedSize;
        private float currentHeight =
            DeucarianMorphingMenuMotion.CollapsedSize;
        private float expansionProgress;
        private float panelWidth;
        private bool isDisposed;

        public DeucarianMorphingMenu(
            MonoBehaviour host,
            VisualElement body,
            DeucarianMorphingMenuLayout layout = null)
        {
            this.host = host ??
                throw new ArgumentNullException(nameof(host));
            this.layout = layout ?? new DeucarianMorphingMenuLayout();
            this.layout.Validate();
            EnsureDocument();
            Build(body ?? new VisualElement());
            runtimeTooltip =
                DeucarianRuntimeTooltipPresenter.CreateForDocument(
                    host,
                    Document);
            runtimeTooltip.Bind(Button);
            runtimeTooltip.BindTree(Body);
            inputGuard = this.layout.BindInputGuard?.Invoke(Root);
            buttonInteraction.Bind(Button, ApplyTheme);
            Button.clicked += ToggleExpanded;
            Root.RegisterCallback<KeyDownEvent>(OnKeyDown);
            Root.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            BindThemeProvider();
            ApplyStateImmediately(false);
            ApplyVisibility();
            ApplyTheme();
            Document.enabled = host.isActiveAndEnabled;
        }

        public event Action<bool> ExpandedChanged;

        public UIDocument Document { get; private set; }
        public VisualElement Root { get; private set; }
        public Button Scrim { get; private set; }
        public VisualElement MenuRoot { get; private set; }
        public VisualElement Chrome { get; private set; }
        public VisualElement ButtonHost { get; private set; }
        public Button Button { get; private set; }
        public VisualElement MenuIcon { get; private set; }
        public VisualElement CloseIcon { get; private set; }
        public VisualElement Panel { get; private set; }
        public VisualElement Body { get; private set; }
        public bool IsExpanded => expanded;
        public bool IsVisible => visible;
        public float ExpansionProgress => expansionProgress;
        public DeucarianRuntimeTooltipPresenter RuntimeTooltip =>
            runtimeTooltip;

        public void SetExpanded(
            bool value,
            bool notify = true,
            bool animate = true)
        {
            bool changed = expanded != value;
            expanded = value;
            UpdateControlCopy();
            StopAnimation();
            if (changed && notify)
            {
                ExpandedChanged?.Invoke(value);
            }

            if (!animate ||
                !Application.isPlaying ||
                !layout.ResolveShouldAnimate() ||
                !host.isActiveAndEnabled)
            {
                ApplyStateImmediately(value);
                ApplyTheme();
                return;
            }

            morphRoutine = host.StartCoroutine(AnimateMorph(value));
            ApplyTheme();
        }

        public void SetVisible(bool value)
        {
            visible = value;
            ApplyVisibility();
        }

        public void RefreshPresentation()
        {
            ApplyBodyPresentation(expansionProgress);
            ApplyTheme();
        }

        public void OnDisable()
        {
            StopAnimation();
            ApplyStateImmediately(expanded);
            if (Document != null)
            {
                Document.enabled = false;
            }
        }

        public void OnEnable()
        {
            if (Document != null)
            {
                Document.enabled = true;
            }

            ApplyStateImmediately(expanded);
            ApplyVisibility();
            ApplyTheme();
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            StopAnimation();
            UnbindThemeProvider();
            buttonInteraction.Dispose();
            runtimeTooltip?.Dispose();
            runtimeTooltip = null;
            inputGuard?.Dispose();
            inputGuard = null;
            if (Button != null)
            {
                Button.clicked -= ToggleExpanded;
            }

            if (Root != null)
            {
                Root.UnregisterCallback<KeyDownEvent>(OnKeyDown);
                Root.UnregisterCallback<GeometryChangedEvent>(
                    OnGeometryChanged);
                Root.RemoveFromHierarchy();
            }

            if (documentObject != null)
            {
                UnityObjectUtility.DestroySafely(documentObject);
                documentObject = null;
            }

            isDisposed = true;
        }

        public static float ResolveExpandedWidth(float availableWidth)
        {
            return ResolveExpandedWidth(
                availableWidth,
                DeucarianMorphingMenuLayout.ReferenceEdgeMargin,
                DeucarianMorphingMenuLayout.ReferenceMaximumWidth);
        }

        public static float ResolveExpandedWidth(
            float availableWidth,
            float edgeMargin,
            float maximumWidth)
        {
            return Mathf.Clamp(
                availableWidth - Mathf.Max(0f, edgeMargin) * 2f,
                DeucarianMorphingMenuMotion.CollapsedSize,
                Mathf.Max(
                    DeucarianMorphingMenuMotion.CollapsedSize,
                    maximumWidth));
        }

        private void EnsureDocument()
        {
            documentObject = new GameObject(DocumentObjectName);
            Document = documentObject.AddComponent<UIDocument>();
            DeucarianUIRuntime.Configure(
                Document,
                DeucarianUISurfaceRole.Menu);

            UIDocument parentDocument =
                host.GetComponentInParent<UIDocument>(true);
            if (parentDocument == null ||
                parentDocument.panelSettings == Document.panelSettings)
            {
                documentObject.transform.SetParent(host.transform, false);
            }
        }

        private void Build(VisualElement body)
        {
            VisualElement documentRoot = Document.rootVisualElement;
            documentRoot.Clear();
            documentRoot.pickingMode = PickingMode.Ignore;
            ApplyFullScreen(documentRoot);
            StyleSheet styleSheet =
                DeucarianUIRuntimeAssets.LoadControlIslandStyleSheet();
            if (styleSheet != null &&
                !documentRoot.styleSheets.Contains(styleSheet))
            {
                documentRoot.styleSheets.Add(styleSheet);
            }

            Root = new VisualElement
            {
                name = RootName,
                focusable = true,
                pickingMode = PickingMode.Ignore
            };
            ApplyFullScreen(Root);
            Scrim = CreateScrim();
            MenuRoot = CreateMenuRoot();
            Chrome = CreateChrome();
            ButtonHost = CreateButtonHost();
            Button = CreateButton();
            MenuIcon = BuildSettingsIcon();
            CloseIcon = BuildCloseIcon();
            CloseIcon.style.opacity = 0f;
            DeucarianControlIslandVisualStyle.AddIconClasses(MenuIcon);
            DeucarianControlIslandVisualStyle.AddIconClasses(CloseIcon);
            Button.Add(MenuIcon);
            Button.Add(CloseIcon);
            ButtonHost.Add(Button);
            Chrome.Add(ButtonHost);

            Panel = CreatePanel();
            Body = body;
            Body.style.flexGrow = 1f;
            Panel.Add(Body);
            Chrome.Add(Panel);
            MenuRoot.Add(Chrome);
            Root.Add(Scrim);
            Root.Add(MenuRoot);
            documentRoot.Add(Root);
        }

        private Button CreateScrim()
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

        private VisualElement CreateMenuRoot()
        {
            VisualElement root = new VisualElement
            {
                name = MenuRootName,
                pickingMode = PickingMode.Ignore
            };
            root.style.position = Position.Absolute;
            root.style.right = layout.EdgeMargin;
            root.style.top = layout.EdgeMargin;
            root.style.width = layout.MaximumWidth;
            root.style.alignItems = Align.FlexEnd;
            return root;
        }

        private static VisualElement CreateChrome()
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

        private static VisualElement CreateButtonHost()
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

        private Button CreateButton()
        {
            Button button = new Button
            {
                name = ButtonName,
                text = string.Empty,
                tooltip = layout.OpenTooltip,
                pickingMode = PickingMode.Position
            };
            DeucarianControlIslandVisualStyle.AddIconButtonClasses(button);
            DeucarianControlIslandVisualStyle.ApplyIconButtonLayout(
                button,
                DeucarianControlIslandVisualStyle.CompactIconButton);
            return button;
        }

        private static VisualElement CreatePanel()
        {
            VisualElement panel = new VisualElement
            {
                name = PanelName,
                pickingMode = PickingMode.Ignore
            };
            panel.style.display = DisplayStyle.None;
            panel.style.visibility = Visibility.Hidden;
            panel.style.opacity = 0f;
            panel.style.translate = new Translate(
                0f,
                DeucarianMorphingMenuMotion.BodyHiddenOffset,
                0f);
            panel.style.alignSelf = Align.Stretch;
            panel.style.paddingLeft = 16f;
            panel.style.paddingRight = 16f;
            panel.style.paddingTop = 10f;
            panel.style.paddingBottom = 14f;
            panel.style.flexDirection = FlexDirection.Column;
            return panel;
        }

        private static VisualElement BuildSettingsIcon()
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

        private static VisualElement BuildCloseIcon()
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

        private static VisualElement CreateIcon(string name)
        {
            VisualElement icon = new VisualElement
            {
                name = name,
                pickingMode = PickingMode.Ignore
            };
            DeucarianControlIslandVisualStyle.ApplyCenteredIconLayout(icon);
            return icon;
        }

        private void ToggleExpanded()
        {
            SetExpanded(!expanded);
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (!expanded || evt.keyCode != KeyCode.Escape)
            {
                return;
            }

            SetExpanded(false);
            evt.StopPropagation();
        }

        private IEnumerator AnimateMorph(bool expanding)
        {
            float startExpansion = Mathf.Clamp01(expansionProgress);
            float targetExpansion = expanding ? 1f : 0f;
            float expandedWidth = ResolveConfiguredExpandedWidth(
                ResolvePanelWidth());
            float startWidth = ResolveRenderedDimension(
                Chrome.resolvedStyle.width,
                currentWidth);
            float startHeight = ResolveRenderedDimension(
                Chrome.resolvedStyle.height,
                currentHeight);
            float targetWidth = expanding
                ? expandedWidth
                : DeucarianMorphingMenuMotion.CollapsedSize;
            float targetHeight = DeucarianMorphingMenuMotion.CollapsedSize;

            Chrome.style.display = DisplayStyle.Flex;
            Chrome.pickingMode = PickingMode.Position;
            Panel.style.display = DisplayStyle.Flex;
            Panel.style.width = expandedWidth;
            Panel.style.visibility = expanding
                ? Visibility.Hidden
                : Visibility.Visible;
            if (expanding)
            {
                yield return null;
                float bodyHeight = ResolveRenderedDimension(
                    Panel.resolvedStyle.height,
                    layout.ExpandedFallbackHeight -
                    DeucarianMorphingMenuMotion.CollapsedSize);
                targetHeight = Mathf.Max(
                    layout.ExpandedFallbackHeight,
                    DeucarianMorphingMenuMotion.CollapsedSize + bodyHeight);
                Panel.style.visibility = Visibility.Visible;
            }

            float distance = Mathf.Abs(
                targetExpansion - startExpansion);
            float duration =
                DeucarianMorphingMenuMotion.ResolveDuration(expanding) *
                distance;
            if (duration <= 0f)
            {
                ApplyStateImmediately(expanding);
                morphRoutine = null;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration && layout.ResolveShouldAnimate())
            {
                float linear = Mathf.Clamp01(elapsed / duration);
                float eased = DeucarianMorphingMenuMotion.Ease(
                    expanding,
                    linear);
                float progress = Mathf.Lerp(
                    startExpansion,
                    targetExpansion,
                    eased);
                ApplyDimensions(
                    Mathf.Lerp(startWidth, targetWidth, eased),
                    Mathf.Lerp(startHeight, targetHeight, eased));
                ApplyBodyPresentation(progress);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            ApplyStateImmediately(expanding);
            morphRoutine = null;
        }

        private void ApplyStateImmediately(bool value)
        {
            expanded = value;
            UpdateControlCopy();
            Chrome.style.display = DisplayStyle.Flex;
            Chrome.pickingMode = PickingMode.Position;
            if (value)
            {
                float width = ResolveConfiguredExpandedWidth(
                    ResolvePanelWidth());
                currentWidth = width;
                currentHeight = layout.ExpandedFallbackHeight;
                Chrome.style.width = width;
                Chrome.style.minWidth = width;
                Chrome.style.maxWidth = width;
                Chrome.style.height = StyleKeyword.Auto;
                Chrome.style.minHeight = layout.ExpandedFallbackHeight;
                Chrome.style.maxHeight = StyleKeyword.None;
                Panel.style.display = DisplayStyle.Flex;
                Panel.style.visibility = Visibility.Visible;
                Panel.style.width = StyleKeyword.Auto;
                ApplyBodyPresentation(1f);
            }
            else
            {
                ApplyDimensions(
                    DeucarianMorphingMenuMotion.CollapsedSize,
                    DeucarianMorphingMenuMotion.CollapsedSize);
                ApplyBodyPresentation(0f);
                Panel.style.visibility = Visibility.Hidden;
                Panel.style.display = DisplayStyle.None;
            }
        }

        private void ApplyBodyPresentation(float value)
        {
            expansionProgress = Mathf.Clamp01(value);
            float opacity = DeucarianMorphingMenuMotion
                .ResolveBodyOpacity(expansionProgress);
            MenuIcon.style.opacity = 1f - expansionProgress;
            CloseIcon.style.opacity = expansionProgress;
            Panel.style.opacity = opacity;
            Panel.style.translate = new Translate(
                0f,
                Mathf.Lerp(
                    DeucarianMorphingMenuMotion.BodyHiddenOffset,
                    0f,
                    opacity),
                0f);
            Panel.pickingMode = opacity >= 0.999f
                ? PickingMode.Position
                : PickingMode.Ignore;
        }

        private void ApplyDimensions(float width, float height)
        {
            currentWidth = Mathf.Max(
                DeucarianMorphingMenuMotion.CollapsedSize,
                width);
            currentHeight = Mathf.Max(
                DeucarianMorphingMenuMotion.CollapsedSize,
                height);
            Chrome.style.width = currentWidth;
            Chrome.style.minWidth = currentWidth;
            Chrome.style.maxWidth = currentWidth;
            Chrome.style.height = currentHeight;
            Chrome.style.minHeight = currentHeight;
            Chrome.style.maxHeight = currentHeight;
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            float width = evt.newRect.width;
            if (!IsFinitePositive(width))
            {
                return;
            }

            panelWidth = width;
            MenuRoot.style.width = ResolveConfiguredExpandedWidth(width);
            StopAnimation();
            ApplyStateImmediately(expanded);
        }

        private float ResolvePanelWidth()
        {
            if (IsFinitePositive(panelWidth))
            {
                return panelWidth;
            }

            float rootWidth = Root.contentRect.width;
            return IsFinitePositive(rootWidth)
                ? rootWidth
                : Mathf.Max(
                    DeucarianMorphingMenuMotion.CollapsedSize +
                    layout.EdgeMargin * 2f,
                    Screen.width);
        }

        private float ResolveConfiguredExpandedWidth(float availableWidth)
        {
            return ResolveExpandedWidth(
                availableWidth,
                layout.EdgeMargin,
                layout.MaximumWidth);
        }

        private void UpdateControlCopy()
        {
            Button.text = string.Empty;
            Button.tooltip = expanded
                ? layout.CloseTooltip
                : layout.OpenTooltip;
        }

        private void ApplyVisibility()
        {
            MenuRoot.style.display = visible
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            MenuRoot.pickingMode = PickingMode.Ignore;
        }

        private void BindThemeProvider()
        {
            RefreshThemeProviderBinding();
        }

        private void UnbindThemeProvider()
        {
            if (themeProvider == null)
            {
                return;
            }

            themeProvider.ThemeChanged -= OnThemeChanged;
            themeProvider.StyleChanged -= OnStyleChanged;
        }

        private void RefreshThemeProviderBinding()
        {
            DeucarianThemeProvider resolved = layout.ThemeProvider ??
                host.GetComponentInParent<DeucarianThemeProvider>();
            if (resolved == themeProvider)
            {
                return;
            }

            if (themeProvider != null)
            {
                themeProvider.ThemeChanged -= OnThemeChanged;
                themeProvider.StyleChanged -= OnStyleChanged;
            }

            themeProvider = resolved;
            if (themeProvider != null)
            {
                themeProvider.ThemeChanged += OnThemeChanged;
                themeProvider.StyleChanged += OnStyleChanged;
            }
        }

        private void OnThemeChanged(DeucarianTheme theme)
        {
            ApplyTheme();
        }

        private void OnStyleChanged(DeucarianThemeStyle style)
        {
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            RefreshThemeProviderBinding();
            DeucarianTheme theme = themeProvider != null
                ? themeProvider.CurrentTheme
                : null;
            theme = DeucarianGlassPanelStyle.ResolveTheme(
                theme,
                layout.ThemeContext ?? host);
            DeucarianGlassPanelStyle.ApplyPanel(
                Chrome,
                theme,
                layout.ThemeContext ?? host);
            AlignButtonToChrome();
            DeucarianControlIslandVisualStyle.ApplyIconButtonLayout(
                Button,
                DeucarianControlIslandVisualStyle.CompactIconButton,
                theme,
                layout.ThemeContext ?? host);
            DeucarianControlIslandVisualStyle.ApplyCenteredIconLayout(
                MenuIcon);
            DeucarianControlIslandVisualStyle.ApplyCenteredIconLayout(
                CloseIcon);

            var state = new DeucarianIconButtonVisualState(
                true,
                Button.enabledInHierarchy,
                expanded,
                buttonInteraction.Hovered,
                buttonInteraction.Pressed,
                buttonInteraction.Focused);
            DeucarianControlIslandVisualStyle.ApplyIconButtonState(
                Button,
                MenuIcon,
                theme,
                state,
                layout.ThemeContext ?? host);
            DeucarianControlIslandVisualStyle.ApplyIconButtonState(
                Button,
                CloseIcon,
                theme,
                state,
                layout.ThemeContext ?? host);
            Color tint = DeucarianControlIslandVisualStyle.ResolveIconTint(
                theme,
                state,
                layout.ThemeContext ?? host);
            ApplyIconPrimitiveTint(MenuIcon, tint);
            ApplyIconPrimitiveTint(CloseIcon, tint);
            layout.ApplyBodyTheme?.Invoke(theme);
            runtimeTooltip?.ApplyTheme(
                theme,
                themeProvider != null
                    ? themeProvider.CurrentStyle
                    : null);
            // Theme styling owns color, scale, and interaction state. The
            // morph owns icon cross-fade and body presentation, so restore
            // those values after generic icon styling has run.
            ApplyBodyPresentation(expansionProgress);
        }

        private void AlignButtonToChrome()
        {
            float rightBorder = Mathf.Max(
                0f,
                Chrome.style.borderRightWidth.value);
            float topBorder = Mathf.Max(
                0f,
                Chrome.style.borderTopWidth.value);
            ButtonHost.style.translate = new Translate(
                rightBorder,
                -topBorder,
                0f);
        }

        private static void ApplyIconPrimitiveTint(
            VisualElement icon,
            Color color)
        {
            foreach (VisualElement primitive in icon.Children())
            {
                primitive.style.backgroundColor = color;
                foreach (VisualElement child in primitive.Children())
                {
                    child.style.backgroundColor = color;
                }
            }
        }

        private void StopAnimation()
        {
            if (morphRoutine == null)
            {
                return;
            }

            host.StopCoroutine(morphRoutine);
            morphRoutine = null;
        }

        private static void ApplyFullScreen(VisualElement element)
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

        private static void SetFixedSize(
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

        private static void ApplyRadius(
            VisualElement element,
            float radius)
        {
            element.style.borderTopLeftRadius = radius;
            element.style.borderTopRightRadius = radius;
            element.style.borderBottomLeftRadius = radius;
            element.style.borderBottomRightRadius = radius;
        }

        private static void SetBorder(
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

        private static float ResolveRenderedDimension(
            float resolved,
            float fallback)
        {
            return IsFinitePositive(resolved) ? resolved : fallback;
        }

        private static bool IsFinitePositive(float value)
        {
            return !float.IsNaN(value) &&
                   !float.IsInfinity(value) &&
                   value > 0f;
        }
    }
}
