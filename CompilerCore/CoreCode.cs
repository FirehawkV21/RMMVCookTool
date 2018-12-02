using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CompilerCore
{
    public static class CoreCode
    {
        //Some variables needed for the compiler task.
        private static readonly Process CompilerProcess = new Process();
        public static readonly ProcessStartInfo CompilerInfo = new ProcessStartInfo();
        public static string[] FileMap;

        //This bit of code handles copying a directory to a different location.
        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
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
        public static void FileFinder(string path, string extension)
        {
            FileMap = Directory.GetFiles(path, extension, SearchOption.AllDirectories);
        }

        public static void CleanupBin()
        {
            //Do a normal loop for each entry on the FileMap array.
            foreach (var file in FileMap)
            {
                //This buffer makes the necessary query to do a search for the binaries.
                //We want to keep the file name to do the search.
                var fileBuffer = Path.GetFileNameWithoutExtension(file);
                //This does a small search in the path specified in the FileMap.
                //Adding the .* will allow us to search all the files that have an extension.
                var deletionMap = Directory.GetFiles(file.Replace(fileBuffer + ".js", ""), fileBuffer + ".*");
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
        public static void CompilerWorkerTask(string file, string extension, bool removeJs)
        {
            //Removing the JavaScript extension. Needed to place our own File Extension.
            var fileBuffer = file.Replace(".js", "");
            //Setting up the compiler by throwing in two arguments.
            //The first bit (the one with the file variable) is the source.
            //The second bit (the one with the fileBuffer variable) makes the final file.
            CompilerInfo.Arguments = "\"" + file + "\"" + " " + "\"" + fileBuffer + "." + extension + "\"";
            //Making sure not to show the nwjc window. That program doesn't show anything of usefulness.   
            CompilerInfo.CreateNoWindow = true;
            CompilerInfo.WindowStyle = ProcessWindowStyle.Hidden;
            //Run the compiler.
            Process.Start(CompilerInfo)?.WaitForExit();
            //If the user asked to remove the JS files, delete them.
            if (removeJs) File.Delete(file);
        }

        public static void RunTest(string sdkLocation, string projectLocation)
        {
            if (File.Exists(Path.Combine(sdkLocation, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "nwjs.exe" : "nwjs")))
                Process.Start(Path.Combine(sdkLocation, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "nwjs.exe": "nwjs"), "--nwapp=\"" + projectLocation + "\"");
            else if (File.Exists(Path.Combine(sdkLocation, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Game.exe" : "Game")))
                Process.Start(Path.Combine(sdkLocation, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Game.exe": "Game"),
                    "--nwapp=\"" + projectLocation + "\"");
        }

    }
}