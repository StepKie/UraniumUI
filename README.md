<div align="center">
    <img align="center" src="./art/logo.svg" width="33%">
    <h1 align="center">UraniumUI</h1>
    <p><strong>The presentation framework for .NET MAUI.</strong></p>
    <p>Fill the UI gaps in plain MAUI without adopting a second app model, proprietary layer, or special coding style.</p>
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
    <a href="https://discord.gg/nN7Yvch73v">Discord</a>
</p>

UraniumUI is a free and open-source presentation framework for .NET MAUI. It fills the UI gaps plain MAUI leaves to every team: polished controls, validation-aware forms, dialogs, icons, and app-ready presentation patterns without moving your app into a proprietary black box.

You keep writing regular .NET MAUI: XAML, `ContentPage`, bindings, styles, resources, handlers, dependency injection, MVVM, and platform APIs. UraniumUI attaches to that model instead of replacing it.

It is not just a collection of styled controls. UraniumUI provides the building blocks for real app screens: generated forms, validation mapping, dialog abstractions, theme resources, icon packs, state-aware views, layout primitives, and extensibility points for your own controls.

## No Framework Tax

Some UI stacks ask you to move your app into their way of building software. UraniumUI does not.

There is no required base ViewModel, proprietary navigation model, custom application layer, generated project structure, or new UI DSL. Adopt one control, a validation-aware field set, or a full Material presentation layer. Your app remains a MAUI app.

Plain MAUI in, better UI out.

## The Mental Model

.NET MAUI gives you the platform foundation. UraniumUI adds the presentation architecture that most production apps end up rebuilding.

| App need | UraniumUI provides |
| --- | --- |
| Build forms quickly | `AutoFormView` generates editors from your model and lets you override editor mappings when needed. |
| Validate forms consistently | InputKit validation, DataAnnotations integration, async validation, and automatic validation path mapping for generated fields. |
| Keep UI native and flexible | Controls and handlers built on MAUI primitives instead of a closed, all-or-nothing rendering stack. |
| Standardize app presentation | Material theme, color resources, style resources, cascading styling, icons, dialogs, and reusable page infrastructure. |
| Escape the defaults | Replace generated editors, customize layouts, add page attachments, create themes, or use native MAUI APIs directly. |

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

## Aha Moment: Dynamic Forms

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

## Feature Map

| Area | What you get | Docs |
| --- | --- | --- |
| Dynamic forms | Model-driven forms, generated editors, custom editor mapping, property name mapping, layout customization, form dialogs. | [AutoFormView](https://enisn-projects.io/docs/en/uranium/latest/infrastructure/AutoFormView) |
| Validation | InputKit validation, DataAnnotations package, async form validation, generated-field validation path mapping. | [Validations](https://enisn-projects.io/docs/en/uranium/latest/validations/Index) |
| Material presentation | Text fields, picker fields, buttons, chips, checkboxes, radio buttons, containers, elevation, DataGrid, TreeView, TabView, BottomSheet, Backdrop. | [Material Theme](https://enisn-projects.io/docs/en/uranium/latest/themes/material/Index) |
| Dialogs | `IDialogService`, default modal-page dialogs, Mopups integration, CommunityToolkit integration, prompt, confirmation, progress, custom view, and form dialogs. | [Dialogs](https://enisn-projects.io/docs/en/uranium/latest/dialogs/Index) |
| Page architecture | `UraniumContentPage`, page attachments, `StatefulContentView`, `DynamicContentView`, `GridLayout`, `ExpanderView`, `Dropdown`, `CalendarView`. | [Core](https://enisn-projects.io/docs/en/uranium/latest/infrastructure/UraniumContentPage) |
| Theming | Color system, style resources, cascading styling, custom themes, icon packs, light and dark mode support. | [Color System](https://enisn-projects.io/docs/en/uranium/latest/theming/ColorSystem) |
| Effects and web components | Blur effects and WebView-based code rendering. | [Blurs](https://enisn-projects.io/docs/en/uranium/latest/Blurs), [CodeView](https://enisn-projects.io/docs/en/uranium/latest/web-components/CodeView) |

## Package Map

| Package | Purpose |
| --- | --- |
| `UraniumUI` | Core controls, handlers, dialogs, layouts, `AutoFormView`, and extensibility infrastructure. |
| `UraniumUI.Material` | Material presentation layer and Material editor mappings for generated forms. |
| `UraniumUI.Validations.DataAnnotations` | DataAnnotations integration for forms and generated editors. |
| `UraniumUI.Dialogs.Mopups` | Dialog implementation backed by Mopups. |
| `UraniumUI.Dialogs.CommunityToolkit` | Dialog implementation backed by .NET MAUI Community Toolkit. |
| `UraniumUI.Icons.MaterialSymbols` | Material Symbols icon fonts and glyph helpers. |
| `UraniumUI.Icons.FontAwesome` | Font Awesome icon fonts and glyph helpers. |
| `UraniumUI.Icons.SegoeFluent` | Segoe Fluent icon fonts and glyph helpers. |
| `UraniumUI.Blurs` | Cross-platform blur effects. |
| `UraniumUI.WebComponents` | WebView-based components such as `CodeView`. |
| `UraniumUI.Templates` | Project and item templates for new UraniumUI apps and pages. |

## Supported Targets

| Target | Support |
| --- | --- |
| Current version | `.NET 10` LTS |
| .NET 8 | Use UraniumUI `v2.6` through `v2.12` |
| .NET 6 and .NET 7 | Use UraniumUI `v2.5` |

Supported MAUI platforms:

- Android
- iOS
- Mac Catalyst
- Windows
- Tizen, with limited support and optional setup

## Documentation

- [Getting Started](https://enisn-projects.io/docs/en/uranium/latest/Getting-Started)
- [AutoFormView](https://enisn-projects.io/docs/en/uranium/latest/infrastructure/AutoFormView)
- [Validations](https://enisn-projects.io/docs/en/uranium/latest/validations/Index)
- [Material Theme](https://enisn-projects.io/docs/en/uranium/latest/themes/material/Index)
- [Dialogs](https://enisn-projects.io/docs/en/uranium/latest/dialogs/Index)
- [Icons](https://enisn-projects.io/docs/en/uranium/latest/theming/Icons)
- [Blurs](https://enisn-projects.io/docs/en/uranium/latest/Blurs)

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
