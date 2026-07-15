# Frosted Control Island

Sample defaults for the Frosted Glass Deucarian control island with rounded-square icon
buttons, animated visibility, and a compact scrubber. Runtime consumers can resolve
Frosted Glass, Fluent Acrylic, and Material Dark density metrics directly with
`DeucarianControlIslandProfiles.Resolve(style)`.

Import this sample after installing `com.deucarian.ui`, then assign the preset to your
own app-level adapter. Buttons and scrubbers share a 4 px horizontal margin. The package
owns the geometry and motion defaults; your app should
still map its own theme roles, icons, labels, and row order.
