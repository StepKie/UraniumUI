# Blurs
UraniumUI supports blur effects on MAUI. You can use it on any control by using `BlurEffect`.

Apple has its own blur effect and Windows has a Brush for it. So it's implemented natively for those platforms.
Android blur is best-effort because Android doesn't provide the same arbitrary backdrop blur primitive as iOS `UIVisualEffectView` or WinUI `AcrylicBrush`. By default, Android uses a lightweight material-style tinted strategy and keeps the capture-based [Dimezis/BlurView](https://github.com/Dimezis/BlurView) pipeline as an explicit opt-in for backdrop blur.



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
    | `Default` | Uses the material-style tinted surface. This avoids blurring the effect host content and avoids backdrop capture. |
    | `RenderEffect` | Uses Android's native `RenderEffect` for the effect host layer on Android 12/API 31+. This blurs the host view's own rendered layer, including its content and children, not the backdrop behind it. Falls back to `Material` below API 31. |
    | `Material` | Uses only the configured tint color and opacity. This is the cheapest option and does not capture the backdrop. |
    | `RealtimeCapture` | Uses capture-based backdrop blur. It is throttled by `AndroidRealtimeCaptureFps`, but it still redraws the backdrop into a bitmap repeatedly. |
    | `StaticCapture` | Captures and blurs once, then refreshes only after size/root changes or `InvalidateAndroidBlur()`. This is useful for static backdrops. |

    ```xml
    <StackLayout>
        <StackLayout.Effects>
            <uranium:BlurEffect AndroidStrategy="RealtimeCapture" />
        </StackLayout.Effects>
        <!-- Your content goes here -->
    </StackLayout>
    ```

- AndroidRealtimeCaptureFps: `int` (Default: `15`) - Defines the maximum capture update rate for `RealtimeCapture`. Values are clamped between `1` and `60`.

- AndroidCaptureDownsampleFactor: `float` (Default: `8`) - Defines how much the Android capture bitmap is downsampled before blur. Larger values improve performance but reduce blur quality. Values are clamped between `1` and `32`.

## Android limitations

Android `RenderEffect` blurs the layer owned by the effect host. It is lightweight, but it is not equivalent to arbitrary backdrop blur behind the control. If it is applied to a container, the container content is blurred too.

Do not use `RenderEffect` as a dialog, `ContentPage`, or `TabbedPage` backdrop blur replacement. It targets the native Android view generated for the MAUI element, so it cannot blur the page behind that view the same way iOS `UIVisualEffectView` does.

Use `AndroidStrategy="RealtimeCapture"` only when you explicitly need backdrop blur behind an in-page surface. Prefer small, mostly static surfaces for this strategy because scrolling, repeated dialog open/close, and navigation loops can allocate and redraw bitmaps frequently.

Use `AndroidStrategy="StaticCapture"` when the backdrop behind the blur surface does not move. You can request a refresh from code when the backdrop changes:

```csharp
blurEffect.InvalidateAndroidBlur();
```
