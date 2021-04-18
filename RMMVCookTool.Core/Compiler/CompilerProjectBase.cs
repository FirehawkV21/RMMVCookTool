using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RMMVCookTool.Core.Compiler
{
    public class CompilerProjectBase
    {
        public Lazy<ProcessStartInfo> CompilerInfo { get; } = new Lazy<ProcessStartInfo>(() => new ProcessStartInfo(), true);
        protected static readonly string ArchiveName = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "app.nw" : "package.nw";
    }
}
