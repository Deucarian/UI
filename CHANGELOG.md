# Changelog

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
