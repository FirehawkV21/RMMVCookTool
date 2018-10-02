# RMMVCookTool

## What is this tool?
This tool is a GUI wrapper (and a standalone console app) for NW.js' Compiler tool (included in the SDK of the program). It allows RPG Maker MV game developers to protect the game's source code and plugins from being stolen by compiling the files to their binary form. The tool automates the generation of the binaries. This does sacrifice the cross-platform capabilites, however, and the binaries work only on the version of the SDK you used.

## The Components

### CompilerCore
This houses the common code between the GUI and the Console App.

### nwjsCookToolUI
The GUI part of the compiler tool.

### nwjsCompilerCLI
The standalone, cross-platform console app. Used for compiling a project quickly.

## System Requirements

- Windows 7 Service Pack 1 and newer.
- Microsoft .NET Framework 4.7.2 (.NET Core 2.1 or newer for the standalone console app)
- NW.js SDK. Any version you want.

## Compiling

You will need Visual Studio 2017, along with the Microsoft .NET Framework 4.7.2 SDK (.NET Core SDK 2.1 or newer as well, if you want to compile the standalone console app). Once you do have both, open the solution file.

## Libraries Used
- [Xceed WPF Toolkit](https://github.com/xceedsoftware/wpftoolkit)
- [Ookii.Dialogs](http://http://www.ookii.org/Software/Dialogs/)
