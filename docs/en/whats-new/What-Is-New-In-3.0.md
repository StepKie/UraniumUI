# What's New in UraniumUI v3.0

UraniumUI v3.0 is the .NET 10 generation of UraniumUI. It updates the framework baseline, introduces new MAUI-rendered controls, improves form validation workflows, and fixes several high-impact issues across data, dialogs, inputs, buttons, and platform effects.

> v3.0.0 has not been published yet. This article describes the current v3 work represented by the `develop` branch.

If you are upgrading an existing app, read the [Migration Guide to v3.0](../migration-guides/Migrating-To-3.0.md) before updating packages.

## Highlights

- .NET 10 and .NET MAUI 10 support.
- New `CalendarView` control.
- New date prompt dialog API and CalendarView-backed `DatePickerField`.
- New `Select` and Material `SelectField` controls.
- New async validation support in `FormView`.
- Virtualized `TreeView` for larger data sets.
- DataGrid improvements for headers and auto-generated columns.
- Important bug fixes for Android Release builds, compiled bindings, dialogs, form validation, binding contexts, buttons, and blur effects.

## Platform and package baseline

UraniumUI v3 targets .NET 10 only. .NET 9 support remains available in UraniumUI v2.16.0.

The v3 branch uses:

- .NET MAUI `10.0.71`
- `InputKit.Maui` `4.6.0`
- `Plainer.Maui` `1.8.0`
- `CommunityToolkit.Maui` `13.0.0` for `UraniumUI.Dialogs.CommunityToolkit`

Related PRs:

- [#1049 build: update MAUI to 10.0.71 and drop net9](https://github.com/enisn/UraniumUI/pull/1049)
- [#1050 docs: expand README positioning](https://github.com/enisn/UraniumUI/pull/1050)

## CalendarView

v3 adds `uranium:CalendarView`, a cross-platform MAUI-rendered calendar control.

`CalendarView` supports nullable selection, minimum and maximum dates, culture-aware week layout, month navigation, year selection, and style classes for calendar parts.

```xml
<uranium:CalendarView
    SelectedDate="{Binding SelectedDate}"
    MinimumDate="{Binding MinimumDate}"
    MaximumDate="{Binding MaximumDate}" />
```

Learn more in the [CalendarView documentation](../infrastructure/CalendarView.md).

Related PRs:

- [#1013 Introduce custom CalendarView](https://github.com/enisn/UraniumUI/pull/1013)
- [#1019 Polish CalendarView transitions](https://github.com/enisn/UraniumUI/pull/1019)

## Date prompts and DatePickerField

Dialogs now include a date prompt API through `IDialogService.DisplayDatePromptAsync(...)`. The built-in default, Mopups, and CommunityToolkit dialog providers support it.

```csharp
var selectedDate = await dialogService.DisplayDatePromptAsync(
    title: "Select a date",
    selectedDate: DateTime.Today);
```

`material:DatePickerField` now uses this date prompt and `CalendarView` instead of relying on the native picker interaction. It also supports empty values by making `Date`, `MinimumDate`, and `MaximumDate` nullable.

```csharp
public DateTime? BirthDate { get; set; }
```

Learn more in the [dialogs documentation](../dialogs/Index.md) and [DatePickerField documentation](../themes/material/components/DatePickerField.md).

Related PRs:

- [#1012 Support nullable DatePickerField dates](https://github.com/enisn/UraniumUI/pull/1012)
- [#1015 Add date prompt dialog support](https://github.com/enisn/UraniumUI/pull/1015)
- [#1017 Migrate DatePickerField to date prompt with CalendarView](https://github.com/enisn/UraniumUI/pull/1017)

## Select and SelectField

v3 introduces a MAUI-rendered `uranium:Select` control and a Material `material:SelectField` wrapper.

The new select controls provide overlay-based single selection, templated items, templated selected values, keyboard interaction, and a Material input-field experience without depending on native picker behavior.

```xml
<material:SelectField
    Title="Country"
    ItemsSource="{Binding Countries}"
    SelectedItem="{Binding SelectedCountry}" />
```

Learn more in the [Select documentation](../infrastructure/Select.md) and [SelectField documentation](../themes/material/components/SelectField.md).

Related PR:

- [#1039 Introduce MAUI Select with custom template support](https://github.com/enisn/UraniumUI/pull/1039)

## Async FormView validation

v3 adds `UraniumUI.Controls.FormView`, which extends the InputKit form workflow with async validation support.

New validation APIs include:

- `SubmitAsync(...)`
- `ValidateFormAsync(...)`
- `IFormValidator`
- `FormValidationHandler`
- `ValidationModel`
- `ShowValidationSummary`
- `IsBusy` and `IsValidating`
- attached `FormView.ValidationPath`
- attached `FormView.IsBusyIndicator`

This makes server-side validation and other async validation workflows easier to integrate with generated forms and dialogs.

```csharp
if (await formView.SubmitAsync())
{
    // Continue after successful validation.
}
```

`AutoFormView` participates in the new validation flow automatically by assigning validation paths to generated editors.

Learn more in the [Material validation documentation](../themes/material/Validations.md).

Related PRs:

- [#1034 Add async form validation support](https://github.com/enisn/UraniumUI/pull/1034)
- [#1043 fix: validate FormView bindable layout children](https://github.com/enisn/UraniumUI/pull/1043)

## TreeView virtualization

`material:TreeView` was reworked to use a flat, virtualized `CollectionView` internally. This improves large tree performance and avoids the cost of recursively creating a deep visual tree.

The TreeView also now exposes selection change events and commands for single and multiple selection scenarios.

```xml
<material:TreeView
    ItemsSource="{Binding Nodes}"
    SelectedItem="{Binding SelectedNode}"
    SelectedItemChangedCommand="{Binding SelectedNodeChangedCommand}" />
```

Learn more in the [TreeView documentation](../themes/material/components/TreeView.md).

Related PRs:

- [#1024 Rework TreeView with flat virtualization](https://github.com/enisn/UraniumUI/pull/1024)
- [#1044 feat: add TreeView selection change events](https://github.com/enisn/UraniumUI/pull/1044)

## DataGrid improvements

DataGrid receives two useful improvements in v3.

You can hide column headers with `ShowHeaders`:

```xml
<material:DataGrid
    ItemsSource="{Binding Items}"
    ShowHeaders="False" />
```

You can also exclude properties from auto-generated columns with `[DataGridIgnore]`:

```csharp
public class Person
{
    public string Name { get; set; }

    [DataGridIgnore]
    public string InternalNote { get; set; }
}
```

Learn more in the [DataGrid documentation](../themes/material/components/DataGrid.md).

Related PRs:

- [#1009 Add ShowHeaders option to DataGrid](https://github.com/enisn/UraniumUI/pull/1009)
- [#1048 feat: add DataGridIgnore for auto columns](https://github.com/enisn/UraniumUI/pull/1048)

## Input and dialog refinements

v3 also improves smaller but important input and dialog experiences:

- `MultiplePickerField` adds chip and checkbox styling options.
- `TextField` adds an `IsSpellCheckEnabled` toggle.
- `InputField` supports formatted floating titles through `TitleFormattedText`.
- Custom view dialogs can now be cancellable with a `Task<bool>` result.
- Input clear icons better follow theme changes and initial values.

Related PRs:

- [#1025 Add cancellable custom view dialogs](https://github.com/enisn/UraniumUI/pull/1025)
- [#1036 Dialog checkboxes, MultiplePickerField chip styling](https://github.com/enisn/UraniumUI/pull/1036)
- [#1037 Fix InputField formatted title support](https://github.com/enisn/UraniumUI/pull/1037)
- [#1038 Add spell check toggle to TextField](https://github.com/enisn/UraniumUI/pull/1038)
- [#1040 Fix InputField clear icon theme updates](https://github.com/enisn/UraniumUI/pull/1040)
- [#1045 fix: show TextField clear button for initialized text](https://github.com/enisn/UraniumUI/pull/1045)

## Important fixes

v3 includes several fixes that should be noticeable in production apps.

### DataGrid Android Release crash

A DataGrid crash that appeared in Android Release builds was fixed.

Related PR:

- [#1042 Fix DataGrid Android release crash](https://github.com/enisn/UraniumUI/pull/1042)

### DataAnnotations and compiled bindings

`DataAnnotationsBehavior` now works better with compiled and typed bindings, including nested paths, and avoids duplicate validation messages from previously applied validations.

Related PR:

- [#1031 Fix DataAnnotations behavior with compiled bindings](https://github.com/enisn/UraniumUI/pull/1031)

### Form dialogs validate before closing

Built-in form dialogs now wait for form validation before closing. Invalid forms stay open so the user can correct validation errors.

Related PRs:

- [#1033 Fix CommunityToolkit form dialog validation](https://github.com/enisn/UraniumUI/pull/1033)
- [#1034 Add async form validation support](https://github.com/enisn/UraniumUI/pull/1034)

### Binding and visual-state fixes

Several binding and visual-state issues were fixed:

- `DropdownField.SelectedItem` is now TwoWay by default.
- `TabView` cached content now gets the expected binding context.
- `EditorField` preserves text color across theme changes.
- `ButtonView` no longer keeps stale pressed state after navigation.
- Transparent `ButtonView` backgrounds are preserved on press.

Related PRs:

- [#1011 fix: preserve EditorField text color across theme changes](https://github.com/enisn/UraniumUI/pull/1011)
- [#1026 Fix DropdownField selected item binding](https://github.com/enisn/UraniumUI/pull/1026)
- [#1035 Fix TabView content binding context](https://github.com/enisn/UraniumUI/pull/1035)
- [#1046 Fix stale button pressed state after navigation](https://github.com/enisn/UraniumUI/pull/1046)
- [#1047 fix: preserve transparent ButtonView backgrounds on press](https://github.com/enisn/UraniumUI/pull/1047)

### Dialog and blur stability

The default dialog close flow, CommunityToolkit dialog rendering, and blur platform effects were stabilized.

Related PRs:

- [#1018 Stabilize default dialog close flow](https://github.com/enisn/UraniumUI/pull/1018)
- [#1021 Fix CommunityToolkit dialog double border rendering](https://github.com/enisn/UraniumUI/pull/1021)
- [#1022 Stabilize blur platform effects](https://github.com/enisn/UraniumUI/pull/1022)

## Upgrade notes

v3 includes breaking changes for apps that depend on older target frameworks or custom implementations of UraniumUI extension points.

Before upgrading, review the [Migration Guide to v3.0](../migration-guides/Migrating-To-3.0.md), especially if your app uses:

- .NET 9 target frameworks.
- `DatePickerField` bindings to non-nullable `DateTime` properties.
- a custom `IDialogService` implementation.
- a custom `IDropdown` implementation.
- TreeView internals such as `TreeViewNodeHolderView` or `AllNodeViews`.
- custom form dialog code that closes immediately after calling `Submit()`.

## Related milestone

You can view the full v3.0 milestone PR list on GitHub:

- [v3.0 milestone pull requests](https://github.com/enisn/UraniumUI/pulls?q=is%3Apr+milestone%3Av3.0)
