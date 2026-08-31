# DatePickerField
DatePickerField is a control that allows users to select a date. By default it embeds the platform date picker, matching `TimePickerField`'s dialog; setting `UseNativePicker` to `false` opens the UraniumUI date prompt backed by the custom `CalendarView` instead. Nullable dates, clear, and same-date reselection behave consistently in both modes.

- [Material Design Date Pickers](https://material.io/components/date-pickers)

## Usage

DatePickerField is included in the `UraniumUI.Material.Controls` namespace. You should add it to your XAML like this:

```xml
xmlns:material="http://schemas.enisn-projects.io/dotnet/maui/uraniumui/material"
xmlns:m="clr-namespace:UraniumUI.Icons.MaterialSymbols;assembly=UraniumUI.Icons.MaterialSymbols"
```

Then you can use it like this:

```xml
<material:DatePickerField Title="Pick a Date" />
```

| Light | Dark |
| --- | --- |
| ![MAUI Material Design DatePicker](../../../../images/datepickerfield-demo-light-android.gif) | ![MAUI Material Design DatePicker](../../../../images/datepickerfield-demo-dark-ios.gif) |


## Icon
DatePickerFields support setting an icon on the left side of the control. You can set the icon by setting the `Icon` property. The icon can be any `ImageSource` object. FontImageSource is recommended as Icon since its color can be changed when focused.

```xml
<material:DatePickerField Title="Pick a Date" Icon="{FontImageSource FontFamily=MaterialOutlined, Glyph={x:Static m:MaterialOutlined.Calendar_month}}"  />
```

| Light | Dark |
| --- | --- |
| ![MAUI Material Input](../../../../images/datepickerfield-icon-light-android.gif) | ![MAUI Material Input](../../../../images/datepickerfield-icon-dark-ios.gif) |

## AllowClear
DatePickerFields support clearing the selected date by setting the `AllowClear` property to `true`. Default value is `true`. You can make it `false` to disable clearing.

Clearing the field sets `Date` to `null`. `Date`, `MinimumDate`, and `MaximumDate` support nullable `DateTime` values, so they can be bound to `DateTime?` view-model properties.

`MinimumDate` and `MaximumDate` are passed to the calendar prompt and disable dates outside the allowed range. Cancelling the prompt leaves `Date` unchanged, while selecting Clear returns `null`.

```xml
<material:DatePickerField 
    Title="Pick a Date (Clearable)"
    AllowClear="True" />

<material:DatePickerField 
    Title="Pick a Date (Unclearable)"
    AllowClear="False" />
```

| Dark | Light|
| --- | --- |
| ![MAUI Material Input](../../../../images/datepickerfield-allowclear-dark-android.gif) | ![MAUI Material Input](../../../../images/datepickerfield-allowclear-light-android.gif) |

## UseNativePicker
DatePickerField can either embed the platform date picker (default) or open the UraniumUI calendar prompt. Set `UseNativePicker` to `false` to use the calendar prompt backed by `CalendarView`.

```xml
<material:DatePickerField Title="Pick a Date" UseNativePicker="False" />
```

The platform picker gives `DatePickerField` and `TimePickerField` the same look and feel on each platform. The calendar prompt renders the same UraniumUI calendar on every platform instead. `MinimumDate` and `MaximumDate` are honored in both modes.

## Accessibility

Use `Title` as the visible field label and set `MinimumDate`/`MaximumDate` when the valid range is constrained. The date prompt uses `CalendarView`, which renders selectable days and years as MAUI buttons and provides semantic descriptions for previous/next navigation.

When the date is required or constrained, include validation text that explains the accepted range. If the date picker appears inside a custom dialog or bottom sheet, verify initial focus, dismiss behavior, and focus return in the target platform.

## Validation
DatePickerField supports validation rules such as `MinValueValidation` and `MaxValueValidation`. You can use them like this:

```xml
<material:DatePickerField Title="Pick a date" Icon="{FontImageSource FontFamily=MaterialOutlined, Glyph={x:Static m:MaterialOutlined.Alarm}}">
    <validation:MinValueValidation MinValue="9/18/2022" />
    <validation:MaxValueValidation MaxValue="12/31/2022" />
</material:DatePickerField>
```

| Light | Dark |
| --- | --- |
| ![MAUI Material Input](../../../../images/datepickerfield-validation-light-android.gif) | ![MAUI Material Input](../../../../images/datepickerfield-validation-dark-ios.gif) |


### FormView Compatibility
DatePickerField is fully compatible with [FormView](https://enisn-projects.io/docs/en/inputkit/latest/components/controls/FormView). You can use it inside a FormView and it will work as expected.

```xml
 <input:FormView Spacing="20">
    <material:DatePickerField Title="Pick a date" Icon="{FontImageSource FontFamily=MaterialOutlined, Glyph={x:Static m:MaterialOutlined.Calendar_month}}">
        <validation:MinValueValidation MinValue="9/18/2022"  />
        <validation:MaxValueValidation MaxValue="12/31/2022" />
    </material:DatePickerField>

    <Button StyleClass="TextButton"
            Text="Submit"
            input:FormView.IsSubmitButton="True"/>

</input:FormView>
```
