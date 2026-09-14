using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.UI
{
    public sealed partial class DeucarianRuntimeTooltipPresenter
    {
        private readonly List<VisualElement> controlIslands = new List<VisualElement>();

        private void CollectControlIslands()
        {
            controlIslands.Clear();
            // All viewer documents share this package-owned panel; avoid their visible controls.
            (sourceRoot.panel?.visualTree ?? sourceRoot)
                .Query<VisualElement>(className: DeucarianControlIslandElementStyle.ToolbarClass)
                .ToList(controlIslands);
        }

        private Vector2 AvoidControlIslands(Vector2 position, Vector2 viewport, Vector2 size)
        {
            Rect occupied = default;
            bool found = false;
            Rect proposed = new Rect(position, size);
            foreach (var island in controlIslands)
            {
                if (island.panel == null || !island.visible ||
                    island.resolvedStyle.display == DisplayStyle.None) continue;
                Rect bounds = island.worldBound;
                Vector2 min = TooltipPosition(bounds.min);
                Vector2 max = TooltipPosition(bounds.max);
                Rect local = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
                // Only the same horizontal control stack can obstruct this tooltip.
                if (local.xMax < proposed.xMin || local.xMin > proposed.xMax) continue;
                occupied = found
                    ? Rect.MinMaxRect(Mathf.Min(occupied.xMin, local.xMin),
                        Mathf.Min(occupied.yMin, local.yMin),
                        Mathf.Max(occupied.xMax, local.xMax), Mathf.Max(occupied.yMax, local.yMax))
                    : local;
                found = true;
            }
            return found && proposed.Overlaps(occupied)
                ? ResolvePlacement(occupied, anchor, viewport, size)
                : position;
        }

        private void PositionBubble()
        {
            if (!visible || bubble == null || tooltipRoot == null)
            {
                return;
            }

            Vector2 viewportSize = ResolveElementSize(tooltipRoot);
            float availableWidth = Mathf.Max(MinimumWidth, viewportSize.x - 2f * EdgeInset);
            bubble.style.maxWidth = Mathf.Min(240f, availableWidth);
            Vector2 bubbleSize = ResolveElementSize(bubble);
            bubbleSize.x = Mathf.Max(MinimumWidth, bubbleSize.x);
            bubbleSize.y = Mathf.Max(MinimumHeight, bubbleSize.y);
            Vector2 position = ResolvePlacement(
                ResolveTargetBounds(),
                anchor,
                viewportSize,
                bubbleSize);

            position = AvoidControlIslands(position, viewportSize, bubbleSize);
            bubble.style.left = position.x;
            bubble.style.top = position.y;
        }

        private void OnBubbleGeometryChanged(GeometryChangedEvent evt)
        {
            if (visible)
            {
                PositionBubble();
            }
        }

        private void OnLayoutGeometryChanged(GeometryChangedEvent evt)
        {
            if (visible)
            {
                PositionBubble();
            }
        }

        private void OnTargetGeometryChanged(GeometryChangedEvent evt)
        {
            if (visible && evt.currentTarget == pendingTarget)
            {
                PositionBubble();
            }
        }

        private void OnTargetDetached(DetachFromPanelEvent evt)
        {
            if (evt.currentTarget == pendingTarget)
            {
                Hide();
            }
        }

        /// <summary>
        /// Resolves a stable, viewport-clamped tooltip position. Targets in the
        /// lower half prefer an above placement; targets in the upper half
        /// prefer below. This keeps the tooltip away from the control it
        /// describes and from adjacent bottom control islands.
        /// </summary>
        public static Vector2 ResolvePlacement(
            Rect targetBounds,
            Vector2 fallbackAnchor,
            Vector2 viewportSize,
            Vector2 tooltipSize)
        {
            float viewportWidth = Mathf.Max(0f, viewportSize.x);
            float viewportHeight = Mathf.Max(0f, viewportSize.y);
            float tooltipWidth = Mathf.Max(0f, tooltipSize.x);
            float tooltipHeight = Mathf.Max(0f, tooltipSize.y);
            bool hasTarget = targetBounds.width > 0f &&
                             targetBounds.height > 0f;

            float left = hasTarget
                ? targetBounds.center.x - tooltipWidth * 0.5f
                : fallbackAnchor.x;
            float top = fallbackAnchor.y;
            if (hasTarget)
            {
                float aboveTop = targetBounds.yMin -
                                 TargetGap -
                                 tooltipHeight;
                float belowTop = targetBounds.yMax + TargetGap;
                float aboveSpace = targetBounds.yMin - EdgeInset;
                float belowSpace = viewportHeight -
                                   EdgeInset -
                                   targetBounds.yMax;
                bool fitsAbove = aboveTop >= EdgeInset;
                bool fitsBelow = belowTop + tooltipHeight <=
                                 viewportHeight - EdgeInset;
                bool preferAbove = targetBounds.center.y >=
                                   viewportHeight * 0.5f;

                if (preferAbove)
                {
                    top = fitsAbove || !fitsBelow
                        ? aboveTop
                        : belowTop;
                }
                else
                {
                    top = fitsBelow || !fitsAbove
                        ? belowTop
                        : aboveTop;
                }

                if (!fitsAbove && !fitsBelow)
                {
                    top = aboveSpace >= belowSpace
                        ? aboveTop
                        : belowTop;
                }
            }
            else if (viewportHeight > 0f &&
                     top + tooltipHeight + EdgeInset > viewportHeight)
            {
                top = fallbackAnchor.y - tooltipHeight - TargetGap;
            }

            if (viewportWidth > 0f)
            {
                left = Mathf.Clamp(
                    left,
                    EdgeInset,
                    Mathf.Max(
                        EdgeInset,
                        viewportWidth - tooltipWidth - EdgeInset));
            }

            if (viewportHeight > 0f)
            {
                top = Mathf.Clamp(
                    top,
                    EdgeInset,
                    Mathf.Max(
                        EdgeInset,
                        viewportHeight - tooltipHeight - EdgeInset));
            }

            return new Vector2(left, top);
        }

        private Rect ResolveTargetBounds()
        {
            if (pendingTarget == null)
            {
                return default;
            }

            Rect worldBounds = pendingTarget.worldBound;
            Vector2 minimum = TooltipPosition(
                new Vector2(worldBounds.xMin, worldBounds.yMin));
            Vector2 maximum = TooltipPosition(
                new Vector2(worldBounds.xMax, worldBounds.yMax));
            return Rect.MinMaxRect(
                Mathf.Min(minimum.x, maximum.x),
                Mathf.Min(minimum.y, maximum.y),
                Mathf.Max(minimum.x, maximum.x),
                Mathf.Max(minimum.y, maximum.y));
        }

        private Vector2 TooltipPosition(Vector2 panelPosition) =>
            tooltipRoot != null
                ? tooltipRoot.WorldToLocal(panelPosition)
                : panelPosition;

        private static Vector2 ResolveElementSize(VisualElement element)
        {
            if (element == null)
            {
                return Vector2.zero;
            }

            float width = ResolveDimension(
                element.resolvedStyle.width,
                element.contentRect.width,
                element.layout.width);
            float height = ResolveDimension(
                element.resolvedStyle.height,
                element.contentRect.height,
                element.layout.height);
            return new Vector2(width, height);
        }

        private static float ResolveDimension(
            float resolved,
            float content,
            float layout)
        {
            if (IsUsableDimension(resolved))
            {
                return resolved;
            }

            if (IsUsableDimension(content))
            {
                return content;
            }

            return IsUsableDimension(layout) ? layout : 0f;
        }

        private static bool IsUsableDimension(float value) =>
            !float.IsNaN(value) &&
            !float.IsInfinity(value) &&
            value > 0f;

        private static bool HasTooltip(VisualElement target) =>
            target != null &&
            !string.IsNullOrWhiteSpace(target.tooltip);
    }
}
