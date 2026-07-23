using Deucarian.Common;
using UnityEngine;

namespace Deucarian.UI
{
    public readonly struct DeucarianMotionProfile
    {
        public DeucarianMotionProfile(
            float enterSeconds,
            float exitSeconds,
            DeucarianEasing enterEasing,
            DeucarianEasing exitEasing,
            float hiddenScale,
            float visibleScale,
            float hiddenOffsetY)
        {
            EnterSeconds = Mathf.Max(0f, enterSeconds);
            ExitSeconds = Mathf.Max(0f, exitSeconds);
            EnterEasing = enterEasing;
            ExitEasing = exitEasing;
            HiddenScale = Mathf.Max(0f, hiddenScale);
            VisibleScale = Mathf.Max(0f, visibleScale);
            HiddenOffsetY = hiddenOffsetY;
        }

        public float EnterSeconds { get; }
        public float ExitSeconds { get; }
        public DeucarianEasing EnterEasing { get; }
        public DeucarianEasing ExitEasing { get; }
        public float HiddenScale { get; }
        public float VisibleScale { get; }
        public float HiddenOffsetY { get; }

        public static DeucarianMotionProfile UiPanel => new DeucarianMotionProfile(
            0.18f,
            0.14f,
            DeucarianEasing.EaseOutSoftBack,
            DeucarianEasing.EaseInCubic,
            0.9f,
            1f,
            0f);

        public static DeucarianMotionProfile MediaOverlay => new DeucarianMotionProfile(
            0.32f,
            0.2f,
            DeucarianEasing.EaseOutSoftBack,
            DeucarianEasing.EaseInCubic,
            0.82f,
            1f,
            -0.08f);

        public static DeucarianMotionProfile IconSwap => new DeucarianMotionProfile(
            0.18f,
            0.14f,
            DeucarianEasing.EaseOutCubic,
            DeucarianEasing.EaseInCubic,
            0.92f,
            1f,
            0f);

        public static DeucarianMotionProfile ControlState => new DeucarianMotionProfile(
            0.14f,
            0.12f,
            DeucarianEasing.EaseOutCubic,
            DeucarianEasing.EaseInCubic,
            0.96f,
            1f,
            0f);

        public float Evaluate(bool entering, float progress)
        {
            return DeucarianEasingUtility.Evaluate(
                entering ? EnterEasing : ExitEasing,
                progress);
        }

        public float Duration(bool entering)
        {
            return entering ? EnterSeconds : ExitSeconds;
        }
    }
}
