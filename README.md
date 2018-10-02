# RMMVCookTool

## What is this tool?
This tool is a GUI wrapper for NW.js' Compiler tool (included in the SDK of the program). It allows RPG Maker MV game developers to protect the game's source code and plugins from being stolen by compiling the files to their binary form. The tool automates the generation of the binaries. This does sacrifice the cross-platform capabilites, however, and the binaries work only on the version of the SDK you used.

## The Components

### CompilerCore
This houses the common code between the GUI and the Console App.

### nwjsCookToolUI
The GUI part of the compiler tool.

### nwjsCompilerCLI
The cross-platform console app. Used for compiling a project quickly.

## System Requirements

- Windows 7 Service Pack 1 and newer.
- Microsoft .NET Framework 4.7.2
- NW.js SDK. Any version you want.

## Compiling

You will need Visual Studio 2017, along with the Microsoft .NET Framework 4.7.2 SDK. Once you do have both, open the solution file.

## Libraries Used
- [Xceed WPF Toolkit](https://github.com/xceedsoftware/wpftoolkit)
- [Ookii.Dialogs](https://github.com/acemod13/RMMVCookTool/releases/tag/1.2.0-20180925)
