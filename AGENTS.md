# Deucarian UI Agent Notes

Package ID: `com.deucarian.ui`
Repository: `Deucarian/UI`

Follow the canonical Deucarian governance docs in [Package Registry](https://github.com/Deucarian/Package-Registry/blob/develop/ARCHITECTURE.md), especially capability ownership and dependency rules.

## Ownership

This package owns:

- Reusable runtime UI presentation primitives, UI motion helpers, glass panel application, icon swap behavior, control island geometry, icon button styling, and scrubber chrome.

Registered capabilities:
- `ui-presentation-primitives`
- `ui-motion`

This package must not own:

- Collection-to-prefab binding, UI routing/flow, generic theme governance, XR world controls, app-specific report/media behavior, camera navigation, or package installation.

## Dependencies

Allowed dependency shape:

- May depend on Common for approved runtime primitives, Theming for shared color/style concepts, and Unity UI/UI Toolkit modules for presentation primitives.

Required dependencies and why:

- `com.deucarian.common`: approved shared runtime primitive owner.
- `com.deucarian.theming`: shared theme/style concepts used by the UI presentation primitives.
- `com.unity.modules.uielements`: UI Toolkit primitives.
- `com.unity.ugui`: uGUI primitives.

Optional/version-defined dependencies:

- None.

Architecture exceptions:

- None.

## Policies

- Logging: Do not add diagnostics/logging unless package behavior actually needs it; if needed, use Deucarian Logging and update all metadata together.
- Common: Use Common-owned helpers instead of local copies when production code needs approved shared cleanup/runtime primitives.
- Editor UI: No shared editor shell ownership.
- Diagnostics: Do not become Diagnostics; expose only package-local presentation state when needed.
- Testing: Test fixture teardown may use `DestroyImmediate` directly.

## Validation

Run the shared validator before committing:

```powershell
python C:/Repositories/Package-Registry/Tools/deucarian_package_validator.py --registry-root C:/Repositories/Package-Registry --repository-root . --config deucarian-package.json
```

Also run existing repository tests when changing code or asmdefs. Documentation-only updates should still run `git diff --check`.

## Codex Guidance

- Inspect current files before changing anything.
- Work on `develop`; do not edit or merge `main` unless the task is promotion-only.
- Do not edit `Library/PackageCache`.
- Do not guess package versions or dependency versions.
- Do not add package dependencies casually; update asmdefs, `package.json`, `deucarian-package.json`, Package Registry, Package Installer fallback catalog, and Bootstrap fallback catalog together when a dependency is truly required.
- Do not create local copies of shared helpers.
- Keep commits focused and report exactly what changed and what was validated.

## Before Adding Code

- Confirm the change fits this package's ownership boundary.
- Reuse existing local patterns and helpers.
- Avoid broad refactors without audit support.
- Preserve runtime behavior unless the task explicitly asks to change it.

## Before Adding A Dependency

- Is the capability already owned by that package?
- Is it used by production code, editor code, sample code, or tests?
- Does the asmdef reference match `package.json`?
- Does `deucarian-package.json` need updating?
- Does Package Registry need updating?
- Does Package Installer fallback catalog need updating?
- Does Bootstrap fallback catalog need updating?
- Are exact versions propagated without guessing?

## Before Adding A Helper

- Is this package the capability owner?
- Is this behavior repeated in at least three production packages?
- Is there an existing owner package?
- Should this remain local?
- Has the audit been updated?

## Debug And Unity Object Lifetime

- Direct Unity Debug calls are forbidden in production code.
- This package currently does not own production Unity object cleanup.
- Test fixture teardown may use `DestroyImmediate` directly.
