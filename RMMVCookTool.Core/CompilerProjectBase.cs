using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace RMMVCookTool.Core
{
    public class CompilerProjectBase
    {
        public Lazy<ProcessStartInfo> CompilerInfo { get; } = new Lazy<ProcessStartInfo>(() => new ProcessStartInfo(), true);
        protected static readonly string ArchiveName = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "app.nw" : "package.nw";
    }
}
