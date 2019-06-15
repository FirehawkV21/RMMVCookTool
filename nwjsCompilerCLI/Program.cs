using System;
using System.IO;
using System.Net.Mime;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using CompilerCore;

namespace nwjsCompilerCLI
{
    class Program
    {
        private static bool _testProject;
        private static int _compressProject = 3;
        private static string _sdkLocation;
        private static string _projectLocation;
        private static string _fileExtension = "bin";
        private static bool _parallelMode;
        private static bool _settingsSet;
        private static bool _removeJsFiles;
        private static int _checkDeletion = 1;
        private static int _compressionLevel = 0;

        private static void Main(string[] args)
        {
            string stringBuffer;
            Console.WriteLine("================================================");
            Console.WriteLine("= RPG Maker MV Cook Tool (.NET Core CLI Version)");
            Console.WriteLine("= Version D1.01 ({0})", Assembly.GetExecutingAssembly().GetName().Version);
            Console.WriteLine("= Developed by AceOfAces.");
            Console.WriteLine("= Licensed under the MIT license.");
            Console.WriteLine("================================================\n");

            if (args.Length >= 1)
            {   
                for (int argnum = 0; argnum < args.Length; argnum++)
                {
                    switch (args[argnum])
                    {
                        //Turn on Parallel mode.
                        case "--Parallel":
                            _parallelMode = true;
                            Console.WriteLine("Compiler is now running in parallel mode.");
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
                                Console.WriteLine("SDK Location OK.");
                                Console.ResetColor();
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.DarkRed;
                                Console.WriteLine(
                                    !Directory.Exists(_sdkLocation)
                                        ? "The location of the SDK doesn't exist."
                                        : "The compiler isn't there. Please pick the folder that has the nwjc file");
                                Console.ResetColor();
                                Console.WriteLine("Push Enter/Return to exit.");
                                Console.ReadLine();
                                Environment.Exit(1);
                            }
                            break;

                        //Set the Project Location.
                        case "--ProjectLocation":
                            stringBuffer = args[argnum + 1];
                            _projectLocation = stringBuffer.Replace("\"", "");
                            if (argnum <= args.Length - 1 && Directory.Exists(_projectLocation) &&
                                Directory.Exists(Path.Combine(_projectLocation, "www", "js")))
                            {
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.WriteLine("Project Location OK.");
                                Console.ResetColor();
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.DarkRed;
                                Console.WriteLine(!Directory.Exists(Path.Combine(_projectLocation, "www", "js"))
                                    ? "The location of the project doesn't exist."
                                    : "The js folder doesn't exist.");
                                Console.ResetColor();
                                Console.WriteLine("Push Enter/Return to exit.");
                                Console.ReadLine();
                                Environment.Exit(1);
                            }
                            break;

                        //Set the File Extension.
                        case "--FileExtension":
                            //Check if the next variable in the args array is a command line argument or it's the end of the array.
                            if (argnum >= args.Length - 1 && args[argnum].Contains("--")) continue;
                            _fileExtension = args[argnum + 1];
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.WriteLine("The file extension is set to " + _fileExtension);
                            Console.ResetColor();
                            break;

                        //This command line argument is for packaging the app after compressing (if the --ReleaseMode flag is active.
                        case "--PackageApp":
                            // Check that test mode is active. Since this ain't working, it will show this message and close.
                            if (_testProject)
                            {
                                Console.ForegroundColor = ConsoleColor.DarkRed;
                                Console.WriteLine("You can't compress and test the project at the moment.");
                                Console.ResetColor();
                                Console.WriteLine("Push Enter/Return to exit.");
                                Console.ReadLine();
                                Environment.Exit(1);
                            }
                            //Else, either just compress or compress and delete the files.
                            else
                            {
                                if (argnum + 1 <= args.Length - 1)
                                {
                                    _compressProject = args[argnum + 1] == "Final" ? 1 : 2;
                                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                                    Console.WriteLine((argnum + 1 <= args.Length - 1) && args[argnum + 1] == "Final"
                                        ? "The project's files will be compressed (the files will be deleted after compressing)."
                                        : "The Project's files will be compressed");
                                    Console.ResetColor();
                                }
                                else _compressProject = 2;
                            }
                            break;

                        //This command line argument deletes the JavaScript files after compiling.
                        case "--ReleaseMode":
                            _checkDeletion = 2;
                            _removeJsFiles = true;
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.WriteLine("The JavaScript files will be deleted after compilation.");
                            Console.ResetColor();
                            break;

                        //This command line argument starts the nwjs app to test the project.
                        case "--TestMode":
                            if (_compressProject <= 2)
                            {
                                Console.ForegroundColor = ConsoleColor.DarkRed;
                                Console.WriteLine("You can't compress and test the project at the moment.");
                                Console.ResetColor();
                                Console.WriteLine("Push Enter/Return to exit.");
                                Console.ReadLine();
                                Environment.Exit(1);
                            }
                            else
                            {
                                _testProject = true;
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.WriteLine("NW.js will start after compiling.");
                                Console.ResetColor();
                            }
                            break;
                        case "--SetCompressionLevel":
                            if (argnum + 1 <= args.Length - 1 && !args[argnum].Contains("--"))
                            {
                                switch (argnum + 1)
                                {
                                    case 2:
                                        _compressionLevel = 2;
                                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                                        Console.WriteLine("No compression will be used for the archive.");
                                        Console.ResetColor();
                                        break;
                                    case 1:
                                        _compressionLevel = 1;
                                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                                        Console.WriteLine("The fastest compression will be used for the archive.");
                                        Console.ResetColor();
                                        break;
                                    default:
                                        _compressionLevel = 0;
                                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                                        Console.WriteLine("The optimal compression will be used for the archive (this is the default).");
                                        Console.ResetColor();
                                        break;
                                }
                            }
                            break;
                    }
                }

                //Check if both the _projectLocation and _sdkLocation variables are not null.
                if (_projectLocation != null && _sdkLocation != null) _settingsSet = true;
                else if (_projectLocation == null && _sdkLocation != null)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("The Project location is not set. Please set it with the --ProjectLocation \"<project location>\".");
                    Console.ResetColor();
                    Console.WriteLine("Push Enter/Return to exit.");
                    Console.ReadLine();
                    Environment.Exit(1);
                }
                else if (_sdkLocation == null && _projectLocation != null)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine(
                        "The SDK location is not set. Please set it with the --SDKLocation \"<project location>\".");
                    Console.ResetColor();
                    Console.WriteLine("Push Enter/Return to exit.");
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
                    Console.WriteLine("Where's the SDK location? ");
                    _sdkLocation = Console.ReadLine();
                    if (_sdkLocation == null) Console.WriteLine("Please insert the path for the SDK please.\n");
                    else if (!Directory.Exists(_sdkLocation))
                        Console.Write("The directory isn't there. Please select an existing folder.\n");
                } while (_sdkLocation == null || !Directory.Exists(_sdkLocation));

                do
                {
                    //Ask the user what project to compile. Check if the folder is there and there's a js folder.
                    Console.WriteLine("\nWhere's the project you want to compile? ");
                    _projectLocation = Console.ReadLine();

                    if (_projectLocation == null) Console.WriteLine("Please specify the location of the folder.\n");
                    else if (!Directory.Exists(_projectLocation))
                        Console.WriteLine("The folder you've selected isn't present.\n");
                    else if (!Directory.Exists(Path.Combine(_projectLocation, "www", "js")))
                        Console.WriteLine("There is no js folder.\n");
                } while (_projectLocation == null || !Directory.Exists(_projectLocation) ||
                         !Directory.Exists(Path.Combine(_projectLocation, "www", "js")));


                //Ask the user for the file extension.
                Console.Write("\nWhat Extension will your game use (leave empty for .bin)? ");
                _fileExtension = Console.ReadLine();
                if (string.IsNullOrEmpty(_fileExtension)) _fileExtension = "bin";
                //This is the check if the tool should delete the JS files.
                Console.WriteLine(
                    "\nDo you want to:\n1. Test that the binary files are loaded properly?\n2. Prepare for publishing?\n(Default is 1) ");
                stringBuffer = Console.ReadLine();
                int.TryParse(stringBuffer, out _checkDeletion);
                _removeJsFiles = (_checkDeletion == 2);
                //}

                char charBuffer;
                if (_checkDeletion == 2)
                {
                    Console.WriteLine(
                        "\nWould you like to compress the game's files to an archive?\n1.Yes (delete the files as well).\n2.Yes (but leave the files intact).\n3. No.\n(Default is 3)");
                    charBuffer = Console.ReadKey().KeyChar;
                    _compressProject = !char.IsLetterOrDigit(charBuffer) ? Convert.ToInt32(charBuffer) : 3;
                }
                else
                {
                    //Ask if the user would like to test with nwjs.
                    Console.WriteLine("\nWould you like to test the project after compiling? (Y/N, Default is N)\n");
                    charBuffer = Console.ReadKey().KeyChar;
                    if (char.IsLetterOrDigit(charBuffer))
                    {
                        switch (charBuffer)
                        {
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
                    }
                    else _testProject = false;
                }
            }


            //The folder that the tool looks for.
            const string folderMap = "js";
            //Finding all the JS files.
            CoreCode.FileFinder(Path.Combine(_projectLocation, "www",  folderMap), "*.js");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("[{0}]", DateTime.Now);
            Console.ResetColor();
            Console.WriteLine("Removing binary files (if present)...");
            CoreCode.CleanupBin();
            //Preparing the compiler task.
            CoreCode.CompilerInfo.FileName = Path.Combine(_sdkLocation, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "nwjc.exe" : "nwjc");
            try
            {
                //Read from the FileMap (which is located in the CompilerCore library.
                //Compilation is done in parallel. Handy for multi-core systems.
                if (_parallelMode)
                {
                    Parallel.For(0, CoreCode.FileMap.Length, index =>
                    {
                        //Print the status of the compiler. Show which thread is compiling what as well.
                        Console.WriteLine("\n" + DateTime.Now + "\nThread #" + Thread.CurrentThread.ManagedThreadId +
                                          " is compiling " + CoreCode.FileMap[index] + "...\n");
                        //Call the compiler task.
                        CoreCode.CompilerWorkerTask(CoreCode.FileMap[index], _fileExtension, _removeJsFiles);
                        Console.WriteLine("\n" + DateTime.Now + "\nThread #" + Thread.CurrentThread.ManagedThreadId +
                                          " finished compiling " + CoreCode.FileMap[index] + ".\n");
                    });
                }

                else
                {
                    for(int index = 0; index < CoreCode.FileMap.Length; index++) {
                        //Print the status of the compiler. Show which thread is compiling what as well.
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.Write("[{0}]", DateTime.Now);
                        Console.ResetColor();
                        Console.WriteLine("Compiling " + CoreCode.FileMap[index] + "...");
                        //Call the compiler task.
                        CoreCode.CompilerWorkerTask(CoreCode.FileMap[index], _fileExtension, _removeJsFiles);
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.Write("[{0}]", DateTime.Now);
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine("Finished compiling " + CoreCode.FileMap[index] + ".");
                        Console.ResetColor();
                    }
                }
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write("[{0}]", DateTime.Now);
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("Finished compiling files.");
                Console.ResetColor();
                if (_testProject)
                {
                    Console.WriteLine("\nNW.js will now start. Give it a few seconds to start.");
                    CoreCode.RunTest(_sdkLocation, _projectLocation);
                }
                else if (_compressProject < 3 && _checkDeletion == 2)
                {
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write("[{0}]", DateTime.Now);
                    Console.ResetColor();
                    Console.WriteLine("Copying the game files to a temporary location...");
                    CoreCode.PreparePack(_projectLocation);
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write("[{0}]", DateTime.Now);
                    Console.ResetColor();
                    Console.WriteLine("Compressing files...");
                    CoreCode.CompressFiles(_projectLocation, _compressionLevel);
                    if (_compressProject == 1)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.Write("[{0}]", DateTime.Now);
                        Console.ResetColor();
                        Console.WriteLine("Deleting source files...");
                        CoreCode.DeleteFiles(_projectLocation);
                    }
                }
                Console.WriteLine("\nThe task was completed.");

            }
            catch (Exception e)
            {
                //TODO Improve the handling of the errors.
                Console.WriteLine(e);
                throw;

            }
            //Ask the user to press Enter (or Return).
            if (_settingsSet) return;
            Console.WriteLine("Push Enter/Return to exit.");
            Console.ReadLine();
        }
    }
}