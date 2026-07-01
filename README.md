# Deucarian UI

## Overview

Deucarian UI provides reusable runtime UI presentation primitives for Deucarian Unity projects.

Package ID: `com.deucarian.ui`

Current package version: `0.2.0`.

The package owns shared UI motion, glass panel application, icon swap behavior, icon button layout,
control island geometry, icon button interaction state, and lightweight scrubber chrome.

## Installation

Install through Unity Package Manager with a Git URL:

```json
{
  "dependencies": {
    "com.deucarian.ui": "https://github.com/Deucarian/UI.git#main"
  }
}
```

For development builds, use:

```json
"com.deucarian.ui": "https://github.com/Deucarian/UI.git#develop"
```

## Public API

- `DeucarianMotionProfile`: reusable enter/exit/crossfade motion profile values.
- `DeucarianAnimatedVisibility`: cancellable UI Toolkit visibility animation helper.
- `DeucarianIconSwap`: single-slot two-icon swap helper for UI Toolkit buttons.
- `DeucarianUIToolkitGlassPanel`: applies Deucarian frosted glass style to UI Toolkit panels.
- `DeucarianUGUIGlassPanel`: applies Deucarian frosted glass style to uGUI images/graphics.
- `DeucarianControlIslandStyle`: reusable compact control island and icon button geometry.
- `DeucarianIconButtonStyle`: reusable icon button visual state, palette, interaction, and state application helpers.
- `DeucarianScrubberStyle`: reusable compact scrubber metrics, palette, and state application helpers.
- `DeucarianControlIslandPreset`: ScriptableObject defaults for control island geometry.

## Samples

- `Samples~/Frosted Control Island`: default frosted control island preset for rounded-square icon buttons,
  compact stacked rows, and compact scrubber sizing.

## Notes

This package does not own app-specific routing, report viewer media behavior, camera navigation, or collection-to-prefab binding.
