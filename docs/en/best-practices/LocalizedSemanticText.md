# Localized Semantic Text

UraniumUI generates semantic descriptions and hints for built-in icon-only actions, validation state, read-only state, disabled state, and selected or expanded state in several controls. The generated text is centralized in `UraniumUIAccessibilityOptions`, so applications can localize these strings once instead of editing each control template.

Use this page when your app needs screen-reader text in a language other than the default English strings, or when you want to publish a reusable language pack for multiple apps.

## Configure App Defaults

Configure semantic text in `MauiProgram.cs` after `UseUraniumUI()` and before `Build()`:

```csharp
using UraniumUI.Options;

builder
    .UseMauiApp<App>()
    .UseUraniumUI()
    .UseUraniumUIMaterial()
    .ConfigureUraniumUIAccessibility(options =>
    {
        options.ClearTextDescription = "Metni temizle";
        options.ClearTextHint = "Geçerli değeri temizler.";
        options.ClearAutocompleteTextDescription = "Metni temizle";
        options.ClearAutocompleteTextHint = "Otomatik tamamlama değerini temizler.";

        options.ClearSelectionDescription = "Seçimi temizle";
        options.ClearSelectionHint = "Seçili değeri temizler.";
        options.ClearDateDescription = "Tarihi temizle";
        options.ClearDateHint = "Seçili tarihi temizler.";
        options.ClearTimeDescription = "Saati temizle";
        options.ClearTimeHint = "Seçili saati temizler.";

        options.ShowPasswordDescription = "Parolayı göster";
        options.HidePasswordDescription = "Parolayı gizle";
        options.PasswordVisibilityToggleHint = "Parola görünürlüğünü değiştirir.";

        options.ValidationErrorDescription = "Doğrulama hatası";
        options.ValidationErrorHintFormat = "Hata: {0}";
        options.ReadOnlyHint = "Salt okunur.";
        options.DisabledHint = "Devre dışı.";

        options.OpenDatePickerHint = "Tarih seçiciyi açar.";
        options.RemoveChipDescriptionFormat = "{0} öğesini kaldır";
        options.RemoveChipHint = "Bu seçili öğeyi kaldırır.";

        options.ChangeCalendarYearDescription = "Yılı değiştir";
        options.ChangeCalendarYearHint = "Yıl seçimini gösterir.";
        options.PreviousMonthDescription = "Önceki ay";
        options.NextMonthDescription = "Sonraki ay";

        options.ExpandBottomSheetDescription = "Alt sayfayı genişlet";
        options.CollapseBottomSheetDescription = "Alt sayfayı daralt";
        options.ToggleBottomSheetHint = "Alt sayfayı açar veya kapatır.";

        options.SelectTabHint = "Bu sekmeyi seçer.";
        options.SelectedTabDescriptionFormat = "{0}, seçili";

        options.ExpandTreeNodeDescriptionFormat = "{0} öğesini genişlet";
        options.CollapseTreeNodeDescriptionFormat = "{0} öğesini daralt";
        options.TreeNodeDescriptionFormat = "{0}";
        options.SelectedTreeNodeDescriptionFormat = "{0}, seçili";
        options.TreeNodeHint = "Bu öğeyi seçer.";
    });
```

The `{0}` token is replaced with the current validation message, chip text, tab title, or tree node text depending on the option. Use the token where it fits naturally in the target language.

## Control Coverage

The centralized options are used by generated UraniumUI semantics in these areas:

| Option area | Controls or behavior |
| --- | --- |
| Clear text | `TextField`, `AutoCompleteTextField` clear buttons |
| Clear selection | `PickerField`, `DropdownField`, `SelectField` clear buttons |
| Date and time | `DatePickerField`, `TimePickerField`, `CalendarView` navigation |
| Password visibility | `TextFieldPasswordShowHideAttachment` |
| Validation state | Material `InputField` validation icon, label, input hint, and validation announcement text |
| Field state | Material `TextField` and `EditorField` read-only and disabled hints |
| Selection and expansion | `Chip`, `TabView`, `TreeView`, generated `BottomSheetView` anchor |

## Control-Level Overrides

Use `UraniumUIAccessibilityOptions` for reusable defaults. Use MAUI `SemanticProperties` directly when one control instance needs more specific text than the global default.

```xml
<material:ButtonView
    SemanticProperties.Description="Müşteri ayrıntılarını aç"
    SemanticProperties.Hint="Seçili müşterinin ayrıntı sayfasını açar"
    TappedCommand="{Binding OpenCustomerCommand}">
    <Image Source="person_details.png" />
</material:ButtonView>
```

For Material fields, `Title` or `TitleFormattedText` is generated as the semantic description of the inner input unless the app already set `SemanticProperties.Description` on that inner input.

```xml
<material:TextField
    Title="E-posta"
    SemanticProperties.Hint="İş hesabınızın e-posta adresini girin" />
```

## Reusable Language Packs

A language pack can be a small class library or NuGet package that exposes an extension method for `MauiAppBuilder`. The extension should call `ConfigureUraniumUIAccessibility` and set all language-specific strings in one place.

```csharp
using UraniumUI.Options;

namespace UraniumUI.Accessibility.Turkish;

public static class UraniumUITurkishAccessibilityExtensions
{
    public static MauiAppBuilder UseUraniumUITurkishAccessibility(this MauiAppBuilder builder)
    {
        return builder.ConfigureUraniumUIAccessibility(options =>
        {
            options.ClearTextDescription = "Metni temizle";
            options.ClearTextHint = "Geçerli değeri temizler.";
            options.ClearSelectionDescription = "Seçimi temizle";
            options.ClearSelectionHint = "Seçili değeri temizler.";
            options.ShowPasswordDescription = "Parolayı göster";
            options.HidePasswordDescription = "Parolayı gizle";
            options.PasswordVisibilityToggleHint = "Parola görünürlüğünü değiştirir.";
            options.ValidationErrorDescription = "Doğrulama hatası";
            options.ValidationErrorHintFormat = "Hata: {0}";
            options.ReadOnlyHint = "Salt okunur.";
            options.DisabledHint = "Devre dışı.";
            options.SelectedTabDescriptionFormat = "{0}, seçili";
            options.ExpandTreeNodeDescriptionFormat = "{0} öğesini genişlet";
            options.CollapseTreeNodeDescriptionFormat = "{0} öğesini daralt";
        });
    }
}
```

Apps can then install the package and opt in with one line:

```csharp
builder
    .UseUraniumUI()
    .UseUraniumUIMaterial()
    .UseUraniumUITurkishAccessibility();
```

## Pack Guidelines

- Translate action intent, not icon names. Use "Clear text", not "X icon".
- Keep labels short because screen readers announce them frequently.
- Keep hints as complete action descriptions.
- Preserve `{0}` placeholders in format strings unless the target phrase does not need the dynamic value.
- Test with the target platform screen reader because announcement order and punctuation can differ by platform.
- Combine language-pack defaults with per-control `SemanticProperties` when a screen needs domain-specific wording.

## Related Pages

- [Accessibility Best Practices](Accessibility.md)
- [Clickable Areas](ClickableAreas.md)
- [InputField](../themes/material/components/InputField.md)
- [TextField](../themes/material/components/TextField.md)
- [Validations](../themes/material/Validations.md)
