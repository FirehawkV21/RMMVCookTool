using System.ComponentModel;
using System.Reflection;
using Prism;
using Prism.Navigation.Regions;
using RMMVCookTool.Core.Utilities;

namespace RMMVCookTool.GUI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainWindow(IRegionManager regionManager)
    {
        InitializeComponent();
        regionManager.RegisterViewWithRegion("MainShell", typeof(Views.MainView));
        regionManager.RegisterViewWithRegion("SecondaryShell", typeof(Views.AboutView));
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        CompilerUtilities.StartEngineLogger("CompilerGUI", true);
        CompilerUtilities.RecordToLog($"Cook Tool GUI, version {Assembly.GetExecutingAssembly().GetName().Version} started.", 0);
    }
    
    private void Window_Closing(object sender, CancelEventArgs e) => CompilerUtilities.CloseLog();
}
