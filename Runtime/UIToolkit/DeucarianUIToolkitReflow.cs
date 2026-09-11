using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.UI
{
    /// <summary>Inverts layout changes into a temporary offset, composed by the caller with other item motion.</summary>
    public sealed class DeucarianUIToolkitReflow : IDisposable
    {
        private readonly VisualElement element;
        private readonly Action<Vector2> applyOffset;
        private readonly DeucarianLayoutTransition motion = new DeucarianLayoutTransition();
        private VisualElement parent;
        private float duration = DeucarianLayoutTransition.DefaultDurationSeconds;
        private bool disposed;

        public DeucarianUIToolkitReflow(VisualElement element, Action<Vector2> applyOffset)
        {
            this.element = element ?? throw new ArgumentNullException(nameof(element));
            this.applyOffset = applyOffset ?? throw new ArgumentNullException(nameof(applyOffset));
            element.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        public Vector2 Offset => motion.IsInitialized ? motion.Current - motion.Target : Vector2.zero;
        public float DurationSeconds
        {
            get => duration;
            set
            {
                duration = float.IsNaN(value) || float.IsInfinity(value) ? 0 : Mathf.Max(0, value);
                if (duration <= 0) { motion.Complete(); Apply(); }
            }
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (disposed || evt.target != element || evt.newRect.width <= 0 || evt.newRect.height <= 0) return;
            if (parent != element.parent) { parent = element.parent; motion.Reset(); }
            motion.MoveTo(evt.newRect.position, duration);
            Apply();
        }

        public void Advance(float seconds) { if (!disposed) { motion.Advance(seconds); Apply(); } }
        public void Reset() { motion.Reset(); parent = null; Apply(); }
        private void Apply() { if (!disposed) applyOffset(Offset); }
        public void Dispose()
        {
            if (disposed) return;
            Reset();
            disposed = true;
            element.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }
    }
}
