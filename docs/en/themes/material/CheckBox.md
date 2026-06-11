# CheckBox
CheckBox is a control that allows the user to choose a boolean value. Use `UraniumUI.Material.Controls.CheckBox` for UraniumUI's Material-styled checkbox.

![MAUI Material Design CheckBox](https://lh3.googleusercontent.com/lzGG5PeqO9Eru69AybrsH5EgrB_PAIA2OIi17xLEQHdqRhLBjS8HLjYwVxzMbJ7DJFEnjTeZT_5iP2r4SSv-WgiC5LosxEtFcMTt=w1064-v0)

## Features

UraniumUI's CheckBox supports Material styling and exposes common color properties such as `Color`, `BorderColor`, `TextColor`, and `IconColor`.


## Usage

CheckBox is defined in `UraniumUI.Material.Controls` namespace. You can use it like this:

```xml
xmlns:material="http://schemas.enisn-projects.io/dotnet/maui/uraniumui/material"
```

```xml
<StackLayout MaximumWidthRequest="400">
    <material:CheckBox Text="Option 1" />
    <material:CheckBox Text="Option 2" IsChecked="True" />
    <material:CheckBox Text="Option 3 (Disabled)" IsDisabled="True" />
    <material:CheckBox Text="Option 4 (Disabled)" IsDisabled="True" IsChecked="True" />

    <BoxView />

    <material:CheckBox Text="Option 1" LabelPosition="Before" />
    <material:CheckBox Text="Option 2" LabelPosition="Before" IsChecked="True" />
    <material:CheckBox Text="Option 3 (Disabled)" IsDisabled="True" LabelPosition="Before" />
    <material:CheckBox Text="Option 4 (Disabled)" IsDisabled="True" IsChecked="True" LabelPosition="Before" />
</StackLayout>
```

## Colors

You can set colors per checkbox.

```xml
<material:CheckBox
    Text="Option 1"
    Color="DeepSkyBlue"
    BorderColor="Gray"
    TextColor="Black"
    IconColor="White" />
```

For app-wide styling, target the Material checkbox type in your resources.

```xml
<ResourceDictionary
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:material="http://schemas.enisn-projects.io/dotnet/maui/uraniumui/material">
    <Style TargetType="material:CheckBox">
        <Setter Property="Color" Value="DeepSkyBlue" />
        <Setter Property="TextColor" Value="Black" />
    </Style>
</ResourceDictionary>
```

And result will be like this:

| Dark - Desktop | Light - Mobile |
| --- | --- |
| ![MAUI Material Design CheckBox](../../images/checkbox-demo-dark.gif) | ![MAUI Material Design CheckBox](../../images/checkbox-demo-light.gif)  |
