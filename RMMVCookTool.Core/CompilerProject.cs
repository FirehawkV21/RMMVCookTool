﻿using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace RMMVCookTool.Core
{
    public class CompilerProject : CompilerProjectBase
    {
        private readonly string _projectLocation;
        public readonly List<string> FileMap;

        public string FileExtension { get; set; }

        public bool RemoveSourceCodeAfterCompiling { get; set; }

        public bool CompressFilesToPackage { get; set; }

        public bool RemoveFilesAfterCompression { get; set; }

        public int CompressionModeLevel { get; set; }


        public CompilerProject(string project)
        {
            _projectLocation = project;
            FileMap = new List<string>();
            FileMap = CompilerUtilities.FileFinder(_projectLocation, "*.js");
        }

        public CompilerProject(string project, string fileExtension, bool removeAfterCompile, bool compressToPackage, bool removeAfterCompression, int compressionLevel)
        {
            _projectLocation = project;
            RemoveSourceCodeAfterCompiling = removeAfterCompile;
            FileExtension = fileExtension;
            CompressFilesToPackage = compressToPackage;
            RemoveFilesAfterCompression = removeAfterCompression;
            CompressionModeLevel = compressionLevel;
            FileMap = new List<string>();
            FileMap = CompilerUtilities.FileFinder(_projectLocation, "*.js");
        }

        //This method starts the nw.exe file.
        /// <summary>
        /// Starts the NW.js compiler.
        /// </summary>
        /// <param name="index">The index in the list.</param>
        public void CompileFile(int index)
        {
            //Removing the JavaScript extension. Needed to place our own File Extension.
            //Setting up the compiler by throwing in two arguments.
            //The first bit (the one with the file variable) is the source.
            //The second bit (the one with the fileBuffer variable) makes the final file.
            CompilerInfo.Arguments = "\"" + FileMap[index] + "\" \"" +
                                     FileMap[index].Replace(".js", "." + FileExtension) + "\"";
            //Making sure not to show the nwjc window. That program doesn't show anything of usefulness.   
            CompilerInfo.CreateNoWindow = true;
            CompilerInfo.WindowStyle = ProcessWindowStyle.Hidden;
            //Run the compiler.
            Process.Start(CompilerInfo)?.WaitForExit();
            //If the user asked to remove the JS files, delete them.
            if (RemoveSourceCodeAfterCompiling) File.Delete(FileMap[index]);
        }

        //This method starts the nw.exe file.
        /// <summary>
        /// Starts the NW.js binary.
        /// </summary>
        /// <param name="sdkLocation">The location of the NW.js SDK folder.</param>
        public void RunTest(in string sdkLocation)
        {
            if (File.Exists(Path.Combine(sdkLocation,
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "nwjs.exe" : "nwjs")))
                Process.Start(
                    Path.Combine(sdkLocation,
                        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "nwjs.exe" : "nwjs"),
                    "--nwapp=\"" + _projectLocation + "\"");
            else if (File.Exists(Path.Combine(sdkLocation,
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Game.exe" : "Game")))
                Process.Start(
                    Path.Combine(sdkLocation,
                        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Game.exe" : "Game"),
                    "--nwapp=\"" + _projectLocation + "\"");
        }

        //This method compresses the files found on the temporary location.
        /// <summary>
        /// Compresses the game's files (after copying them in a temporary location) to a zip file named package.nw (app.nw on Mac).
        /// </summary>
        public void CompressFiles()
        {
            string[] tempString =
                _projectLocation.Split(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? '\\' : '/');
            var packageOutput = Path.Combine(_projectLocation, ArchiveName);
            if (File.Exists(packageOutput)) File.Delete(packageOutput);
            using (ZipArchive packageArchive = ZipFile.Open(packageOutput, ZipArchiveMode.Create))
            {
                //Temporary prepare a string for stripping.
                string stripPart = _projectLocation + (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\\" : "/");
                //List all the files in the game's www folder.
                IEnumerable<string> gameFiles =
                    CompilerUtilities.FileFinder(Path.Combine(_projectLocation, tempString[^1]), "*");
                foreach (var file in gameFiles)
                {
                    //Start adding files.
                    switch (CompressionModeLevel)
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
                packageArchive.CreateEntryFromFile(Path.Combine(_projectLocation, "package.json"), "package.json");
            }

        }

        //This method deletes the projects files.
        /// <summary>
        /// Deletes the project's files. Best used after compressing the project.
        /// </summary>
        public void DeleteFiles()
        {
            if (Directory.Exists(Path.Combine(_projectLocation, "www"))) Directory.Delete(Path.Combine(_projectLocation, "www"), true);
            if (File.Exists(Path.Combine(_projectLocation, "package.json"))) File.Delete(Path.Combine(_projectLocation, "package.json"));

        }
    }
}