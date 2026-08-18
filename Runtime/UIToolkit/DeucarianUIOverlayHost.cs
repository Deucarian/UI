using System;
using System.Collections.Generic;
using Deucarian.Common;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Deucarian.UI
{
    /// <summary>
    /// Explicit lease for one isolated container on a package-owned transient
    /// overlay document. Disposing the last lease releases the document and
    /// its cloned non-clearing PanelSettings.
    /// </summary>
    public sealed class DeucarianUIOverlayLease : IDisposable
    {
        private DeucarianUIOverlayHost.OverlayLayer layer;

        internal DeucarianUIOverlayLease(
            DeucarianUIOverlayHost.OverlayLayer owner,
            VisualElement root,
            DeucarianUISurfaceRole role)
        {
            layer = owner;
            Root = root;
            Role = role;
        }

        public UIDocument Document =>
            layer != null && layer.IsAlive ? layer.Document : null;
        public VisualElement Root { get; }
        public DeucarianUISurfaceRole Role { get; }
        public bool IsDisposed => layer == null || !layer.IsAlive;

        public void Dispose()
        {
            if (layer == null)
            {
                return;
            }

            DeucarianUIOverlayHost.OverlayLayer owner = layer;
            layer = null;
            owner.Release(Root);
        }
    }

    /// <summary>
    /// Shared host for menu, modal, and tooltip overlay documents. Layers are
    /// ref-counted per source scene, canonical PanelSettings identity, and
    /// semantic role; every lease receives its own VisualElement container.
    /// </summary>
    public static class DeucarianUIOverlayHost
    {
        private const string ObjectNamePrefix =
            "DeucarianRuntimeOverlayLayer";
        private static readonly List<OverlayLayer> Layers =
            new List<OverlayLayer>();

        static DeucarianUIOverlayHost()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        public static DeucarianUIOverlayLease Acquire(
            UIDocument sourceDocument,
            DeucarianUISurfaceRole role,
            string containerName = null)
        {
            if (sourceDocument == null)
            {
                throw new ArgumentNullException(nameof(sourceDocument));
            }

            if (!DeucarianUIDepth.IsTransient(role))
            {
                throw new ArgumentException(
                    "Overlay hosts are reserved for Menu, Modal, and " +
                    "Tooltip surface roles.",
                    nameof(role));
            }

            if (!DeucarianUIRuntime.HasCanonicalPanelSettings(sourceDocument))
            {
                throw new InvalidOperationException(
                    "The source UIDocument must first be configured through " +
                    "DeucarianUIRuntime.Configure.");
            }

            PanelSettings settings = sourceDocument.panelSettings;
            Scene sourceScene = sourceDocument.gameObject.scene;
            if (!sourceScene.IsValid() || !sourceScene.isLoaded)
            {
                throw new InvalidOperationException(
                    "The source UIDocument must belong to a loaded scene.");
            }

            OverlayLayer layer = FindOrCreate(
                settings,
                role,
                sourceScene);
            return layer.Acquire(containerName);
        }

        private static OverlayLayer FindOrCreate(
            PanelSettings settings,
            DeucarianUISurfaceRole role,
            Scene sourceScene)
        {
            for (int i = Layers.Count - 1; i >= 0; i--)
            {
                OverlayLayer candidate = Layers[i];
                if (candidate == null || !candidate.IsAlive)
                {
                    Layers.RemoveAt(i);
                    candidate?.DisposeLayer();
                    continue;
                }

                if (candidate.SourcePanelSettings == settings &&
                    candidate.Role == role &&
                    candidate.SourceSceneHandle == sourceScene.handle)
                {
                    return candidate;
                }
            }

            var layer = new OverlayLayer(settings, role, sourceScene);
            Layers.Add(layer);
            return layer;
        }

        private static void OnSceneUnloaded(Scene scene)
        {
            for (int i = Layers.Count - 1; i >= 0; i--)
            {
                OverlayLayer candidate = Layers[i];
                if (candidate == null)
                {
                    Layers.RemoveAt(i);
                    continue;
                }

                if (candidate.SourceSceneHandle != scene.handle)
                {
                    continue;
                }

                Layers.RemoveAt(i);
                candidate.DisposeLayer();
            }
        }

        internal static bool IsManaged(
            UIDocument document,
            DeucarianUISurfaceRole role)
        {
            if (document == null)
            {
                return false;
            }

            for (int i = Layers.Count - 1; i >= 0; i--)
            {
                OverlayLayer candidate = Layers[i];
                if (candidate == null || !candidate.IsAlive)
                {
                    Layers.RemoveAt(i);
                    candidate?.DisposeLayer();
                    continue;
                }

                if (candidate.Document == document &&
                    candidate.Role == role)
                {
                    return candidate.IsConfigured;
                }
            }

            return false;
        }

        internal sealed class OverlayLayer
        {
            private readonly GameObject layerObject;
            private readonly PanelSettings overlayPanelSettings;
            private int leaseCount;
            private bool disposed;

            internal OverlayLayer(
                PanelSettings sourcePanelSettings,
                DeucarianUISurfaceRole role,
                Scene sourceScene)
            {
                SourcePanelSettings = sourcePanelSettings;
                Role = role;
                SourceSceneHandle = sourceScene.handle;
                int depth = DeucarianUIDepth.Resolve(role);
                overlayPanelSettings = UnityEngine.Object.Instantiate(
                    sourcePanelSettings);
                overlayPanelSettings.name =
                    "DeucarianRuntime" + role + "PanelSettings";
                // Transient overlays composite above existing content. They
                // must never erase either the color or depth buffer below.
                overlayPanelSettings.clearColor = false;
                overlayPanelSettings.clearDepthStencil = false;
                overlayPanelSettings.sortingOrder = depth;

                layerObject = new GameObject(ObjectNamePrefix + role);
                SceneManager.MoveGameObjectToScene(
                    layerObject,
                    sourceScene);
                layerObject.hideFlags = HideFlags.DontSave;
                Document = layerObject.AddComponent<UIDocument>();
                Document.panelSettings = overlayPanelSettings;
                Document.sortingOrder = depth;
                Root = Document.rootVisualElement;
                Root.Clear();
                Root.pickingMode = PickingMode.Ignore;
                Root.style.position = Position.Absolute;
                Root.style.left = 0f;
                Root.style.right = 0f;
                Root.style.top = 0f;
                Root.style.bottom = 0f;
            }

            internal PanelSettings SourcePanelSettings { get; }
            internal DeucarianUISurfaceRole Role { get; }
            internal int SourceSceneHandle { get; }
            internal UIDocument Document { get; }
            internal VisualElement Root { get; }
            internal bool IsAlive =>
                !disposed && layerObject != null && Document != null;
            internal bool IsConfigured
            {
                get
                {
                    if (!IsAlive ||
                        Document.panelSettings != overlayPanelSettings)
                    {
                        return false;
                    }

                    int depth = DeucarianUIDepth.Resolve(Role);
                    return Mathf.Approximately(
                               Document.sortingOrder,
                               depth) &&
                           Mathf.Approximately(
                               overlayPanelSettings.sortingOrder,
                               depth) &&
                           !overlayPanelSettings.clearColor &&
                           !overlayPanelSettings.clearDepthStencil;
                }
            }

            internal DeucarianUIOverlayLease Acquire(string containerName)
            {
                var container = new VisualElement
                {
                    name = string.IsNullOrWhiteSpace(containerName)
                        ? "Deucarian" + Role + "Overlay"
                        : containerName,
                    pickingMode = PickingMode.Ignore
                };
                container.style.position = Position.Absolute;
                container.style.left = 0f;
                container.style.right = 0f;
                container.style.top = 0f;
                container.style.bottom = 0f;
                Root.Add(container);
                leaseCount++;
                return new DeucarianUIOverlayLease(
                    this,
                    container,
                    Role);
            }

            internal void Release(VisualElement container)
            {
                container?.RemoveFromHierarchy();
                leaseCount = Mathf.Max(0, leaseCount - 1);
                if (disposed || leaseCount > 0)
                {
                    return;
                }

                Layers.Remove(this);
                DisposeLayer();
            }

            internal void DisposeLayer()
            {
                if (disposed)
                {
                    return;
                }

                disposed = true;
                if (Document != null &&
                    Document.panelSettings == overlayPanelSettings)
                {
                    Document.panelSettings = null;
                }

                UnityObjectUtility.DestroySafely(overlayPanelSettings);
                UnityObjectUtility.DestroySafely(layerObject);
            }
        }
    }
}
