# ButtonView
ButtonView is a plain control that allows you place your content inside it. It is a simple wrapper around the [StatefulContentView](../../../en/infrastructure/StatefulContentView.md) control. 

ButtonView included in **Material Theme**.

## Usage

`ButtonView` is defined in `UraniumUI.Material.Controls` namespace. You can add it to your XAML like this:

```xml
xmlns:material="http://schemas.enisn-projects.io/dotnet/maui/uraniumui/material"
```

Then you can use it like this:

```xml
<material:ButtonView>
    <Label Text="Hello UraniumUI 👋" />
</material:ButtonView>
```

![uraniumui buttonview](../../images/buttonview-demo.png)

## Keyboard and Accessibility

Use `ButtonView` for custom cards, icon buttons, and visual actions that need Material styling but should still behave like controls. On Windows, `ButtonView` is tab-focusable, shows the system focus visual, and supports `Enter`/`Space` activation through its handler.

Provide semantic text for icon-only or card-like actions:

```xml
<material:ButtonView
    TappedCommand="{Binding OpenCustomerCommand}"
    SemanticProperties.Description="Open customer details"
    SemanticProperties.Hint="Opens the selected customer profile">
    <Grid Padding="16" ColumnDefinitions="*,Auto">
        <Label Text="Customer details" />
        <Label Grid.Column="1" Text=">" />
    </Grid>
</material:ButtonView>
```

For broader guidance, see [Clickable Areas](../../best-practices/ClickableAreas.md).


## Customizations
You can customize the `ButtonView` by using the style properties. You can use the following template to create your own style:

```xml
<Style TargetType="material:ButtonView" ApplyToDerivedTypes="True" CanCascade="True" BaseResourceKey="UraniumUI.Material.Controls.ButtonView.Base">
    <Setter Property="BackgroundColor" Value="{StaticResource Primary}" />
    <Setter Property="Padding" Value="10" />
    <Setter Property="StrokeShape" Value="{RoundRectangle CornerRadius=20}"/>
    <Setter Property="VisualStateManager.VisualStateGroups">
        <VisualStateGroupList>
            <VisualStateGroup x:Name="CommonStates">
                <VisualState x:Name="PointerOver">
                    <VisualState.Setters>
                        <Setter Property="uranium:DynamicTint.BackgroundColorOpacity" Value="0.9" />
                    </VisualState.Setters>
                </VisualState>
                <VisualState x:Name="Normal"/>
                <VisualState x:Name="Pressed">
                    <VisualState.Setters>
                        <Setter Property="uranium:DynamicTint.BackgroundColorOpacity" Value="0.8" />
                    </VisualState.Setters>
                </VisualState>
            </VisualStateGroup>
        </VisualStateGroupList>
    </Setter>
</Style>
```

> **Note**: Make sure the following namespaces exist in your XAML file:
> - `xmlns:material="http://schemas.enisn-projects.io/dotnet/maui/uraniumui/material"`
> - `xmlns:uranium="http://schemas.enisn-projects.io/dotnet/maui/uraniumui"`
