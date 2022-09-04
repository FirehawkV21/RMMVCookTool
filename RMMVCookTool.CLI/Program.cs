using RMMVCookTool.CLI.Properties;
using RMMVCookTool.Core.Compiler;
using RMMVCookTool.Core.Utilities;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace RMMVCookTool.CLI;

internal class Program
{
    private static readonly Lazy<CompilerProject> newProject = new(() => new CompilerProject(), true);
    private static SetupMenu setupTool = new();

    private static void Main(string[] args)
    {
        #region Print App Info
        CompilerUtilities.StartEngineLogger("CompilerCLI", false);
        Console.WriteLine(Resources.SpilterText);
        Console.WriteLine(Resources.ProgramNameText);
        Console.WriteLine(Resources.ProgramVersionString, Assembly.GetExecutingAssembly().GetName().Version);
        Console.WriteLine(Resources.ProgramAuthorText);
        Console.WriteLine(Resources.ProgramLicenseText);
        Console.WriteLine(Resources.SpilterText);
        CompilerUtilities.RecordToLog($"Cook Tool CLI, version {Assembly.GetExecutingAssembly().GetName().Version} started.", 0);
        #endregion
        #region Command line arguments
        if (args.Length >= 1)
        {
            Rule argsTab = new()
            {
                Title = Resources.CommandLineArgsTitle,
                Alignment = Justify.Left
            };
            AnsiConsole.Write(argsTab);
            Table argsTable = new();
            argsTable.AddColumn(Resources.SettingTitle);
            argsTable.AddColumn(Resources.ValueTitle);
            argsTable.Border = TableBorder.Rounded;
            CompilerUtilities.RecordToLog("Command Line Arguments loaded. Processing...", 0);
            for (int argnum = 0; argnum < args.Length; argnum++)
            {
                string stringBuffer;
                switch (args[argnum])
                {
                    //Set the SDK Location
                    case "--SDKLocation":
                        stringBuffer = args[argnum + 1];
                        setupTool.SdkLocation = stringBuffer.Replace("\"", "");
                        if (argnum <= args.Length - 1 && Directory.Exists(setupTool.SdkLocation) &&
                            File.Exists(Path.Combine(setupTool.SdkLocation,
                                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "nwjc.exe" : "nwjc")))
                        {
                            CompilerUtilities.RecordToLog($"NW.js compiler found at {setupTool.SdkLocation}.", 0);
                            argsTable.AddRow(Resources.SDKLocationEntry, setupTool.SdkLocation);
                            Console.ResetColor();
                        }
                        else
                        {
                            CompilerUtilities.RecordToLog($"NW.js compiler not found at {setupTool.SdkLocation}. Double check that the the nwjc executable is there.", 2);
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            Console.WriteLine(!Directory.Exists(setupTool.SdkLocation) ?
                                Resources.SDKLocationInexistantText :
                                Resources.CompilerMissingErrorText);
                            Console.ResetColor();
                            Console.WriteLine(Resources.PushEnterToExitText);
                            Console.ReadLine();
                            Environment.Exit(1);
                        }
                        break;

                    //Set the Project Location.
                    case "--ProjectLocation":
                        stringBuffer = args[argnum + 1];
                        newProject.Value.ProjectLocation = stringBuffer.Replace("\"", "");
                        if (argnum <= args.Length - 1 && (Directory.Exists(newProject.Value.ProjectLocation) && File.Exists(Path.Combine(newProject.Value.ProjectLocation, "package.json"))))
                        {
                            CompilerUtilities.RecordToLog($"RPG Maker MV/MZ project found at {newProject.Value.ProjectLocation}.", 0);
                            argsTable.AddRow(Resources.ProjectLocationEntry, newProject.Value.ProjectLocation);
                            Console.ResetColor();
                        }
                        else
                        {
                            CompilerUtilities.RecordToLog($"RPG Maker MV/MZ project was not found at {newProject.Value.ProjectLocation}.", 2);
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            Console.WriteLine(!File.Exists(Path.Combine(newProject.Value.ProjectLocation, "project.json")) ? Resources.JsonFileMissingErrorText : Resources.ProjectLocationInexistantText);
                            Console.ResetColor();
                            Console.WriteLine(Resources.PushEnterToExitText);
                            Console.ReadLine();
                            Environment.Exit(1);
                        }
                        break;

                    //Set the File Extension.
                    case "--FileExtension":
                        //Check if the next variable in the args array is a command line argument or it's the end of the array.
                        if (argnum >= args.Length - 1 && args[argnum].Contains("--")) CompilerUtilities.RecordToLog($"File extension not set. Keeping the extension to .bin.", 1);
                        else
                        {
                            newProject.Value.Setup.FileExtension = args[argnum + 1];
                            CompilerUtilities.RecordToLog($"File extension set to .{newProject.Value.Setup.FileExtension}.", 0);
                            argsTable.AddRow(Resources.FileExtensionEntry, newProject.Value.Setup.FileExtension);
                            Console.ResetColor();
                        }
                        break;

                    //This command line argument is for packaging the app after compressing (if the --ReleaseMode flag is active.
                    case "--PackageApp":
                        // Check that test mode is active. Since this ain't working, it will show this message and close.
                        if (setupTool.TestProject)
                        {
                            CompilerUtilities.RecordToLog($"Cannot package the app when test mode is turned on. Aborting job.", 2);
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            Console.WriteLine(Resources.CannotCompressAndTestErrorText);
                            Console.ResetColor();
                            Console.WriteLine(Resources.PushEnterToExitText);
                            Console.ReadLine();
                            Environment.Exit(1);
                        }
                        //Else, either just compress or compress and delete the files.
                        else
                        {
                            CompilerUtilities.RecordToLog($"The project will be compressed after compiling.", 0);
                            if (setupTool.CompressProject == 2)
                            {
                                if (argnum + 1 <= args.Length - 1)
                                {
                                    setupTool.CompressProject = args[argnum + 1] == "Final" ? 1 : 2;
                                    argsTable.AddRow(Resources.CompressionEntry, ((argnum + 1 <= args.Length - 1) && args[argnum + 1] == "Final") ? Resources.CompressAndRemoveEntry : Resources.CompressOnlyEntry);
                                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                                    Console.WriteLine((argnum + 1 <= args.Length - 1) && args[argnum + 1] == "Final" ?
                                        Resources.ProjectFilesRemovalAfterCompressionText :
                                        Resources.ProjectFilesCompressionConfirmText);
                                    CompilerUtilities.RecordToLog(((argnum + 1 <= args.Length - 1) && args[argnum + 1] == "Final") ? $"The source files will be removed." : $"Only compression will occur.", 0);
                                    Console.ResetColor();
                                }
                                else
                                {
                                    CompilerUtilities.RecordToLog($"Only compression will occur.", 0);
                                    argsTable.AddRow(Resources.CompressionEntry, Resources.CompressOnlyEntry);
                                    setupTool.CompressProject = 2;
                                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                                    Console.WriteLine(Resources.ProjectFilesCompressionConfirmText);
                                    Console.ResetColor();
                                }
                            }
                            else
                            {
                                CompilerUtilities.RecordToLog($"Compression cannot occur if --ReleaseApp isn't specified. Skipping.", 0);
                                Console.ForegroundColor = ConsoleColor.DarkYellow;
                                Console.WriteLine(Resources.CompressionNotPermittedText);
                                Console.ResetColor();
                            }
                        }
                        break;

                    //This command line argument deletes the JavaScript files after compiling.
                    case "--ReleaseMode":
                        setupTool.CompressProject = 2;
                        CompilerUtilities.RecordToLog($"JS files will be removed.", 0);
                        newProject.Value.Setup.RemoveSourceFiles = true;
                        Console.ResetColor();
                        break;

                    //This command line argument starts the nwjs app to test the project.
                    case "--TestMode":
                        if (setupTool.CompressProject <= 2)
                        {
                            CompilerUtilities.RecordToLog("Cannot test the app when the packaging option is turned on. Aborting job.", 2);
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            Console.WriteLine(Resources.CannotCompressAndTestErrorText);
                            Console.ResetColor();
                            Console.WriteLine(Resources.PushEnterToExitText);
                            Console.ReadLine();
                            Environment.Exit(1);
                        }
                        else
                        {
                            CompilerUtilities.RecordToLog("Test mode is turned on.", 0);
                            argsTable.AddRow(Resources.TestAfterCompilingEntry, Resources.CommonWordYes);
                            setupTool.TestProject = true;
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.WriteLine(Resources.nwjsTestStartingText);
                            Console.ResetColor();
                        }
                        break;
                    case "--SetCompressionLevel":
                        if (argnum + 1 <= args.Length - 1 && !args[argnum].Contains("--"))
                        {
                            switch (argnum + 1)
                            {
                                case 2:
                                    CompilerUtilities.RecordToLog($"Compression is set to No Compression.", 0);
                                    newProject.Value.Setup.CompressionLevel = 2;
                                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                                    Console.WriteLine(Resources.NoCompressionConfirmationText);
                                    Console.ResetColor();
                                    break;
                                case 1:
                                    CompilerUtilities.RecordToLog($"Compression is set to Fastest.", 0);
                                    newProject.Value.Setup.CompressionLevel = 1;
                                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                                    Console.WriteLine(Resources.FastestCompressionConfirmationText);
                                    Console.ResetColor();
                                    break;
                                default:
                                    CompilerUtilities.RecordToLog($"Compression is set to Optimal.", 0);
                                    newProject.Value.Setup.CompressionLevel = 0;
                                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                                    Console.WriteLine(Resources.OptimalCompressionCOnfirmationText);
                                    Console.ResetColor();
                                    break;
                            }
                        }
                        break;
                }
            }
            if (setupTool.CompressProject < 3) switch (newProject.Value.Setup.CompressionLevel)
                {
                    case 2:
                        argsTable.AddRow(Resources.CompressionLevelEntry, Resources.NoCompression);
                        break;
                    case 1:
                        argsTable.AddRow(Resources.CompressionLevelEntry, Resources.FastestCompression);
                        break;
                    case 0:
                        argsTable.AddRow(Resources.CompressionLevelEntry, Resources.OptimalCompression);
                        break;
                }
            if (setupTool.CheckDeletion == 2) argsTable.AddRow(Resources.RemoveSourceFilesEntry, Resources.CommonWordYes);
            else argsTable.AddRow(Resources.RemoveSourceFilesEntry, Resources.CommonWordNo);
            #endregion
            setupTool.CheckSettings(newProject.Value);
            if (setupTool.SettingsSet) AnsiConsole.Write(argsTable);
            else setupTool.SetupWorkload(newProject.Value);

            #region Workload Code
            Rule workTab = new()
            {
                Title = Resources.WorkTitle,
                Alignment = Justify.Left
            };
            AnsiConsole.Write(workTab);
            //Find the game folder.
            Stopwatch timer = new();
            Stopwatch totalTime = new();
            timer.Start();
            totalTime.Start();
            CompilerUtilities.RecordToLog("Attempting to read the package.json file.", 0);
            newProject.Value.GameFilesLocation = CompilerUtilities.GetProjectFilesLocation(Path.Combine(newProject.Value.ProjectLocation, "package.json"));
            if (newProject.Value.GameFilesLocation is "Null" or "Unknown")
            {
                timer.Stop();
                totalTime.Stop();
                //If the Json read returns nothing, throw an error to tell the user to double check their json file.
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(Resources.JsonReferenceError);
                Console.ResetColor();
            }
            else //If the read returned a valid folder, start the compiler process.
            {
                //Finding all the JS files.
                CompilerUtilities.RecordToLog("Preparing project...", 0);
                newProject.Value.FileMap ??= new List<string>(CompilerUtilities.FileFinder(Path.Combine(newProject.Value.ProjectLocation), "*.js"));

                CompilerUtilities.RecordToLog($"Found {newProject.Value.FileMap.Count} JS files.", 0);
                AnsiConsole.Status()
                    .Start("[darkcyan]" + Resources.BinaryRemovalText + "[/]", spinny =>
                    {
                        CompilerUtilities.RemoveDebugFiles(newProject.Value.ProjectLocation);
                        CompilerUtilities.CleanupBin(newProject.Value.FileMap);
                    });
                //Preparing the compiler task.
                newProject.Value.CompilerInfo.Value.FileName = Path.Combine(setupTool.SdkLocation, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "nwjc.exe" : "nwjc");
                timer.Stop();
                CompilerUtilities.RecordToLog($"Completed preparations. (Time to prepare:{timer.Elapsed}/Total Time (so far):{totalTime.Elapsed})", 0);
                timer.Reset();
                try
                {
                    timer.Start();
                    AnsiConsole.Progress()
                        .Start(progress =>
                        {
                            ProgressTask compilerTask = progress.AddTask(Resources.DarkcyanCompilingJSFilesText);
                            compilerTask.MaxValue = 131;
                            while (!progress.IsFinished)
                            {
                                for (int i = 0; i < newProject.Value.FileMap.Count; i++)
                                {
                                    CompilerUtilities.RecordToLog($"Compiling {newProject.Value.FileMap[i]}...", 0);
                                    newProject.Value.CompileFile(i);
                                    CompilerUtilities.RecordToLog($"Compiled {newProject.Value.FileMap[i]}.", 0);
                                    compilerTask.Increment(1);
                                }
                                compilerTask.StopTask();
                            }
                        });

                    timer.Stop();
                    CompilerUtilities.RecordToLog($"Completed the compilation. (Time elapsed:{timer.Elapsed}/Total Time (so far):{totalTime.Elapsed}", 0);
                    timer.Reset();
                    if (setupTool.TestProject)
                    {
                        Console.WriteLine(Resources.NwjsStartingTestNotificationText);
                        newProject.Value.RunTest(setupTool.SdkLocation);
                    }
                    else if (setupTool.CompressProject < 3 && setupTool.CompressProject == 2)
                    {
                        CompilerUtilities.RecordToLog(Resources.PackagingGameText, 0);
                        timer.Start();
                        AnsiConsole.Status()
                            .Start(Resources.FileCompressionText, spin => newProject.Value.CompressFiles());
                        timer.Stop();
                        CompilerUtilities.RecordToLog($"Packaged the game. (Time to package:{timer.Elapsed}/Total Time (so far):{totalTime.Elapsed}", 0);
                    }
                    totalTime.Stop();
                    CompilerUtilities.RecordToLog($"Task completed in {totalTime.Elapsed}", 0);
                    Console.WriteLine(Resources.TaskCompleteText);

                }
                catch (ArgumentNullException e)
                {
                    CompilerUtilities.RecordToLog(e);
                    AnsiConsole.WriteException(e);
                    throw;
                }
                catch (Exception e)
                {
                    CompilerUtilities.RecordToLog(e);
                    AnsiConsole.WriteException(e);
                    throw;

                }
            }

            //Ask the user to press Enter (or Return).
            if (!setupTool.SettingsSet)
            {
                Console.WriteLine(Resources.PushEnterToExitText);
                Console.ReadLine();
                CompilerUtilities.CloseLog();
            }
        }
        #endregion
    }
}
