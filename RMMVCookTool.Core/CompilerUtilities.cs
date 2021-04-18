using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RMMVCookTool.Core
{
    public static class CompilerUtilities
    {
        //This bit of code handles copying a directory to a different location.
        /// <summary>
        /// Copy a folder (with it's contents) to a specified location.
        /// </summary>
        /// <param name="sourceDirName">The path of the folder to copy from.</param>
        /// <param name="destDirName">The path where the folder will be copied to.</param>
        /// <param name="copySubDirs">Copy the subdirectories as well.</param>
        public static void DirectoryCopy(in string sourceDirName, string destDirName, bool copySubDirs)
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

        public static void CleanupBin(in List<string> fileMap)
        {
            //Do a normal loop for each entry on the FileMap array.
            foreach (string file in fileMap)
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

        public static void RemoveDebugFiles(in string projectLocation)
        {
            if (File.Exists(Path.Combine(projectLocation, "js", "jsconfig.json"))) File.Delete(Path.Combine(projectLocation, "js", "jsconfig.json"));
            var TSDeletionMap = Directory.GetFiles(projectLocation, "*.d.ts");
            foreach (string file in TSDeletionMap) File.Delete(file);
            var JsFileMaps = Directory.GetFiles(projectLocation, "*.js.map");
            foreach (string file in JsFileMaps) File.Delete(file);
        }
    }
}
