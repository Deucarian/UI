using System;
using Deucarian.Tweens;

namespace Deucarian.UI
{
    public enum DeucarianVisibilityPhase
    {
        Hidden,
        Entering,
        Visible,
        Exiting
    }

    /// <summary>
    /// Compatibility adapter for UI motion profiles. Tweens owns reversible progress;
    /// the consumer chooses the time source and applies it to its presentation.
    /// </summary>
    public sealed class DeucarianVisibilityTransition
    {
        private readonly DeucarianMotionProfile profile;
        private readonly VisibilityProgress transition = new VisibilityProgress();

        public DeucarianVisibilityTransition(DeucarianMotionProfile profile)
        {
            this.profile = profile;
            transition.Completed += _ => Completed?.Invoke(this);
        }

        public event Action<DeucarianVisibilityTransition> Completed;
        public DeucarianMotionProfile Profile => profile;
        public float Progress => transition.Progress;
        public DeucarianVisibilityPhase Phase => transition.Phase switch
        {
            VisibilityPhase.Entering => DeucarianVisibilityPhase.Entering,
            VisibilityPhase.Visible => DeucarianVisibilityPhase.Visible,
            VisibilityPhase.Exiting => DeucarianVisibilityPhase.Exiting,
            _ => DeucarianVisibilityPhase.Hidden
        };
        public bool IsAnimating => transition.IsAnimating;
        public bool IsHiding => transition.Phase == VisibilityPhase.Exiting;
        public float RemainingSeconds => transition.RemainingSeconds;

        public float Show(bool restartFromHidden = false)
        {
            if (restartFromHidden) transition.Reset(false);
            return transition.MoveTo(true, profile.EnterSeconds, profile.EnterEasing);
        }

        public float Hide() => transition.MoveTo(false, profile.ExitSeconds, profile.ExitEasing);
        public bool Advance(float deltaSeconds) => transition.Advance(deltaSeconds);
        public void Complete() => transition.Complete();
        public void Reset(bool visible) => transition.Reset(visible);
        public void SetProgress(float progress) => transition.SetProgress(progress);
    }
}
