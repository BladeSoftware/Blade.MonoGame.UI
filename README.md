# Blade.MonoGame.UI

A retained-mode, WPF/Avalonia-inspired UI library for [MonoGame](https://www.monogame.net/), with templateable controls, Material-Design-flavoured theming, data binding, and full mouse/keyboard/touch/gamepad input support.

[![License: MPL 2.0](https://img.shields.io/badge/License-MPL%202.0-brightgreen.svg)](LICENSE)
[![NuGet](https://img.shields.io/nuget/v/BladeSoftware.MonoGame.UI.svg)](https://www.nuget.org/packages/BladeSoftware.MonoGame.UI/)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)

## Overview

Blade.MG.UI brings a modern, declarative UI toolkit to MonoGame games and tools. Instead of hand-rolling immediate-mode widgets every frame, you compose a tree of `Control`/`Container` objects - buttons, panels, grids, lists, dialogs, and more - once, and the library handles layout, hit-testing, focus, theming, and rendering for you every frame.

## Features

- **Rich control set** - buttons, labels, checkboxes, combo boxes, text boxes, list views, tree views, menus, tab panels, scroll panels, dialogs, app bars, property editors, and more.
- **WPF-style layout engine** - a measure/arrange pass powering `Grid` (star/auto/pixel sizing), `StackPanel`, `DockPanel`, `Border`, margins/padding, min/max sizes, and alignment.
- **Flexible sizing** - set `Width`/`Height` in pixels (`200`), percent (`"50%"`), viewport units, or star sizing inside a `Grid`.
- **Templated controls** - every `TemplatedControl` (`Button`, `CheckBox`, `ComboBox`, `TextBox`, list items, tree nodes, tab headers, menu items...) can be re-skinned by supplying your own template class.
- **Theming** - a Material-Design-3-flavoured `UITheme` with built-in Light/Dark themes; swap themes at runtime with `UIManager.SetTheme()` and every open window refreshes immediately.
- **Data binding** - `Binding<T>` and `ObservableBinding<T>` give you reactive, gettable/settable properties on almost every control, including binding to `INotifyPropertyChanged` view models.
- **Full input pipeline** - mouse, keyboard, touch, and gamepad input, with focus/tab navigation, hover tracking, drag support, and hit-testing built in.
- **Rich eventing** - sync and async (awaitable) events for click, double/right-click, mouse down/up, wheel, tap, long-press, focus, and hover changes - async handlers can safely `await` modal dialogs without blocking the game loop.
- **Modal dialogs & menus** - `ModalBase`/`MessageDialog`/`Menu` support an `await dialog.ShowAsync(game)` pattern that suspends only the calling code while the game keeps rendering and updating.
- **Control caching** - optional render-to-texture caching (`ICacheable`, `ControlCache`) for expensive or static control subtrees, to reduce per-frame draw cost.
- **Custom fonts** - built on [FontStashSharp](https://github.com/FontStashSharp/FontStashSharp) with a bundled default font, plus a `FontService` for registering additional named TTF fonts at any size.
- **Debug tooling** - drop-in `FpsUI` and `UIHierarchyOverlay` windows, plus `UIManager.RenderControlHitBoxes` for visualising layout bounds.

## Installation

Blade.MG.UI targets **.NET 10** and MonoGame's `DesktopGL` platform (other MonoGame platforms are supported too - see `Examples.Android` in this repo).

```powershell
dotnet add package BladeSoftware.MonoGame.UI
```

Or via the Package Manager Console:

```powershell
Install-Package BladeSoftware.MonoGame.UI
```

## Quick Start

### 1. Create the `UIManager` in your `Game`

```csharp
public class MyGame : Game
{
    private UIManager uiManager;

    protected override void Initialize()
    {
        Services.AddService(typeof(UIManager), uiManager = new UIManager(this));

        base.Initialize();
    }

    protected override void LoadContent()
    {
        base.LoadContent();

        uiManager.Add(new MainMenuWindow());
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        uiManager.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        base.Draw(gameTime);

        uiManager.Draw(null, gameTime, null);
    }
}
```

### 2. Build a window out of controls

```csharp
using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;

public class MainMenuWindow : UIWindow
{
    public override void LoadContent()
    {
        base.LoadContent();

        var layout = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignmentType.Center,
            VerticalAlignment = VerticalAlignmentType.Center,
            Width = "300px",
        };

        AddChild(layout);

        layout.AddChild(new Label { Text = "Welcome!", FontSize = 32 });

        layout.AddChild(new Button
        {
            Text = "Start Game",
            Height = 48,
            Margin = new Thickness(0, 20, 0, 0),
            OnPrimaryClick = (sender, e) => StartGame(),
        });
    }

    private void StartGame()
    {
        // ...
    }
}
```

### 3. Lay out a `Grid` with pixel/star columns

```csharp
var grid = new Grid();

grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Pixel, 200) });
grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1) });

grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Auto) });
grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1) });

grid.AddChild(new Label { Text = "Sidebar" }, column: 0, row: 0);
grid.AddChild(new Panel { Background = Color.White }, column: 1, row: 0);
```

## Core Concepts

| Concept | Description |
|---|---|
| `UIManager` | Owns every `UIWindow`, dispatches input, drives layout, and renders each frame. Registered once per `Game`, typically via `Game.Services`. |
| `UIWindow` | The root of a UI tree - a full-screen HUD, a menu screen, or a popup/modal. |
| `UIComponent` | Base class for everything in the tree; carries layout properties (`Width`, `Height`, `Margin`, `Padding`, alignment, `Visibility`, `Transform`...). |
| `Control` / `Container` | `Control` hosts a single `Content` child (like a `ContentControl`); `Container` hosts a `Children` collection. |
| `TemplatedControl` | Controls whose visual tree is generated from a swappable `TemplateType` (e.g. `ButtonTemplate`), enabling full re-skinning without subclassing the control. |
| `Binding<T>` | A gettable/settable reactive value used for most control properties, so UI state can be driven from - or pushed back to - your game/view-model code. |
| `UITheme` | A Material-Design-3-style palette (`Primary`/`Secondary`/`Tertiary`/`Surface`/`Error`/etc.) resolved by controls at render time. |

## Controls

| Category | Controls |
|---|---|
| Layout | `Panel`, `Grid`, `StackPanel`, `DockPanel`, `Border`, `ScrollPanel`, `TabPanel`, `ExpansionPanel`, `SplitterBar` |
| Input | `Button`, `IconButton`, `CheckBox`, `ComboBox`, `TextBox`, `ScrollBar` |
| Data & navigation | `ListView`, `TreeView`, `Menu`/`MenuItem`, `PropertyEditor` |
| Chrome & dialogs | `AppBar`, `ModalBase`, `MessageDialog` |
| Content | `Label`, `Image`, `Rectangle` |

Every `TemplatedControl` above ships with a default template under `Controls/Templates/` that can be replaced to completely restyle the control while keeping its behaviour.

## Theming

```csharp
// Set the app-wide theme at startup, before any windows are created
UIManager.DefaultTheme = DefaultThemes.DarkTheme();

// ...or swap it live at any time - every open window refreshes immediately
UIManager.SetTheme(DefaultThemes.LightTheme());

UIManager.ThemeChanged += theme => Debug.WriteLine($"Theme changed to {theme.Name}");
```

Themes expose Material-Design-3-style color roles (`Primary`, `OnPrimary`, `Secondary`, `Surface`, `Background`, `Error`, `Warning`, `Success`, `Info`, `Disabled`, ...) that controls consume automatically; per-control style overrides always take precedence over the active theme.

## Project Structure

This repository contains three solutions:

| Solution | Description |
|---|---|
| `Source/Blade.MG.UI.sln` | The library itself (`Blade.MG.UI.csproj`) - the only project referenced by the published NuGet package. |
| `Source/Blade.MG.UI.Examples.sln` | A showcase app with a help page per control, plus DesktopGL and Android platform heads. |
| `Source/Blade.MG.UI.Tests.sln` | MSTest unit tests (`BladeUI.UnitTesting`) plus sample game projects used for manual/visual testing. |

## Building from Source

Blade.MG.UI references the sibling [Blade.MonoGame.Core](https://github.com/BladeSoftware/Blade.MonoGame.Core) repository via a relative project reference, so clone it alongside this repository:

```powershell
git clone https://github.com/BladeSoftware/Blade.MonoGame.Core
git clone https://github.com/BladeSoftware/Blade.MonoGame.UI
```

Then open `Source/Blade.MG.UI.sln` (or `.Examples.sln` / `.Tests.sln`) in Visual Studio, or build from the CLI:

```powershell
dotnet build Source/Blade.MG.UI.sln
```

## Dependencies

- [MonoGame.Framework.DesktopGL](https://www.monogame.net/)
- [FontStashSharp.MonoGame](https://github.com/FontStashSharp/FontStashSharp) - text layout/rendering
- [Microsoft.VisualStudio.Threading](https://github.com/microsoft/vs-threading) - async coordination for modal dialogs
- [Blade.MonoGame.Core](https://github.com/BladeSoftware/Blade.MonoGame.Core) - shared primitives and input helpers

## Contributing

Issues and pull requests are welcome. Please open an issue to discuss significant changes before submitting a PR.

## License

Licensed under the [Mozilla Public License 2.0](LICENSE).
