using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.UI
{
    /// <summary>
    /// Central runtime composition policy for Deucarian screen-space UI.
    /// It owns the canonical PanelSettings identity and semantic ordering for
    /// both UI Toolkit documents and screen-space uGUI canvases.
    /// </summary>
    public static class DeucarianUIRuntime
    {
        public static bool CanonicalPanelSettingsAvailable =>
            DeucarianUIRuntimeAssets.LoadRuntimePanelSettings() != null;

        public static void Configure(
            UIDocument document,
            DeucarianUISurfaceRole role)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            int depth = DeucarianUIDepth.Resolve(role);
            PanelSettings settings = RequireCanonicalPanelSettings();
            document.panelSettings = settings;
            document.sortingOrder = depth;
        }

        public static void ConfigureScreenSpaceCanvas(
            Canvas canvas,
            DeucarianUISurfaceRole role)
        {
            if (canvas == null)
            {
                throw new ArgumentNullException(nameof(canvas));
            }

            int depth = DeucarianUIDepth.Resolve(role);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = depth;
        }

        /// <summary>
        /// Reports whether a document uses the requested semantic depth and
        /// either the canonical package panel or a package-managed cloned
        /// panel created by <see cref="DeucarianUIOverlayHost"/>.
        /// </summary>
        public static bool IsConfigured(
            UIDocument document,
            DeucarianUISurfaceRole role)
        {
            return document != null &&
                   ((HasCanonicalPanelSettings(document) &&
                     Mathf.Approximately(
                         document.sortingOrder,
                         DeucarianUIDepth.Resolve(role))) ||
                    DeucarianUIOverlayHost.IsManaged(document, role));
        }

        /// <summary>
        /// Reports whether a screen-space canvas uses the requested shared
        /// semantic depth.
        /// </summary>
        public static bool IsConfigured(
            Canvas canvas,
            DeucarianUISurfaceRole role)
        {
            return canvas != null &&
                   canvas.renderMode == RenderMode.ScreenSpaceOverlay &&
                   (canvas.isRootCanvas || canvas.overrideSorting) &&
                   canvas.sortingOrder == DeucarianUIDepth.Resolve(role);
        }

        /// <summary>
        /// Reports whether a document references the one package-owned
        /// runtime PanelSettings asset. Consumers do not load or compare the
        /// asset directly.
        /// </summary>
        public static bool HasCanonicalPanelSettings(UIDocument document)
        {
            if (document == null || document.panelSettings == null)
            {
                return false;
            }

            PanelSettings settings =
                DeucarianUIRuntimeAssets.LoadRuntimePanelSettings();
            return settings != null && document.panelSettings == settings;
        }

        internal static PanelSettings RequireCanonicalPanelSettings()
        {
            PanelSettings settings =
                DeucarianUIRuntimeAssets.LoadRuntimePanelSettings();
            if (settings == null)
            {
                throw new InvalidOperationException(
                    "The package-owned DeucarianRuntimePanelSettings " +
                    "resource could not be loaded.");
            }

            return settings;
        }
    }
}
