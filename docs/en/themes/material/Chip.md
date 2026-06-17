# Chip
`Chip` is a compact material surface for showing a short value with an optional remove action.

## Usage

`Chip` is included in the `UraniumUI.Material.Controls` namespace.

```xml
xmlns:material="http://schemas.enisn-projects.io/dotnet/maui/uraniumui/material"
```

```xml
<material:Chip Text="Mango" />
```

## Properties

- `Text`: The text shown inside the chip.
- `TextColor`: The text color.
- `IsDestroyVisible`: Shows or hides the remove button. Default is `true`.
- `SelfDestruct`: Removes the chip from its parent after the remove action. Default is `true`.
- `DestroyCommand`: Runs when the remove button is tapped.

## Accessibility

Use clear `Text` because it is the main accessible label for the chip. When the remove action is visible, verify it with keyboard and screen reader in your target platforms. If the chip represents something important, consider adding an explicit remove button outside the chip or adding semantic text to the chip until the built-in remove affordance exposes a dynamic description.

## Example

```xml
<HorizontalStackLayout Spacing="8">
    <material:Chip Text="Apple" />
    <material:Chip Text="Orange" IsDestroyVisible="False" />
</HorizontalStackLayout>
```
