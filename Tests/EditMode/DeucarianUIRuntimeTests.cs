using System;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Deucarian.UI.Tests
{
    public sealed class DeucarianUIRuntimeTests
    {
        [Test]
        public void SemanticSurfaceDepthsAreUniqueAndStrictlyOrdered()
        {
            var roles = new[]
            {
                DeucarianUISurfaceRole.PrimaryControls,
                DeucarianUISurfaceRole.ContextControls,
                DeucarianUISurfaceRole.MediaControls,
                DeucarianUISurfaceRole.Status,
                DeucarianUISurfaceRole.Menu,
                DeucarianUISurfaceRole.Modal,
                DeucarianUISurfaceRole.Tooltip
            };

            for (int i = 1; i < roles.Length; i++)
            {
                Assert.That(
                    DeucarianUIDepth.Resolve(roles[i]),
                    Is.GreaterThan(
                        DeucarianUIDepth.Resolve(roles[i - 1])),
                    roles[i] + " must render above " + roles[i - 1]);
            }

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                DeucarianUIDepth.Resolve(
                    (DeucarianUISurfaceRole)int.MaxValue));
        }

        [Test]
        public void LegacyMenuSortingPropertyCannotOverrideSharedPolicy()
        {
            var layout = new DeucarianMorphingMenuLayout();
            int canonical = DeucarianUIDepth.Resolve(
                DeucarianUISurfaceRole.Menu);

#pragma warning disable 618
            Assert.That(layout.SortingOrder, Is.EqualTo(canonical));
            Assert.DoesNotThrow(() => layout.SortingOrder = canonical);
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                layout.SortingOrder = canonical + 1);
#pragma warning restore 618
        }

        [Test]
        public void ConfigureDocumentOwnsCanonicalPanelAndSemanticDepth()
        {
            GameObject root = new GameObject("Configured UI Document");
            UIDocument document = root.AddComponent<UIDocument>();
            PanelSettings foreignSettings =
                ScriptableObject.CreateInstance<PanelSettings>();
            document.panelSettings = foreignSettings;
            document.sortingOrder = DeucarianUIDepth.Resolve(
                DeucarianUISurfaceRole.MediaControls);
            try
            {
                Assert.True(
                    DeucarianUIRuntime.CanonicalPanelSettingsAvailable);
                Assert.False(
                    DeucarianUIRuntime.HasCanonicalPanelSettings(document));
                Assert.False(
                    DeucarianUIRuntime.IsConfigured(
                        document,
                        DeucarianUISurfaceRole.MediaControls));

                DeucarianUIRuntime.Configure(
                    document,
                    DeucarianUISurfaceRole.MediaControls);

                Assert.True(
                    DeucarianUIRuntime.HasCanonicalPanelSettings(document));
                Assert.True(
                    DeucarianUIRuntime.IsConfigured(
                        document,
                        DeucarianUISurfaceRole.MediaControls));
                Assert.That(
                    document.sortingOrder,
                    Is.EqualTo(
                        DeucarianUIDepth.Resolve(
                            DeucarianUISurfaceRole.MediaControls)));
                Assert.That(
                    document.panelSettings,
                    Is.Not.SameAs(foreignSettings));
                Assert.False(
                    DeucarianUIRuntime.IsConfigured(
                        document,
                        DeucarianUISurfaceRole.Status));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                UnityEngine.Object.DestroyImmediate(foreignSettings);
            }
        }

        [Test]
        public void CanonicalPanelOwnsPackageTextSettings()
        {
            PanelSettings panelSettings =
                DeucarianUIRuntimeAssets.LoadRuntimePanelSettings();
            PanelTextSettings textSettings =
                DeucarianUIRuntimeAssets.LoadRuntimePanelTextSettings();

            Assert.That(panelSettings, Is.Not.Null);
            Assert.That(textSettings, Is.Not.Null);
            Assert.That(panelSettings.textSettings, Is.SameAs(textSettings));
        }

        [Test]
        public void ConfigureScreenSpaceCanvasOwnsModeAndSemanticDepth()
        {
            GameObject root = new GameObject("Configured UI Canvas");
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            try
            {
                DeucarianUIRuntime.ConfigureScreenSpaceCanvas(
                    canvas,
                    DeucarianUISurfaceRole.Status);

                Assert.That(
                    canvas.renderMode,
                    Is.EqualTo(RenderMode.ScreenSpaceOverlay));
                Assert.True(canvas.isRootCanvas || canvas.overrideSorting);
                Assert.True(
                    DeucarianUIRuntime.IsConfigured(
                        canvas,
                        DeucarianUISurfaceRole.Status));
                Assert.That(
                    canvas.sortingOrder,
                    Is.EqualTo(
                        DeucarianUIDepth.Resolve(
                            DeucarianUISurfaceRole.Status)));
                Assert.False(
                    DeucarianUIRuntime.IsConfigured(
                        canvas,
                        DeucarianUISurfaceRole.Menu));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void OverlayHostRequiresCanonicalSourceAndTransientRole()
        {
            GameObject root = new GameObject("Overlay Contract Source");
            UIDocument document = root.AddComponent<UIDocument>();
            try
            {
                Assert.Throws<InvalidOperationException>(() =>
                    DeucarianUIOverlayHost.Acquire(
                        document,
                        DeucarianUISurfaceRole.Tooltip));

                DeucarianUIRuntime.Configure(
                    document,
                    DeucarianUISurfaceRole.PrimaryControls);

                Assert.Throws<ArgumentException>(() =>
                    DeucarianUIOverlayHost.Acquire(
                        document,
                        DeucarianUISurfaceRole.Status));
                Assert.Throws<ArgumentNullException>(() =>
                    DeucarianUIOverlayHost.Acquire(
                        null,
                        DeucarianUISurfaceRole.Tooltip));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void OverlayLeasesShareRoleLayerAndIsolateContainers()
        {
            Scene testScene = EditorSceneManager.NewPreviewScene();
            GameObject firstObject = new GameObject("First Overlay Source");
            GameObject secondObject = new GameObject("Second Overlay Source");
            SceneManager.MoveGameObjectToScene(firstObject, testScene);
            SceneManager.MoveGameObjectToScene(secondObject, testScene);
            UIDocument firstDocument = firstObject.AddComponent<UIDocument>();
            UIDocument secondDocument =
                secondObject.AddComponent<UIDocument>();
            DeucarianUIRuntime.Configure(
                firstDocument,
                DeucarianUISurfaceRole.PrimaryControls);
            DeucarianUIRuntime.Configure(
                secondDocument,
                DeucarianUISurfaceRole.ContextControls);
            DeucarianUIOverlayLease firstTooltip = null;
            DeucarianUIOverlayLease secondTooltip = null;
            DeucarianUIOverlayLease modal = null;
            UIDocument tooltipDocument = null;
            UIDocument modalDocument = null;
            try
            {
                firstTooltip = DeucarianUIOverlayHost.Acquire(
                    firstDocument,
                    DeucarianUISurfaceRole.Tooltip,
                    "First Tooltip Container");
                secondTooltip = DeucarianUIOverlayHost.Acquire(
                    secondDocument,
                    DeucarianUISurfaceRole.Tooltip,
                    "Second Tooltip Container");
                modal = DeucarianUIOverlayHost.Acquire(
                    firstDocument,
                    DeucarianUISurfaceRole.Modal,
                    "Modal Container");
                tooltipDocument = firstTooltip.Document;
                modalDocument = modal.Document;

                Assert.That(
                    secondTooltip.Document,
                    Is.SameAs(tooltipDocument));
                Assert.That(modalDocument, Is.Not.SameAs(tooltipDocument));
                Assert.That(
                    firstTooltip.Root,
                    Is.Not.SameAs(secondTooltip.Root));
                Assert.That(
                    firstTooltip.Root.parent,
                    Is.SameAs(tooltipDocument.rootVisualElement));
                Assert.That(
                    secondTooltip.Root.parent,
                    Is.SameAs(tooltipDocument.rootVisualElement));
                Assert.That(
                    tooltipDocument.panelSettings,
                    Is.Not.SameAs(firstDocument.panelSettings));
                Assert.False(tooltipDocument.panelSettings.clearColor);
                Assert.False(
                    tooltipDocument.panelSettings.clearDepthStencil);
                Assert.That(
                    tooltipDocument.panelSettings.sortingOrder,
                    Is.EqualTo(
                        DeucarianUIDepth.Resolve(
                            DeucarianUISurfaceRole.Tooltip)));
                Assert.True(
                    DeucarianUIRuntime.IsConfigured(
                        tooltipDocument,
                        DeucarianUISurfaceRole.Tooltip));
                Assert.False(
                    DeucarianUIRuntime.HasCanonicalPanelSettings(
                        tooltipDocument));
                Assert.That(
                    tooltipDocument.sortingOrder,
                    Is.GreaterThan(modalDocument.sortingOrder));

                firstTooltip.Dispose();
                firstTooltip.Dispose();
                Assert.True(firstTooltip.IsDisposed);
                Assert.That(firstTooltip.Root.parent, Is.Null);
                Assert.That(tooltipDocument, Is.Not.Null);

                secondTooltip.Dispose();
                secondTooltip = null;
                Assert.True(tooltipDocument == null);
                Assert.That(modalDocument, Is.Not.Null);
            }
            finally
            {
                firstTooltip?.Dispose();
                secondTooltip?.Dispose();
                modal?.Dispose();
                UnityEngine.Object.DestroyImmediate(firstObject);
                UnityEngine.Object.DestroyImmediate(secondObject);
                if (testScene.IsValid() && testScene.isLoaded)
                {
                    EditorSceneManager.ClosePreviewScene(testScene);
                }
            }
        }

        [Test]
        public void OverlayLayersAreScopedToSourceSceneAndUnloadIndependently()
        {
            Scene originalScene = SceneManager.GetActiveScene();
            Scene firstScene = default;
            Scene secondScene = default;
            DeucarianUIOverlayLease firstTooltip = null;
            DeucarianUIOverlayLease secondTooltip = null;
            try
            {
                firstScene = EditorSceneManager.NewPreviewScene();
                secondScene = EditorSceneManager.NewPreviewScene();

                GameObject firstSource =
                    new GameObject("First Scene Tooltip Source");
                GameObject secondSource =
                    new GameObject("Second Scene Tooltip Source");
                SceneManager.MoveGameObjectToScene(firstSource, firstScene);
                SceneManager.MoveGameObjectToScene(secondSource, secondScene);
                UIDocument firstDocument =
                    firstSource.AddComponent<UIDocument>();
                UIDocument secondDocument =
                    secondSource.AddComponent<UIDocument>();
                DeucarianUIRuntime.Configure(
                    firstDocument,
                    DeucarianUISurfaceRole.PrimaryControls);
                DeucarianUIRuntime.Configure(
                    secondDocument,
                    DeucarianUISurfaceRole.PrimaryControls);

                firstTooltip = DeucarianUIOverlayHost.Acquire(
                    firstDocument,
                    DeucarianUISurfaceRole.Tooltip);
                secondTooltip = DeucarianUIOverlayHost.Acquire(
                    secondDocument,
                    DeucarianUISurfaceRole.Tooltip);

                Assert.That(
                    firstTooltip.Document,
                    Is.Not.SameAs(secondTooltip.Document));
                Assert.That(
                    firstTooltip.Document.gameObject.scene.handle,
                    Is.EqualTo(firstScene.handle));
                Assert.That(
                    secondTooltip.Document.gameObject.scene.handle,
                    Is.EqualTo(secondScene.handle));
                Assert.True(
                    DeucarianUIRuntime.IsConfigured(
                        firstTooltip.Document,
                        DeucarianUISurfaceRole.Tooltip));
                Assert.True(
                    DeucarianUIRuntime.IsConfigured(
                        secondTooltip.Document,
                        DeucarianUISurfaceRole.Tooltip));

                EditorSceneManager.ClosePreviewScene(firstScene);
                firstScene = default;

                Assert.True(firstTooltip.IsDisposed);
                Assert.That(firstTooltip.Document, Is.Null);
                Assert.False(secondTooltip.IsDisposed);
                Assert.True(
                    DeucarianUIRuntime.IsConfigured(
                        secondTooltip.Document,
                        DeucarianUISurfaceRole.Tooltip));
                Assert.That(
                    secondTooltip.Document.gameObject.scene.handle,
                    Is.EqualTo(secondScene.handle));
            }
            finally
            {
                firstTooltip?.Dispose();
                secondTooltip?.Dispose();
                if (firstScene.IsValid() && firstScene.isLoaded)
                {
                    EditorSceneManager.ClosePreviewScene(firstScene);
                }

                if (secondScene.IsValid() && secondScene.isLoaded)
                {
                    EditorSceneManager.ClosePreviewScene(secondScene);
                }

                if (originalScene.IsValid() && originalScene.isLoaded)
                {
                    SceneManager.SetActiveScene(originalScene);
                }
            }
        }

        [Test]
        public void RuntimeConfigurationRejectsNullTargets()
        {
            Assert.Throws<ArgumentNullException>(() =>
                DeucarianUIRuntime.Configure(
                    null,
                    DeucarianUISurfaceRole.PrimaryControls));
            Assert.Throws<ArgumentNullException>(() =>
                DeucarianUIRuntime.ConfigureScreenSpaceCanvas(
                    null,
                    DeucarianUISurfaceRole.PrimaryControls));
            Assert.False(
                DeucarianUIRuntime.HasCanonicalPanelSettings(null));
            Assert.False(
                DeucarianUIRuntime.IsConfigured(
                    (UIDocument)null,
                    DeucarianUISurfaceRole.PrimaryControls));
            Assert.False(
                DeucarianUIRuntime.IsConfigured(
                    (Canvas)null,
                    DeucarianUISurfaceRole.PrimaryControls));
        }
    }
}
