using System.Runtime;
using System.Windows;

namespace nwjsCookToolUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
          ProfileOptimization.SetProfileRoot(System.AppDomain.CurrentDomain.BaseDirectory);
          ProfileOptimization.SetProfileRoot("RMMVCompiler.Profile");
        }
    }
}
