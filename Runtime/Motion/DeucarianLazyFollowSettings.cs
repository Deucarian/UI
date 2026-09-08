using System;
using UnityEngine;

namespace Deucarian.UI
{
    [Serializable]
    public struct DeucarianLazyFollowSettings
    {
        [Tooltip("Movement in world units before the UI starts following."), Min(0)]
        public float positionDeadZone;
        [Tooltip("Rotation in degrees before the UI starts following."), Range(0, 180)]
        public float rotationDeadZone;
        [Tooltip("Response time in seconds. Zero follows immediately once outside the dead zone."), Range(0, 2)]
        public float smoothingSeconds;

        public static DeucarianLazyFollowSettings Default => new DeucarianLazyFollowSettings
        {
            positionDeadZone = 0.025f, rotationDeadZone = 3f, smoothingSeconds = 0.18f
        };

        public DeucarianLazyFollowSettings Sanitized() => new DeucarianLazyFollowSettings
        {
            positionDeadZone = FiniteRange(positionDeadZone, 0, 10),
            rotationDeadZone = FiniteRange(rotationDeadZone, 0, 180),
            smoothingSeconds = FiniteRange(smoothingSeconds, 0, 2)
        };

        private static float FiniteRange(float value, float min, float max) =>
            float.IsNaN(value) || float.IsInfinity(value) ? min : Mathf.Clamp(value, min, max);
    }
}
