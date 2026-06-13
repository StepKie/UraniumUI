# SelectField

`SelectField` wraps the core [`Select`](../../../infrastructure/Select.md) control in a Material `InputField`. Use it when you need the overlay-hosted, templated `Select` behavior with Material input styling, validation, and optional clear support.

## Usage

`SelectField` is included in the `UraniumUI.Material.Controls` namespace. Add the Material namespace to your XAML:

```xml
xmlns:material="http://schemas.enisn-projects.io/dotnet/maui/uraniumui/material"
```

Then use it like this:

```xml
<material:SelectField
    Title="Profile"
    ItemsSource="{Binding Profiles}"
    SelectedItem="{Binding SelectedProfile}"
    ItemDisplayBinding="{Binding Name}" />
```

## Templates

`SelectField` forwards `ItemTemplate`, `SelectedItemTemplate`, and `ItemDisplayBinding` to its inner `Select` control.

```xml
<material:SelectField
    Title="Profile"
    ItemsSource="{Binding Profiles}"
    SelectedItem="{Binding SelectedProfile}"
    AllowClear="True">
    <material:SelectField.ItemTemplate>
        <DataTemplate>
            <Grid Padding="12" ColumnDefinitions="36,*" ColumnSpacing="12">
                <Border WidthRequest="32" HeightRequest="32" StrokeThickness="0" BackgroundColor="{Binding Color}">
                    <Border.StrokeShape>
                        <RoundRectangle CornerRadius="16" />
                    </Border.StrokeShape>
                    <Label Text="{Binding Initial}" TextColor="White" FontAttributes="Bold" HorizontalOptions="Center" VerticalOptions="Center" />
                </Border>
                <VerticalStackLayout Grid.Column="1" Spacing="2">
                    <Label Text="{Binding Name}" FontAttributes="Bold" />
                    <Label Text="{Binding Description}" FontSize="Micro" Opacity=".65" />
                </VerticalStackLayout>
            </Grid>
        </DataTemplate>
    </material:SelectField.ItemTemplate>
</material:SelectField>
```

## AllowClear

Set `AllowClear` to `true` to show a clear button when a value is selected.

```xml
<material:SelectField
    Title="Profile"
    ItemsSource="{Binding Profiles}"
    SelectedItem="{Binding SelectedProfile}"
    AllowClear="True" />
```

## Validation

`SelectedItem` is used as the field value for validation rules.

```xml
<material:SelectField
    Title="Profile"
    ItemsSource="{Binding Profiles}"
    SelectedItem="{Binding SelectedProfile}">
    <material:SelectField.Validations>
        <validation:RequiredValidation />
    </material:SelectField.Validations>
</material:SelectField>
```

## Properties

| Property | Description |
| --- | --- |
| `ItemsSource` | The collection of items to display. |
| `SelectedItem` | The selected item. The default binding mode is `TwoWay`. |
| `ItemDisplayBinding` | Binding used to display item text when no item template is provided. |
| `ItemTemplate` | Template used for each opened list item. |
| `SelectedItemTemplate` | Template used for the closed selected value. Falls back to `ItemTemplate`. |
| `AllowClear` | Shows a clear button when a value is selected. |
| `SelectedItemChangedCommand` | Command executed when `SelectedItem` changes. |
| `DropdownView` | The inner `Select` control. |

## Events

| Event | Description |
| --- | --- |
| `SelectedItemChanged` | Raised when `SelectedItem` changes. |

## Methods

| Method | Description |
| --- | --- |
| `Close()` | Closes the inner `Select` overlay if it is open. |
