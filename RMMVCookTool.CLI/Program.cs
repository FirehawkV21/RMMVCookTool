﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using RMMVCookTool.CLI.Properties;
using RMMVCookTool.Core;

namespace RMMVCookTool.CLI
{
    class Program
    {
        private static readonly Lazy<CompilerProject> newProject = new(() => new CompilerProject(), true);
        private static bool _testProject;
        private static int _compressProject = 3;
        private static string _sdkLocation;
        private static bool _parallelMode;
        private static bool _settingsSet;
        private static int _checkDeletion = 1;

        private static void Main(string[] args)
        {
            #region Print App Info
            Console.WriteLine(Resources.SpilterText);
            Console.WriteLine(Resources.ProgramNameText);
            Console.WriteLine(Resources.ProgramVersionString, Assembly.GetExecutingAssembly().GetName().Version);
            Console.WriteLine(Resources.ProgramAuthorText);
            Console.WriteLine(Resources.ProgramLicenseText);
            Console.WriteLine(Resources.SpilterText);
            #endregion
            #region Command line arguments
            if (args.Length >= 1)
            {
                for (int argnum = 0; argnum < args.Length; argnum++)
                {
                    string stringBuffer;
                    switch (args[argnum])
                    {

                        //Turn on Parallel mode.
                        case "--Parallel":
                            _parallelMode = true;
                            Console.WriteLine(Resources.ParallelModeActiveText);
                            break;

                        //Set the SDK Location
                        case "--SDKLocation":
                            stringBuffer = args[argnum + 1];
                            _sdkLocation = stringBuffer.Replace("\"", "");
                            if (argnum <= args.Length - 1 && Directory.Exists(_sdkLocation) &&
                                File.Exists(Path.Combine(_sdkLocation,
                                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "nwjc.exe" : "nwjc")))
                            {
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.WriteLine(Resources.SDKLocationConfirmationText);
                                Console.ResetColor();
                            }
                            else
                            {
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
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.WriteLine(Resources.ProjectLocationConfirmationText);
                                Console.ResetColor();
                            }
                            else
                            {
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
                            if (argnum >= args.Length - 1 && args[argnum].Contains("--")) continue;
                            newProject.Value.FileExtension = args[argnum + 1];
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.WriteLine(Resources.FileExtensionSetText + newProject.Value.FileExtension);
                            Console.ResetColor();
                            break;

                        //This command line argument is for packaging the app after compressing (if the --ReleaseMode flag is active.
                        case "--PackageApp":
                            // Check that test mode is active. Since this ain't working, it will show this message and close.
                            if (_testProject)
                            {
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
                                if (_checkDeletion == 2)
                                {
                                    if (argnum + 1 <= args.Length - 1)
                                    {
                                        _compressProject = args[argnum + 1] == "Final" ? 1 : 2;
                                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                                        Console.WriteLine((argnum + 1 <= args.Length - 1) && args[argnum + 1] == "Final" ?
                                            Resources.ProjectFilesRemovalAfterCompressionText :
                                            Resources.ProjectFilesCompressionConfirmText);
                                        Console.ResetColor();
                                    }
                                    else
                                    {
                                        _compressProject = 2;
                                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                                        Console.WriteLine(Resources.ProjectFilesCompressionConfirmText);
                                        Console.ResetColor();
                                    }
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                                    Console.WriteLine(Resources.CompressionNotPermittedText);
                                    Console.ResetColor();
                                }
                            }
                            break;

                        //This command line argument deletes the JavaScript files after compiling.
                        case "--ReleaseMode":
                            _checkDeletion = 2;
                            newProject.Value.RemoveSourceCodeAfterCompiling = true;
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.WriteLine(Resources.JavascriptDeletionConfirmationTet);
                            Console.ResetColor();
                            break;

                        //This command line argument starts the nwjs app to test the project.
                        case "--TestMode":
                            if (_compressProject <= 2)
                            {
                                Console.ForegroundColor = ConsoleColor.DarkRed;
                                Console.WriteLine(Resources.CannotCompressAndTestErrorText);
                                Console.ResetColor();
                                Console.WriteLine(Resources.PushEnterToExitText);
                                Console.ReadLine();
                                Environment.Exit(1);
                            }
                            else
                            {
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
                                        newProject.Value.CompressionModeLevel = 2;
                                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                                        Console.WriteLine(Resources.NoCompressionConfirmationText);
                                        Console.ResetColor();
                                        break;
                                    case 1:
                                        newProject.Value.CompressionModeLevel = 1;
                                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                                        Console.WriteLine(Resources.FastestCompressionConfirmationText);
                                        Console.ResetColor();
                                        break;
                                    default:
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
                if (newProject.Value.ProjectLocation != null && _sdkLocation != null) _settingsSet = true;
                else if (newProject.Value.ProjectLocation == null && _sdkLocation != null)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine(Resources.ProjectNotSetErrorText);
                    Console.ResetColor();
                    Console.WriteLine(Resources.PushEnterToExitText);
                    Console.ReadLine();
                    Environment.Exit(1);
                }
                else if (_sdkLocation == null && newProject.Value.ProjectLocation != null)
                {
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
            }
            #endregion

            #region Workload Code
            //Find the game folder.
            newProject.Value.GameFilesLocation = JsonProcessor.FindGameFolder(Path.Combine(newProject.Value.ProjectLocation, "package.json"));
            if (newProject.Value.GameFilesLocation == "Null" || newProject.Value.GameFilesLocation == "Unknown")
            {
                //If the Json read returns nothing, throw an error to tell the user to double check their json file.
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(Resources.JsonReferenceError);
                Console.ResetColor();
            }
            else //If the read returned a valid folder, start the compiler process.
            {
                //Finding all the JS files.
                newProject.Value.FileMap ??= new List<string>(CompilerUtilities.FileFinder(Path.Combine(newProject.Value.ProjectLocation), "*.js"));
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write(Resources.DateTimeFormatText, DateTime.Now);
                Console.ResetColor();
                Console.WriteLine(Resources.BinaryRemovalText);
                CompilerUtilities.CleanupBin(newProject.Value.FileMap);
                CompilerUtilities.RemoveDebugFiles(newProject.Value.ProjectLocation);
                //Preparing the compiler task.
                newProject.Value.CompilerInfo.Value.FileName = Path.Combine(_sdkLocation, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "nwjc.exe" : "nwjc");
                try
                {
                    //Read from the FileMap.
                    //Compilation is done in parallel. Handy for multi-core systems.
                    if (_parallelMode)
                    {
                        Parallel.For(0, newProject.Value.FileMap.Count, index =>
                        {
                                //Print the status of the compiler. Show which thread is compiling what as well.
                                Console.WriteLine(@"[" + DateTime.Now + Resources.ThreadWord +
                                              Thread.CurrentThread.ManagedThreadId +
                                              Resources.CompilingWord + newProject.Value.FileMap[index] + @"...\n");
                                //Call the compiler task.
                                newProject.Value.CompileFile(index);
                            Console.WriteLine(@"[" + DateTime.Now + Resources.ThreadWord +
                                              Thread.CurrentThread.ManagedThreadId +
                                              Resources.FinishedCompilingText + newProject.Value.FileMap[index] + @".\n");
                        });
                    }
                    else
                    {
                        for (var i = 0; i < newProject.Value.FileMap.Count; i++)
                        {
                            //Print the status of the compiler. Show which thread is compiling what as well.
                            Console.ForegroundColor = ConsoleColor.DarkCyan;
                            Console.Write(Resources.DateTimeFormatText, DateTime.Now);
                            Console.ResetColor();
                            Console.WriteLine(Resources.CompilingWord2 + newProject.Value.FileMap[i] + @"...");
                            //Call the compiler task.
                            newProject.Value.CompileFile(i);
                            Console.ForegroundColor = ConsoleColor.DarkCyan;
                            Console.Write(Resources.DateTimeFormatText, DateTime.Now);
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.WriteLine(Resources.FinishedCompilingText2 + newProject.Value.FileMap[i] + @".");
                            Console.ResetColor();
                        }
                    }

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
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.Write(Resources.DateTimeFormatText, DateTime.Now);
                        Console.ResetColor();
                        Console.WriteLine(Resources.FileCompressionText);
                        newProject.Value.CompressFiles();
                        if (_compressProject == 1)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkCyan;
                            Console.Write(Resources.DateTimeFormatText, DateTime.Now);
                            Console.ResetColor();
                            Console.WriteLine(Resources.SourceFileDeletionText);
                            newProject.Value.DeleteFiles();
                        }
                    }

                    Console.WriteLine(Resources.TaskCompleteText);

                }
                catch (ArgumentNullException e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                catch (Exception e)
                {
                    //TODO Improve the handling of the errors.
                    Console.WriteLine(e);
                    throw;

                }
            }



            //Ask the user to press Enter (or Return).
            if (_settingsSet) return;
            Console.WriteLine(Resources.PushEnterToExitText);
            Console.ReadLine();
        }
        #endregion
    }
}