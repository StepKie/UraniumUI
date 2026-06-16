# Accessibility Follow-Up Report

This report separates implementation follow-up work from the marketing and documentation updates. It is based on a source audit of keyboard handling, focus behavior, semantic properties, validation text, and gesture-only interaction patterns.

## Summary

UraniumUI already has important accessibility foundations:

- `StatefulContentView` provides focusability, pressed/hover/tapped commands, and hardware keyboard event plumbing in platform handlers.
- Material `ButtonView` is focusable and supports `Enter`/`Space` activation on Windows.
- Material `CheckBox` and `RadioButton` are focusable and support `Enter`/`Space` toggling on Windows.
- `Select` has keyboard navigation for open, close, active item movement, selection, `Home`, `End`, and `Escape`.
- `Select` generates `SemanticProperties.Description` and `SemanticProperties.Hint` from the placeholder or selected value.
- Material `InputField` changes border, title, and icon color on focus through `AccentColor`.
- `CalendarView` uses MAUI `Button` controls for day/year selection and assigns semantic descriptions to previous/next navigation.
- `SelectField` assigns semantic description and hint text to its clear selection action.

The main gaps are semantic coverage for icon-only actions, keyboard/focus behavior for gesture-only surfaces, popup focus management, and richer accessibility contracts for complex controls.

## Implemented In This PR

- Added centralized `UraniumUIAccessibilityOptions` defaults so apps and language packs can localize generated semantic labels and hints through `ConfigureUraniumUIAccessibility`.
- Mapped Material `InputField.Title`/`TitleFormattedText` to the inner input semantic description and hid the visual floating label from the accessibility tree.
- Added validation semantics: validation label descriptions, validation icon descriptions, input hint updates, and best-effort screen-reader announcements when validation enters an error state.
- Added read-only and disabled semantic hints for Material text/editor fields while preserving native `IsReadOnly`/`IsEnabled` propagation.
- Added visible focus beyond the border color change by applying a focus shadow to Material input borders.
- Added semantic labels/hints for built-in icon-only clear actions on `TextField`, `AutoCompleteTextField`, `PickerField`, `DatePickerField`, `TimePickerField`, `DropdownField`, and `SelectField`.
- Fixed `TextField.DisallowClearButtonFocus` so it actually removes the clear button from keyboard focus instead of enabling focus.
- Added dynamic semantic labels for `TextFieldPasswordShowHideAttachment`.
- Added semantic remove text for `Chip` remove buttons.
- Converted the generated `BottomSheetView` anchor to a focusable `StatefulContentView` with expand/collapse semantic text.
- Made `CalendarView` month/year toggle focusable and keyboard activatable.
- Made non-focusable custom `TabView` headers keyboard-selectable and exposed selected-state semantics.
- Wired `TreeViewNodeView` row/expander actions through existing interactive command paths and added expand/collapse/selected semantic text.

Remaining larger follow-up work: popup/dialog focus trap and focus restore, full `Escape` dismissal contracts for overlays, DataGrid row/cell keyboard semantics, DatePickerField opener replacement with a non-breaking focusable surface, and formal masked-input documentation if that control is introduced.

## Critical Follow-Up Items

| Priority | Area | Current state | Recommended work |
| --- | --- | --- | --- |
| High | Popup/Dialog/BottomSheet | Bottom sheet header and outside dismissal use `TapGestureRecognizer`; dialog providers rely on provider behavior for focus and dismissal. | Define a popup accessibility contract: initial focus, focus trap/restore, explicit close action, `Escape`/Back dismissal, semantic title/message, and screen-reader announcements. |
| High | BottomSheet | `BottomSheetView` generated anchor is a passive `ContentView` with tap/pan gestures and no semantic description. | Replace or wrap the header with `StatefulContentView`/`ButtonView`, add generated semantic description/hint for expand/collapse, and support keyboard toggle and dismissal. |
| High | TabView | `TabView` adds `TapGestureRecognizer` to headers and relies on template content for focusability. Default template contains a `Button`, but the wrapper gesture is still the selection mechanism. | Make tab headers explicitly keyboard-selectable, expose selected state semantically, and document/implement arrow-key tab navigation if desired. |
| High | DatePickerField/CalendarView | `DatePickerField` opens the prompt from a label `TapGestureRecognizer`; `CalendarView` month/year header uses a tapped `Label`. | Make the date field opener focusable and keyboard activatable. Make month/year header a focusable control with semantic text and keyboard activation. Consider arrow-key day grid navigation. |
| High | ComboBox/AutoComplete | `Select` is strong, but `AutoCompleteTextField` clear action is a `ContentView` plus tap gesture and lacks semantic text. `DropdownField` delegates to native/dropdown behavior and has less documented keyboard coverage than `SelectField`. | Add semantic descriptions and focusable clear actions. Prefer `SelectField` for templated combo-box UX. Define autocomplete keyboard expectations for suggestions and clearing. |
| High | Validation error text | Material validation renders visible label text, but no explicit semantic announcement or field association was found. | Add semantic metadata for validation labels, consider announcing changed validation state, and ensure field-level errors map to inputs in screen readers. |
| High | Custom clickable cards/rows | Several source locations still use `TapGestureRecognizer` for primary actions. | Replace important gesture-only interactions with `Button`, `ButtonView`, or `StatefulContentView`. Keep gestures only as redundant pointer shortcuts. |
| Medium | DataGrid | Selection has visual states, but there is no row/cell semantic contract or keyboard selection/navigation contract in source. | Define accessible row, cell, header, empty, loading, and selection semantics. Use real buttons for action cells and document keyboard expectations. |
| Medium | MaskedInput | No UraniumUI `MaskedInput` control or dedicated masked-input documentation was found in `src` or `docs`. | If masked input is planned or provided through InputKit/custom `TextField`, document format hints, screen-reader behavior, raw vs formatted value, and validation messages. |
| Medium | TreeView | Rows are already wrapped by `StatefulContentView` and the default expander is a `ButtonView`, but Windows paths still wire row/expander actions through `TapGestureRecognizer`. No semantic expand/collapse description was found. | Connect the existing row/expander wrappers to keyboard activation consistently, and add semantic descriptions/hints for expand/collapse and selected state. |

## Screen Reader Description Gaps

| Control or pattern | Observed gap | Suggested semantic text |
| --- | --- | --- |
| `TextField` clear icon | No `SemanticProperties.Description` found. The `DisallowClearButtonFocus` binding also appears suspicious because it binds directly to `IsFocusable`. | Description: "Clear text". Hint: "Clears the current value." Verify/invert focus binding if needed. |
| `AutoCompleteTextField` clear icon | Clear icon is a `ContentView` with `TapGestureRecognizer`; no description or hint found. | Description: "Clear text". Hint: "Clears the current autocomplete value." |
| `PickerField`, `DatePickerField`, `TimePickerField`, `DropdownField` clear icons | Clear icons use `StatefulContentView`, but explicit semantic descriptions were not found except for `SelectField`. | Description: "Clear selection", "Clear date", or "Clear time". Hint: "Clears the selected value." |
| `TextFieldPasswordShowHideAttachment` | Icon changes between eye and eye-slash, but no dynamic semantic description or hint was found. | Description should switch between "Show password" and "Hide password". Hint: "Toggles password visibility." |
| `Chip` remove icon | Remove action is a `StatefulContentView` with an X icon and no semantic text. | Description: "Remove {chip text}". Hint: "Removes this selected item." |
| `BottomSheetView` generated anchor | Visual drag/toggle handle has no semantic description. | Description: "Expand bottom sheet" or "Collapse bottom sheet" based on state. |
| `TreeView` expander | Default expander is a `ButtonView`, but the chevron icon has no semantic expand/collapse text. | Description: "Expand {node}" or "Collapse {node}". |
| `TabView` icon-only custom headers | Header templates can be arbitrary and may omit readable text. | Description should match the tab title. Hint: "Selects this tab." |
| `DataGrid` icon-only action cells | Custom `CellItemTemplate` can use icons without labels. | Description should include action and row context, such as "Delete Ada Lovelace". |

## Gesture-Only Source Locations To Review

These are source locations where `TapGestureRecognizer` appears in important interaction paths:

| Source | Notes |
| --- | --- |
| `src/UraniumUI.Material/Attachments/BottomSheetView.cs` | Header toggle and outside close use tap gestures. |
| `src/UraniumUI.Material/Controls/DatePickerField.cs` | Date prompt opens from a tapped label. |
| `src/UraniumUI.Material/Controls/MultiplePickerField.cs` | Whole field opens selection prompt from a tap gesture. |
| `src/UraniumUI.Material/Controls/TabView.cs` | Header wrapper gets a tap gesture to select the tab. |
| `src/UraniumUI.Material/Controls/TreeViewNodeView.cs` | Row and expander are wrapped by interactive UraniumUI controls, but Windows action wiring uses tap gestures instead of the wrapper command path. |
| `src/UraniumUI.Material/Controls/AutoCompleteTextField.cs` | Clear icon uses `ContentView` plus tap gesture. |
| `src/UraniumUI/Controls/CalendarView.cs` | Month/year label toggles year selection through tap gesture. |
| `src/UraniumUI/Controls/PopupOverlay.cs` | Dismiss layer uses tap gesture only. |

Recommended pattern: use `Button`, `ButtonView`, or `StatefulContentView` for the primary accessible action, then keep tap/pan gestures only where they are additional pointer affordances.

## Component Notes

### Popup, Dialog, BottomSheet

Current docs now tell app authors to provide visible headings, explicit close buttons, keyboard-reachable actions, and semantic descriptions. Implementation follow-up should add library-level behavior for focus movement, focus restore, `Escape`/Back dismissal, and screen-reader context changes.

### TabView

Default tab headers include a button, which is good. The wrapper gesture and custom template requirements are still risky. Implementation should enforce or assist focusable tab headers and expose selected state. A future enhancement could support arrow-key navigation between tabs.

### DataGrid

Docs now recommend meaningful titles, real buttons for action cells, and semantic descriptions for icon-only actions. Implementation can improve with row/cell semantics, selection announcements, header metadata, and a keyboard navigation model.

### DatePicker and Calendar

`CalendarView` already has semantic previous/next buttons and native day/year buttons. The month/year header and DatePickerField opener should be upgraded from gesture-only labels to focusable controls. A roving-focus date grid would improve keyboard use.

### ComboBox and AutoComplete

`Select`/`SelectField` are the strongest accessible combo-box style path today because they include keyboard behavior and semantic hints. `AutoCompleteTextField`, `DropdownField`, `PickerField`, and clear actions need stronger semantics and keyboard verification.

### MaskedInput

No UraniumUI source or docs for a masked input control were found. If the project treats InputKit `AdvancedEntry` or a custom `TextField` behavior as masked input, add a docs page explaining expected format, raw value, formatted value, validation, and screen-reader guidance.

### Validation Error Text

The visual validation label is useful and should remain. Follow-up should connect validation messages to screen readers and consider an announcement mechanism when validation state changes after submit or text input.

### Custom Cards and TapGestureRecognizer

Docs now include `best-practices/ClickableAreas.md`. Implementation follow-up should replace internal important gesture-only patterns where possible, or wire existing interactive wrappers to their command/key paths, and add tests for keyboard activation in reusable primitives.

## Suggested Test Coverage

Add focused tests where MAUI-level behavior can be verified:

- `StatefulContentView` and `ButtonView` keyboard activation does not double-fire `PressedCommand`/`TappedCommand`.
- `Select` handles `Enter`, `Space`, `Escape`, `Up`, `Down`, `Home`, and `End` consistently.
- `Select` generated semantic description/hint does not overwrite explicitly set app semantics.
- Clear actions expose semantic descriptions on each field type.
- Validation labels expose readable text and state changes can be observed by screen readers or automation where possible.
- Bottom sheet explicit close control and keyboard dismissal behavior.
- Tab headers remain focusable when custom templates are used.
