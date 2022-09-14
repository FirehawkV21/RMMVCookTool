using RMMVCookTool.CLI.Properties;
using RMMVCookTool.Core.Utilities;
using System;
using System.Reflection;

namespace RMMVCookTool.CLI;

internal sealed class Program
{
    private static readonly CompilerEngine engine = new();

    private static void Main(string[] args)
    {
        #region Print App Info
        CompilerUtilities.StartEngineLogger("CompilerCLI", false);
        Console.WriteLine(Resources.SpilterText);
        Console.WriteLine(Resources.ProgramNameText);
        Console.WriteLine(Resources.ProgramVersionString, Assembly.GetExecutingAssembly().GetName().Version);
        Console.WriteLine(Resources.ProgramAuthorText);
        Console.WriteLine(Resources.ProgramLicenseText);
        Console.WriteLine(Resources.SpilterText);
        CompilerUtilities.RecordToLog($"Cook Tool CLI, version {Assembly.GetExecutingAssembly().GetName().Version} started.", 0);
        #endregion
        if (args.Length >= 1) engine.ProcessCommandLineArguments(args);
        else engine.StartSetup();
        engine.StartWorker();

    }
}
