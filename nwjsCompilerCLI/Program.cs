using System;
using System.IO;
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
        private static int _checkDeletion;

        private static void Main(string[] args)
        {
            char charBuffer;
            Console.WriteLine("================================================");
            Console.WriteLine("= RPG Maker MV Cook Tool (.NET Core CLI Version)");
            Console.WriteLine("= Version D1.00 Update 1");
            Console.WriteLine("= Developed by AceOfAces.");
            Console.WriteLine("= Licensed under GNU General Public License v3.");
            Console.WriteLine("================================================");

            if (args.Length >= 1)
            {   
                for (int argnum = 0; argnum < args.Length; argnum++)
                {
                    if (args[argnum] == "--Parallel")
                    {
                        _parallelMode = true;
                        Console.WriteLine("Compiler is now running in parallel mode.");
                    }
                    else if (args[argnum] == "--SDKLocation")
                    {
                        _sdkLocation = args[argnum + 1];
                        _sdkLocation.Replace("\"", "");
                        if (Directory.Exists(_sdkLocation)) Console.WriteLine("SDK Location OK.");
                        else
                        {
                            Console.WriteLine("The location of the SDK doesn't exist.");
                            Console.WriteLine("Push Enter/Return to exit.");
                            Console.ReadLine();
                            Environment.Exit(1);
                        }
                    }
                    else if (args[argnum] == "--ProjectLocation")
                    {
                        _projectLocation = args[argnum + 1];
                        _projectLocation.Replace("\"", "");
                        if (Directory.Exists(_projectLocation)) Console.WriteLine("Project Location OK.");
                        else
                        {
                            Console.WriteLine("The location of the project doesn't exist.");
                            Console.WriteLine("Push Enter/Return to exit.");
                            Console.ReadLine();
                            Environment.Exit(1);
                        }
                    }
                    else if (args[argnum] == "--FileExtension")
                    {
                        if (_fileExtension.Contains("--")) continue;
                        _fileExtension = args[argnum + 1];
                        Console.WriteLine("The file extension is set to " + _fileExtension);
                    }
                    else if (args[argnum] == "--PackageApp")
                    {
                        _compressProject = args[argnum + 1] == "Final" ? 1 : 2;
                    }
                    else if (args[argnum] == "--ReleaseMode")
                        _checkDeletion = 2;
                    else if (args[argnum] == "--TestMode")
                    {
                        if (_compressProject >= 2)
                        {
                            Console.WriteLine("You can't compress and test the project at the moment.");
                            Console.WriteLine("Push Enter/Return to exit.");
                            Console.ReadLine();
                            Environment.Exit(1);
                        }
                        else _testProject = true;
                    }
                }

                if (_projectLocation != null && _sdkLocation != null) _settingsSet = true;
                else if (_projectLocation == null && _sdkLocation != null)
                {
                    Console.WriteLine("The Project location is not set. Please set it with the --ProjectLocation \"<project location>\".");
                    Console.WriteLine("Push Enter/Return to exit.");
                    Console.ReadLine();
                    Environment.Exit(1);
                }
                else if (_sdkLocation == null && _projectLocation != null)
                {
                    Console.WriteLine(
                        "The SDK location is not set. Please set it with the --SDKLocation \"<project location>\".");
                    Console.WriteLine("Push Enter/Return to exit.");
                    Console.ReadLine();
                    Environment.Exit(1);
                }
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

                //Leave the commented code as is for now.
                //do 
                //{
                //    Console.WriteLine(
                //        "\nWould you like to:\n1. Compile JavaScript?\n2. Compile and package to package.nw?\n");
                //    modeSelected = Convert.ToInt32(Console.Read());
                //    if (modeSelected < 1 && modeSelected > 2) Console.Write("You didn't pick any of the answers. Try again.\n");
                //}
                //while (modeSelected < 1 && modeSelected > 2);

                //if (modeSelected != 3)
                //{

                //Ask the user for the file extension.
                Console.Write("\nWhat Extension will your game use (leave empty for .bin)? ");
                _fileExtension = Console.ReadLine();
                if (string.IsNullOrEmpty(_fileExtension)) _fileExtension = "bin";
                //This is the check if the tool should delete the JS files.
                Console.WriteLine(
                    "\nDo you want to:\n1. Test that the binary files are loaded properly?\n2. Prepare for publishing?\n(Default is 1) ");
                var checkBuffer = Console.ReadLine();
                int.TryParse(checkBuffer, out _checkDeletion);
                _removeJsFiles = (_checkDeletion == 2);
                //}

                if (_checkDeletion == 2)
                {
                    Console.WriteLine(
                        "\nWould you like to compress the game's files to an archive?\n1.Yes (delete the files as well).\n2.Yes (but leave the files intact).\n3. No.\n(Default is 3)");
                    charBuffer = Console.ReadKey().KeyChar;
                    if (!char.IsLetterOrDigit(charBuffer))
                    {
                        _compressProject = Convert.ToInt32(charBuffer);
                    }
                    else _compressProject = 3;
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
            Console.WriteLine("\n" + DateTime.Now + "\nRemoving binary files (if present)...\n");
            CoreCode.CleanupBin();
            //Preparing the compiler task.
            CoreCode.CompilerInfo.FileName = Path.Combine(_sdkLocation, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "nwjc.exe" : "nwjc");
            try
            {
                //Read from the FileMap (which is located in the CompilerCore library.
                //Compilation is done in parallel. Handy for multi-core systems.
                if (_parallelMode)
                {
                    Parallel.ForEach(CoreCode.FileMap, fileName =>
                    {
                        //Print the status of the compiler. Show which thread is compiling what as well.
                        Console.WriteLine("\n" + DateTime.Now + "\nThread #" + Thread.CurrentThread.ManagedThreadId +
                                          " is compiling " + fileName + "...\n");
                        //Call the compiler task.
                        CoreCode.CompilerWorkerTask(fileName, _fileExtension, _removeJsFiles);
                        Console.WriteLine("\n" + DateTime.Now + "\nThread #" + Thread.CurrentThread.ManagedThreadId +
                                          " finished compiling " + fileName + ".\n");
                    });
                }

                else
                {
                    foreach(string fileName in CoreCode.FileMap) {
                        //Print the status of the compiler. Show which thread is compiling what as well.
                        Console.WriteLine("\n" + DateTime.Now + "\nCompiling " + fileName + "...\n");
                        //Call the compiler task.
                        CoreCode.CompilerWorkerTask(fileName, _fileExtension, _removeJsFiles);
                        Console.WriteLine("\n" + DateTime.Now + "\nFinished compiling " + fileName + ".\n");
                    }
                }

                Console.WriteLine("\nFinished compiling.");
                if (_testProject)
                    CoreCode.RunTest(_sdkLocation, _projectLocation);
                else if (_compressProject < 3 && _checkDeletion == 2)
                {
                    Console.WriteLine("Copying the game files to a temporary location...");
                    CoreCode.PreparePack(_projectLocation);
                    Console.WriteLine("Compressing files...");
                    CoreCode.CompressFiles(_projectLocation);
                    if (_compressProject == 2)
                    {
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
            Console.WriteLine("Push Enter/Return to exit.");
            Console.ReadLine();
        }
    }
}