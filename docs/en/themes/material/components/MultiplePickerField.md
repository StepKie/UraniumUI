# MultiplePickerField
MultiplePickerField is a component that allows you to select multiple values from a list of options.

| Light | Dark |
| --- | --- |
| ![MAUI Multiple selection](../../../../images/multiplepickerfield-demo-light.gif) | ![MAUI Multiple selection](../../../../images/multiplepickerfield-demo-dark.gif) |


## Usage
MultiplePickerField is included in the `UraniumUI.Material.Controls` namespace. You should add it to your XAML like this:

```xml
xmlns:material="http://schemas.enisn-projects.io/dotnet/maui/uraniumui/material"
xmlns:m="clr-namespace:UraniumUI.Icons.MaterialSymbols;assembly=UraniumUI.Icons.MaterialSymbols"
```

Then you can use it like this:

```xml
<material:MultiplePickerField Title="Pick some options">
    <material:MultiplePickerField.ItemsSource>
        <x:Array Type="{x:Type x:String}">
            <x:String>Option 1</x:String>
            <x:String>Option 2</x:String>
            <x:String>Option 3</x:String>
            <x:String>Option 4</x:String>
            <x:String>Option 5</x:String>
            <x:String>Option 6</x:String>
        </x:Array>
    </material:MultiplePickerField.ItemsSource>
</material:MultiplePickerField>
```

## Item Display Binding
Use `ItemDisplayBinding` when `ItemsSource` contains objects and the field should show a specific property instead of `ToString()`. The binding is used for both selected chips and picker dialog items.

```xml
<material:MultiplePickerField
    Title="Pick tags"
    ItemsSource="{Binding Tags}"
    SelectedItems="{Binding SelectedTags}"
    ItemDisplayBinding="{Binding Name}" />
```

## Selected Indexes
Use `SelectedIndexes` to bind selected item positions in `ItemsSource`. The property stays synchronized with `SelectedItems`; changing either collection updates the other.

```xml
<material:MultiplePickerField
    Title="Pick some options"
    ItemsSource="{Binding Options}"
    SelectedIndexes="{Binding SelectedOptionIndexes}" />
```

## Icon
TextFields support setting an icon on the left side of the control. You can set the icon by setting the `Icon` property. The icon can be any `ImageSource` object. FontImageSource is recommended as Icon since its color can be changed when focused.

```xml
 <material:MultiplePickerField
    Title="Pick some options"
    Icon="{FontImageSource FontFamily=MaterialOutlined, Glyph={x:Static m:MaterialOutlined.Mail}}"/>
```

## AccentColor
The color that is used to fill border and icon of control when it's focused. You can change it by setting `AccentColor` property of the control.

```xml
 <material:MultiplePickerField
    Title="Pick some options"
    AccentColor="DeepSkyBlue"/>
```
## Hide Chip Remove Icon
You can hide the remove icon shown on selected chips by setting `IsChipRemoveVisible` to `False`. This keeps the field editable through the picker dialog while removing the inline chip delete action.

```xml
<material:MultiplePickerField
    Title="Pick some options"
    IsChipRemoveVisible="False" />
```

## Selection Colors
Selected items are shown as chips. You can change their colors directly on `MultiplePickerField`.

```xml
<material:MultiplePickerField
    Title="Pick some options"
    ItemsSource="{Binding Items}"
    ChipBackgroundColor="DeepSkyBlue"
    ChipTextColor="White"
    ChipDestroyIconColor="White" />
```

## Dialog CheckBox Colors
`MultiplePickerField` uses a CheckBox prompt internally. You can also change the colors of those internal checkboxes directly on the field.

```xml
<material:MultiplePickerField
    Title="Pick some options"
    ItemsSource="{Binding Items}"
    CheckBoxColor="DeepSkyBlue"
    CheckBoxBorderColor="Gray"
    CheckBoxTextColor="Black"
    CheckBoxIconColor="White" />
```

If you want the same checkbox colors across the whole app, define a style for `material:CheckBox` in your app resources.

## Accessibility

Use `Title` as the visible label and keep selected chip text descriptive. `MultiplePickerField` opens a checkbox prompt, so the same dialog accessibility rules apply: clear title, keyboard-reachable actions, visible validation text, and explicit accept/cancel buttons.

If selected chips can be removed inline, verify the chip remove action with keyboard and screen reader in your target platforms. When removal must be guaranteed accessible, also provide a clearly labeled remove action in the picker dialog or surrounding form.
