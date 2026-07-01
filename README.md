# Deucarian UI

## Overview

Deucarian UI provides reusable runtime UI presentation primitives for Deucarian Unity projects.

Package ID: `com.deucarian.ui`

Current package version: `0.1.0`.

The package owns shared UI motion, glass panel application, icon swap behavior, icon button layout, control island geometry, and lightweight scrubber chrome.

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

## Notes

This package does not own app-specific routing, report viewer media behavior, camera navigation, or collection-to-prefab binding.
