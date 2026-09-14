using Deucarian.Tweens;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.UI
{
    public sealed class DeucarianIconSwap : ITweenUpdate
    {
        private readonly MonoBehaviour host;
        private readonly VisualElement firstIcon;
        private readonly VisualElement secondIcon;
        private readonly DeucarianMotionProfile profile;
        private TweenHandle handle;
        private float startOpacity;
        private float targetOpacity;
        private float elapsed;
        private float duration;
        private bool firstVisible;
        private float firstOpacity;

        public DeucarianIconSwap(
            MonoBehaviour host,
            VisualElement firstIcon,
            VisualElement secondIcon,
            DeucarianMotionProfile profile)
        {
            this.host = host;
            this.firstIcon = firstIcon;
            this.secondIcon = secondIcon;
            this.profile = profile;
        }

        public bool IsAnimating => handle.IsActive;
        public bool FirstVisible => firstVisible;

        public void SetFirstVisible(bool visible, bool animate)
        {
            Stop();

            bool changed = firstVisible != visible;
            firstVisible = visible;
            if (!changed || !animate || host == null || !host.isActiveAndEnabled || !Application.isPlaying)
            {
                firstOpacity = visible ? 1f : 0f;
                SetImmediate(firstIcon, secondIcon, visible);
                return;
            }

            startOpacity = firstOpacity;
            targetOpacity = visible ? 1f : 0f;
            elapsed = 0f;
            duration = Mathf.Max(0.0001f, profile.Duration(visible) * Mathf.Abs(targetOpacity - startOpacity));
            if (firstIcon == null || secondIcon == null)
            {
                firstOpacity = targetOpacity;
                SetImmediate(firstIcon, secondIcon, visible);
                return;
            }
            firstIcon.style.display = secondIcon.style.display = DisplayStyle.Flex;
            handle = TweenRuntime.Scheduler.Schedule(this);
        }

        public void Stop()
        {
            handle.Cancel();
            handle = default;
        }

        public static void ConfigureIconSlot(VisualElement icon, float buttonSize, float iconSize)
        {
            if (icon == null)
            {
                return;
            }

            // Centre within the actual content box, including any reserved border.
            float halfIcon = iconSize * 0.5f;
            icon.style.position = Position.Absolute;
            icon.style.width = iconSize;
            icon.style.height = iconSize;
            icon.style.minWidth = iconSize;
            icon.style.minHeight = iconSize;
            icon.style.maxWidth = iconSize;
            icon.style.maxHeight = iconSize;
            icon.style.left = Length.Percent(50f);
            icon.style.top = Length.Percent(50f);
            icon.style.marginLeft = -halfIcon;
            icon.style.marginTop = -halfIcon;
            icon.pickingMode = PickingMode.Ignore;
        }

        public static void SetImmediate(VisualElement firstIcon, VisualElement secondIcon, bool firstVisible)
        {
            SetIcon(firstIcon, firstVisible, firstVisible ? 1f : 0f);
            SetIcon(secondIcon, !firstVisible, firstVisible ? 0f : 1f);
        }

        bool ITweenUpdate.IsAlive => host != null && host.isActiveAndEnabled && firstIcon != null && secondIcon != null;

        bool ITweenUpdate.Advance(float scaledSeconds, float unscaledSeconds)
        {
            elapsed += unscaledSeconds;
            firstOpacity = Mathf.Lerp(startOpacity, targetOpacity,
                profile.Evaluate(firstVisible, Mathf.Clamp01(elapsed / duration)));
            firstIcon.style.opacity = firstOpacity;
            secondIcon.style.opacity = 1f - firstOpacity;
            return elapsed < duration;
        }

        void ITweenUpdate.Stopped(TweenHandle stopped, TweenStopReason reason)
        {
            if (handle != stopped) return;
            handle = default;
            if (reason != TweenStopReason.Completed) return;
            firstOpacity = targetOpacity;
            SetImmediate(firstIcon, secondIcon, firstVisible);
        }

        private static void SetIcon(VisualElement icon, bool visible, float opacity)
        {
            if (icon == null)
            {
                return;
            }

            icon.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            icon.style.opacity = Mathf.Clamp01(opacity);
        }
    }
}
