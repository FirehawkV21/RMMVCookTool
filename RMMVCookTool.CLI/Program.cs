using DustInTheWind.ConsoleTools.Spinners;
using RMMVCookTool.CLI.Properties;
using RMMVCookTool.Core.Compiler;
using RMMVCookTool.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace RMMVCookTool.CLI
{
    class Program
    {
        private static readonly Lazy<CompilerProject> newProject = new(() => new CompilerProject(), true);
        private static bool _testProject;
        private static int _compressProject = 3;
        private static string _sdkLocation;
        private static bool _settingsSet;
        private static int _checkDeletion = 1;

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
                CompilerUtilities.RecordToLog("Command Line Arguments loaded. Processing...", 0);
                for (int argnum = 0; argnum < args.Length; argnum++)
                {
                    string stringBuffer;
                    switch (args[argnum])
                    {
                        //Set the SDK Location
                        case "--SDKLocation":
                            stringBuffer = args[argnum + 1];
                            _sdkLocation = stringBuffer.Replace("\"", "");
                            if (argnum <= args.Length - 1 && Directory.Exists(_sdkLocation) &&
                                File.Exists(Path.Combine(_sdkLocation,
                                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "nwjc.exe" : "nwjc")))
                            {
                                CompilerUtilities.RecordToLog($"NW.js compiler found at {_sdkLocation}.", 0);
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.WriteLine(Resources.SDKLocationConfirmationText);
                                Console.ResetColor();
                            }
                            else
                            {
                                CompilerUtilities.RecordToLog($"NW.js compiler not found at {_sdkLocation}. Double check that the the nwjc executable is there.", 2);
                                Console.ForegroundColor = ConsoleColor.DarkRed;
                                Console.WriteLine(!Directory.Exists(_sdkLocation) ?
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
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.WriteLine(Resources.ProjectLocationConfirmationText);
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
                                newProject.Value.FileExtension = args[argnum + 1];
                                CompilerUtilities.RecordToLog($"File extension set to .{newProject.Value.FileExtension}.", 0);
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.WriteLine(Resources.FileExtensionSetText + newProject.Value.FileExtension);
                                Console.ResetColor();
                            }
                            break;

                        //This command line argument is for packaging the app after compressing (if the --ReleaseMode flag is active.
                        case "--PackageApp":
                            // Check that test mode is active. Since this ain't working, it will show this message and close.
                            if (_testProject)
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
                                if (_checkDeletion == 2)
                                {
                                    if (argnum + 1 <= args.Length - 1)
                                    {
                                        _compressProject = args[argnum + 1] == "Final" ? 1 : 2;
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
                                        _compressProject = 2;
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
                            _checkDeletion = 2;
                            CompilerUtilities.RecordToLog($"JS files will be removed.", 0);
                            newProject.Value.RemoveSourceCodeAfterCompiling = true;
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.WriteLine(Resources.JavascriptDeletionConfirmationTet);
                            Console.ResetColor();
                            break;

                        //This command line argument starts the nwjs app to test the project.
                        case "--TestMode":
                            if (_compressProject <= 2)
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
                                _testProject = true;
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
                                        newProject.Value.CompressionModeLevel = 2;
                                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                                        Console.WriteLine(Resources.NoCompressionConfirmationText);
                                        Console.ResetColor();
                                        break;
                                    case 1:
                                        CompilerUtilities.RecordToLog($"Compression is set to Fastest.", 0);
                                        newProject.Value.CompressionModeLevel = 1;
                                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                                        Console.WriteLine(Resources.FastestCompressionConfirmationText);
                                        Console.ResetColor();
                                        break;
                                    default:
                                        CompilerUtilities.RecordToLog($"Compression is set to Optimal.", 0);
                                        newProject.Value.CompressionModeLevel = 0;
                                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                                        Console.WriteLine(Resources.OptimalCompressionCOnfirmationText);
                                        Console.ResetColor();
                                        break;
                                }
                            }
                            break;
                    }
                }
                #endregion
                #region Workload Check
                //Check if both the _projectLocation and _sdkLocation variables are not null.
                if (newProject.Value.ProjectLocation != null && _sdkLocation != null)
                {
                    CompilerUtilities.RecordToLog("Settings set. Starting the job...", 0);
                    _settingsSet = true;
                }
                else if (newProject.Value.ProjectLocation == null && _sdkLocation != null)
                {
                    CompilerUtilities.RecordToLog("Project location not set. Aborting job.", 2);
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine(Resources.ProjectNotSetErrorText);
                    Console.ResetColor();
                    Console.WriteLine(Resources.PushEnterToExitText);
                    Console.ReadLine();
                    Environment.Exit(1);
                }
                else if (_sdkLocation == null && newProject.Value.ProjectLocation != null)
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

            if (!_settingsSet)
            {
                do
                {
                    //Ask the user where is the SDK. Check if the folder's there.
                    Console.WriteLine(Resources.SDKLocationQuestion);
                    _sdkLocation = Console.ReadLine();
                    if (_sdkLocation == null) Console.WriteLine(Resources.SDKLocationIsNullText);
                    else if (!Directory.Exists(_sdkLocation)) Console.Write(Resources.SDKDirectoryMissing);
                } while (_sdkLocation == null || !Directory.Exists(_sdkLocation));
                #endregion

                #region Workload Questionaire
                do
                {
                    //Ask the user what project to compile. Check if the folder is there and there's a js folder.
                    Console.WriteLine(Resources.ProjectLocationQuestion);
                    newProject.Value.ProjectLocation = Console.ReadLine();

                    if (newProject.Value.ProjectLocation == null) Console.WriteLine(Resources.ProjectLocationIsNullText);
                    else if (!Directory.Exists(newProject.Value.ProjectLocation))
                        Console.WriteLine(Resources.ProjetDirectoryMissingErrorText);
                    else if (!Directory.Exists(Path.Combine(newProject.Value.ProjectLocation, "www", "js")))
                        Console.WriteLine(Resources.ProjectJsFolderMissing);
                    else if (!File.Exists(Path.Combine(newProject.Value.ProjectLocation, "package.json")))
                        Console.WriteLine(Resources.JsonFileMissingErrorText);
                } while (newProject.Value.ProjectLocation == null || !Directory.Exists(newProject.Value.ProjectLocation) ||
                         !Directory.Exists(Path.Combine(newProject.Value.ProjectLocation, "www", "js")));

                //Ask the user for the file extension.
                Console.Write(Resources.FileExtensionQuestion);
                newProject.Value.FileExtension = Console.ReadLine();
                if (string.IsNullOrEmpty(newProject.Value.FileExtension)) newProject.Value.FileExtension = "bin";
                //This is the check if the tool should delete the JS files.
                Console.WriteLine(Resources.WorkloadQuestion);
                string stringBuffer = Console.ReadLine();
                int.TryParse(stringBuffer, out _checkDeletion);
                newProject.Value.RemoveSourceCodeAfterCompiling = _checkDeletion == 2;

                char charBuffer;
                if (_checkDeletion == 2)
                {
                    Console.WriteLine(
                        Resources.CompressionQuestion);
                    charBuffer = Console.ReadKey().KeyChar;
                    _compressProject = !char.IsLetterOrDigit(charBuffer) ? Convert.ToInt32(charBuffer) : 3;
                }
                else
                {
                    //Ask if the user would like to test with nwjs.
                    Console.WriteLine(Resources.TestProjectQuestion);
                    charBuffer = Console.ReadKey().KeyChar;
                    if (char.IsLetterOrDigit(charBuffer))
                    {
                        _testProject = charBuffer switch
                        {
                            'Y' or 'y' or 'Ν' or 'ν' => true,
                            _ => false,
                        };
                    }
                    else _testProject = false;
                }
                CompilerUtilities.RecordToLog($"Current setup of the job:\nCompiler Location:{_sdkLocation}\nProject Location:{newProject.Value.ProjectLocation}\nFile Extension:{newProject.Value.FileExtension}\nRemove Source Files? {newProject.Value.RemoveSourceCodeAfterCompiling}\nPackage game?:{newProject.Value.CompressFilesToPackage}\nRemove game files after packaging?:{newProject.Value.RemoveFilesAfterCompression}\nCompression Mode:{newProject.Value.CompressionModeLevel}", 0);
            }
            #endregion

            #region Workload Code
            //Find the game folder.
            Stopwatch timer = new();
            Stopwatch totalTime = new();
            timer.Start();
            totalTime.Start();
            CompilerUtilities.RecordToLog("Attempting to read the package.json file.", 0);
            newProject.Value.GameFilesLocation = CompilerUtilities.GetProjectFilesLocation(Path.Combine(newProject.Value.ProjectLocation, "package.json"));
            if (newProject.Value.GameFilesLocation == "Null" || newProject.Value.GameFilesLocation == "Unknown")
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
                using (Spinner work1 = new())
                {
                    work1.Label = Resources.BinaryRemovalText;
                    work1.DoneText = "\rRemoved binary files. Starting the compiler job...";
                    work1.Display();
                    CompilerUtilities.RemoveDebugFiles(newProject.Value.ProjectLocation);
                    CompilerUtilities.CleanupBin(newProject.Value.FileMap);
                    work1.Close();

                }
                //Preparing the compiler task.
                newProject.Value.CompilerInfo.Value.FileName = Path.Combine(_sdkLocation, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "nwjc.exe" : "nwjc");
                timer.Stop();
                CompilerUtilities.RecordToLog($"Completed preparations. (Time to prepare:{timer.Elapsed}/Total Time (so far):{totalTime.Elapsed})", 0);
                timer.Reset();
                try
                {
                    ProgressBar progress = new()
                    {
                        UnitOfMeasurement = "/" + newProject.Value.FileMap.Count + " JS Files",
                        MaxValue = newProject.Value.FileMap.Count,
                        LabelText = "Compiling"
                    };

                    timer.Start();
                    ManualResetEventSlim finishEvent = new();
                    finishEvent.Reset();
                    Task.Run<Task>(async () =>
                    {
                        progress.Display();
                        for (var i = 0; i < newProject.Value.FileMap.Count; i++)
                        {
                            CompilerUtilities.RecordToLog($"Compiling {newProject.Value.FileMap[i]}...", 0);
                            progress.LabelText = Resources.CompilingWord2 + newProject.Value.FileMap[i] + @"...";
                            progress.Value = i + 1;
                            newProject.Value.CompileFile(i);
                            CompilerUtilities.RecordToLog($"Compiled {newProject.Value.FileMap[i]}.", 0);
                            await Task.Delay(10);
                        }
                        finishEvent.Set();
                    }).ConfigureAwait(false);
                    finishEvent.Wait();
                    progress.Close();
                    timer.Stop();
                    CompilerUtilities.RecordToLog($"Completed the compilation. (Time elapsed:{timer.Elapsed}/Total Time (so far):{totalTime.Elapsed}", 0);
                    timer.Reset();
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write(Resources.DateTimeFormatText, DateTime.Now);
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine(Resources.CompilerTaskComplete);
                    Console.ResetColor();
                    if (_testProject)
                    {
                        Console.WriteLine(Resources.NwjsStartingTestNotificationText);
                        newProject.Value.RunTest(_sdkLocation);
                    }
                    else if (_compressProject < 3 && _checkDeletion == 2)
                    {
                        CompilerUtilities.RecordToLog("Packaging the game...", 0);
                        timer.Start();
                        using (Spinner work2 = new())
                        {
                            work2.Label = Resources.FileCompressionText;
                            work2.DoneText = "\rPackaged the game.";
                            work2.Display();
                            newProject.Value.CompressFiles();
                            work2.Close();
                        }
                        timer.Stop();
                        CompilerUtilities.RecordToLog($"Packaged the game. (Time to package:{timer.Elapsed}/Total Time (so far):{totalTime.Elapsed}", 0);
                        if (_compressProject == 1)
                        {
                            timer.Reset();
                            CompilerUtilities.RecordToLog("Removing the original files...", 0);
                            timer.Start();
                            using (Spinner work3 = new())
                            {
                                work3.Label = Resources.SourceFileDeletionText;
                                work3.DoneText = "\rRemoved the source files.";
                                Console.ForegroundColor = ConsoleColor.DarkCyan;
                                Console.WriteLine(Resources.SourceFileDeletionText);
                                newProject.Value.DeleteFiles();
                            }
                            timer.Stop();
                            CompilerUtilities.RecordToLog($"Removed the files. (Time to remove:{timer.Elapsed}/Total Time (so far):{totalTime.Elapsed}", 0);
                        }
                    }
                    totalTime.Stop();
                    CompilerUtilities.RecordToLog($"Task completed in {totalTime.Elapsed}", 0);
                    Console.WriteLine(Resources.TaskCompleteText);

                }
                catch (ArgumentNullException e)
                {
                    CompilerUtilities.RecordToLog(e);
                    Console.WriteLine(e);
                    throw;
                }
                catch (Exception e)
                {
                    //TODO Improve the handling of the errors.
                    CompilerUtilities.RecordToLog(e);
                    Console.WriteLine(e);
                    throw;

                }
            }

            //Ask the user to press Enter (or Return).
            if (!_settingsSet)
            {
                Console.WriteLine(Resources.PushEnterToExitText);
                Console.ReadLine();
                CompilerUtilities.CloseLog();
            }
        }
        #endregion
    }
}