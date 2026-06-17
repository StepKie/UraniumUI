# AGENTS.md

## Build And Test
- Use the .NET SDK from `global.json`: `10.0.100` with `rollForward` set to `latestFeature`.
- Full CI shape from repo root is: `dotnet workload install maui`, `dotnet build`, then `dotnet test`.
- Local build shortcut from `build.ps1`: `dotnet restore` then `dotnet build --no-restore`.
- Focused library build: `dotnet build src/UraniumUI/UraniumUI.csproj -f net10.0`; choose a platform TFM such as `net10.0-windows10.0.19041.0` only when platform code/resources are involved.
- Focused tests are xUnit `net10.0` projects, for example `dotnet test test/UraniumUI.Tests/UraniumUI.Tests.csproj --filter FullyQualifiedName~CalendarView_Test` or `dotnet test test/UraniumUI.Material.Tests/UraniumUI.Material.Tests.csproj --filter FullyQualifiedName~TextField_Test`.
- Tests create a global MAUI `Application.Current` through `test/UraniumUI.Tests.Core/ApplicationExtensions.cs`; affected test assemblies disable xUnit parallelization, so do not re-enable parallel test execution casually.

## Package Boundaries
- `src/UraniumUI` is core. Add-on packages such as `UraniumUI.Material`, `UraniumUI.Blurs`, `UraniumUI.WebComponents`, and `UraniumUI.Dialogs.*` reference core; avoid making core depend on theme, dialog implementation, icon, blur, or web-component packages.
- Most library packages target `net10.0` plus MAUI platform TFMs. `UraniumUI.Validations.DataAnnotations` targets only `net10.0` but still has `UseMaui=true`.
- Public MAUI registration entrypoints live in package-level extension files: `UseUraniumUI`, `UseUraniumUIMaterial`, `UseUraniumUIBlurs`, and `UseUraniumUIWebComponents`. The demo app in `demo/UraniumApp/MauiProgram.cs` shows the full wiring.

## XAML And API Gotchas
- New public XAML controls/resources need `XmlnsDefinition` updates in the owning package `AssemblyInfo.cs`. Core uses prefix `uranium` at `http://schemas.enisn-projects.io/dotnet/maui/uraniumui`; Material uses prefix `material` at the `/material` namespace.
- `AutoFormView` default editor mappings are configured in `src/UraniumUI/Options/AutoFormViewOptionsExtensions.cs`; Material replaces those mappings in `src/UraniumUI.Material/Extensions/AutoFormViewMaterialConfigurationExtensions.cs`.
- Icon packages embed font files and expose font registration methods (`AddMaterialSymbolsFonts`, `AddFontAwesomeIconFonts`, `AddFluentIconFonts`); update the extension method and embedded resources together.

## Accessibility Requirements
- Treat accessibility as part of the component contract, not a follow-up polish item. New or changed interactive controls must be reachable by keyboard unless they are explicitly decorative.
- For custom controls, verify tab focus, visible focus state, Enter/Space activation, Escape dismissal for popups/overlays, arrow-key navigation for list/menu/select patterns, disabled-state behavior, and screen-reader descriptions/hints.
- Prefer existing focusable primitives such as `StatefulContentView` and its handlers before adding new platform handlers. If a new handler is needed, keep focus and key behavior consistent across supported platforms.
- Add focused tests for MAUI-level accessibility behavior where possible, and document keyboard interaction in the component docs.

## Docs And Templates
- DocFX config is `docs/docfx.json`; it generates API metadata from `src/**/*.csproj` into root `api/` and site output into `_site/`. Treat `api/*.yml` as generated output, not source prose.
- Docs navigation exists in both `docs/en/toc.yml` and `docs/en/docs-nav.json`; update both when adding, moving, or removing docs pages.
- Template short names/options are defined in `templates/*/.template.config/template.json` (`uraniumui-app`, `uraniumui-blank-app`, `uraniumcontentpage`). When changing template behavior, update the matching template `MauiProgram.cs`/csproj and docs examples together.
- Local template smoke test: from `templates/`, run `./install.ps1`, then create a temp app with `dotnet new uraniumui-app` or `dotnet new uraniumui-blank-app`.

## Packaging
- Shared package metadata/version for libraries is in `common.props`; MAUI package versions are selected in `maui.common.props` by target framework.
- Root `pack.ps1` packs libraries, then changes into `templates/` and packs templates; it removes `*.nupkg` files at the end, so use a separate `dotnet pack -o <dir>` command if you need to inspect artifacts.
