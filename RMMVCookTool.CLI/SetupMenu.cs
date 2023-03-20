using RMMVCookTool.CLI.Properties;
using RMMVCookTool.Core.Compiler;
using RMMVCookTool.Core.Utilities;
using Spectre.Console;
using System;
using System.IO;

namespace RMMVCookTool.CLI;
public sealed class SetupMenu
{
    public bool TestProject { get; set; }
    public int CheckDeletion { get; set; } = 1;
    public int CompressProject { get; set; } = 3;
    public string SdkLocation { get; set; }
    public bool SettingsSet { get; set; }
    public void CheckSettings(in CompilerProject newProject)
    {
        //Check if both the _projectLocation and _sdkLocation variables are not null.
        if (newProject.ProjectLocation != null && SdkLocation != null)
        {
            CompilerUtilities.RecordToLog("Settings set. Starting the job...", 0);
            SettingsSet = true;
        }
        else if (newProject.ProjectLocation == null && SdkLocation != null)
        {
            CompilerUtilities.RecordToLog("Project location not set. Aborting job.", 2);
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(Resources.ProjectNotSetErrorText);
            Console.ResetColor();
            Console.WriteLine(Resources.PushEnterToExitText);
            Console.ReadLine();
            Environment.Exit(1);
        }
        else if (SdkLocation == null && newProject.ProjectLocation != null)
        {
            CompilerUtilities.RecordToLog("NW.js compiler location not set. Aborting job.", 2);
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(Resources.SDKLocationNotSetErrorText);
            Console.ResetColor();
            Console.WriteLine(Resources.PushEnterToExitText);
            Console.ReadLine();
            Environment.Exit(1);
        }
        Console.WriteLine("");
    }

    public void SetupWorkload(in CompilerProject newProject)
    {
        Rule setupTab = new()
        {
            Title = Resources.SetupTitle
        };
        setupTab.LeftJustified();
        AnsiConsole.Write(setupTab);
        do
        {
            //Ask the user where is the SDK. Check if the folder's there.
            SdkLocation = AnsiConsole.Prompt(new TextPrompt<string>(Resources.SDKLocationQuestion));
            if (SdkLocation == null) Console.WriteLine(Resources.SDKLocationIsNullText);
            else if (!Directory.Exists(SdkLocation)) Console.Write(Resources.SDKDirectoryMissing);
        } while (SdkLocation == null || !Directory.Exists(SdkLocation));
        do
        {
            //Ask the user what project to compile. Check if the folder is there and there's a js folder.
            newProject.ProjectLocation = AnsiConsole.Prompt(new TextPrompt<string>(Resources.ProjectLocationQuestion));
            if (newProject.ProjectLocation == null) Console.WriteLine(Resources.ProjectLocationIsNullText);
            else if (!Directory.Exists(newProject.ProjectLocation))
                Console.WriteLine(Resources.ProjetDirectoryMissingErrorText);
            else if (!Directory.Exists(Path.Combine(newProject.ProjectLocation, "www", "js")))
                Console.WriteLine(Resources.ProjectJsFolderMissing);
            else if (!File.Exists(Path.Combine(newProject.ProjectLocation, "package.json")))
                Console.WriteLine(Resources.JsonFileMissingErrorText);
        } while (newProject.ProjectLocation == null || !Directory.Exists(newProject.ProjectLocation) ||
                 !Directory.Exists(Path.Combine(newProject.ProjectLocation, "www", "js")));

        //Ask the user for the file extension.
        newProject.Setup.FileExtension = AnsiConsole.Prompt(new TextPrompt<string>(Resources.FileExtensionQuestion).DefaultValue("bin").AllowEmpty());
        //This is the check if the tool should delete the JS files.
        CheckDeletion = AnsiConsole.Prompt(new TextPrompt<int>(Resources.WorkloadQuestion)
            .DefaultValue(1)
            .Validate(choice => choice switch
            {
                > 2 => ValidationResult.Error(),
                < 1 => ValidationResult.Error(),
                _ => ValidationResult.Success()
            }));
        newProject.Setup.RemoveSourceFiles = CheckDeletion == 2;

        if (CheckDeletion == 2)
        {
            CompressProject = AnsiConsole.Prompt(new TextPrompt<int>(Resources.CompressionQuestion)
            .DefaultValue(3)
            .Validate(choice => choice switch
            {
                > 3 => ValidationResult.Error(),
                < 1 => ValidationResult.Error(),
                _ => ValidationResult.Success()
            }));
        }
        else
        {
            //Ask if the user would like to test with nwjs.
            if (AnsiConsole.Confirm(Resources.TestProjectQuestion)) TestProject = true;
        }
        CompilerUtilities.RecordToLog($"Current setup of the job:\nCompiler Location:{SdkLocation}\nProject Location:{newProject.ProjectLocation}\nFile Extension:{newProject.Setup.FileExtension}\nRemove Source Files? {newProject.Setup.RemoveSourceFiles}\nPackage game?:{newProject.Setup.CompressProjectFiles}\nRemove game files after packaging?:{newProject.Setup.RemoveFilesAfterCompression}\nCompression Mode:{newProject.Setup.CompressionLevel}", 0);
    }

}


