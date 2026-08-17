using Deucarian.Common;
using UnityEngine;

namespace Deucarian.UI
{
    /// <summary>
    /// Shared motion contract for a compact control that expands into its own
    /// surface. Feature routing and menu contents remain consumer-owned.
    /// </summary>
    public static class DeucarianMorphingMenuMotion
    {
        public const float CollapsedSize = 40f;
        public const float ExpandSeconds = 0.18f;
        public const float CollapseSeconds = 0.14f;
        public const float BodyRevealThreshold = 0.35f;
        public const float BodyHiddenOffset = -4f;

        public static float ResolveBodyOpacity(float expansionProgress)
        {
            if (expansionProgress <= BodyRevealThreshold)
            {
                return 0f;
            }

            return Mathf.Clamp01(
                (expansionProgress - BodyRevealThreshold) /
                (1f - BodyRevealThreshold));
        }

        public static float ResolveDuration(bool expanding) =>
            expanding ? ExpandSeconds : CollapseSeconds;

        public static float Ease(bool expanding, float progress)
        {
            return DeucarianEasingUtility.Evaluate(
                expanding
                    ? DeucarianEasing.EaseOutCubic
                    : DeucarianEasing.EaseInCubic,
                progress);
        }
    }
}
