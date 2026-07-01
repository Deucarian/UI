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
            if (routine != null && host != null)
            {
                host.StopCoroutine(routine);
                routine = null;
            }

            bool changed = firstVisible != visible;
            firstVisible = visible;
            if (!changed || !animate || host == null || !host.isActiveAndEnabled || !Application.isPlaying)
            {
                SetImmediate(firstIcon, secondIcon, visible);
                return;
            }

            VisualElement from = visible ? secondIcon : firstIcon;
            VisualElement to = visible ? firstIcon : secondIcon;
            routine = host.StartCoroutine(Animate(from, to, visible));
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

        private IEnumerator Animate(VisualElement from, VisualElement to, bool finalFirstVisible)
        {
            if (from == null || to == null)
            {
                SetImmediate(firstIcon, secondIcon, finalFirstVisible);
                routine = null;
                yield break;
            }

            from.style.display = DisplayStyle.Flex;
            to.style.display = DisplayStyle.Flex;
            float duration = Mathf.Max(0.0001f, profile.EnterSeconds);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float eased = profile.Evaluate(true, elapsed / duration);
                from.style.opacity = 1f - eased;
                to.style.opacity = eased;
                elapsed += Time.deltaTime;
                yield return null;
            }

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
