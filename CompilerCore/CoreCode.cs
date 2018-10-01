using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
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
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            Parallel.ForEach(files, file =>
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            });

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                Parallel.ForEach(dirs, subDir =>
                {
                    string tempPath = Path.Combine(destDirName, subDir.Name);
                    DirectoryCopy(subDir.FullName, tempPath, true);
                });
            }
        }

        //This the code to search for files. Pretty simple, actually.
        public static void FileFinder(string path, string extension)
        {
            FileMap = Directory.GetFiles(path, extension, SearchOption.AllDirectories);
        }
        //The code that handles the nwjc.
        public static void CompilerWorkerTask(string file, string extension, bool removeJs)
        {
                //Removing the JavaScript extension. Needed to place our own File Extension.
                string fileBuffer = file.Replace(".js", "");
                //Setting up the compiler by throwing in two arguemnts.
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
    }
}
