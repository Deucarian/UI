# Deucarian UI

## What this is

Deucarian UI provides reusable runtime UI presentation primitives for Deucarian Unity projects. It owns shared screen-space UI layering and transient overlays alongside UI motion, glass panel application, icon swap behavior, icon button layout, control island geometry, icon button interaction state, and lightweight scrubber chrome.

Package ID: `com.deucarian.ui`

Current package version: `0.2.6`.

## When to use it

- You need shared UI Toolkit or uGUI presentation primitives without adopting an app-specific UI shell.
- You want consistent control island, icon button, scrubber, glass, and motion styling.
- You need reusable runtime helpers that can pair with Deucarian Theming while staying independent from UI Binding or UI Flow.

## When not to use it

- You need collection-to-prefab binding; use `com.deucarian.ui-binding`.
- You need screen routing, modal flow, guards, or back navigation; use `com.deucarian.ui-flow`.
- You need XR world-space pressable controls; use `com.deucarian.xr-ui`.
- You need app-specific report/media behavior, camera navigation, or toolbar command routing.

## Install

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

## Unity compatibility

Requires Unity `2022.3` or newer.

## 60-second quick start

1. Install the package through Unity Package Manager or the Deucarian Package Installer.
2. Import the `Frosted Control Island` sample.
3. Assign `DeucarianFrostedControlIslandPreset` to your app-level adapter or copy the values into your own control island setup.
4. Use the runtime style helpers from your own UI Toolkit or uGUI code.

```csharp
using Deucarian.UI;
using UnityEngine.UIElements;

public static class UiIslandSetup
{
    public static void ApplyGlass(VisualElement panel)
    {
        if (panel == null)
        {
            return;
        }

        DeucarianUIToolkitGlassPanel.AddClass(panel);
    }
}
```

## Runtime layering contract

`com.deucarian.ui` is the single authority for Deucarian screen-space UI
ordering. Consumers choose a semantic role and let the package assign both the
canonical PanelSettings and the concrete sorting depth:

```csharp
DeucarianUIRuntime.Configure(
    document,
    DeucarianUISurfaceRole.PrimaryControls);

DeucarianUIRuntime.ConfigureScreenSpaceCanvas(
    statusCanvas,
    DeucarianUISurfaceRole.Status);
```

The guaranteed order is:

`PrimaryControls < ContextControls < MediaControls < Status < Menu < Modal < Tooltip`

Use `DeucarianUIRuntime.IsConfigured(...)` for runtime diagnostics and tests,
and `DeucarianUIRuntime.HasCanonicalPanelSettings(...)` when only canonical
PanelSettings identity matters. Consumer packages must not create or ship a
second PanelSettings asset, instantiate PanelSettings at runtime, or assign
numeric `sortingOrder` values.

Transient menu, modal, and tooltip content can acquire an isolated container
from `DeucarianUIOverlayHost`. Its explicit `DeucarianUIOverlayLease` owns the
container lifetime; leases sharing a source scene and role share one
non-clearing overlay document, and the final disposal releases that document.
The overlay object lives in the source `UIDocument` scene, so unloading one
additive scene cannot invalidate overlay leases owned by another scene. The
package runtime tooltip presenter already composes this host and requires a
source document configured through `DeucarianUIRuntime.Configure`, so tooltip
consumers only bind tooltip text and targets.

This contract covers screen-space UI Toolkit and uGUI presentation. Sprite
renderer ordering, world-space XR canvases, and editor windows remain with
their respective rendering, XR UI, and Editor package owners.

Editor tests can enforce that boundary with
`DeucarianUILayeringArchitectureValidator.ValidateRuntimeRoot(...)`. It reports
the exact file, line, column, and rule for consumer-owned PanelSettings assets
or references and direct panel/depth assignments. The optional allowlist accepts
only exact Runtime-relative C# files and suppresses only non-UI
`sortingOrder` assignments; all panel and canvas ownership rules still apply.

## Samples

- `Samples~/Frosted Control Island`: default frosted control island preset for rounded-square icon buttons, compact stacked rows, and compact scrubber sizing.

## Public API map

- `DeucarianMotionProfile`: reusable enter/exit/crossfade motion profile values.
- `DeucarianVisibilityTransition`: renderer-independent, reversible visibility state with caller-driven timing, remaining duration, phase, progress, and completion notification.
- `DeucarianAnimatedVisibility`: cancellable and reversible UI Toolkit visibility animation helper that coalesces repeated targets, retains completion callbacks, and supports explicit immediate transitions.
- `DeucarianAnimatedProgress`: interruptible normalized motion for custom UI-control presentation.
- `DeucarianAnimatedIconButton`: interruptible UI Toolkit icon-button state animation that changes logical interactivity immediately.
- `DeucarianAnimatedScrubber`: interruptible UI Toolkit scrubber state animation that changes logical interactivity immediately.
- `DeucarianIconSwap`: reversible single-slot two-icon crossfade helper for UI Toolkit buttons.
- `DeucarianUIToolkitGlassPanel`: applies Deucarian frosted glass style to UI Toolkit panels.
- `DeucarianUGUIGlassPanel`: applies Deucarian frosted glass style to uGUI images/graphics.
- `DeucarianControlIslandStyle`: reusable compact control island, icon button, and scrubber geometry application.
- `DeucarianControlIslandVisualStyle`: complete package-owned control-island composition for layout, glass chrome, semantic theme states, scrubbers, and centered overlay icons.
- `DeucarianMorphingMenu` and `DeucarianMorphingMenuLayout`: complete package-owned top-right menu scaffold with canonical state glyphs, responsive glass chrome, motion, visibility/picking behavior, theme presentation, and consumer-neutral body/input hooks.
- `DeucarianControlIslandColorRoleIds`: consumer-neutral active/inactive palette roles for branded viewer control islands.
- `DeucarianUIRuntimeAssets`: canonical package-owned stylesheet and runtime PanelSettings resource access.
- `DeucarianUISurfaceRole`, `DeucarianUIDepth`, and `DeucarianUIRuntime`: semantic, package-owned screen-space ordering plus canonical UI Toolkit PanelSettings composition for UI Toolkit and uGUI consumers.
- `DeucarianUIOverlayHost` and `DeucarianUIOverlayLease`: ref-counted, non-clearing transient overlay documents with isolated consumer containers.
- `DeucarianRuntimeTooltipPresenter`: target-aware runtime tooltip presentation composed on the package-owned topmost overlay.
- `DeucarianUILayeringArchitectureValidator` (Editor): reusable package-boundary validation for canonical PanelSettings and semantic screen-space depth ownership.
- `DeucarianControlIslandProfile` and `DeucarianControlIslandProfiles`: Comfortable, Standard, and Compact geometry resolved from `DeucarianThemeDensity`. Legacy Frosted Glass, Fluent Acrylic, and Material Dark IDs remain supported when density is unspecified. All profiles share the same 4 px item margin and vertical inset rhythm, while panel radius resolves independently from the style's shape profile.
- `DeucarianIconButtonStyle`: reusable icon button visual state, palette, interaction, and state application helpers.
- `DeucarianScrubberStyle`: reusable compact scrubber metrics, palette, and state application helpers.
- `DeucarianControlIslandPreset`: ScriptableObject defaults for control island geometry.

## Integrations

Works with:

- `com.deucarian.common` for shared runtime primitives.
- `com.deucarian.theming` for Deucarian theme/style concepts.
- Unity UI Toolkit and uGUI.

Optional integrations:

- None.

Does not own:

- Collection-to-prefab binding.
- UI routing or flow.
- XR world controls.
- Camera navigation.
- App-specific report or media behavior.

## Troubleshooting

- If UI Toolkit glass styling does not show, confirm the target `VisualElement` is attached and receiving the intended classes/styles.
- If sample values do not appear, import the `Frosted Control Island` sample through Unity Package Manager or the Package Installer before looking for the preset.
- If icons or labels are wrong, keep the mapping in the consuming app; this package provides layout and state primitives, not app command content.
- If `DeucarianUIRuntime.IsConfigured` is false, configure the document or screen-space canvas through the runtime API instead of assigning PanelSettings or sorting order locally.

## Validation

Run the shared package validator:

```powershell
python C:/Repositories/Package-Registry/Tools/deucarian_package_validator.py --registry-root C:/Repositories/Package-Registry --repository-root . --config deucarian-package.json
```

Run Unity EditMode tests when changing runtime code or asmdefs.

## Architecture / Contributor Notes

See [AGENTS.md](AGENTS.md) for ownership, dependency, and validation guidance.

## License

See [LICENSE.md](LICENSE.md).
