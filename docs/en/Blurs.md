# Blurs
UraniumUI supports blur effects on MAUI. You can use it on any control by using `BlurEffect`.

Apple has its own blur effect and Windows has a Brush for it. So it's implemented natively for those platforms.
Android blur is best-effort because Android doesn't provide the same arbitrary backdrop blur primitive as iOS `UIVisualEffectView` or WinUI `AcrylicBrush`. By default, Android uses a lightweight native/material strategy and keeps the capture-based [Dimezis/BlurView](https://github.com/Dimezis/BlurView) pipeline as an explicit opt-in for realtime backdrop blur.



## Showcase

![Blurs](images/blurs-demo-dark-light.gif)

| Windows | iOS | Android |
| --- | --- | --- | 
| <img src="images/blurs-demo-scrolling-windows.gif" alt="MAUI Acrylic Blur" height="480" /> | <img src="images/blurs-demo-scrolling-ios.png" alt="MAUI Acrylic Blur" height="480" /> | <img src="images/blurs-demo-scrolling-android.png" alt="MAUI Acrylic Blur" height="480" /> |

## Getting Started

### Setting-up
Blur effect isn't part of UraniumUI by default. It's included in a separated assembly which is `UraniumUI.Blurs`. You have to add that package to your application first.

- Install [UraniumUI.Blurs](https://www.nuget.org/packages/UraniumUI.Blurs) package to your project.
    ```bash
    dotnet add package UraniumUI.Blurs
    ```

- After installing that assembly, you should add `UseUraniumUIBlurs()` method to your application builder in **Program.cs**.
    ```csharp
    builder
        .UseMauiApp<App>()
        .UseUraniumUI()
        .UseUraniumUIBlurs() // 👈 Here it is
        .ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        })
        //...
    ```

### Usage

BlurEffect is defined in `UraniumUI.Blurs` namespace. But its assembly exports that namespace by default. So you can use same xml namespace with UraniumUI.

```xml
xmlns:uranium="http://schemas.enisn-projects.io/dotnet/maui/uraniumui"
```

BlurEffect is a `Effect` which means you can use it on any control. 

> **On Android**, it's recommended to use it on a `Layout` such as `StackLayout`, `Grid`, `AbsoluteLayout`, `FlexLayout`, etc. to avoid overlapping issues.


```xml
<StackLayout>
    <StackLayout.Effects>
        <uranium:BlurEffect />
    </StackLayout.Effects>
    <!-- Your content goes here -->
</StackLayout>
```

![MAUI Blur Effect](images/blurs-example-simple-light.png)

## Properties

- Mode: `BlurMode` - Defines the blur mode. It can be `Light` or `Dark`. By default it follows the current application theme.


    ```xml
    <StackLayout>
        <StackLayout.Effects>
            <uranium:BlurEffect Mode="Dark" />
        </StackLayout.Effects>
        <!-- Your content goes here -->
    </StackLayout>
    ```

    ![MAUI Blur Effect Dark](images/blurs-example-simple-dark.png)

- AccentColor: `Color` - Defines the tint color of the blur effect.


    ```xml
    <StackLayout>
        <StackLayout.Effects>
            <uranium:BlurEffect AccentColor="Purple"/>
        </StackLayout.Effects>
        <!-- Your content goes here -->
    </StackLayout>
    ```

    ![MAUI Blur Effect Accent](images/blurs-example-accent-light-purple.png)

- AccentOpacity: `float` (Default: `0.2`) - Defines the opacity of the tint color.

    ```xml
    <StackLayout>
        <StackLayout.Effects>
            <uranium:BlurEffect Mode="Dark" AccentOpacity="0.8" />
        </StackLayout.Effects>
        <!-- Your content goes here -->
    </StackLayout>
    ```
    
    ![MAUI Blur Effect Accent Opacity](images/blurs-example-accent-dark-opacity.png)

- AndroidStrategy: `AndroidBlurStrategy` (Default: `Default`) - Defines which Android blur strategy is used.

    | Value | Behavior |
    | --- | --- |
    | `Default` | Uses `RenderEffect` on Android 12/API 31+ and a material-style tinted surface on older Android versions. |
    | `RenderEffect` | Uses Android's native `RenderEffect` for the effect host layer on Android 12/API 31+. Falls back to `Material` below API 31. |
    | `Material` | Uses only the configured tint color and opacity. This is the cheapest option and does not capture the backdrop. |
    | `RealtimeCapture` | Uses the previous capture-based backdrop blur pipeline. This can be expensive because it redraws the backdrop into a bitmap and blurs it repeatedly. |

    ```xml
    <StackLayout>
        <StackLayout.Effects>
            <uranium:BlurEffect AndroidStrategy="RealtimeCapture" />
        </StackLayout.Effects>
        <!-- Your content goes here -->
    </StackLayout>
    ```

## Android limitations

Android `RenderEffect` blurs the layer owned by the effect host. It is lightweight, but it is not equivalent to arbitrary backdrop blur behind the control.

Use `AndroidStrategy="RealtimeCapture"` only when you explicitly need backdrop blur behind an in-page surface. Prefer small, mostly static surfaces for this strategy because scrolling, repeated dialog open/close, and navigation loops can allocate and redraw bitmaps frequently.
