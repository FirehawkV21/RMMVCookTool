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
    }
}
