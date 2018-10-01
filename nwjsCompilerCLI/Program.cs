using System;
using System.IO;
using System.Threading;
using CompilerCore;

namespace nwjsCompilerCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            string SdkLocation;
            string ProjectLocation;
            string fileExtension = "bin";
            int modeSelected;
            bool removeJsFiles = false;
            do
            {
                Console.WriteLine("Where's the SDK location? ");
                SdkLocation = Console.ReadLine();
                if (SdkLocation == null) Console.WriteLine("Please insert the path for the SDK please.\n");
                else if (!Directory.Exists(SdkLocation))
                    Console.Write("The directory isn't there. Please select an existing folder.\n");
            } while (SdkLocation == null || !Directory.Exists(SdkLocation));

        Console.WriteLine("\nWhere's the project where you want to compile? ");
            ProjectLocation = Console.ReadLine();

            //do
            //{
            //    Console.WriteLine(
            //        "Would you like:\n1. Compile JavaScript?\n2. Compile and package to package.nw?\n3. Test the game?\n");
            //    modeSelected = Console.Read();
            //    if (modeSelected < 1 && modeSelected > 2)Console.Write("You didn't pick any of the answers. Try again.");
            //}
            // while (modeSelected < 1 && modeSelected > 2);

            //if (modeSelected != 3)
            //{
                Console.Write("\nWhat Extension will your game use (leave empty for .bin)? ");
                fileExtension = Console.ReadLine() ?? "bin";
            Console.WriteLine("\nDo you want to:\n1. Test that the binary files are loaded properly?\n2. Prepare for publishing?");
                int checkDeletion = Console.Read();
                 removeJsFiles = (checkDeletion == 2);
            //}
            //switch (modeSelected)
            //{
            //    case 1:
            //    case 2:
                    string folderMap = "js";
                    CoreCode.FileFinder(ProjectLocation + "\\www\\" + folderMap + "\\", "*.js");
                    CoreCode.CompilerInfo.FileName = SdkLocation + "\\nwjc.exe";
                    try
                    {
                        foreach (var fileName in CoreCode.FileMap)
                        {
                            Console.WriteLine("\n" + DateTime.Now + "\nCompiling " + fileName + "...\n");
                            Thread.Sleep(200);
                            CoreCode.CompilerWorkerTask(fileName, fileExtension, removeJsFiles);
                            Thread.Sleep(200);
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;

                    }
                    //break;
            //}
            Console.WriteLine("Push Enter/Return to exit.");
            Console.ReadLine();
        }
    }
}
