using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.UI
{
    internal static class DeucarianTooltipPlacementResolver
    {
        internal const float EdgeInset = 10f;
        private const float TargetGap = 9f;

        internal static Vector2 AvoidObstacles(Vector2 preferred, Rect target, Vector2 viewport,
            Vector2 size, IReadOnlyList<Rect> obstacles)
        {
            if (IsClear(preferred, size, obstacles)) return preferred;

            // Keep a hint alongside its originating row when another row blocks it.
            foreach (Rect row in obstacles)
            {
                if (!row.Contains(target.center)) continue;
                Vector2 left = new Vector2(row.xMin - TargetGap - size.x, target.center.y - size.y * .5f);
                Vector2 right = new Vector2(row.xMax + TargetGap, left.y);
                bool leftFits = Fits(left, size, viewport) && IsClear(left, size, obstacles);
                bool rightFits = Fits(right, size, viewport) && IsClear(right, size, obstacles);
                if (leftFits || rightFits)
                    return leftFits && (!rightFits || target.center.x - row.xMin <= row.xMax - target.center.x)
                        ? left : right;
            }

            Vector2 best = preferred;
            float bestDistance = float.PositiveInfinity;
            float x = Mathf.Clamp(target.center.x - size.x * .5f, EdgeInset,
                Mathf.Max(EdgeInset, viewport.x - size.x - EdgeInset));
            foreach (Rect obstacle in obstacles)
            {
                Consider(new Vector2(x, obstacle.yMin - TargetGap - size.y));
                Consider(new Vector2(x, obstacle.yMax + TargetGap));
            }
            return best;

            void Consider(Vector2 candidate)
            {
                if (!Fits(candidate, size, viewport) || !IsClear(candidate, size, obstacles)) return;
                float distance = (candidate + size * .5f - target.center).sqrMagnitude;
                if (distance >= bestDistance) return;
                bestDistance = distance;
                best = candidate;
            }
        }

        private static bool Fits(Vector2 position, Vector2 size, Vector2 viewport) =>
            position.x >= EdgeInset && position.y >= EdgeInset &&
            position.x + size.x <= viewport.x - EdgeInset &&
            position.y + size.y <= viewport.y - EdgeInset;

        private static bool IsClear(Vector2 position, Vector2 size, IReadOnlyList<Rect> obstacles)
        {
            Rect bounds = new Rect(position, size);
            foreach (Rect obstacle in obstacles) if (bounds.Overlaps(obstacle)) return false;
            return true;
        }

        internal static Vector2 Resolve(
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

    }
}
