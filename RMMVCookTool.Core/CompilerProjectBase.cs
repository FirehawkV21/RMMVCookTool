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
        public static readonly ProcessStartInfo CompilerInfo = new ProcessStartInfo();
        protected static readonly string ArchiveName = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "app.nw" : "package.nw";
        protected static readonly string TempFolderLocation = Path.Combine(Path.GetTempPath(), "nwjspackage");
    }
}
