using System.IO.Compression;
using RMMVCookTool.Core.Utilities;
using System.Linq;

namespace RMMVCookTool.Core.Compiler;

public class CompilerProject : CompilerProjectBase
{
    public string ProjectLocation { get; set; }
    public List<string> FileMap { get; set; }
    public string GameFilesLocation { get; set; }
    public ProjectSettings Setup { get; set; }

    public CompilerProject()
    {
        Setup.FileExtension = ".bin";
        Setup.CompressionLevel = 0;
    }

    public CompilerProject(string project)
    {
        ProjectLocation = project;
        Setup.FileExtension = ".bin";
        Setup.CompressionLevel = 0;
        FileMap = new List<string>();
        FileMap = CompilerUtilities.FileFinder(ProjectLocation, "*.js");
    }

    public CompilerProject(string project, string fileExtension, bool removeAfterCompile, bool compressToPackage, bool removeAfterCompression, int compressionLevel)
    {
        ProjectLocation = project;
        Setup.RemoveSourceFiles = removeAfterCompile;
        Setup.FileExtension = fileExtension;
        Setup.CompressProjectFiles = compressToPackage;
        Setup.RemoveFilesAfterCompression = removeAfterCompression;
        Setup.CompressionLevel = compressionLevel;
        FileMap = new List<string>();
        FileMap = CompilerUtilities.FileFinder(ProjectLocation, "*.js");
    }

    //This method starts the nw.exe file.
    /// <summary>
    /// Starts the NW.js compiler.
    /// </summary>
    /// <param name="index">The index in the list.</param>
    public void CompileFile(int index)
    {
        CompilerUtilities.RecordToLog("Setting up the compiler...", 3);
        //Removing the JavaScript extension. Needed to place our own File Extension.
        //Setting up the compiler by throwing in two arguments.
        //The first bit (the one with the file variable) is the source.
        //The second bit (the one with the fileBuffer variable) makes the final file.
        CompilerInfo.Value.Arguments = "\"" + FileMap[index] + "\" \"" +
                                 FileMap[index].Replace(".js", "." + Setup.FileExtension, StringComparison.Ordinal) + "\"";
        //Making sure not to show the nwjc window. That program doesn't show anything of usefulness.
        CompilerInfo.Value.CreateNoWindow = true;
        CompilerInfo.Value.WindowStyle = ProcessWindowStyle.Hidden;
        //Run the compiler.
        CompilerUtilities.RecordToLog($"nwjc processing the file {FileMap[index]}...", 3);
        Process.Start(CompilerInfo.Value)?.WaitForExit();

        //If the user asked to remove the JS files, delete them.
        if (Setup.RemoveSourceFiles) {
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
        string packageOutput = Path.Combine(ProjectLocation, ArchiveName);
        if (File.Exists(packageOutput)) File.Delete(packageOutput);
        //Temporary prepare a string for stripping.
        string stripPart = ProjectLocation + (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\\" : "/");
        //List all the files in the game's www folder.
        CompilerUtilities.RecordToLog("Cataloging files...", 3);
        List<string> tempList = CompilerUtilities.FileFinder(GameFilesLocation, "*");
        List<string> gameFiles = FilterFiles(tempList);

        CompilerUtilities.RecordToLog($"Found {gameFiles.Count}", 3);
        using (ZipArchive packageArchive = ZipFile.Open(packageOutput, ZipArchiveMode.Create))
        {
            foreach (string file in gameFiles)
            {
                CompilerUtilities.RecordToLog($"Compressing {file}...", 3);                    
                //Start adding files.
                switch (Setup.CompressionLevel)
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
                if (Setup.RemoveFilesAfterCompression) File.Delete(file);

            }

            //Add the project.json files to finish the package.
            if (File.Exists(Path.Combine(ProjectLocation, "package.json")) && ProjectLocation != GameFilesLocation)
            {
                packageArchive.CreateEntryFromFile(Path.Combine(ProjectLocation, "package.json"), "package.json");
                if (Setup.RemoveFilesAfterCompression) File.Delete(Path.Combine(ProjectLocation, "package.json"));
            }
            if (Setup.RemoveFilesAfterCompression) CleanupFolders();
        }

    }

    /// <summary>
    /// Filters the list from the files of nwjs.
    /// </summary>
    /// <param name="originalList">A list that needs cleaning.</param>
    /// <returns>A list that has refrences of nwjs files removed.</returns>
    public static List<string> FilterFiles (in List<string> originalList)
    {
        List<string> finalList = originalList;
        bool notCleanedUp = true;
        while (notCleanedUp)
        {
            notCleanedUp = false;
            foreach (string file in finalList)
            {
                if (file.Contains(".pak") || file.Contains(".pak.info") || file.Contains(".exe") || file.Contains(".nexe") || file.Contains(".dll") || file.Contains("pnacl") || file.Contains("swiftshader") || file.Contains("credits.html") || file.Contains(".dat") || file.Contains("v8_context_snapshot.bin") || file.Contains("save") || file.Contains(".nw"))
                {
                    notCleanedUp = true;
                    finalList.Remove(file);
                    break;
                }
            }
        }

        return finalList;
    }

    private void CleanupFolders()
    {
        List<string> AllFolders = Directory.EnumerateDirectories(ProjectLocation).ToList();
        List<string> GameFolders = new();
        GameFolders.AddRange(from string directory in AllFolders
                             where !directory.Contains("swiftshader") && !directory.Contains("locales") && !directory.Contains("pnacl")
                             select directory);
        foreach (string folder in from string folder in GameFolders
                               where Directory.Exists(folder)
                               select folder)
        {
            Directory.Delete(folder, true);
        }
    }
}
