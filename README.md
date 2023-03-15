# RPG Maker MV/MZ Cook Tool
[![Crowdin](https://badges.crowdin.net/rpg-maker-mv-cook-tool/localized.svg)](https://crowdin.com/project/rpg-maker-mv-cook-tool)

## Intro
This tool is a GUI wrapper (and a standalone console app) for NW.js' Compiler tool (included in the SDK of the program). It allows RPG Maker MV and MZ game developers to protect the game's source code and plugins from being stolen by compiling the files to their binary form. The tool automates the generation of the binaries. This does sacrifice the cross-platform capabilities, however, and the binaries work only on the version of the SDK you used.

## The Components

### RMMVCookTool.Core
This houses the common code between the GUI and the Console App.

### RMMVCookTool.UI
The GUI part of the compiler tool.

### RMMVCookTool.CLI
The standalone, cross-platform console app. Used for compiling a project quickly.

## System Requirements
-  Windows 10 (Version 1803) and newer.
-  Microsoft .NET 7.
-  NW.js SDK. Any version you want.

## Compiling

You will need Visual Studio 2022, along with the Microsoft .NET 7 SDK. Once you do have both, open the solution file. Alternatively, run the following in a terminal (assuming that you have the .NET 7 SDK installed):

### Cleaning
```powershell
dotnet cake --rebuild #Include -rebuild to clean the solution before compiling.
```
### Building:
```powershell
dotnet cake --buildUi # Builds the UI
dotnet cake --buildCli # Builds the CLI
```

### Publishing
```powershell
dotnet cake --publishUi # Publishes a standalone version of the UI.
dotnet cake --publishCli # Publishes a native version of the CLI.
dotnet cake --publishUiOnArm # Publishes the Windows On Arm version of the UI.
dotnet cake --publishCliOnArm # Publishes the native Arm version of the CLI (requires the Arm64 C++ compiler).
```
Note: This will use Cake build to compile the project. Run `dotnet tool restore` in the terminal before running the above commands.

See this documentation to set up the C++ compiler needed for NativeAOT (CLI for now): https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/

## Libraries Used
-  [Dirkster.NumericUpDownLib](https://github.com/Dirkster99/NumericUpDownLib)
-  [Ookii.Dialogs](https://github.com/ookii-dialogs/ookii-dialogs-wpf)
-  [Spectre.Console](https://spectreconsole.net/)
-  [Serilog](https://serilog.net/)
-  [Prism](https://prismlibrary.com)
