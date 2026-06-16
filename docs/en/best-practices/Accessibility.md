# Accessibility Best Practices

Accessibility is part of the component contract in UraniumUI. The library builds on .NET MAUI controls and handlers so apps can keep platform accessibility services, keyboard focus, native buttons, semantic properties, and screen-reader metadata instead of moving to a custom rendering model.

Use this page as a checklist when building production screens with UraniumUI.

## Critical Interaction Areas

Prioritize accessibility checks for these areas because they often contain custom interaction logic:

- Popup, dialog, bottom sheet, and backdrop surfaces
- `TabView` headers and custom header templates
- `DataGrid` cells, selection columns, and action columns
- `DatePickerField` and `CalendarView` date selection flows
- `Select`, `SelectField`, `DropdownField`, and `AutoCompleteTextField`
- Masked or formatted text inputs added through custom controls
- Validation error text and form-level validation summaries
- Custom cards, rows, list items, and any area made clickable with gestures

## Keyboard Access

Interactive UI must be reachable without a pointer when a hardware keyboard is available. Prefer controls that already have focus and activation behavior:

- Use MAUI `Button` for ordinary actions.
- Use UraniumUI `ButtonView` when the action needs custom visual content.
- Use UraniumUI `StatefulContentView` when building a reusable custom interactive primitive.
- Use `Select` or `SelectField` for overlay selection that needs keyboard navigation.
- Avoid using a bare `TapGestureRecognizer` as the only activation path for important actions.

Recommended keyboard expectations:

| Pattern | Expected behavior |
| --- | --- |
| Button or clickable card | `Tab` focuses it, `Enter` and `Space` activate it, focus state is visible. |
| Select or combo box | `Enter`/`Space` opens, `Up`/`Down` moves, `Home`/`End` jumps, `Escape` closes. |
| Popup or bottom sheet | Initial focus moves inside, `Escape` closes when dismissal is allowed, focus returns to the opener. |
| Tabs | Tab headers are focusable; custom headers must expose a command, selected state, and visible focus. |
| Data grid | Row actions use buttons; selection controls are keyboard reachable; custom cells do not hide focusable content. |

## Focus States

Users need to see where keyboard focus is. UraniumUI provides visible focus behavior through platform focus visuals for focusable primitives, while Material input fields change border, title, and icon color through `AccentColor` when their inner input is focused.

When customizing styles:

- Do not remove focus outlines without replacing them with an equally visible state.
- Keep enough contrast between focused, hovered, pressed, disabled, selected, and error states.
- Test light and dark themes.
- Test custom `ButtonView` and `StatefulContentView` styles with keyboard focus, not only pointer hover.

## Screen Reader Semantics

Use MAUI semantic properties for labels, hints, and descriptions that are not obvious from visible text.

```xml
<material:ButtonView
    SemanticProperties.Description="Open customer details"
    SemanticProperties.Hint="Opens the selected customer in a details page"
    TappedCommand="{Binding OpenCustomerCommand}">
    <Grid Padding="16" ColumnDefinitions="*,Auto">
        <Label Text="Customer details" />
        <Image Source="chevron_right.png" />
    </Grid>
</material:ButtonView>
```

Apply semantic metadata when:

- The control is icon-only.
- The visible text is abbreviated.
- A custom template replaces default text.
- A button changes state, such as show/hide password or expand/collapse.
- Validation text appears dynamically.
- A popup, dialog, bottom sheet, or backdrop changes the active context.

## Validation Text

Validation text must be visible and understandable without relying only on color or an icon.

Recommended practice:

- Use clear validation messages, not only a red border or warning icon.
- Keep the message near the field it describes.
- Add a form-level summary when a submit action validates multiple fields.
- Use `uranium:FormView.ValidationPath` for manually created forms so server-side or async validation errors map to the correct field.
- For highly regulated or complex forms, add explicit `SemanticProperties.Description` or `SemanticProperties.Hint` to fields that need extra instruction.

## Custom Templates

Custom templates are powerful, but they can remove default accessibility behavior if they replace buttons or labels with passive layouts.

When using templates in `TabView`, `TreeView`, `DataGrid`, `Select`, or `AutoFormView`:

- Keep action elements as `Button`, `ButtonView`, or `StatefulContentView`. For `TreeView`, the row already owns its interactive wrapper; use extra controls only for additional row actions.
- Add semantic descriptions to icon-only controls.
- Do not rely on color alone for selected, active, required, or error state.
- Make decorative images non-essential to understanding the row.
- Keep item text available as real text where possible so screen readers can announce it.

## Related Pages

- [Clickable Areas](ClickableAreas.md)
- [StatefulContentView](../infrastructure/StatefulContentView.md)
- [ButtonView](../themes/material/ButtonView.md)
- [Select](../infrastructure/Select.md)
- [SelectField](../themes/material/components/SelectField.md)
- [InputField](../themes/material/components/InputField.md)
- [Validations](../themes/material/Validations.md)
