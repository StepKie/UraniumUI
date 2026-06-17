# Material Theme
This is an implementation of the Material Theme for the Uranium UI. Visit [Material Design Guideline](https://m3.material.io/) for more information.

Material controls include focus-aware input fields, button states, selection controls, and custom surfaces that can be used in keyboard-heavy apps. When building custom cards, tab headers, data-grid cells, or overlay actions, follow the [Accessibility](../../best-practices/Accessibility.md) and [Clickable Areas](../../best-practices/ClickableAreas.md) best practices.


## Installation

- Install the [UraniumUI.Material](https://www.nuget.org/packages/UraniumUI.Material/) NuGet package.

- Go to **MauiProgram.cs** and call `UseUraniumUIMaterial()` method in builder chain.

    ```csharp
    builder
        .UseMauiApp<App>()
        .UseUraniumUI()
        .UseUraniumUIMaterial() // 👈 This line should be added.
        .ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        });
    ```

> _Order of calling `UseUraniumUI()` and `UseUraniumUIMaterial()` is not important. But both should be called._
