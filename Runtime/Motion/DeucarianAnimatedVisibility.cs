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
        private Coroutine routine;

        public DeucarianAnimatedVisibility(
            MonoBehaviour host,
            VisualElement element,
            DeucarianMotionProfile profile)
        {
            this.host = host;
            this.element = element;
            this.profile = profile;
        }

        public bool IsAnimating => routine != null;

        public void SetVisible(bool visible)
        {
            Stop();
            if (element == null)
            {
                return;
            }

            if (host == null || !host.isActiveAndEnabled || !Application.isPlaying)
            {
                ApplyProgress(element, profile, visible ? 1f : 0f, visible);
                element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
                return;
            }

            routine = host.StartCoroutine(Animate(visible));
        }

        public void Stop()
        {
            if (routine != null && host != null)
            {
                host.StopCoroutine(routine);
            }

            routine = null;
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

            float duration = Mathf.Max(0.0001f, profile.Duration(visible));
            float elapsed = 0f;
            while (elapsed < duration)
            {
                ApplyProgress(element, profile, elapsed / duration, visible);
                elapsed += Time.deltaTime;
                yield return null;
            }

            ApplyProgress(element, profile, 1f, visible);
            if (!visible)
            {
                element.style.display = DisplayStyle.None;
            }

            routine = null;
        }
    }
}
