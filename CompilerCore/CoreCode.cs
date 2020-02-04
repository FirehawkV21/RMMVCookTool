using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CompilerCore
{
    public static class CoreCode
    {
        //Some variables needed for the compiler task.
        public static readonly ProcessStartInfo CompilerInfo = new ProcessStartInfo();
        private static readonly string ArchiveName = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "app.nw" : "package.nw";
        private static readonly string TempFolderLocation = Path.Combine(Path.GetTempPath(), "nwjspackage");

        //This bit of code handles copying a directory to a different location.
        /// <summary>
        /// Copy a folder (with it's contents) to a specified location.
        /// </summary>
        /// <param name="sourceDirName">The path of the folder to copy from.</param>
        /// <param name="destDirName">The path where the folder will be copied to.</param>
        /// <param name="copySubDirs">Copy the subdirectories as well.</param>
        private static void DirectoryCopy(in string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            var dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);

            var dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName)) Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            var files = dir.GetFiles();
            Parallel.ForEach(files, file =>
            {
                var temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            });

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
                Parallel.ForEach(dirs, subDir =>
                {
                    var tempPath = Path.Combine(destDirName, subDir.Name);
                    DirectoryCopy(subDir.FullName, tempPath, true);
                });
        }

        //This the code to search for files. Pretty simple, actually.

        /// <summary>
        /// Runs a search for files and adds them to the array.
        /// </summary>
        /// <param name="path">Path for Search.</param>
        /// <param name="extension">File Extension.</param>
        public static List<string> FileFinder(in string path, in string extension)
        {
            return Directory.EnumerateFiles(path, extension, SearchOption.AllDirectories).ToList();
        }
        //This bit of code removes binary files when requested. Must run after FileFinder.
        /// <summary>
        /// Removes binary files found in the FileMap array.
        /// </summary>
        public static void CleanupBin(in List<string> fileMap)
        {
            //Do a normal loop for each entry on the FileMap array.
#pragma warning disable CA1062 // Validate arguments of public methods
            foreach (string file in fileMap)
#pragma warning restore CA1062 // Validate arguments of public methods
            {
                //This does a small search in the path specified in the FileMap.
                //Adding the .* will allow us to search all the files that have an extension.
                var deletionMap = Directory.GetFiles(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file) + ".*");
                //Doing a parallel loop here to speed up the cleanup process.
                Parallel.ForEach(deletionMap, fileToDelete =>
                {
                    //Run a check if the file in the array is actually a JavaScript file.
                    //If not, delete it.
                    if (fileToDelete != file) File.Delete(fileToDelete);
                });
                //Cleaning up the deletionMap array before refilling it.
                Array.Clear(deletionMap, 0, deletionMap.Length);
            }
        }

        //The code that handles the nwjc.
        /// <summary>
        /// The code that handles the compilation process.
        /// </summary>
        /// <param name="file">The file to compile.</param>
        /// <param name="extension">The desired file extension.</param>
        /// <param name="removeJs">Remove the JS file after compiling.</param>
        public static void CompilerWorkerTask(in string file, in string extension, in bool removeJs)
        {
            //Removing the JavaScript extension. Needed to place our own File Extension.
            //Setting up the compiler by throwing in two arguments.
            //The first bit (the one with the file variable) is the source.
            //The second bit (the one with the fileBuffer variable) makes the final file.
            CompilerInfo.Arguments = "\"" + file + "\" \"" + file.Replace(".js", "." + extension) + "\"";
            //Making sure not to show the nwjc window. That program doesn't show anything of usefulness.   
            CompilerInfo.CreateNoWindow = true;
            CompilerInfo.WindowStyle = ProcessWindowStyle.Hidden;
            //Run the compiler.
            Process.Start(CompilerInfo)?.WaitForExit();
            //If the user asked to remove the JS files, delete them.
            if (removeJs) File.Delete(file);
        }
        //This method starts the nw.exe file.
        /// <summary>
        /// Starts the NW.js binary.
        /// </summary>
        /// <param name="sdkLocation">The location of the NW.js SDK folder.</param>
        /// <param name="projectLocation">The location of the project.</param>
        public static void RunTest(in string sdkLocation, in string projectLocation)
        {
            if (File.Exists(Path.Combine(sdkLocation, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "nwjs.exe" : "nwjs")))
                Process.Start(Path.Combine(sdkLocation, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "nwjs.exe" : "nwjs"), "--nwapp=\"" + projectLocation + "\"");
            else if (File.Exists(Path.Combine(sdkLocation, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Game.exe" : "Game")))
                Process.Start(Path.Combine(sdkLocation, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Game.exe" : "Game"),
                    "--nwapp=\"" + projectLocation + "\"");
        }
        //This method is used to build an archive.
        /// <summary>
        /// Copies the files to a temporary location (for use with CompressFiles).
        /// </summary>
        /// <param name="inputFolder">The location of the project to compress.</param>
        public static void PreparePack(in string inputFolder)
        {
            if (Directory.Exists(TempFolderLocation)) Directory.Delete(TempFolderLocation, true);
            DirectoryCopy(Path.Combine(inputFolder, "www"),
                Path.Combine(TempFolderLocation, "www"), true);
            File.Copy(Path.Combine(inputFolder, "package.json"),
                Path.Combine(TempFolderLocation, "package.json"), true);
        }
        //This method compresses the files found on the temporary location.
        /// <summary>
        /// Compresses the game's files (after copying them in a temporary location) to a zip file named package.nw (app.nw on Mac).
        /// </summary>
        /// <param name="projectLocation">The source path. Helpful for cases that the drop area is different.</param>
        /// <param name="gameFolder">The location of the game folder. Used for reference so it can find the correct folder.</param>
        /// <param name="deployArea">The destination path for the archive.</param>
        /// <param name="compressionSelector">What kind of compression will be applied? 0 = Optimal, 1 = Fastest, 2 = None</param>
        /// <param name="useSafeMode">Use the old but safe way of compressing the files.</param>
        public static void CompressFiles(in string projectLocation, in string gameFolder, in string deployArea, in int compressionSelector, in bool useSafeMode)
        {
            string[] tempString = gameFolder.Split(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? '\\' : '/');
            var packageOutput = Path.Combine(deployArea, ArchiveName);
            if (File.Exists(packageOutput)) File.Delete(packageOutput);

            if (!useSafeMode)
            {
                using (ZipArchive packageArchive = ZipFile.Open(packageOutput, ZipArchiveMode.Create))
                {
                    //Temporary prepare a string for stripping.
                    string stripPart = projectLocation + (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\\" : "/");
                    //List all the files in the game's www folder.
                    IEnumerable<string> gameFiles = FileFinder(Path.Combine(projectLocation, tempString[^1]), "*");
                    foreach (var file in gameFiles)
                    {
                        //Start adding files.
                        switch (compressionSelector)
                        {
                            case 2:
                                packageArchive.CreateEntryFromFile(file, file.Replace(stripPart, ""),
                                    CompressionLevel.NoCompression);
                                break;
                            case 1:
                                packageArchive.CreateEntryFromFile(file, file.Replace(stripPart, ""),
                                    CompressionLevel.Fastest);
                                break;
                            default:
                                packageArchive.CreateEntryFromFile(file, file.Replace(stripPart, ""),
                                    CompressionLevel.Optimal);
                                break;
                        }
                    }
                    //Add the project.json files to finish the package.
                    packageArchive.CreateEntryFromFile(Path.Combine(projectLocation, "package.json"), "package.json");
                };
            }
            else
            {
                //Compress the files from the temp location.
                switch (compressionSelector)
                {
                    case 2:
                        ZipFile.CreateFromDirectory(TempFolderLocation,
                            packageOutput, CompressionLevel.NoCompression, false);
                        break;
                    case 1:
                        ZipFile.CreateFromDirectory(TempFolderLocation,
                            packageOutput, CompressionLevel.Fastest, false);
                        break;
                    default:
                        ZipFile.CreateFromDirectory(TempFolderLocation,
                            packageOutput, CompressionLevel.Optimal, false);
                        break;
                }
                //Delete the temp folder once it finishes.
                Directory.Delete(TempFolderLocation, true);
            }

        }
        //This method deletes the projects files.
        /// <summary>
        /// Deletes the project's files. Best used after compressing the project.
        /// </summary>
        /// <param name="projectLocation">The location of the project.</param>
        public static void DeleteFiles(in string projectLocation)
        {
            if (Directory.Exists(Path.Combine(projectLocation, "www"))) Directory.Delete(Path.Combine(projectLocation, "www"), true);
            if (File.Exists(Path.Combine(projectLocation, "package.json"))) File.Delete(Path.Combine(projectLocation, "package.json"));

        }

    }
}