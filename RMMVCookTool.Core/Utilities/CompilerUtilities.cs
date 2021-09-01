using Serilog;
using System.Text.Json;

namespace RMMVCookTool.Core.Utilities
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
            CompilerUtilities.RecordToLog("Searching for existing binary files...", 3);
            //Do a normal loop for each entry on the FileMap array.
            foreach (string file in fileMap)
            {
                RecordToLog($"Checking for binary files that are related to {file}.", 3);
                //This does a small search in the path specified in the FileMap.
                //Adding the .* will allow us to search all the files that have an extension.
                var deletionMap = Directory.GetFiles(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file) + ".*");
                RecordToLog($"Found {deletionMap.Length} files related to {Path.GetFileName(file)}.", 3);
                //Doing a parallel loop here to speed up the cleanup process.
                Parallel.ForEach(deletionMap, fileToDelete =>
                {
                    //Run a check if the file in the array is actually a JavaScript file.
                    //If not, delete it.
                    if (fileToDelete != file){
                        RecordToLog($"Thread #{Thread.CurrentThread.ManagedThreadId} is deleting {Path.GetFileName(fileToDelete)}...", 3);
                        File.Delete(fileToDelete);
                        }
                });
                //Cleaning up the deletionMap array before refilling it.
                Array.Clear(deletionMap, 0, deletionMap.Length);
            }
            RecordToLog("Completed the removal of the previous binary files.", 3);
        }

        public static void RemoveDebugFiles(in string projectLocation)
        {
            RecordToLog("Checking for the jsconfig.json file...", 3);
            if (File.Exists(Path.Combine(projectLocation, "js", "jsconfig.json")))
            {
                RecordToLog("Found it. Removing...", 3);
                File.Delete(Path.Combine(projectLocation, "js", "jsconfig.json"));
            }
            else RecordToLog("Not found.", 3);
            RecordToLog("Searching for Typescript definition files...", 3);
            var TSDeletionMap = Directory.GetFiles(projectLocation, ".d.ts", SearchOption.AllDirectories);
            RecordToLog($"Found {TSDeletionMap.Length}.", 3);
            foreach (string file in TSDeletionMap)
            {
                RecordToLog($"Removing{file}...",3);
                File.Delete(file);
            }
            RecordToLog("Searching for JS Map files...", 3);
            var JsFileMaps = Directory.GetFiles(projectLocation, ".js.map", SearchOption.AllDirectories);
            RecordToLog($"Found {JsFileMaps.Length} JS Map files.",3);
            foreach (string file in JsFileMaps) {
                RecordToLog($"Removing{file}...", 3);
                File.Delete(file); 
            }
            RecordToLog("Completed the removal of debug files.", 3);
        }

        public static string GetProjectFilesLocation(string projectLocation)
        {
            if (File.Exists(projectLocation))
            {
                var input = File.ReadAllText(projectLocation);
                using (JsonDocument inputJson = JsonDocument.Parse(input))
                {
                    var tempstring = inputJson.RootElement.GetProperty("main");
                    if (tempstring.GetString() != null)
                    {
                        string[] dataPart = tempstring.GetString().Split('/');
                        string tempString2 = dataPart[0];
                        if (dataPart.Length >= 2)
                        {
                            for (int i = 1; i < dataPart.Length - 2; i++)
                            {
                                tempString2 += dataPart[i] + ((RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) ? "\\" : "/");
                            }
                        }
                        else
                        {
                            if (tempString2.Contains(".html")) tempString2 = "";
                        }
                        return Path.Combine(projectLocation.Replace(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\\package.json" : "/package.json", "", StringComparison.Ordinal), tempString2);
                    }
                    else return "Null";
                }
            }
            else return "Unknown";
        }

        public static void StartEngineLogger (string CompilerName, bool needsConsoleForLog)
        {
#if DEBUG
            Log.Logger = (needsConsoleForLog) ? new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Console().WriteTo.File(Path.Combine(Path.GetTempPath(), $"CompilerSession-{CompilerName}-{DateTime.Now:yyyyMMdd-hhmm}.log")).CreateLogger() : new LoggerConfiguration().MinimumLevel.Debug().WriteTo.File(Path.Combine(Path.GetTempPath(), $"CompilerSession-{CompilerName}-{DateTime.Now:yyyyMMdd-hhmm}.log")).CreateLogger();
#else
            Log.Logger = (needsConsoleForLog) ? new LoggerConfiguration().MinimumLevel.Information().WriteTo.Console().WriteTo.File(Path.Combine(Path.GetTempPath(), $"CompilerSession-{CompilerName}-{DateTime.Now:yyyyMMdd-hhmm}.log")).CreateLogger() : new LoggerConfiguration().MinimumLevel.Information().WriteTo.File(Path.Combine(Path.GetTempPath(), $"CompilerSession-{CompilerName}-{DateTime.Now:yyyyMMdd-hhmm}.log")).CreateLogger();
#endif
        }

        public static void RecordToLog(Exception ex)
        {
            Log.Fatal(ex, $" Crash! ");
        }

        public static void RecordToLog(string message, int type)
        {
            switch (type)
            {
                case 3:
                    Log.Debug($"[Internal mechanism]{message}");
                    break;
                case 2:
                    Log.Error($"{message}");
                    break;
                case 1:
                    Log.Warning($"{message}");
                    break;
                case 0:
                    Log.Information($"{message}");
                    break;
            }
        }

        public static void CloseLog()
        {
            Log.CloseAndFlush();
        }
    }
}
