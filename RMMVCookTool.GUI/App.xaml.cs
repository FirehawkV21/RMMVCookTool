global using System;
global using System.Windows;
global using RMMVCookTool.GUI.Properties;
global using Ookii.Dialogs.Wpf;
global using System.IO;
using Prism.DryIoc;
using Prism.Ioc;

namespace RMMVCookTool.GUI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : PrismApplication
{
    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        // register other needed services here
    }

    protected override Window CreateShell()
    {
        var w = Container.Resolve<MainWindow>();
        return w;
    }
}
