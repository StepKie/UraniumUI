<div align="center">
    <img align="center" src="./art/logo.svg" width="33%">
    <h1 align="center">UraniumUI</h1>
    <p><strong>The open-source presentation framework for production .NET MAUI apps.</strong></p>
    <p>Build app-ready MAUI screens with Material controls, dynamic forms, dialogs, data components, keyboard-aware interactions, focus states, theming, icons, overlays, and effects without leaving XAML or MVVM.</p>
</div>

<div align="center">
   <a href="https://www.codefactor.io/repository/github/enisn/uraniumui"><img src="https://www.codefactor.io/repository/github/enisn/uraniumui/badge"></a>
   <a href="https://www.nuget.org/packages/UraniumUI/"><img src="https://img.shields.io/nuget/v/UraniumUI?color=blue&logo=nuget"></a>
   <a href="https://www.nuget.org/packages/UraniumUI/"><img src="https://img.shields.io/nuget/dt/UraniumUI.svg"></a>
   <a href="./LICENSE"><img src="https://img.shields.io/github/license/enisn/UraniumUI.svg"></a>
   <a href="https://enisn.visualstudio.com/Uranium%20UI/_build/latest?definitionId=15&branchName=develop"><img src="https://enisn.visualstudio.com/Uranium%20UI/_apis/build/status/enisn.UraniumUI?branchName=develop"></a>
   <a href="https://discord.gg/nN7Yvch73v"><img src="https://img.shields.io/discord/1277612890668404798"></a>
</div>

<p align="center">
    <a href="https://enisn-projects.io/docs/en/uranium/latest/Getting-Started">Documentation</a> |
    <a href="https://www.nuget.org/packages/UraniumUI/">NuGet</a> |
    <a href="https://www.nuget.org/packages/UraniumUI.Templates/">Templates</a> |
    <a href="./demo/UraniumApp">Demo App</a> |
    <a href="https://discord.gg/nN7Yvch73v">Discord</a>
</p>

UraniumUI is a free and open-source presentation layer for .NET MAUI. It fills the UI gaps plain MAUI leaves to every team: Material-styled inputs and buttons, generated forms, validation mapping, keyboard-friendly selection controls, focus-aware custom surfaces, data grids, tree views, tab views, dialogs, bottom sheets, backdrops, icons, blur effects, code views, templates, and app-ready presentation patterns.

You keep writing regular .NET MAUI: XAML, `ContentPage`, `Shell`, bindings, styles, resources, handlers, dependency injection, MVVM, and platform APIs. UraniumUI attaches to that model instead of replacing it.

It is not just a collection of styled controls and it is not only an AutoFormView package. UraniumUI provides the building blocks for real app screens: form workflows, data-heavy views, hierarchical navigation, app surfaces, visual system resources, native-MAUI handlers, and extensibility points for your own controls.

## Who It Is For

- .NET MAUI teams building production line-of-business, admin, data-entry, or internal tools.
- Teams that want a Material presentation layer without moving away from XAML, MVVM, resources, or MAUI handlers.
- Apps that need more than basic controls: validation-aware fields, data grids, tree views, tabs, dialogs, bottom sheets, and reusable page surfaces.
- Developers who want incremental adoption: use one control, add one package, or start from a full template.
- Teams that need accessible interactions: visible focus states, keyboard navigation, semantic hints, and custom clickable areas that behave like real controls.

## No Framework Tax

Some UI stacks ask you to move your app into their way of building software. UraniumUI does not.

There is no required base ViewModel, proprietary navigation model, custom application layer, generated project structure, or new UI DSL. Adopt one control, a validation-aware field set, or a full Material presentation layer. Your app remains a MAUI app.

Plain MAUI in, better UI out.

## The Mental Model

.NET MAUI gives you the platform foundation. UraniumUI adds the presentation architecture that most production apps end up rebuilding.

| App need | UraniumUI provides |
| --- | --- |
| Build forms quickly | `AutoFormView` generates editors from your model, while `FormView` handles validation, submit/reset behavior, busy state, and validation summaries. |
| Validate consistently | InputKit validation, DataAnnotations integration, async validators, and property-path mapping for generated or hand-written fields. |
| Build data screens | `DataGrid`, `Paginator`, `TreeView`, `TabView`, `Select`, `Dropdown`, `CalendarView`, and Material input fields for real application workflows. |
| Add app surfaces | `IDialogService`, modal-page dialogs, Mopups and CommunityToolkit dialog providers, form dialogs, `BottomSheetView`, and `BackdropView`. |
| Keep interaction accessible | Focusable custom surfaces, keyboard-aware `Select`/`SelectField`, Material input focus states, semantic descriptions for supported controls, and best-practice guidance for custom clickable areas. |
| Standardize presentation | Material color and style resources, light/dark tokens, button variants, containers, dividers, elevation, icon packs, cascading styles, and blur effects. |
| Keep UI native and flexible | Controls and handlers built on MAUI primitives instead of a closed rendering stack or proprietary application model. |
| Escape the defaults | Replace generated editors, customize templates, override styles, add page attachments, create themes, or use native MAUI APIs directly. |

## Accessibility

Accessibility is a first-class concern in UraniumUI. The library keeps MAUI primitives available while adding focus-aware Material inputs, keyboard-activated custom surfaces, generated semantic hints in selection controls, and guidance for building custom cards, rows, and icon actions without losing accessibility.

Start with the accessibility docs when building these areas: popups, dialogs, bottom sheets, `TabView`, `DataGrid`, date pickers, combo/autocomplete fields, validation messages, masked or formatted inputs, and custom clickable cards.

Learn more: [Accessibility best practices](https://enisn-projects.io/docs/en/uranium/latest/best-practices/Accessibility) and [Clickable areas](https://enisn-projects.io/docs/en/uranium/latest/best-practices/ClickableAreas).

## Quick Start

### Start a New App

Install the templates and create a ready-to-run UraniumUI project:

```bash
dotnet new install UraniumUI.Templates
dotnet new uraniumui-app -n MyMauiApp
```

For a lighter starter project:

```bash
dotnet new uraniumui-blank-app -n MyMauiApp
```

You can also generate a `UraniumContentPage` item:

```bash
dotnet new uraniumcontentpage -n CustomerPage -na MyMauiApp
```

Templates can configure icon packages, dialog integration, and blur support during project creation.

### Add UraniumUI to an Existing App

Install the Material package. It references the core UraniumUI package and configures Material controls and `AutoFormView` editor mappings.

```bash
dotnet add package UraniumUI.Material
```

Register UraniumUI in `MauiProgram.cs`:

```csharp
using UraniumUI;

builder
    .UseMauiApp<App>()
    .UseUraniumUI()
    .UseUraniumUIMaterial();
```

Add Material resources in `App.xaml`:

```xml
<Application xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:material="http://schemas.enisn-projects.io/dotnet/maui/uraniumui/material"
             x:Class="MyMauiApp.App">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary x:Name="appColors" Source="Resources/Styles/Colors.xaml" />
                <ResourceDictionary x:Name="appStyles" Source="Resources/Styles/Styles.xaml" />
                <material:StyleResource ColorsOverride="{x:Reference appColors}" BasedOn="{x:Reference appStyles}" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

Read the full onboarding guide: [Getting Started](https://enisn-projects.io/docs/en/uranium/latest/Getting-Started).

## What You Can Build

UraniumUI is useful when you need to ship complete MAUI screens, not just one-off controls. These are common places where it removes repeated UI plumbing.

### Forms Without Boilerplate

Instead of hand-writing every field, binding, validation message, and layout row, describe the form with your model and let UraniumUI generate the editable UI.

```csharp
using System.ComponentModel.DataAnnotations;

public class RegisterViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string FullName { get; set; } = string.Empty;

    [Range(1, 10)]
    public int NumberOfSeats { get; set; }

    [Display(Name = "I accept the terms and conditions")]
    public bool AcceptedTerms { get; set; }
}
```

```xml
<uranium:AutoFormView Source="{Binding .}" />
```

Enable DataAnnotations validation once:

```bash
dotnet add package UraniumUI.Validations.DataAnnotations
```

```csharp
using UraniumUI.Options;
using UraniumUI.Validations;

builder.Services.Configure<AutoFormViewOptions>(options =>
{
    options.ValidationFactory = DataAnnotationValidation.CreateValidations;
});
```

The same model can now drive generated editors, display names, and validation messages. If a screen needs custom behavior, override the editor mapping, change the generated layout, or replace a generated field with your own MAUI view.

Learn more: [AutoFormView](https://enisn-projects.io/docs/en/uranium/latest/infrastructure/AutoFormView) and [DataAnnotations validation](https://enisn-projects.io/docs/en/uranium/latest/validations/DataAnnotations).

### Data Screens Without Rebuilding Tables

Bind a collection and let `DataGrid` render rows, headers, empty states, selection columns, templates, and auto-generated columns when you want them.

```xml
<material:DataGrid ItemsSource="{Binding Customers}" UseAutoColumns="True" />
```

When the screen needs more control, define explicit `DataGridColumn` items, cell templates, header templates, selection columns, and pair the grid with `Paginator`.

Learn more: [DataGrid](https://enisn-projects.io/docs/en/uranium/latest/themes/material/components/DataGrid) and [Paginator](https://enisn-projects.io/docs/en/uranium/latest/themes/material/components/Paginator).

### Navigation, Hierarchies, And Multi-Section UI

Use higher-level controls for the patterns that show up in real apps: hierarchical data, tabbed content, searchable/selectable lists, calendars, expanders, and dropdowns.

```xml
<material:TreeView ItemsSource="{Binding Nodes}" SelectionMode="Multiple" />

<material:TabView>
    <material:TabItem Title="Overview">
        <material:TabItem.ContentTemplate>
            <DataTemplate>
                <Label Text="Overview content" />
            </DataTemplate>
        </material:TabItem.ContentTemplate>
    </material:TabItem>
</material:TabView>
```

TreeView supports custom item templates, expansion state binding, lazy loading, single/multiple selection, and hierarchical checkbox selection. TabView supports lazy content templates, custom headers, dynamic tabs, placement options, and caching strategies.

Learn more: [TreeView](https://enisn-projects.io/docs/en/uranium/latest/themes/material/components/TreeView), [TabView](https://enisn-projects.io/docs/en/uranium/latest/themes/material/components/TabView), [Select](https://enisn-projects.io/docs/en/uranium/latest/infrastructure/Select), and [CalendarView](https://enisn-projects.io/docs/en/uranium/latest/infrastructure/CalendarView).

### App Surfaces, Dialogs, And Visual Polish

Use `UraniumContentPage` attachments for surfaces that belong to the page instead of manually layering grids and overlays.

```xml
<uranium:UraniumContentPage.Attachments>
    <material:BottomSheetView IsPresented="{Binding IsFiltersOpen}">
        <VerticalStackLayout Padding="24" Spacing="16">
            <Label Text="Filters" FontAttributes="Bold" />
            <material:TextField Title="Search" Text="{Binding SearchText}" />
        </VerticalStackLayout>
    </material:BottomSheetView>
</uranium:UraniumContentPage.Attachments>
```

The same presentation layer includes `BackdropView`, `IDialogService`, form dialogs, prompt dialogs, progress dialogs, optional Mopups and CommunityToolkit providers, Material color/style resources, icon packages, and blur effects.

Learn more: [Bottom Sheet](https://enisn-projects.io/docs/en/uranium/latest/themes/material/components/BottomSheet), [Backdrop](https://enisn-projects.io/docs/en/uranium/latest/themes/material/components/Backdrop), [Dialogs](https://enisn-projects.io/docs/en/uranium/latest/dialogs/Index), and [Blurs](https://enisn-projects.io/docs/en/uranium/latest/Blurs).

## Feature Map

| Area | What you get | Docs |
| --- | --- | --- |
| Forms and validation | `FormView`, `AutoFormView`, generated editors, validation summaries, busy state, async validators, `ValidationPath`, InputKit validation, DataAnnotations integration, and form dialogs. | [AutoFormView](https://enisn-projects.io/docs/en/uranium/latest/infrastructure/AutoFormView), [Validations](https://enisn-projects.io/docs/en/uranium/latest/validations/Index) |
| Core infrastructure | `UraniumContentPage`, page attachments, `StatefulContentView`, `DynamicContentView`, `GridLayout`, MAUI handlers, and primitives for custom interactive controls. | [UraniumContentPage](https://enisn-projects.io/docs/en/uranium/latest/infrastructure/UraniumContentPage), [StatefulContentView](https://enisn-projects.io/docs/en/uranium/latest/infrastructure/StatefulContentView) |
| Accessibility and interaction | Best-practice docs for keyboard navigation, visible focus, semantic descriptions, validation text, and custom clickable areas built with `ButtonView` or `StatefulContentView` instead of bare gestures. | [Accessibility](https://enisn-projects.io/docs/en/uranium/latest/best-practices/Accessibility), [Clickable Areas](https://enisn-projects.io/docs/en/uranium/latest/best-practices/ClickableAreas) |
| Core components | `CalendarView`, `Select`, `Dropdown`, `AutoCompleteView`, `ExpanderView`, and `SelectableLabel`. | [Core Components](https://enisn-projects.io/docs/en/uranium/latest/infrastructure/CalendarView) |
| Material inputs | `InputField`, `TextField`, `EditorField`, `AutoCompleteTextField`, `DropdownField`, `SelectField`, `PickerField`, `MultiplePickerField`, `DatePickerField`, `TimePickerField`, validation display, clear buttons, icons, and floating-label field styling. | [Material Inputs](https://enisn-projects.io/docs/en/uranium/latest/themes/material/components/InputField) |
| Buttons and selection | Material button styles, `ButtonView`, `Chip`, `CheckBox`, `RadioButton`, and `RadioButtonGroupView`. | [Buttons](https://enisn-projects.io/docs/en/uranium/latest/themes/material/Buttons), [Chip](https://enisn-projects.io/docs/en/uranium/latest/themes/material/Chip) |
| Data and navigation | `DataGrid`, `DataGridColumn`, `DataGridSelectionColumn`, `Paginator`, `TreeView`, `TreeViewHierarchicalSelectBehavior`, `TabView`, and `TabItem`. | [DataGrid](https://enisn-projects.io/docs/en/uranium/latest/themes/material/components/DataGrid), [TreeView](https://enisn-projects.io/docs/en/uranium/latest/themes/material/components/TreeView), [TabView](https://enisn-projects.io/docs/en/uranium/latest/themes/material/components/TabView) |
| Surfaces and overlays | `BottomSheetView`, `BackdropView`, `IDialogService`, default modal-page dialogs, Mopups provider, CommunityToolkit provider, confirmation prompts, text/date prompts, progress dialogs, custom view dialogs, and form dialogs. | [Bottom Sheet](https://enisn-projects.io/docs/en/uranium/latest/themes/material/components/BottomSheet), [Backdrop](https://enisn-projects.io/docs/en/uranium/latest/themes/material/components/Backdrop), [Dialogs](https://enisn-projects.io/docs/en/uranium/latest/dialogs/Index) |
| Styling, theming, and effects | Material color resources, style resources, light/dark tokens, cascading styling, custom themes, containers, dividers, elevation, Material Symbols, Font Awesome, Fluent icons, and blur/acrylic effects. | [Color System](https://enisn-projects.io/docs/en/uranium/latest/theming/ColorSystem), [Icons](https://enisn-projects.io/docs/en/uranium/latest/theming/Icons), [Blurs](https://enisn-projects.io/docs/en/uranium/latest/Blurs) |
| Web components | `CodeView` for WebView-backed syntax-highlighted code rendering with bundled highlight.js assets and themes. | [CodeView](https://enisn-projects.io/docs/en/uranium/latest/web-components/CodeView) |
| Templates | Full app template, blank app template, and `UraniumContentPage` item template with optional icon, dialog, and blur setup. | [Getting Started](https://enisn-projects.io/docs/en/uranium/latest/Getting-Started) |

## Package Map

| Package | Purpose |
| --- | --- |
| `UraniumUI` | Core controls, handlers, `FormView`, `AutoFormView`, dialogs, page infrastructure, layouts, and extensibility primitives. |
| `UraniumUI.Material` | Material presentation layer, theme resources, Material controls, app surfaces, data/navigation controls, and Material editor mappings for generated forms. |
| `UraniumUI.Validations.DataAnnotations` | DataAnnotations integration for manual forms and generated editors. |
| `UraniumUI.Dialogs.Mopups` | `IDialogService` implementation backed by Mopups. |
| `UraniumUI.Dialogs.CommunityToolkit` | `IDialogService` implementation backed by .NET MAUI Community Toolkit popups. |
| `UraniumUI.Icons.MaterialSymbols` | Material Symbols icon fonts, font aliases, and glyph helpers for outlined, rounded, sharp, and filled variants. |
| `UraniumUI.Icons.FontAwesome` | Font Awesome Free regular and solid icon fonts with glyph helpers. |
| `UraniumUI.Icons.SegoeFluent` | Segoe Fluent icon font and glyph helpers for Fluent/Windows-aligned apps. |
| `UraniumUI.Icons.MaterialIcons` | Legacy Material Icons package; prefer `UraniumUI.Icons.MaterialSymbols` for new apps. |
| `UraniumUI.Blurs` | Cross-platform blur/acrylic effects and optional blurred dialog surfaces. |
| `UraniumUI.WebComponents` | WebView-backed components such as `CodeView` for syntax-highlighted code rendering. |
| `UraniumUI.Templates` | Project and item templates for new UraniumUI apps and pages. |

## Supported Targets

| Target | Support |
| --- | --- |
| UraniumUI v3.0+ | `.NET 10` |
| .NET 9 | Supported up to UraniumUI `v2.16.0` |
| .NET 8 | Use UraniumUI `v2.6` through `v2.12` |
| .NET 6 and .NET 7 | Use UraniumUI `v2.5` |

Supported MAUI platforms:

- Android
- iOS
- Mac Catalyst
- Windows
- Tizen, with limited support and optional setup

## Demo App

The repository includes a runnable MAUI demo app in [`demo/UraniumApp`](./demo/UraniumApp). It wires the same registration path used by real apps and contains pages for Material inputs, buttons, chips, `DataGrid`, `TreeView`, `TabView`, `BottomSheetView`, `BackdropView`, dialogs, validations, `AutoFormView`, icons, blurs, calendar, dropdown, select, expander, and layout primitives.

## Documentation

- [Getting Started](https://enisn-projects.io/docs/en/uranium/latest/Getting-Started)
- [Accessibility best practices](https://enisn-projects.io/docs/en/uranium/latest/best-practices/Accessibility)
- [Clickable areas](https://enisn-projects.io/docs/en/uranium/latest/best-practices/ClickableAreas)
- [AutoFormView](https://enisn-projects.io/docs/en/uranium/latest/infrastructure/AutoFormView)
- [Validations](https://enisn-projects.io/docs/en/uranium/latest/validations/Index)
- [Material Theme](https://enisn-projects.io/docs/en/uranium/latest/themes/material/Index)
- [DataGrid](https://enisn-projects.io/docs/en/uranium/latest/themes/material/components/DataGrid)
- [TreeView](https://enisn-projects.io/docs/en/uranium/latest/themes/material/components/TreeView)
- [TabView](https://enisn-projects.io/docs/en/uranium/latest/themes/material/components/TabView)
- [Dialogs](https://enisn-projects.io/docs/en/uranium/latest/dialogs/Index)
- [Icons](https://enisn-projects.io/docs/en/uranium/latest/theming/Icons)
- [Blurs](https://enisn-projects.io/docs/en/uranium/latest/Blurs)
- [CodeView](https://enisn-projects.io/docs/en/uranium/latest/web-components/CodeView)

<img src="art/github-social-preview.png" width="100%">

## Contributing

We welcome contributions and suggestions. Please read the [contributing guide](CONTRIBUTING.md).

You may consider checking out issues with the [good first issue](https://github.com/enisn/UraniumUI/issues?q=is%3Aopen+is%3Aissue+label%3A%22good+first+issue%22) label to make your first contribution.

## Roadmap

See the [milestones](https://github.com/enisn/UraniumUI/milestones) section in the repository.

## License

This project is licensed under the Apache License. See the [LICENSE](LICENSE) file for details.

## Backers

| Special thanks to project supporters 🎉 |
| --- |
| [YvanBrunel](https://twitter.com/YvanBrunel) | <!-- 12☕️ --> 
| [Hottemax](https://github.com/Hottemax) |  <!-- 6☕️ -->
| [tjlangenkamp](https://github.com/tjlangenkamp) | <!-- 5☕️ -->
| [C00lzer0](https://github.com/C00lzer0) |  <!-- 3☕️ -->
| Eric | <!-- 3 ☕-->
| Volker Busch | <!-- 3 ☕-->
| [gpproton](https://github.com/gpproton) |  <!-- 1☕️ -->
| [kmaclagan-pcl](https://www.buymeacoffee.com/enisn) |  <!-- 1☕️ -->
| [@Geramy](https://github.com/Geramy) |  <!-- 1☕️ -->
| [Malko_Josue](https://twitter.com/Malko_Josue) |  <!-- 1☕️ -->
| [Nawa](https://github.com/Nawapoln) | <!-- 1☕ -->
| [JohnStabler](https://github.com/JohnStabler) | <!-- GitHub Sponsor -->
| [jfversluis](https://github.com/jfversluis) | <!-- GitHub Sponsor -->
| [Lucasbk123](https://github.com/Lucasbk123) | <!-- GitHub Sponsor -->
| [laszlodaniel](https://github.com/laszlodaniel) | <!-- GitHub Sponsor -->
| [codychaplin](https://github.com/codychaplin) | <!-- GitHub Sponsor -->
| Juliette Dianne Moss | <!-- patreon -->
| Simon Brettschneider |    <!-- 1☕️ -->
| JohnCKoenig | <!-- 1☕ -->
| 7 M O X D | <!-- 5☕ -->
| _Anonymous people 6☕️_ |  <!-- 4☕️ -->

Donations are spent on infrastructure costs such as the documentation website.

## Support

If UraniumUI helps you ship .NET MAUI apps, you can support the project on <a href="https://www.buymeacoffee.com/enisn">BuyMeACoffee</a>.

<br />

<div align="center">
<a href="https://www.buymeacoffee.com/enisn"><img src="https://img.buymeacoffee.com/button-api/?text=Buy me a coffee&emoji=&slug=enisn&button_colour=40DCA5&font_colour=ffffff&font_family=Lato&outline_colour=000000&coffee_colour=FFDD00" /></a>
</div>

***

## Activity

<div align="center">
  <img src="https://repobeats.axiom.co/api/embed/6fc7aa49770ea08ec85ba5ff5b566df0e9b3ac46.svg" alt="Repobeats analytics image" />
</div>
