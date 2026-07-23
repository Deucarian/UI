using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.UI
{
    public sealed class DeucarianIconSwap
    {
        private readonly MonoBehaviour host;
        private readonly VisualElement firstIcon;
        private readonly VisualElement secondIcon;
        private readonly DeucarianMotionProfile profile;
        private Coroutine routine;
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

        public bool IsAnimating => routine != null;
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

        public static void ConfigureIconSlot(VisualElement icon, float buttonSize, float iconSize)
        {
            if (icon == null)
            {
                return;
            }

            float offset = (buttonSize - iconSize) * 0.5f;
            icon.style.position = Position.Absolute;
            icon.style.width = iconSize;
            icon.style.height = iconSize;
            icon.style.minWidth = iconSize;
            icon.style.minHeight = iconSize;
            icon.style.maxWidth = iconSize;
            icon.style.maxHeight = iconSize;
            icon.style.left = offset;
            icon.style.top = offset;
            icon.pickingMode = PickingMode.Ignore;
        }

        public static void SetImmediate(VisualElement firstIcon, VisualElement secondIcon, bool firstVisible)
        {
            SetIcon(firstIcon, firstVisible, firstVisible ? 1f : 0f);
            SetIcon(secondIcon, !firstVisible, firstVisible ? 0f : 1f);
        }

        private IEnumerator Animate(bool finalFirstVisible)
        {
            if (firstIcon == null || secondIcon == null)
            {
                firstOpacity = finalFirstVisible ? 1f : 0f;
                SetImmediate(firstIcon, secondIcon, finalFirstVisible);
                routine = null;
                yield break;
            }

            firstIcon.style.display = DisplayStyle.Flex;
            secondIcon.style.display = DisplayStyle.Flex;
            float startOpacity = firstOpacity;
            float targetOpacity = finalFirstVisible ? 1f : 0f;
            bool entering = targetOpacity > startOpacity;
            float duration = Mathf.Max(
                0.0001f,
                profile.Duration(entering) * Mathf.Abs(targetOpacity - startOpacity));
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float eased = profile.Evaluate(entering, elapsed / duration);
                firstOpacity = Mathf.Lerp(startOpacity, targetOpacity, eased);
                firstIcon.style.opacity = firstOpacity;
                secondIcon.style.opacity = 1f - firstOpacity;
                elapsed += Time.deltaTime;
                yield return null;
            }

            firstOpacity = targetOpacity;
            SetImmediate(firstIcon, secondIcon, finalFirstVisible);
            routine = null;
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
