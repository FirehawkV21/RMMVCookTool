using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CompilerCore
{
    public class CoreCode
    {
        private static readonly Process CompilerProcess = new Process();
        public static readonly ProcessStartInfo CompilerInfo = new ProcessStartInfo();
        public static string[] FileMap;

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

        public static void CompilerWorkerTask(IEnumerable<string> fileMap, string extension, bool removeJs)
        {
            foreach (var file in fileMap)
            {
                string fileBuffer = file.Replace(".js", "");
               // OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\nCompiling " + file + "...";
                Thread.Sleep(200);
                CompilerInfo.Arguments = "\"" + file + "\"" + " " + "\"" + fileBuffer + "." + extension + "\"";
                CompilerInfo.CreateNoWindow = true;
                CompilerInfo.WindowStyle = ProcessWindowStyle.Hidden;
                CompilerProcess.StartInfo = CompilerInfo;
                CompilerProcess.Start();
                CompilerProcess.WaitForExit();
                if (removeJs) File.Delete(file);
                Thread.Sleep(3000);
                //OutputArea.Text = OutputArea.Text + "\n Compiled on " + DateTime.Now + ".\n";
                Thread.Sleep(200);

            }
        }
    }
}
