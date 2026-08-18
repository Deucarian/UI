using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.UI
{
    /// <summary>
    /// Package-owned composition of the canonical viewer information and
    /// settings menus. It owns slot placement, mutual exclusion, visibility,
    /// lifecycle forwarding, and one centralized expansion event.
    /// </summary>
    public sealed class DeucarianViewerMenuCluster : IDisposable
    {
        private readonly DeucarianViewerMenuClusterLayout layout;
        private bool visible = true;
        private bool coordinating;
        private bool disposed;
        private DeucarianViewerMenuKind? expandedMenu;

        public DeucarianViewerMenuCluster(
            MonoBehaviour host,
            VisualElement informationBody,
            VisualElement settingsBody,
            DeucarianViewerMenuClusterLayout layout = null)
        {
            if (host == null)
            {
                throw new ArgumentNullException(nameof(host));
            }

            this.layout = layout ?? new DeucarianViewerMenuClusterLayout();
            this.layout.Validate();
            InformationMenu = new DeucarianMorphingMenu(
                host,
                informationBody ?? new VisualElement(),
                CreateMenuLayout(DeucarianViewerMenuKind.Information));
            SettingsMenu = new DeucarianMorphingMenu(
                host,
                settingsBody ?? new VisualElement(),
                CreateMenuLayout(DeucarianViewerMenuKind.Settings));
            InformationMenu.ExpandedChanged += OnInformationExpanded;
            SettingsMenu.ExpandedChanged += OnSettingsExpanded;
            RestoreCollapsedSlots();
            ApplyVisibility();
        }

        public event Action<DeucarianViewerMenuKind, bool> ExpandedChanged;

        public DeucarianMorphingMenu InformationMenu { get; }
        public DeucarianMorphingMenu SettingsMenu { get; }
        public DeucarianViewerMenuKind? ExpandedMenu => expandedMenu;
        public bool IsVisible => visible;

        public void SetExpanded(
            DeucarianViewerMenuKind kind,
            bool value,
            bool notify = true,
            bool animate = true)
        {
            ThrowIfDisposed();
            DeucarianViewerMenuKind? previous = expandedMenu;
            coordinating = true;
            try
            {
                DeucarianMorphingMenu target = ResolveMenu(kind);
                if (value)
                {
                    DeucarianMorphingMenu other = ResolveOtherMenu(kind);
                    other.SetExpanded(false, false, false);
                    expandedMenu = kind;
                    target.SetRightInset(layout.EdgeMargin);
                    ApplyVisibility();
                    target.SetExpanded(true, false, animate);
                }
                else
                {
                    target.SetExpanded(false, false, animate);
                    if (expandedMenu == kind)
                    {
                        expandedMenu = null;
                        RestoreCollapsedSlots();
                        ApplyVisibility();
                    }
                }
            }
            finally
            {
                coordinating = false;
            }

            if (notify)
            {
                PublishTransition(previous, expandedMenu);
            }
        }

        public void CollapseAll(bool notify = true, bool animate = true)
        {
            ThrowIfDisposed();
            DeucarianViewerMenuKind? previous = expandedMenu;
            coordinating = true;
            try
            {
                InformationMenu.SetExpanded(false, false, animate);
                SettingsMenu.SetExpanded(false, false, animate);
                expandedMenu = null;
                RestoreCollapsedSlots();
                ApplyVisibility();
            }
            finally
            {
                coordinating = false;
            }

            if (notify)
            {
                PublishTransition(previous, null);
            }
        }

        public void SetVisible(bool value)
        {
            ThrowIfDisposed();
            visible = value;
            ApplyVisibility();
        }

        public void RefreshPresentation()
        {
            ThrowIfDisposed();
            InformationMenu.RefreshPresentation();
            SettingsMenu.RefreshPresentation();
        }

        public void OnDisable()
        {
            if (disposed)
            {
                return;
            }

            InformationMenu.OnDisable();
            SettingsMenu.OnDisable();
        }

        public void OnEnable()
        {
            ThrowIfDisposed();
            InformationMenu.OnEnable();
            SettingsMenu.OnEnable();
            ApplyVisibility();
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            InformationMenu.ExpandedChanged -= OnInformationExpanded;
            SettingsMenu.ExpandedChanged -= OnSettingsExpanded;
            InformationMenu.Dispose();
            SettingsMenu.Dispose();
            ExpandedChanged = null;
        }

        private DeucarianMorphingMenuLayout CreateMenuLayout(
            DeucarianViewerMenuKind kind)
        {
            bool information = kind == DeucarianViewerMenuKind.Information;
            return new DeucarianMorphingMenuLayout
            {
                EdgeMargin = layout.EdgeMargin,
                RightInset = information
                    ? layout.InformationRightInset
                    : layout.EdgeMargin,
                CollapsedIcon = information
                    ? DeucarianMorphingMenuIcon.Information
                    : DeucarianMorphingMenuIcon.Settings,
                MaximumWidth = information
                    ? layout.InformationMaximumWidth
                    : layout.SettingsMaximumWidth,
                ExpandedFallbackHeight = information
                    ? layout.InformationExpandedFallbackHeight
                    : layout.SettingsExpandedFallbackHeight,
                OpenTooltip = information
                    ? layout.OpenInformationTooltip
                    : layout.OpenSettingsTooltip,
                CloseTooltip = information
                    ? layout.CloseInformationTooltip
                    : layout.CloseSettingsTooltip,
                ShouldAnimate = layout.ShouldAnimate,
                BindInputGuard = layout.BindInputGuard,
                ThemeProvider = layout.ThemeProvider,
                ApplyBodyTheme = information
                    ? layout.ApplyInformationBodyTheme
                    : layout.ApplySettingsBodyTheme,
                ThemeContext = layout.ThemeContext
            };
        }

        private void OnInformationExpanded(bool value)
        {
            OnMenuExpanded(DeucarianViewerMenuKind.Information, value);
        }

        private void OnSettingsExpanded(bool value)
        {
            OnMenuExpanded(DeucarianViewerMenuKind.Settings, value);
        }

        private void OnMenuExpanded(
            DeucarianViewerMenuKind kind,
            bool value)
        {
            if (coordinating || disposed)
            {
                return;
            }

            DeucarianViewerMenuKind? previous = expandedMenu;
            coordinating = true;
            try
            {
                if (value)
                {
                    ResolveOtherMenu(kind).SetExpanded(false, false, false);
                    expandedMenu = kind;
                    ResolveMenu(kind).SetRightInset(layout.EdgeMargin);
                    ApplyVisibility();
                }
                else if (expandedMenu == kind)
                {
                    expandedMenu = null;
                    RestoreCollapsedSlots();
                    ApplyVisibility();
                }
            }
            finally
            {
                coordinating = false;
            }

            PublishTransition(previous, expandedMenu);
        }

        private void RestoreCollapsedSlots()
        {
            InformationMenu.SetRightInset(layout.InformationRightInset);
            SettingsMenu.SetRightInset(layout.EdgeMargin);
        }

        private void ApplyVisibility()
        {
            if (!visible)
            {
                InformationMenu.SetVisible(false);
                SettingsMenu.SetVisible(false);
                return;
            }

            InformationMenu.SetVisible(
                !expandedMenu.HasValue ||
                expandedMenu == DeucarianViewerMenuKind.Information);
            SettingsMenu.SetVisible(
                !expandedMenu.HasValue ||
                expandedMenu == DeucarianViewerMenuKind.Settings);
        }

        private void PublishTransition(
            DeucarianViewerMenuKind? previous,
            DeucarianViewerMenuKind? current)
        {
            if (previous == current)
            {
                return;
            }

            if (previous.HasValue)
            {
                ExpandedChanged?.Invoke(previous.Value, false);
            }

            if (current.HasValue)
            {
                ExpandedChanged?.Invoke(current.Value, true);
            }
        }

        private DeucarianMorphingMenu ResolveMenu(
            DeucarianViewerMenuKind kind)
        {
            switch (kind)
            {
                case DeucarianViewerMenuKind.Information:
                    return InformationMenu;
                case DeucarianViewerMenuKind.Settings:
                    return SettingsMenu;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(kind),
                        kind,
                        "Unknown viewer menu kind.");
            }
        }

        private DeucarianMorphingMenu ResolveOtherMenu(
            DeucarianViewerMenuKind kind)
        {
            return kind == DeucarianViewerMenuKind.Information
                ? SettingsMenu
                : kind == DeucarianViewerMenuKind.Settings
                    ? InformationMenu
                    : throw new ArgumentOutOfRangeException(
                        nameof(kind),
                        kind,
                        "Unknown viewer menu kind.");
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(
                    nameof(DeucarianViewerMenuCluster));
            }
        }
    }
}
