using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Deucarian.UI.Tests
{
    public sealed class DeucarianLayoutTransitionTests
    {
        [Test] public void FirstPlacementSnapsAndSubsequentPositionsEaseWithoutOvershoot()
        {
            var motion = new DeucarianLayoutTransition();
            motion.MoveTo(new Vector2(10, 20));
            Assert.That(motion.Current, Is.EqualTo(new Vector2(10, 20)));
            motion.MoveTo(new Vector2(10, 120), 1);
            Assert.That(motion.Current.y, Is.EqualTo(20));
            motion.Advance(.25f);
            Assert.That(motion.Current.y, Is.InRange(20.01f, 119.99f));
            motion.Advance(1);
            Assert.That(motion.Current, Is.EqualTo(motion.Target));
            Assert.That(motion.IsAnimating, Is.False);
        }

        [Test] public void RapidRetargetingStartsAtTheRenderedPositionAndRepeatedTargetsDoNotRestart()
        {
            var motion = new DeucarianLayoutTransition();
            motion.MoveTo(Vector2.zero); motion.MoveTo(Vector2.up * 100, 1); motion.Advance(.4f);
            var current = motion.Current;
            motion.MoveTo(Vector2.down * 100, 1);
            Assert.That(motion.Current, Is.EqualTo(current));
            for (int i = 0; i < 4; i++) { motion.MoveTo(Vector2.down * 100, 1); motion.Advance(.25f); }
            Assert.That(motion.Current, Is.EqualTo(Vector2.down * 100));
        }

        [Test] public void InstantModeResetAndInvalidInputsDoNotLeaveStaleMotion()
        {
            var motion = new DeucarianLayoutTransition();
            motion.MoveTo(Vector2.zero); motion.MoveTo(Vector2.one); motion.Advance(float.NaN);
            Assert.That(motion.Current, Is.EqualTo(Vector2.zero));
            motion.MoveTo(Vector2.one, 0);
            Assert.That(motion.Current, Is.EqualTo(Vector2.one));
            motion.MoveTo(new Vector2(float.NaN, 0));
            Assert.That(motion.Current, Is.EqualTo(Vector2.one));
            motion.Reset(); motion.MoveTo(Vector2.up * 300);
            Assert.That(motion.Current, Is.EqualTo(Vector2.up * 300));
        }

        private sealed class Host : EditorWindow { }

        [UnityTest] public IEnumerator ToolkitLayoutOffsetComposesWithoutOwningTheElementsTransformAndDisposes()
        {
            var host = ScriptableObject.CreateInstance<Host>(); host.Show();
            try
            {
                var parent = new VisualElement(); var row = new VisualElement(); parent.Add(row);
                host.rootVisualElement.Add(parent);
                yield return null;
                yield return null;
                row.transform.position = Vector3.right * 30;
                Vector2 offset = Vector2.zero;
                using var motion = new DeucarianUIToolkitReflow(row, value => offset = value);
                Geometry(row, 20); Geometry(row, 100);
                Assert.That(offset.y, Is.EqualTo(-80));
                Assert.That(row.transform.position.x, Is.EqualTo(30), "The consumer composes the returned offset.");
                motion.Advance(.05f);
                float painted = 100 + offset.y;
                Geometry(row, 200);
                Assert.That(200 + offset.y, Is.EqualTo(painted).Within(.001));
                motion.DurationSeconds = 0; Assert.That(offset, Is.EqualTo(Vector2.zero));
                motion.Dispose(); Geometry(row, 400);
                Assert.That(offset, Is.EqualTo(Vector2.zero));
            }
            finally { host.Close(); }
        }

        private static void Geometry(VisualElement row, float y)
        {
            using var evt = GeometryChangedEvent.GetPooled(Rect.zero, new Rect(0, y, 200, 80));
            evt.target = row; row.SendEvent(evt);
        }
    }
}
