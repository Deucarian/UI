using System;
using System.Collections.Generic;
using Deucarian.Common;
using Deucarian.Theming;
using Deucarian.Theming.UIToolkit;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.UI
{
    /// <summary>
    /// Runtime tooltip surface for player builds, pointer input, and keyboard
    /// focus. The tooltip blocks world input while remaining non-interactive.
    /// </summary>
    public sealed class DeucarianRuntimeTooltipPresenter : IDisposable
    {
        public const string BubbleName = "DeucarianRuntimeTooltip";
        public const string LabelName = "DeucarianRuntimeTooltipLabel";

        private const long PointerDelayMilliseconds = 420L;
        private const long FocusDelayMilliseconds = 180L;
        private const float EdgeInset = 10f;
        private const float TargetGap = 9f;
        private const float PointerOffsetX = 14f;
        private const float PointerOffsetY = 18f;
        private const float MinimumWidth = 180f;
        private const float MinimumHeight = 34f;
        private const long PositionTrackingIntervalMilliseconds = 16L;

        private readonly Component themeContext;
        private readonly VisualElement sourceRoot;
        private readonly VisualElement tooltipRoot;
        private readonly RuntimeTooltipLayer ownedLayer;
        private readonly VisualElement bubble;
        private readonly Label label;
        private readonly List<VisualElement> targets =
            new List<VisualElement>();
        private IVisualElementScheduledItem pendingShow;
        private IVisualElementScheduledItem pendingPointerActivationClear;
        private IVisualElementScheduledItem positionTracking;
        private VisualElement pendingTarget;
        private VisualElement pointerActivatedTarget;
        private Vector2 anchor;
        private bool anchorFromFocus;
        private bool visible;
        private bool disposed;

        public DeucarianRuntimeTooltipPresenter(
            Component context,
            VisualElement tooltipRoot)
            : this(
                context,
                tooltipRoot,
                tooltipRoot,
                null)
        {
        }

        /// <summary>
        /// Creates a dedicated package-owned tooltip document above all normal
        /// product UI. Use this overload for runtime UI made from one or more
        /// UIDocuments.
        /// </summary>
        public static DeucarianRuntimeTooltipPresenter CreateForDocument(
            Component context,
            UIDocument sourceDocument)
        {
            return new DeucarianRuntimeTooltipPresenter(
                context,
                sourceDocument != null
                    ? sourceDocument.rootVisualElement
                    : null,
                RuntimeTooltipLayer.Acquire(sourceDocument));
        }

        private DeucarianRuntimeTooltipPresenter(
            Component context,
            VisualElement root,
            RuntimeTooltipLayer layer)
            : this(
                context,
                root,
                layer != null ? layer.Root : root,
                layer)
        {
        }

        private DeucarianRuntimeTooltipPresenter(
            Component context,
            VisualElement eventRoot,
            VisualElement renderRoot,
            RuntimeTooltipLayer layer)
        {
            themeContext = context;
            sourceRoot = eventRoot;
            tooltipRoot = renderRoot;
            ownedLayer = layer;
            if (sourceRoot == null || tooltipRoot == null)
            {
                return;
            }

            bubble = new VisualElement
            {
                name = BubbleName,
                pickingMode = PickingMode.Position
            };
            bubble.style.display = DisplayStyle.None;
            bubble.style.position = Position.Absolute;
            bubble.style.maxWidth = 320f;
            bubble.style.minWidth = MinimumWidth;
            bubble.style.minHeight = MinimumHeight;
            bubble.style.paddingLeft = 11f;
            bubble.style.paddingRight = 11f;
            bubble.style.paddingTop = 8f;
            bubble.style.paddingBottom = 8f;
            bubble.style.opacity = 0f;

            label = new Label(string.Empty)
            {
                name = LabelName,
                pickingMode = PickingMode.Ignore
            };
            label.style.fontSize = 12f;
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            bubble.Add(label);
            bubble.RegisterCallback<GeometryChangedEvent>(
                OnBubbleGeometryChanged);
            tooltipRoot.Add(bubble);
            sourceRoot.RegisterCallback<TooltipEvent>(
                OnTooltipRequested,
                TrickleDown.TrickleDown);
            sourceRoot.RegisterCallback<GeometryChangedEvent>(
                OnLayoutGeometryChanged);
            if (tooltipRoot != sourceRoot)
            {
                tooltipRoot.RegisterCallback<GeometryChangedEvent>(
                    OnLayoutGeometryChanged);
            }

            ApplyTheme(
                DeucarianGlassPanelStyle.ResolveTheme(null, context));
        }

        public VisualElement Bubble => bubble;
        public Label Label => label;
        public bool IsVisible => visible;
        public UIDocument OverlayDocument => ownedLayer?.Document;
        public bool IsBound(VisualElement target) =>
            target != null && targets.Contains(target);

        public void Bind(VisualElement target)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(
                    nameof(DeucarianRuntimeTooltipPresenter));
            }

            if (target == null || targets.Contains(target))
            {
                return;
            }

            targets.Add(target);
            target.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
            target.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
            target.RegisterCallback<FocusInEvent>(OnFocusIn);
            target.RegisterCallback<FocusOutEvent>(OnFocusOut);
            target.RegisterCallback<GeometryChangedEvent>(
                OnTargetGeometryChanged);
            target.RegisterCallback<DetachFromPanelEvent>(
                OnTargetDetached);
        }

        /// <summary>
        /// Binds a complete runtime UI subtree so controls that gain or change
        /// tooltip copy later keep the same package-owned overlay behavior.
        /// </summary>
        public void BindTree(VisualElement treeRoot)
        {
            if (treeRoot == null)
            {
                return;
            }

            Bind(treeRoot);
            treeRoot.Query<VisualElement>().ForEach(Bind);
        }

        public void ApplyTheme(
            DeucarianTheme theme,
            DeucarianThemeStyle style = null)
        {
            if (bubble == null)
            {
                return;
            }

            DeucarianUIToolkitThemeTypography.Apply(
                bubble,
                theme,
                themeContext);
            DeucarianGlassPanelStyle.ApplyPanel(
                bubble,
                theme,
                style,
                themeContext);
            if (label != null)
            {
                label.style.color =
                    DeucarianControlIslandTheme.ResolveTextColor(
                        theme,
                        themeContext);
            }
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            CancelPendingShow();
            CancelPendingPointerActivationClear();
            Hide();
            for (int i = 0; i < targets.Count; i++)
            {
                VisualElement target = targets[i];
                if (target == null)
                {
                    continue;
                }

                target.UnregisterCallback<PointerEnterEvent>(OnPointerEnter);
                target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
                target.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
                target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
                target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
                target.UnregisterCallback<PointerCancelEvent>(OnPointerCancel);
                target.UnregisterCallback<FocusInEvent>(OnFocusIn);
                target.UnregisterCallback<FocusOutEvent>(OnFocusOut);
                target.UnregisterCallback<GeometryChangedEvent>(
                    OnTargetGeometryChanged);
                target.UnregisterCallback<DetachFromPanelEvent>(
                    OnTargetDetached);
            }

            targets.Clear();
            sourceRoot?.UnregisterCallback<TooltipEvent>(
                OnTooltipRequested,
                TrickleDown.TrickleDown);
            sourceRoot?.UnregisterCallback<GeometryChangedEvent>(
                OnLayoutGeometryChanged);
            if (tooltipRoot != null && tooltipRoot != sourceRoot)
            {
                tooltipRoot.UnregisterCallback<GeometryChangedEvent>(
                    OnLayoutGeometryChanged);
            }
            bubble?.UnregisterCallback<GeometryChangedEvent>(
                OnBubbleGeometryChanged);
            bubble?.RemoveFromHierarchy();
            ownedLayer?.Release();
        }

        private void OnPointerEnter(PointerEnterEvent evt)
        {
            VisualElement target = evt.currentTarget as VisualElement;
            if (!HasTooltip(target))
            {
                return;
            }

            anchor = TooltipPosition(evt.position) +
                     new Vector2(PointerOffsetX, PointerOffsetY);
            QueueShow(target, PointerDelayMilliseconds, false);
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (evt.currentTarget != pendingTarget || anchorFromFocus)
            {
                return;
            }

            anchor = TooltipPosition(evt.position) +
                     new Vector2(PointerOffsetX, PointerOffsetY);
            if (visible)
            {
                PositionBubble();
            }
        }

        private void OnPointerLeave(PointerLeaveEvent evt)
        {
            if (evt.currentTarget == pendingTarget)
            {
                Hide();
            }
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            pointerActivatedTarget = evt.currentTarget as VisualElement;
            Hide();
            CancelPendingPointerActivationClear();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (evt.currentTarget == pointerActivatedTarget &&
                sourceRoot != null)
            {
                pendingPointerActivationClear = sourceRoot.schedule
                    .Execute(ClearPointerActivation)
                    .StartingIn(0L);
            }
        }

        private void OnPointerCancel(PointerCancelEvent evt)
        {
            if (evt.currentTarget == pointerActivatedTarget)
            {
                ClearPointerActivation();
            }
        }

        private void OnFocusIn(FocusInEvent evt)
        {
            VisualElement target = evt.currentTarget as VisualElement;
            if (target != null && target == pointerActivatedTarget)
            {
                ClearPointerActivation();
                Hide();
                return;
            }

            if (!HasTooltip(target))
            {
                return;
            }

            Rect bounds = target.worldBound;
            anchor = TooltipPosition(
                         new Vector2(bounds.xMin, bounds.yMax)) +
                     new Vector2(0f, 9f);
            QueueShow(target, FocusDelayMilliseconds, true);
        }

        private void OnFocusOut(FocusOutEvent evt)
        {
            if (evt.currentTarget == pendingTarget)
            {
                Hide();
            }
        }

        private void OnTooltipRequested(TooltipEvent evt)
        {
            VisualElement target = evt.target as VisualElement;
            if (target != null && target == pointerActivatedTarget)
            {
                evt.StopPropagation();
                return;
            }

            if (!HasTooltip(target))
            {
                return;
            }

            Rect bounds = target.worldBound;
            anchor = TooltipPosition(
                         new Vector2(bounds.xMin, bounds.yMax)) +
                     new Vector2(0f, 9f);
            QueueShow(target, 0L, false);
            evt.StopPropagation();
        }

        private void QueueShow(
            VisualElement target,
            long delayMilliseconds,
            bool fromFocus)
        {
            CancelPendingShow();
            pendingTarget = target;
            anchorFromFocus = fromFocus;
            if (sourceRoot != null)
            {
                pendingShow = sourceRoot.schedule
                    .Execute(ShowPending)
                    .StartingIn(delayMilliseconds);
            }
        }

        private void ShowPending()
        {
            pendingShow = null;
            if (!HasTooltip(pendingTarget) || pendingTarget.panel == null)
            {
                Hide();
                return;
            }

            label.text = pendingTarget.tooltip;
            bubble.style.display = DisplayStyle.Flex;
            bubble.style.opacity = 1f;
            bubble.BringToFront();
            visible = true;
            PositionBubble();
            StartPositionTracking();
        }

        private void PositionBubble()
        {
            if (!visible || bubble == null || tooltipRoot == null)
            {
                return;
            }

            Vector2 viewportSize = ResolveElementSize(tooltipRoot);
            Vector2 bubbleSize = ResolveElementSize(bubble);
            bubbleSize.x = Mathf.Max(MinimumWidth, bubbleSize.x);
            bubbleSize.y = Mathf.Max(MinimumHeight, bubbleSize.y);
            Vector2 position = ResolvePlacement(
                ResolveTargetBounds(),
                anchor,
                viewportSize,
                bubbleSize);

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

        private void Hide()
        {
            CancelPendingShow();
            StopPositionTracking();
            pendingTarget = null;
            visible = false;
            if (bubble != null)
            {
                bubble.style.display = DisplayStyle.None;
                bubble.style.opacity = 0f;
            }
        }

        private void CancelPendingShow()
        {
            if (pendingShow == null)
            {
                return;
            }

            pendingShow.Pause();
            pendingShow = null;
        }

        private void ClearPointerActivation()
        {
            pendingPointerActivationClear = null;
            pointerActivatedTarget = null;
        }

        private void CancelPendingPointerActivationClear()
        {
            if (pendingPointerActivationClear == null)
            {
                return;
            }

            pendingPointerActivationClear.Pause();
            pendingPointerActivationClear = null;
        }

        private void StartPositionTracking()
        {
            if (tooltipRoot == null)
            {
                return;
            }

            if (positionTracking == null)
            {
                positionTracking = tooltipRoot.schedule
                    .Execute(PositionBubble)
                    .Every(PositionTrackingIntervalMilliseconds);
                return;
            }

            positionTracking.Resume();
        }

        private void StopPositionTracking()
        {
            positionTracking?.Pause();
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

        private sealed class RuntimeTooltipLayer
        {
            private const string ObjectName =
                "DeucarianRuntimeTooltipLayer";
            private static readonly List<RuntimeTooltipLayer> Layers =
                new List<RuntimeTooltipLayer>();

            private readonly GameObject layerObject;
            private readonly PanelSettings sourcePanelSettings;
            private readonly PanelSettings overlayPanelSettings;
            private int leaseCount;

            private RuntimeTooltipLayer(PanelSettings settings)
            {
                sourcePanelSettings = settings;
                overlayPanelSettings = UnityEngine.Object.Instantiate(
                    settings);
                overlayPanelSettings.name =
                    "DeucarianRuntimeTooltipPanelSettings";
                // This is a compositing layer. Clearing either buffer from the
                // topmost panel could erase the world or UI rendered below it.
                overlayPanelSettings.clearColor = false;
                overlayPanelSettings.clearDepthStencil = false;
                overlayPanelSettings.sortingOrder =
                    DeucarianUIDepth.Tooltip;
                layerObject = new GameObject(ObjectName)
                {
                    hideFlags = HideFlags.DontSave
                };
                Document = layerObject.AddComponent<UIDocument>();
                Document.panelSettings = overlayPanelSettings;
                Document.sortingOrder = DeucarianUIDepth.Tooltip;
                Root = Document.rootVisualElement;
                Root.Clear();
                Root.pickingMode = PickingMode.Ignore;
                Root.style.position = Position.Absolute;
                Root.style.left = 0f;
                Root.style.right = 0f;
                Root.style.top = 0f;
                Root.style.bottom = 0f;
            }

            public UIDocument Document { get; }
            public VisualElement Root { get; }

            public static RuntimeTooltipLayer Acquire(
                UIDocument sourceDocument)
            {
                if (sourceDocument == null)
                {
                    return null;
                }

                PanelSettings settings = sourceDocument.panelSettings;
                if (settings == null)
                {
                    settings = DeucarianUIRuntimeAssets
                        .LoadRuntimePanelSettings();
                    if (settings != null)
                    {
                        sourceDocument.panelSettings = settings;
                    }
                }

                if (settings == null)
                {
                    return null;
                }

                for (int i = Layers.Count - 1; i >= 0; i--)
                {
                    RuntimeTooltipLayer candidate = Layers[i];
                    if (candidate == null ||
                        candidate.layerObject == null)
                    {
                        Layers.RemoveAt(i);
                        continue;
                    }

                    if (candidate.sourcePanelSettings == settings)
                    {
                        candidate.leaseCount++;
                        return candidate;
                    }
                }

                var layer = new RuntimeTooltipLayer(settings)
                {
                    leaseCount = 1
                };
                Layers.Add(layer);
                return layer;
            }

            public void Release()
            {
                leaseCount = Mathf.Max(0, leaseCount - 1);
                if (leaseCount > 0)
                {
                    return;
                }

                Layers.Remove(this);
                if (Document != null &&
                    Document.panelSettings == overlayPanelSettings)
                {
                    Document.panelSettings = null;
                }

                UnityObjectUtility.DestroySafely(overlayPanelSettings);
                UnityObjectUtility.DestroySafely(layerObject);
            }
        }

        private static bool HasTooltip(VisualElement target) =>
            target != null &&
            !string.IsNullOrWhiteSpace(target.tooltip);
    }
}
