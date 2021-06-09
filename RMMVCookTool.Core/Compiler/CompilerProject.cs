﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RMMVCookTool.Core.Utilities;

namespace RMMVCookTool.Core.Compiler
{
    public class CompilerProject : CompilerProjectBase
    {
        public string ProjectLocation { get; set; }
        public List<string> FileMap { get; set; }

        public string FileExtension { get; set; }

        public string GameFilesLocation { get; set; }

        public bool RemoveSourceCodeAfterCompiling { get; set; }

        public bool CompressFilesToPackage { get; set; }

        public bool RemoveFilesAfterCompression { get; set; }

        public int CompressionModeLevel { get; set; }

        public CompilerProject()
        {
            FileExtension = ".bin";
            CompressionModeLevel = 0;
        }

        public CompilerProject(string project)
        {
            ProjectLocation = project;
            FileExtension = ".bin";
            CompressionModeLevel = 0;
            FileMap = new List<string>();
            FileMap = CompilerUtilities.FileFinder(ProjectLocation, "*.js");
        }

        public CompilerProject(string project, string fileExtension, bool removeAfterCompile, bool compressToPackage, bool removeAfterCompression, int compressionLevel)
        {
            ProjectLocation = project;
            RemoveSourceCodeAfterCompiling = removeAfterCompile;
            FileExtension = fileExtension;
            CompressFilesToPackage = compressToPackage;
            RemoveFilesAfterCompression = removeAfterCompression;
            CompressionModeLevel = compressionLevel;
            FileMap = new List<string>();
            FileMap = CompilerUtilities.FileFinder(ProjectLocation, "*.js");
        }

        //This method starts the nw.exe file.
        /// <summary>
        /// Starts the NW.js compiler.
        /// </summary>
        /// <param name="index">The index in the list.</param>
        [MethodImplAttribute(MethodImplOptions.AggressiveOptimization)]
        public void CompileFile(int index)
        {
            CompilerUtilities.RecordToLog("Setting up the compiler...", 3);
            //Removing the JavaScript extension. Needed to place our own File Extension.
            //Setting up the compiler by throwing in two arguments.
            //The first bit (the one with the file variable) is the source.
            //The second bit (the one with the fileBuffer variable) makes the final file.
            CompilerInfo.Value.Arguments = "\"" + FileMap[index] + "\" \"" +
                                     FileMap[index].Replace(".js", "." + FileExtension, StringComparison.Ordinal) + "\"";
            //Making sure not to show the nwjc window. That program doesn't show anything of usefulness.
            CompilerInfo.Value.CreateNoWindow = true;
            CompilerInfo.Value.WindowStyle = ProcessWindowStyle.Hidden;
            //Run the compiler.
            CompilerUtilities.RecordToLog($"nwjc processing the file {FileMap[index]}...", 3);
            Process.Start(CompilerInfo.Value)?.WaitForExit();

            //If the user asked to remove the JS files, delete them.
            if (RemoveSourceCodeAfterCompiling) {
                CompilerUtilities.RecordToLog($"Removing the file {FileMap[index]}...", 3);
                File.Delete(FileMap[index]); 
            }
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
                    "--nwapp=\"" + ProjectLocation + "\"");
            else if (File.Exists(Path.Combine(sdkLocation,
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Game.exe" : "Game")))
                Process.Start(
                    Path.Combine(sdkLocation,
                        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Game.exe" : "Game"),
                    "--nwapp=\"" + ProjectLocation + "\"");
        }

        //This method compresses the files found on the temporary location.
        /// <summary>
        /// Compresses the game's files (after copying them in a temporary location) to a zip file named package.nw (app.nw on Mac).
        /// </summary>
        public void CompressFiles()
        {
            var packageOutput = Path.Combine(ProjectLocation, ArchiveName);
            if (File.Exists(packageOutput)) File.Delete(packageOutput);
            //Temporary prepare a string for stripping.
            string stripPart = ProjectLocation + (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\\" : "/");
            //List all the files in the game's www folder.
            CompilerUtilities.RecordToLog("Cataloging files...", 3);
            List<string> gameFiles =
                CompilerUtilities.FileFinder(GameFilesLocation, "*");
            CompilerUtilities.RecordToLog($"Found {gameFiles.Count}", 3);
            using (ZipArchive packageArchive = ZipFile.Open(packageOutput, ZipArchiveMode.Create))
            {
                foreach (var file in gameFiles)
                {
                    CompilerUtilities.RecordToLog($"Compressing {file}...", 3);                    
                    //Start adding files.
                    switch (CompressionModeLevel)
                    {
                        case 2:
                            packageArchive.CreateEntryFromFile(file, file.Replace(stripPart, "", StringComparison.Ordinal),
                                CompressionLevel.NoCompression);
                            break;
                        case 1:
                            packageArchive.CreateEntryFromFile(file, file.Replace(stripPart, "", StringComparison.Ordinal),
                                CompressionLevel.Fastest);
                            break;
                        default:
                            packageArchive.CreateEntryFromFile(file, file.Replace(stripPart, "", StringComparison.Ordinal),
                                CompressionLevel.Optimal);
                            break;
                    }
                }

                //Add the project.json files to finish the package.
                packageArchive.CreateEntryFromFile(Path.Combine(ProjectLocation, "package.json"), "package.json");
            }

        }

        //This method deletes the projects files.
        /// <summary>
        /// Deletes the project's files. Best used after compressing the project.
        /// </summary>
        public void DeleteFiles()
        {
            if (Directory.Exists(Path.Combine(ProjectLocation, "www"))) Directory.Delete(Path.Combine(ProjectLocation, "www"), true);
            if (File.Exists(Path.Combine(ProjectLocation, "package.json"))) File.Delete(Path.Combine(ProjectLocation, "package.json"));

        }
    }
}