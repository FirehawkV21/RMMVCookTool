using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CompilerCore;

namespace nwjsCompilerCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            string SdkLocation;
            string ProjectLocation;
            string fileExtension;
            bool removeJsFiles;
            do
            {
                Console.WriteLine("Where's the SDK location? ");
                SdkLocation = Console.ReadLine();
                if (SdkLocation == null) Console.WriteLine("Please insert the path for the SDK please.\n");
                else if (!Directory.Exists(SdkLocation))
                    Console.Write("The directory isn't there. Please select an existing folder.\n");
            } while (SdkLocation == null || !Directory.Exists(SdkLocation));

            do
            {
                Console.WriteLine("\nWhere's the project where you want to compile? ");
                ProjectLocation = Console.ReadLine();
                if (ProjectLocation == null) Console.WriteLine("Please specify the location of the folder.\n");
                else if(!Directory.Exists(ProjectLocation)) Console.WriteLine("The folder you've selected isn't present.\n");
                else if(!Directory.Exists(ProjectLocation + "\\www\\js")) Console.WriteLine("There is no js folder.\n");
            }
             while (ProjectLocation == null || !Directory.Exists(ProjectLocation) ||
                     !Directory.Exists(ProjectLocation + "\\www\\js"));

            //do 
            //{
            //    Console.WriteLine(
            //        "\nWould you like to:\n1. Compile JavaScript?\n2. Compile and package to package.nw?\n");
            //    modeSelected = Convert.ToInt32(Console.Read());
            //    if (modeSelected < 1 && modeSelected > 2) Console.Write("You didn't pick any of the answers. Try again.\n");
            //}
            //while (modeSelected < 1 && modeSelected > 2);

            //if (modeSelected != 3)
            //{
                Console.Write("\nWhat Extension will your game use (leave empty for .bin)? ");
                fileExtension = Console.ReadLine() ?? "bin";
                Console.WriteLine("\nDo you want to:\n1. Test that the binary files are loaded properly?\n2. Prepare for publishing?\n(Default is 1)\n");
                int checkDeletion = Convert.ToInt32(Console.ReadLine());
                removeJsFiles = (checkDeletion == 2);
            //}

                    string folderMap = "js";
                    CoreCode.FileFinder(ProjectLocation + "\\www\\" + folderMap + "\\", "*.js");
                    CoreCode.CompilerInfo.FileName = SdkLocation + "\\nwjc.exe";
                    try
                    {
                        Parallel.ForEach (CoreCode.FileMap, fileName =>
                        {
                            Console.WriteLine("\n" + DateTime.Now + "\nThread #"+ Thread.CurrentThread.ManagedThreadId + " is compiling " + fileName + "...\n");
                            CoreCode.CompilerWorkerTask(fileName, fileExtension, removeJsFiles);
                        });

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;

                    }
            Console.WriteLine("Push Enter/Return to exit.");
            Console.ReadLine();
        }
    }
}
