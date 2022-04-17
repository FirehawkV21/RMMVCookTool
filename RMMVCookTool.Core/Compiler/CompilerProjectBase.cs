global using System;
global using System.Diagnostics;
global using System.Runtime.InteropServices;
global using System.Collections.Generic;
global using System.Linq;
global using System.IO;

namespace RMMVCookTool.Core.Compiler;

public class CompilerProjectBase
{
    public Lazy<ProcessStartInfo> CompilerInfo { get; } = new Lazy<ProcessStartInfo>(() => new ProcessStartInfo(), true);
    protected static readonly string ArchiveName = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "app.nw" : "package.nw";
}
