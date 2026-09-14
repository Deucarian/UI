using System;
using Deucarian.Common;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Deucarian.UI
{
    /// <summary>Owns a menu lease independently of the consumer's UIDocument hierarchy.</summary>
    internal sealed class DeucarianMenuSurface : IDisposable
    {
        private readonly GameObject sourceObject;
        private readonly DeucarianUIOverlayLease lease;

        internal DeucarianMenuSurface(MonoBehaviour host)
        {
            UIDocument source = host.GetComponentInParent<UIDocument>(true);
            if (source == null || !DeucarianUIRuntime.HasCanonicalPanelSettings(source))
            {
                sourceObject = new GameObject(DeucarianMorphingMenu.DocumentObjectName);
                SceneManager.MoveGameObjectToScene(sourceObject, host.gameObject.scene);
                // A child UIDocument inherits its parent's panel; foreign panels need a separate carrier.
                if (source == null) sourceObject.transform.SetParent(host.transform, false);
                source = sourceObject.AddComponent<UIDocument>();
                DeucarianUIRuntime.Configure(source, DeucarianUISurfaceRole.PrimaryControls);
            }
            lease = DeucarianUIOverlayHost.Acquire(source, DeucarianUISurfaceRole.Menu,
                DeucarianMorphingMenu.RootName + "Surface");
        }

        internal UIDocument Document => lease.Document;
        internal VisualElement Container => lease.Root;
        internal void SetEnabled(bool enabled) =>
            Container.style.display = enabled ? DisplayStyle.Flex : DisplayStyle.None;

        public void Dispose()
        {
            lease.Dispose();
            if (sourceObject != null) UnityObjectUtility.DestroySafely(sourceObject);
        }
    }
}
