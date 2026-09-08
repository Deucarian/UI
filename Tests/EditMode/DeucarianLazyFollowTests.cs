using NUnit.Framework;
using UnityEngine;

namespace Deucarian.UI.Tests
{
    public sealed class DeucarianLazyFollowTests
    {
        [Test]
        public void SmallHeadMotionStaysInsideTheDeadZone()
        {
            var motion = new DeucarianLazyFollowState();
            motion.Reset(new Pose(Vector3.zero, Quaternion.identity));
            var result = motion.Advance(new Pose(Vector3.right * 0.01f, Quaternion.Euler(0, 1, 0)),
                DeucarianLazyFollowSettings.Default, 1);
            Assert.That(result.position, Is.EqualTo(Vector3.zero));
            Assert.That(result.rotation, Is.EqualTo(Quaternion.identity));
        }

        [Test]
        public void CrossingTheThresholdStartsFollowingUntilSettled()
        {
            var motion = new DeucarianLazyFollowState();
            motion.Reset(new Pose(Vector3.zero, Quaternion.identity));
            var desired = new Pose(Vector3.right, Quaternion.Euler(0, 40, 0));
            var first = motion.Advance(desired, DeucarianLazyFollowSettings.Default, 0.02f);
            Assert.That(first.position.x, Is.GreaterThan(0).And.LessThan(1));
            for (int i = 0; i < 120; i++) motion.Advance(desired, DeucarianLazyFollowSettings.Default, 0.02f);
            Assert.That(motion.Pose.position, Is.EqualTo(desired.position));
            Assert.That(Quaternion.Angle(motion.Pose.rotation, desired.rotation), Is.LessThan(0.05f));
        }

        [Test]
        public void ResponseDoesNotDependOnFrameRate()
        {
            var slow = new DeucarianLazyFollowState();
            var fast = new DeucarianLazyFollowState();
            var start = new Pose(Vector3.zero, Quaternion.identity);
            var goal = new Pose(Vector3.right, Quaternion.Euler(0, 30, 0));
            slow.Reset(start);
            fast.Reset(start);
            for (int i = 0; i < 10; i++) slow.Advance(goal, DeucarianLazyFollowSettings.Default, 0.02f);
            for (int i = 0; i < 20; i++) fast.Advance(goal, DeucarianLazyFollowSettings.Default, 0.01f);
            Assert.That(Vector3.Distance(slow.Pose.position, fast.Pose.position), Is.LessThan(0.00001f));
            Assert.That(Quaternion.Angle(slow.Pose.rotation, fast.Pose.rotation), Is.LessThan(0.05f));
        }

        [Test]
        public void ZeroResponseSnapsAndInvalidTimeDoesNotPoisonState()
        {
            var motion = new DeucarianLazyFollowState();
            motion.Reset(new Pose(Vector3.zero, Quaternion.identity));
            var goal = new Pose(Vector3.right, Quaternion.identity);
            Assert.That(motion.Advance(goal, default, float.NaN).position, Is.EqualTo(Vector3.zero));
            Assert.That(motion.Advance(goal, default, 0).position, Is.EqualTo(Vector3.zero));
            Assert.That(motion.Advance(goal, default, 0.01f).position, Is.EqualTo(Vector3.right));
        }

        [Test]
        public void ExplicitTargetAndOffsetsRecenterWithoutAnXrOrCameraService()
        {
            var anchor = new GameObject("Anchor");
            var ui = new GameObject("UI");
            try
            {
                anchor.transform.SetPositionAndRotation(Vector3.right, Quaternion.Euler(0, 90, 0));
                var follow = ui.AddComponent<DeucarianLazyFollow>();
                follow.Configure(anchor.transform, Vector3.forward * 2, Vector3.zero, DeucarianLazyFollowSettings.Default);
                Assert.That(Vector3.Distance(ui.transform.position, anchor.transform.TransformPoint(Vector3.forward * 2)), Is.LessThan(0.0001f));
                Assert.That(Quaternion.Angle(ui.transform.rotation, anchor.transform.rotation), Is.LessThan(0.05f));
                follow.Target = null;
                Assert.DoesNotThrow(follow.Recenter);
            }
            finally { Object.DestroyImmediate(ui); Object.DestroyImmediate(anchor); }
        }
    }
}
