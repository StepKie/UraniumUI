# Migration Guide to v3.1

Version 3.1 currently has one breaking change. This guide will be updated if more v3.1 breaking changes are introduced before release.

You can see related PRs with breaking changes [from here](https://github.com/enisn/UraniumUI/pulls?q=is%3Apr+milestone%3Av3.1+label%3A%22breaking-change+%F0%9F%92%94%22).

## TabHeaderItemTemplate uses ItemsSource items

When `material:TabView` uses `ItemsSource`, `TabHeaderItemTemplate` now receives the original source item as its binding context instead of the generated `TabItem` wrapper.

This is a breaking change for custom header templates that bind through `Data`, `Command`, or `IsSelected`. The new behavior allows trim/AOT-friendly compiled bindings with `x:DataType` because the template can bind to the real item type directly.

No change is required for tabs declared directly with `<material:TabItem>`. Their header templates still bind to `TabItem`.

### Source item bindings

Replace `Data` bindings with direct source-item bindings.

**Before:**

```xml
<material:TabView ItemsSource="{Binding Tabs}">
    <material:TabView.TabHeaderItemTemplate>
        <DataTemplate>
            <Label Text="{Binding Data.Title}" />
        </DataTemplate>
    </material:TabView.TabHeaderItemTemplate>
</material:TabView>
```

**After:**

```xml
<material:TabView ItemsSource="{Binding Tabs}">
    <material:TabView.TabHeaderItemTemplate>
        <DataTemplate x:DataType="vm:BrowserTab">
            <Label Text="{Binding Title}" />
        </DataTemplate>
    </material:TabView.TabHeaderItemTemplate>
</material:TabView>
```

### Tab selection commands

Custom `ItemsSource` header templates no longer need to bind `Command` just to select the tab. `TabView` wires the header activation internally.

**Before:**

```xml
<uranium:StatefulContentView TappedCommand="{Binding Command}">
    <Label Text="{Binding Data.Title}" />
</uranium:StatefulContentView>
```

**After:**

```xml
<uranium:StatefulContentView>
    <Label Text="{Binding Title}" />
</uranium:StatefulContentView>
```

If your header has another command, bind that command to your view model as usual. Pass the current source item with `{Binding .}`.

```xml
<Button Text="Close"
        Command="{Binding Source={x:Reference page}, Path=BindingContext.CloseTabCommand}"
        CommandParameter="{Binding .}" />
```

### Selected header styling

Replace `IsSelected` bindings with the attached `TabView.IsHeaderSelected` state on the rendered header view.

**Before:**

```xml
<Grid.Triggers>
    <DataTrigger TargetType="Grid" Binding="{Binding IsSelected}" Value="True">
        <Setter Property="BackgroundColor" Value="{StaticResource Primary}" />
    </DataTrigger>
</Grid.Triggers>
```

**After:**

```xml
<Grid.Triggers>
    <DataTrigger TargetType="Grid"
                 Binding="{Binding Source={RelativeSource Self}, Path=(material:TabView.IsHeaderSelected)}"
                 Value="True">
        <Setter Property="BackgroundColor" Value="{StaticResource Primary}" />
    </DataTrigger>
</Grid.Triggers>
```

### Direct TabItem templates are unchanged

This migration only affects `TabHeaderItemTemplate` when the tab comes from `ItemsSource`.

This still binds to `TabItem`:

```xml
<material:TabView>
    <material:TabView.TabHeaderItemTemplate>
        <DataTemplate>
            <Button Text="{Binding Title}" Command="{Binding Command}" />
        </DataTemplate>
    </material:TabView.TabHeaderItemTemplate>

    <material:TabItem Title="Overview" />
    <material:TabItem Title="Details" />
</material:TabView>
```
