using System;
using System.Collections.Generic;
using Deucarian.Theming;
using Deucarian.Theming.UIToolkit;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.UI
{
    /// <summary>
    /// Runtime tooltip surface for player builds, pointer input, and keyboard
    /// focus. Tooltips leave pointer interaction with the underlying UI intact.
    /// </summary>
    public sealed partial class DeucarianRuntimeTooltipPresenter : IDisposable
    {
        public const string BubbleName = "DeucarianRuntimeTooltip";
        public const string LabelName = "DeucarianRuntimeTooltipLabel";

        private const long PointerDelayMilliseconds = 420L;
        private const long FocusDelayMilliseconds = 180L;
        private const float EdgeInset = 10f;
        private const float TargetGap = 9f;
        private const float PointerOffsetX = 14f;
        private const float PointerOffsetY = 18f;
        private const float MinimumWidth = 24f;
        private const float MinimumHeight = 24f;
        private const long PositionTrackingIntervalMilliseconds = 16L;

        private readonly Component themeContext;
        private readonly VisualElement sourceRoot;
        private readonly VisualElement tooltipRoot;
        private readonly DeucarianUIOverlayLease ownedLayer;
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
        /// product UI. The source document must already be configured through
        /// <see cref="DeucarianUIRuntime.Configure"/> so all documents share
        /// the canonical panel contract.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The source document is not configured with the canonical package
        /// PanelSettings or does not belong to a loaded scene.
        /// </exception>
        public static DeucarianRuntimeTooltipPresenter CreateForDocument(
            Component context,
            UIDocument sourceDocument)
        {
            return new DeucarianRuntimeTooltipPresenter(
                context,
                sourceDocument != null
                    ? sourceDocument.rootVisualElement
                    : null,
                sourceDocument != null
                    ? DeucarianUIOverlayHost.Acquire(
                        sourceDocument,
                        DeucarianUISurfaceRole.Tooltip,
                        "DeucarianRuntimeTooltipPresenter")
                    : null);
        }

        private DeucarianRuntimeTooltipPresenter(
            Component context,
            VisualElement root,
            DeucarianUIOverlayLease layer)
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
            DeucarianUIOverlayLease layer)
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
                pickingMode = PickingMode.Ignore
            };
            bubble.style.display = DisplayStyle.None;
            bubble.style.position = Position.Absolute;
            bubble.style.maxWidth = 240f;
            bubble.style.minWidth = MinimumWidth;
            bubble.style.minHeight = MinimumHeight;
            bubble.style.paddingLeft = 8f;
            bubble.style.paddingRight = 8f;
            bubble.style.paddingTop = 5f;
            bubble.style.paddingBottom = 5f;
            bubble.style.opacity = 0f;

            label = new Label(string.Empty)
            {
                name = LabelName,
                pickingMode = PickingMode.Ignore
            };
            label.style.fontSize = 11f;
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
            ownedLayer?.Dispose();
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
            if (pendingTarget != target) Hide();
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
            CollectControlIslands();
            bubble.style.display = DisplayStyle.Flex;
            bubble.style.opacity = 1f;
            bubble.BringToFront();
            visible = true;
            PositionBubble();
            StartPositionTracking();
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

    }
}
