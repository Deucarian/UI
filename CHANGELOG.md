# Changelog

## 0.2.3 - 2026-08-18

- Added consumer-neutral active/inactive control-island roles, with reference-theme fallbacks, so branded viewers preserve their authored toolbar palettes without application-specific IDs.
- Used the shared viewer reference theme as the package fallback and raised the explicit Theming dependency to `1.0.5`.

## 0.2.2 - 2026-08-18

- Added the canonical package-owned control-island facade, built-in-role theme resolution, glass presentation, centered overlay icon layout, runtime tooltip, and interactive timeline scrubber.
- Added shared morphing-menu motion plus canonical UI Toolkit stylesheet and runtime PanelSettings resources.

## 0.2.1 - 2026-07-17

- Added an importable Frosted Control Island scene and assembly, and aligned exact Deucarian dependencies.

## Unreleased

- Coalesce repeated animated-visibility target requests so active transitions retain their completion callbacks, with explicit immediate-transition support for teardown paths.
- Add a renderer-independent, reversible visibility transition state and use it to drive UI Toolkit animated visibility.
- Add interruptible animated icon-button and scrubber state controllers that apply logical enabled state immediately while tweening presentation.
- Add normalized animated progress for package-driven custom control transitions.
- Add completion callbacks and true in-flight reversal to animated visibility, plus reversible two-icon crossfades.
- Resolve control-island sizing from semantic Comfortable, Standard, and Compact density rather than surface preset identity.
- Resolve panel and concentric nested-control radii independently from the composed shape profile while preserving legacy style-ID fallbacks.
- Add backward-compatible theme-style overloads for icon-button state and scrubber chrome so composed stroke width, color, Borderless behavior, and concentric control radii propagate consistently.

## 0.2.0 - 2026-07-01

- Added reusable icon button visual state, palette, and interaction helpers.
- Added reusable compact scrubber metrics, palette, and state application helpers.
- Added a frosted control island preset ScriptableObject and package sample assets.
- Expanded control island defaults for stacked rows, status labels, and compact scrubber sizing.

## 0.1.0 - 2026-07-01

- Created the initial `com.deucarian.ui` package.
- Added UI motion profiles, animated visibility, icon swap, glass panel, control island, icon button, and scrubber primitives.
