# Clickable Areas

Do not make important UI interactive only by attaching `TapGestureRecognizer` to a `Grid`, `Border`, `Frame`, `Label`, or image. A gesture-only clickable area is easy to build, but it usually misses keyboard focus, `Enter`/`Space` activation, visible pressed state, hover state, and screen-reader hints.

Use this guidance for cards, list rows, menu items, custom buttons, icon buttons, table rows, and any custom surface that acts like a button.

## Recommended Controls

| Scenario | Recommended control |
| --- | --- |
| Simple text or icon action | MAUI `Button` |
| Custom visual action with Material styling | `material:ButtonView` |
| Reusable low-level clickable primitive | `uranium:StatefulContentView` |
| Selectable overlay list | `uranium:Select` or `material:SelectField` |
| Toggle choice | `material:CheckBox` or `material:RadioButton` |

## Avoid Gesture-Only Actions

Avoid this pattern for important actions:

```xml
<Border Padding="16" BackgroundColor="White">
    <Border.GestureRecognizers>
        <TapGestureRecognizer Command="{Binding OpenCommand}" />
    </Border.GestureRecognizers>
    <Label Text="Open details" />
</Border>
```

This can be acceptable for decorative or redundant pointer shortcuts, but not as the only way to activate a feature.

## Use ButtonView For Custom Cards

Use `ButtonView` when the whole card should behave like a button and the visual content is more complex than a native button allows.

```xml
<material:ButtonView
    TappedCommand="{Binding OpenCommand}"
    SemanticProperties.Description="Open customer details"
    SemanticProperties.Hint="Opens the selected customer profile">
    <Grid Padding="16" ColumnDefinitions="*,Auto" ColumnSpacing="12">
        <VerticalStackLayout Spacing="2">
            <Label Text="Ada Lovelace" FontAttributes="Bold" />
            <Label Text="Premium customer" FontSize="Micro" Opacity="0.7" />
        </VerticalStackLayout>
        <Label Grid.Column="1" Text=">" VerticalOptions="Center" />
    </Grid>
</material:ButtonView>
```

`ButtonView` gives custom content a Material surface and state transitions. On Windows it is tab-focusable and supports `Enter`/`Space` activation through its handler.

## Use StatefulContentView For Low-Level Controls

Use `StatefulContentView` when building your own reusable interactive primitive or when you do not want Material `ButtonView` styling.

```xml
<uranium:StatefulContentView
    TappedCommand="{Binding ToggleCommand}"
    SemanticProperties.Description="Expand invoice filters"
    SemanticProperties.Hint="Shows or hides the filter panel">
    <Grid Padding="12" ColumnDefinitions="*,Auto">
        <Label Text="Filters" />
        <Label Grid.Column="1" Text="v" />
    </Grid>
</uranium:StatefulContentView>
```

`StatefulContentView` exposes `Pressed`, `PointerOver`, and `Normal` states, commands for pressed/hover/tapped/long-pressed interactions, and an `IsFocusable` property for controls that should or should not enter the tab order.

## Icon-Only Actions

Icon-only actions need semantic text because the icon glyph is not enough for screen readers.

```xml
<material:ButtonView
    TappedCommand="{Binding DeleteCommand}"
    SemanticProperties.Description="Delete attachment"
    SemanticProperties.Hint="Removes this attachment from the message">
    <Image Source="trash.png" WidthRequest="20" HeightRequest="20" />
</material:ButtonView>
```

## Custom Templates

Use the same rule inside templates:

- In `DataGrid` action columns, use `Button`, `ButtonView`, or `StatefulContentView` for row actions.
- In `TabView.TabHeaderItemTemplate`, use a focusable control with a bound `Command`.
- In `TreeView.ItemTemplate`, do not wrap the whole template in another clickable surface just for row selection; TreeView already owns the row interaction. Use real focusable controls only for extra actions inside the row.
- In `Select.ItemTemplate`, keep item text available and add semantic descriptions for icon-only rows.
- In bottom sheets and dialogs, do not use a passive `Grid` with `TapGestureRecognizer` as the only close or submit action.

## Checklist

Before shipping a custom clickable area, verify:

- It can be reached with `Tab` when it is an important action.
- `Enter` and `Space` activate it when hardware keyboard input is available.
- Focus, hover, pressed, selected, disabled, and error states are visually distinct where relevant.
- Screen readers get a useful `SemanticProperties.Description` and `SemanticProperties.Hint`.
- Icon-only actions have text alternatives.
- Disabled actions are not focusable or do not activate.
- Pointer-only gestures are redundant, not the only path.
