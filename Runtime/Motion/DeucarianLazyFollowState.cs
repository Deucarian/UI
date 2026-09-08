using UnityEngine;

namespace Deucarian.UI
{
    /// <summary>Explicit pose policy: no scene search, XR service, Unity lifecycle, or implicit clock.</summary>
    public sealed class DeucarianLazyFollowState
    {
        private bool initialized;
        private bool moving;
        private bool rotating;
        public Pose Pose { get; private set; }

        public void Reset(Pose pose)
        {
            Pose = pose;
            initialized = true;
            moving = rotating = false;
        }

        public Pose Advance(Pose desired, DeucarianLazyFollowSettings settings, float deltaSeconds)
        {
            if (!initialized) Reset(desired);
            if (float.IsNaN(deltaSeconds) || float.IsInfinity(deltaSeconds) || deltaSeconds <= 0) return Pose;
            settings = settings.Sanitized();
            var pose = Pose;
            float distance = Vector3.Distance(pose.position, desired.position);
            float angle = Quaternion.Angle(pose.rotation, desired.rotation);
            moving |= distance > settings.positionDeadZone;
            rotating |= angle > settings.rotationDeadZone;
            float blend = settings.smoothingSeconds <= 0 ? 1 : 1 - Mathf.Exp(-deltaSeconds / settings.smoothingSeconds);
            if (moving) pose.position = Vector3.LerpUnclamped(pose.position, desired.position, blend);
            if (rotating) pose.rotation = Quaternion.SlerpUnclamped(pose.rotation, desired.rotation, blend);
            if (moving && Vector3.Distance(pose.position, desired.position) <= 0.0001f)
            {
                pose.position = desired.position;
                moving = false;
            }
            if (rotating && Quaternion.Angle(pose.rotation, desired.rotation) <= 0.05f)
            {
                pose.rotation = desired.rotation;
                rotating = false;
            }
            Pose = pose;
            return pose;
        }
    }
}
