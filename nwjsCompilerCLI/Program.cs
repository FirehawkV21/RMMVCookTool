using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using CompilerCore;
using nwjsCompilerCLI.Properties;

namespace nwjsCompilerCLI {
    class Program {
        private static bool _testProject;
        private static int _compressProject = 3;
        private static string _sdkLocation;
        private static string _projectLocation;
        private static string _fileExtension = "bin";
        private static bool _parallelMode;
        private static bool _settingsSet;
        private static bool _removeJsFiles;
        private static int _checkDeletion = 1;
        private static int _compressionLevel;
        private static IEnumerable<string> _fileMap;
        private static bool _compressionSafeMode = false;
        private static string _gameFolder;

        private static void Main (string[] args) {
            #region Print App Info
            Console.WriteLine (Resources.SpilterText);
            Console.WriteLine (Resources.ProgramNameText);
            Console.WriteLine (Resources.ProgramVersionString, Assembly.GetExecutingAssembly ().GetName ().Version);
            Console.WriteLine (Resources.ProgramAuthorText);
            Console.WriteLine (Resources.ProgramLicenseText);
            Console.WriteLine (Resources.SpilterText);
            #endregion
            #region Command line arguments
#pragma warning disable CA1307 // Specify StringComparison
            if (args.Length >= 1) {
                for (int argnum = 0; argnum < args.Length; argnum++) {
                    string stringBuffer;
                    switch (args[argnum]) {

                        case "--CompressionSafeMode":
                            _compressionSafeMode = true;
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.WriteLine(Resources.CompressionSafeModeConfirmationText);
                            break;

                            //Turn on Parallel mode.
                        case "--Parallel":
                            _parallelMode = true;
                            Console.WriteLine (Resources.ParallelModeActiveText);
                            break;

                            //Set the SDK Location
                        case "--SDKLocation":
                            stringBuffer = args[argnum + 1];
                            _sdkLocation = stringBuffer.Replace ("\"", "");
                            if (argnum <= args.Length - 1 && Directory.Exists (_sdkLocation) &&
                                File.Exists(Path.Combine (_sdkLocation,
                                    RuntimeInformation.IsOSPlatform (OSPlatform.Windows) ? "nwjc.exe" : "nwjc"))) {
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.WriteLine(Resources.SDKLocationConfirmationText);
                                Console.ResetColor();
                            } else {
                                Console.ForegroundColor = ConsoleColor.DarkRed;
                                Console.WriteLine (!Directory.Exists(_sdkLocation) ?
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
                            _projectLocation = stringBuffer.Replace ("\"", "");
                            if (argnum <= args.Length - 1 && (Directory.Exists (_projectLocation) && File.Exists(Path.Combine(_projectLocation, "package.json")))) {
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.WriteLine(Resources.ProjectLocationConfirmationText);
                                Console.ResetColor();
                            } else {
                                Console.ForegroundColor = ConsoleColor.DarkRed;
                                Console.WriteLine (!File.Exists (Path.Combine (_projectLocation, "project.json")) ? Resources.JsonFileMissingErrorText : Resources.ProjectLocationInexistantText);
                                    Console.ResetColor();
                                Console.WriteLine(Resources.PushEnterToExitText);
                                Console.ReadLine();
                                Environment.Exit(1);
                            }
                            break;

                            //Set the File Extension.
                        case "--FileExtension":
                            //Check if the next variable in the args array is a command line argument or it's the end of the array.
                            if (argnum >= args.Length - 1 && args[argnum].Contains ("--")) continue;
                            _fileExtension = args[argnum + 1];
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.WriteLine(Resources.FileExtensionSetText + _fileExtension);
                            Console.ResetColor();
                            break;

                            //This command line argument is for packaging the app after compressing (if the --ReleaseMode flag is active.
                        case "--PackageApp":
                            // Check that test mode is active. Since this ain't working, it will show this message and close.
                            if (_testProject) {
                                Console.ForegroundColor = ConsoleColor.DarkRed;
                                Console.WriteLine(Resources.CannotCompressAndTestErrorText);
                                Console.ResetColor();
                                Console.WriteLine(Resources.PushEnterToExitText);
                                Console.ReadLine();
                                Environment.Exit(1);
                            }
                            //Else, either just compress or compress and delete the files.
                            else {
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
                            _removeJsFiles = true;
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.WriteLine(Resources.JavascriptDeletionConfirmationTet);
                            Console.ResetColor();
                            break;

                            //This command line argument starts the nwjs app to test the project.
                        case "--TestMode":
                            if (_compressProject <= 2) {
                                Console.ForegroundColor = ConsoleColor.DarkRed;
                                Console.WriteLine(Resources.CannotCompressAndTestErrorText);
                                Console.ResetColor();
                                Console.WriteLine(Resources.PushEnterToExitText);
                                Console.ReadLine();
                                Environment.Exit(1);
                            } else {
                                _testProject = true;
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.WriteLine(Resources.nwjsTestStartingText);
                                Console.ResetColor();
                            }
                            break;
                        case "--SetCompressionLevel":
                            if (argnum + 1 <= args.Length - 1 && !args[argnum].Contains ("--")) {
                                switch (argnum + 1) {
                                    case 2:
                                        _compressionLevel = 2;
                                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                                        Console.WriteLine(Resources.NoCompressionConfirmationText);
                                        Console.ResetColor();
                                        break;
                                    case 1:
                                        _compressionLevel = 1;
                                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                                        Console.WriteLine(Resources.FastestCompressionConfirmationText);
                                        Console.ResetColor();
                                        break;
                                    default:
                                        _compressionLevel = 0;
                                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                                        Console.WriteLine(Resources.OptimalCompressionCOnfirmationText);
                                        Console.ResetColor();
                                        break;
                                }
                            }
                            break;
                    }
                }
#pragma warning restore CA1307 // Specify StringComparison
                #endregion
            #region Workload Check
                //Check if both the _projectLocation and _sdkLocation variables are not null.
                if (_projectLocation != null && _sdkLocation != null) _settingsSet = true;
                else if (_projectLocation == null && _sdkLocation != null) {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine(Resources.ProjectNotSetErrorText);
                    Console.ResetColor();
                    Console.WriteLine(Resources.PushEnterToExitText);
                    Console.ReadLine();
                    Environment.Exit(1);
                } else if (_sdkLocation == null && _projectLocation != null) {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine(Resources.SDKLocationNotSetErrorText);
                    Console.ResetColor();
                    Console.WriteLine(Resources.PushEnterToExitText);
                    Console.ReadLine();
                    Environment.Exit(1);
                }
                Console.WriteLine("");
            }

            if (!_settingsSet) {
                do {
                    //Ask the user where is the SDK. Check if the folder's there.
                    Console.WriteLine(Resources.SDKLocationQuestion);
                    _sdkLocation = Console.ReadLine();
                    if (_sdkLocation == null) Console.WriteLine(Resources.SDKLocationIsNullText);
                    else if (!Directory.Exists(_sdkLocation)) Console.Write (Resources.SDKDirectoryMissing);
                } while (_sdkLocation == null || !Directory.Exists(_sdkLocation));
                #endregion

                #region Workload Questionaire
                do
                {
                    //Ask the user what project to compile. Check if the folder is there and there's a js folder.
                    Console.WriteLine (Resources.ProjectLocationQuestion);
                    _projectLocation = Console.ReadLine ();

                    if (_projectLocation == null) Console.WriteLine (Resources.ProjectLocationIsNullText);
                    else if (!Directory.Exists (_projectLocation))
                        Console.WriteLine (Resources.ProjetDirectoryMissingErrorText);
                    else if (!Directory.Exists (Path.Combine (_projectLocation, "www", "js")))
                        Console.WriteLine (Resources.ProjectJsFolderMissing);
                    else if (!File.Exists(Path.Combine(_projectLocation, "package.json")))
                        Console.WriteLine(Resources.JsonFileMissingErrorText);
                } while (_projectLocation == null || !Directory.Exists (_projectLocation) ||
                    !Directory.Exists (Path.Combine (_projectLocation, "www", "js")));

                //Ask the user for the file extension.
                Console.Write (Resources.FileExtensionQuestion);
                _fileExtension = Console.ReadLine();
                if (string.IsNullOrEmpty(_fileExtension)) _fileExtension = "bin";
                //This is the check if the tool should delete the JS files.
                Console.WriteLine(Resources.WorkloadQuestion);
                string stringBuffer = Console.ReadLine();
                int.TryParse(stringBuffer, out _checkDeletion);
                _removeJsFiles = _checkDeletion == 2;

                char charBuffer;
                if (_checkDeletion == 2) {
                    Console.WriteLine (
                        Resources.CompressionQuestion);
                    charBuffer = Console.ReadKey().KeyChar;
                    _compressProject = !char.IsLetterOrDigit (charBuffer) ? Convert.ToInt32 (charBuffer) : 3;
                } else {
                    //Ask if the user would like to test with nwjs.
                    Console.WriteLine(Resources.TestProjectQuestion);
                    charBuffer = Console.ReadKey ().KeyChar;
                    if (char.IsLetterOrDigit (charBuffer)) {
                        switch (charBuffer) {
                            case 'Y':
                            case 'y':
                            case 'Ν':
                            case 'ν':
                                _testProject = true;
                                break;
                            default:
                                _testProject = false;
                                break;
                        }
                    } else _testProject = false;
                }
            }
            #endregion

            #region Workload Code
            //Find the game folder.
            JsonProcessor.FindGameFolder(Path.Combine(_projectLocation, "package.json"), out _gameFolder);
                if (_gameFolder == "Null" || _gameFolder == "Unknown")
                {
                    //If the Json read returns nothing, throw an error to tell the user to double check their json file.
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine(Resources.JsonReferenceError);
                    Console.ResetColor();
                }
                else //If the read returned a valid folder, start the compiler process.
                {
                    //Finding all the JS files.
                    _fileMap = CoreCode.FileFinder(Path.Combine(_gameFolder), "*.js");
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write(Resources.DateTimeFormatText, DateTime.Now);
                    Console.ResetColor();
                    Console.WriteLine(Resources.BinaryRemovalText);
                    CoreCode.CleanupBin(_fileMap);
                    //Preparing the compiler task.
                    CoreCode.CompilerInfo.FileName = Path.Combine(_sdkLocation, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "nwjc.exe" : "nwjc");
                    try
                    {
                        //Read from the FileMap.
                        //Compilation is done in parallel. Handy for multi-core systems.
                        if (_parallelMode)
                        {
                            Parallel.ForEach(_fileMap, index =>
                            {
                                //Print the status of the compiler. Show which thread is compiling what as well.
                                Console.WriteLine(@"[" + DateTime.Now + Resources.ThreadWord +
                                                  Thread.CurrentThread.ManagedThreadId +
                                                  Resources.CompilingWord + index + @"...\n");
                                //Call the compiler task.
                                CoreCode.CompilerWorkerTask(index, _fileExtension, _removeJsFiles);
                                Console.WriteLine(@"[" + DateTime.Now + Resources.ThreadWord +
                                                  Thread.CurrentThread.ManagedThreadId +
                                                  Resources.FinishedCompilingText + index + @".\n");
                            });
                        }
                        else
                        {
                            foreach (string index in _fileMap)
                            {
                                //Print the status of the compiler. Show which thread is compiling what as well.
                                Console.ForegroundColor = ConsoleColor.DarkCyan;
                                Console.Write(Resources.DateTimeFormatText, DateTime.Now);
                                Console.ResetColor();
                                Console.WriteLine(Resources.CompilingWord2 + index + @"...");
                                //Call the compiler task.
                                CoreCode.CompilerWorkerTask(index, _fileExtension, _removeJsFiles);
                                Console.ForegroundColor = ConsoleColor.DarkCyan;
                                Console.Write(Resources.DateTimeFormatText, DateTime.Now);
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.WriteLine(Resources.FinishedCompilingText2 + index + @".");
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
                            CoreCode.RunTest(_sdkLocation, _projectLocation);
                        }
                        else if (_compressProject < 3 && _checkDeletion == 2)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkCyan;
                            if (_compressionSafeMode)
                            {
                                Console.Write(Resources.DateTimeFormatText, DateTime.Now);
                                Console.ResetColor();
                                Console.WriteLine(Resources.CopyingToTempLocationText);
                                CoreCode.PreparePack(_projectLocation);
                            }

                            Console.ForegroundColor = ConsoleColor.DarkCyan;
                            Console.Write(Resources.DateTimeFormatText, DateTime.Now);
                            Console.ResetColor();
                            Console.WriteLine(Resources.FileCompressionText);
                            CoreCode.CompressFiles(_projectLocation, _gameFolder, _projectLocation, _compressionLevel,
                                _compressionSafeMode);
                            if (_compressProject == 1)
                            {
                                Console.ForegroundColor = ConsoleColor.DarkCyan;
                                Console.Write(Resources.DateTimeFormatText, DateTime.Now);
                                Console.ResetColor();
                                Console.WriteLine(Resources.SourceFileDeletionText);
                                CoreCode.DeleteFiles(_projectLocation);
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