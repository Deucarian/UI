using System;
using System.Collections;
using Deucarian.Common;
using Deucarian.Theming;
using UnityEngine;
using UnityEngine.UIElements;
using static Deucarian.UI.DeucarianMorphingMenuScaffold;

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
        public const string InformationIconName =
            "DeucarianMorphingMenuInformationIcon";
        public const string InformationIconDotName =
            "DeucarianMorphingMenuInformationDot";
        public const string InformationIconStemName =
            "DeucarianMorphingMenuInformationStem";
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
        private DeucarianMenuSurface surface;
        private float panelHeight;
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
        private float rightInset;
        private DeucarianMorphingMenuIcon collapsedIcon;
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
            rightInset = ResolveNonNegativeFinite(this.layout.RightInset);
            collapsedIcon = this.layout.CollapsedIcon;
            EnsureDocument();
            Build(body ?? new VisualElement());
            runtimeTooltip =
                DeucarianRuntimeTooltipPresenter.CreateForDocument(
                    host,
                    Document, Root);
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
            surface.SetEnabled(host.isActiveAndEnabled);
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
        public float RightInset => rightInset;
        public DeucarianMorphingMenuIcon CollapsedIcon => collapsedIcon;
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

        /// <summary>
        /// Changes the semantic glyph displayed while the menu is collapsed.
        /// </summary>
        public void SetCollapsedIcon(DeucarianMorphingMenuIcon value)
        {
            ValidateCollapsedIcon(value);
            if (collapsedIcon == value)
            {
                return;
            }

            collapsedIcon = value;
            VisualElement previousIcon = MenuIcon;
            MenuIcon = BuildCollapsedIcon(value);
            MenuIcon.style.opacity = 1f - expansionProgress;
            DeucarianControlIslandVisualStyle.AddIconClasses(MenuIcon);
            previousIcon?.RemoveFromHierarchy();
            Button.Insert(0, MenuIcon);
            ApplyTheme();
        }

        /// <summary>
        /// Repositions the menu without rebuilding its document or body.
        /// </summary>
        public void SetRightInset(float value)
        {
            float resolved = ResolveNonNegativeFinite(value);
            if (Mathf.Approximately(rightInset, resolved))
            {
                return;
            }

            rightInset = resolved;
            MenuRoot.style.right = rightInset;
            float availableWidth = ResolvePanelWidth();
            MenuRoot.style.width = ResolveConfiguredExpandedWidth(
                availableWidth);
            // Expansion callbacks are raised before their new morph starts.
            // Keep an in-flight or just-requested morph intact while moving
            // slots; only resize chrome that is already fully expanded.
            if (expanded && expansionProgress >= 0.999f)
            {
                ApplyStateImmediately(true);
                ApplyTheme();
            }
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
                surface.SetEnabled(false);
            }
        }

        public void OnEnable()
        {
            if (Document != null)
            {
                surface.SetEnabled(true);
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

            surface?.Dispose();
            surface = null;

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
            return ResolveExpandedWidth(
                availableWidth,
                edgeMargin,
                edgeMargin,
                maximumWidth);
        }

        public static float ResolveExpandedWidth(
            float availableWidth,
            float leftEdgeMargin,
            float rightInset,
            float maximumWidth)
        {
            return Mathf.Clamp(
                ResolveNonNegativeFinite(availableWidth) -
                ResolveNonNegativeFinite(leftEdgeMargin) -
                ResolveNonNegativeFinite(rightInset),
                DeucarianMorphingMenuMotion.CollapsedSize,
                Mathf.Max(
                    DeucarianMorphingMenuMotion.CollapsedSize,
                    ResolveNonNegativeFinite(maximumWidth)));
        }

        private void EnsureDocument()
        {
            surface = new DeucarianMenuSurface(host);
            Document = surface.Document;
        }

        private void Build(VisualElement body)
        {
            VisualElement documentRoot = surface.Container;
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
            MenuRoot = CreateMenuRoot(rightInset, layout.EdgeMargin, layout.MaximumWidth);
            Chrome = CreateChrome();
            ButtonHost = CreateButtonHost();
            Button = CreateButton(layout.OpenTooltip);
            MenuIcon = BuildCollapsedIcon(collapsedIcon);
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
            Body.style.flexGrow = 0f;
            Body.style.flexShrink = 0f;
            Panel.Add(Body);
            Chrome.Add(Panel);
            MenuRoot.Add(Chrome);
            Root.Add(Scrim);
            Root.Add(MenuRoot);
            documentRoot.Add(Root);
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
                targetHeight = Mathf.Min(ResolveMaximumHeight(), Mathf.Max(
                    layout.ExpandedFallbackHeight,
                    DeucarianMorphingMenuMotion.CollapsedSize + bodyHeight));
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
                currentHeight = Mathf.Min(layout.ExpandedFallbackHeight, ResolveMaximumHeight());
                Chrome.style.width = width;
                Chrome.style.minWidth = width;
                Chrome.style.maxWidth = width;
                Chrome.style.height = StyleKeyword.Auto;
                Chrome.style.minHeight = currentHeight;
                Chrome.style.maxHeight = ResolveMaximumHeight();
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
            panelHeight = evt.newRect.height;
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
                    layout.EdgeMargin +
                    rightInset,
                    Screen.width);
        }

        private float ResolveMaximumHeight()
        {
            float height = IsFinitePositive(panelHeight) ? panelHeight : Root.contentRect.height;
            if (!IsFinitePositive(height)) height = Screen.height;
            return Mathf.Max(DeucarianMorphingMenuMotion.CollapsedSize, height - layout.EdgeMargin * 2f);
        }

        private float ResolveConfiguredExpandedWidth(float availableWidth)
        {
            return ResolveExpandedWidth(
                availableWidth,
                layout.EdgeMargin,
                rightInset,
                layout.MaximumWidth);
        }

        private static void ValidateCollapsedIcon(
            DeucarianMorphingMenuIcon value)
        {
            if (value != DeucarianMorphingMenuIcon.Settings &&
                value != DeucarianMorphingMenuIcon.Information)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "Unknown morphing-menu collapsed icon.");
            }
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

        private static float ResolveNonNegativeFinite(float value)
        {
            return float.IsNaN(value) || float.IsInfinity(value)
                ? 0f
                : Mathf.Max(0f, value);
        }
    }
}
