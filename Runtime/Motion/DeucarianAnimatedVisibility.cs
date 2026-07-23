using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.UI
{
    public sealed class DeucarianAnimatedVisibility
    {
        private readonly MonoBehaviour host;
        private readonly VisualElement element;
        private readonly DeucarianMotionProfile profile;
        private readonly DeucarianVisibilityTransition transition;
        private Coroutine routine;
        private Action pendingCompletion;
        private bool hasTarget;
        private bool targetVisible;

        public DeucarianAnimatedVisibility(
            MonoBehaviour host,
            VisualElement element,
            DeucarianMotionProfile profile)
        {
            this.host = host;
            this.element = element;
            this.profile = profile;
            transition = new DeucarianVisibilityTransition(profile);
        }

        public bool IsAnimating => routine != null;
        public float Progress => transition.Progress;
        public bool TargetVisible => hasTarget && targetVisible;

        public void SetVisible(bool visible)
        {
            SetVisible(visible, true, null);
        }

        public void SetVisible(bool visible, Action completed)
        {
            SetVisible(visible, true, completed);
        }

        public void SetVisible(bool visible, bool animate, Action completed = null)
        {
            if (element == null)
            {
                Stop();
                hasTarget = true;
                targetVisible = visible;
                completed?.Invoke();
                return;
            }

            if (hasTarget && targetVisible == visible)
            {
                if (routine != null)
                {
                    AddPendingCompletion(completed);
                    return;
                }

                ApplyImmediate(visible);
                completed?.Invoke();
                return;
            }

            Stop();
            hasTarget = true;
            targetVisible = visible;
            if (!animate ||
                host == null ||
                !host.isActiveAndEnabled ||
                !Application.isPlaying)
            {
                ApplyImmediate(visible);
                completed?.Invoke();
                return;
            }

            AddPendingCompletion(completed);
            routine = host.StartCoroutine(Animate(visible));
        }

        public void Stop()
        {
            if (routine != null && host != null)
            {
                host.StopCoroutine(routine);
            }

            routine = null;
            pendingCompletion = null;
            hasTarget = false;
        }

        public static void ApplyProgress(
            VisualElement element,
            DeucarianMotionProfile profile,
            float progress,
            bool entering)
        {
            if (element == null)
            {
                return;
            }

            float t = Mathf.Clamp01(progress);
            float eased = profile.Evaluate(entering, t);
            float visibleProgress = entering ? eased : 1f - eased;
            float scale = Mathf.Lerp(profile.HiddenScale, profile.VisibleScale, visibleProgress);
            float offsetY = Mathf.Lerp(profile.HiddenOffsetY, 0f, visibleProgress);
            element.style.opacity = visibleProgress;
            element.style.scale = new Scale(new Vector3(scale, scale, 1f));
            element.style.translate = new Translate(0f, Length.Percent(offsetY * 100f), 0f);
        }

        private IEnumerator Animate(bool visible)
        {
            if (visible)
            {
                element.style.display = DisplayStyle.Flex;
            }

            if (visible)
            {
                transition.Show();
            }
            else
            {
                transition.Hide();
            }

            ApplyVisibleProgress(element, profile, transition.Progress);
            while (transition.IsAnimating)
            {
                yield return null;
                transition.Advance(Time.deltaTime);
                ApplyVisibleProgress(element, profile, transition.Progress);
            }

            if (!visible)
            {
                element.style.display = DisplayStyle.None;
            }

            routine = null;
            Action completed = pendingCompletion;
            pendingCompletion = null;
            completed?.Invoke();
        }

        private void ApplyImmediate(bool visible)
        {
            transition.Reset(visible);
            ApplyVisibleProgress(element, profile, transition.Progress);
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void AddPendingCompletion(Action completed)
        {
            if (completed == null)
            {
                return;
            }

            pendingCompletion -= completed;
            pendingCompletion += completed;
        }

        private static void ApplyVisibleProgress(
            VisualElement element,
            DeucarianMotionProfile profile,
            float visibleProgress)
        {
            if (element == null)
            {
                return;
            }

            float progress = Mathf.Clamp01(visibleProgress);
            float scale = Mathf.Lerp(profile.HiddenScale, profile.VisibleScale, progress);
            float offsetY = Mathf.Lerp(profile.HiddenOffsetY, 0f, progress);
            element.style.opacity = progress;
            element.style.scale = new Scale(new Vector3(scale, scale, 1f));
            element.style.translate = new Translate(0f, Length.Percent(offsetY * 100f), 0f);
        }

    }
}
