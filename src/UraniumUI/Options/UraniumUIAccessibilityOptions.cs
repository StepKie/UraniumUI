namespace UraniumUI.Options;

public class UraniumUIAccessibilityOptions
{
    public string ClearTextDescription { get; set; } = "Clear text";

    public string ClearTextHint { get; set; } = "Clears the current value.";

    public string ClearAutocompleteTextDescription { get; set; } = "Clear text";

    public string ClearAutocompleteTextHint { get; set; } = "Clears the current autocomplete value.";

    public string ClearSelectionDescription { get; set; } = "Clear selection";

    public string ClearSelectionHint { get; set; } = "Clears the selected value.";

    public string ClearDateDescription { get; set; } = "Clear date";

    public string ClearDateHint { get; set; } = "Clears the selected date.";

    public string ClearTimeDescription { get; set; } = "Clear time";

    public string ClearTimeHint { get; set; } = "Clears the selected time.";

    public string ShowPasswordDescription { get; set; } = "Show password";

    public string HidePasswordDescription { get; set; } = "Hide password";

    public string PasswordVisibilityToggleHint { get; set; } = "Toggles password visibility.";

    public string ValidationErrorDescription { get; set; } = "Validation error";

    public string ValidationErrorHintFormat { get; set; } = "Error: {0}";

    public string ReadOnlyHint { get; set; } = "Read only.";

    public string DisabledHint { get; set; } = "Disabled.";

    public string OpenDatePickerHint { get; set; } = "Opens the date picker.";

    public string RemoveChipDescriptionFormat { get; set; } = "Remove {0}";

    public string RemoveChipHint { get; set; } = "Removes this selected item.";

    public string ChangeCalendarYearDescription { get; set; } = "Change year";

    public string ChangeCalendarYearHint { get; set; } = "Shows year selection.";

    public string PreviousMonthDescription { get; set; } = "Previous month";

    public string NextMonthDescription { get; set; } = "Next month";

    public string ExpandBottomSheetDescription { get; set; } = "Expand bottom sheet";

    public string CollapseBottomSheetDescription { get; set; } = "Collapse bottom sheet";

    public string ToggleBottomSheetHint { get; set; } = "Toggles the bottom sheet.";

    public string SelectTabHint { get; set; } = "Selects this tab.";

    public string SelectedTabDescriptionFormat { get; set; } = "{0}, selected";

    public string ExpandTreeNodeDescriptionFormat { get; set; } = "Expand {0}";

    public string CollapseTreeNodeDescriptionFormat { get; set; } = "Collapse {0}";

    public string TreeNodeDescriptionFormat { get; set; } = "{0}";

    public string SelectedTreeNodeDescriptionFormat { get; set; } = "{0}, selected";

    public string TreeNodeHint { get; set; } = "Selects this item.";

    public string FormatValidationErrorHint(string message)
    {
        return FormatSemanticText(ValidationErrorHintFormat, message);
    }

    public string FormatRemoveChipDescription(string text)
    {
        return FormatSemanticText(RemoveChipDescriptionFormat, text);
    }

    public string FormatSelectedTabDescription(string text)
    {
        return FormatSemanticText(SelectedTabDescriptionFormat, text);
    }

    public string FormatExpandTreeNodeDescription(string text)
    {
        return FormatSemanticText(ExpandTreeNodeDescriptionFormat, text);
    }

    public string FormatCollapseTreeNodeDescription(string text)
    {
        return FormatSemanticText(CollapseTreeNodeDescriptionFormat, text);
    }

    public string FormatTreeNodeDescription(string text)
    {
        return FormatSemanticText(TreeNodeDescriptionFormat, text);
    }

    public string FormatSelectedTreeNodeDescription(string text)
    {
        return FormatSemanticText(SelectedTreeNodeDescriptionFormat, text);
    }

    private static string FormatSemanticText(string format, string value)
    {
        if (string.IsNullOrWhiteSpace(format))
        {
            return value;
        }

        try
        {
            return string.Format(format, value);
        }
        catch (FormatException)
        {
            return string.IsNullOrWhiteSpace(value) ? format : $"{format} {value}";
        }
    }
}
