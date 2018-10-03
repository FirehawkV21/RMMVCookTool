using System;
using System.IO;
using System.Runtime;
using System.Windows;

namespace nwjsCookToolUI
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            var profileLocation = Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData), "RMMVCompiler");
            if (!Directory.Exists(profileLocation)) Directory.CreateDirectory(profileLocation);
            ProfileOptimization.SetProfileRoot(profileLocation);
            ProfileOptimization.SetProfileRoot("RMMVCompiler.Profile");
        }
    }
}