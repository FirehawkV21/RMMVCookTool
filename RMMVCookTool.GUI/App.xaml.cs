global using System;
global using System.Windows;
global using RMMVCookTool.GUI.Properties;
global using Ookii.Dialogs.Wpf;
global using System.IO;
using Prism.DryIoc;
using Prism.Ioc;
using System.Runtime;

namespace RMMVCookTool.GUI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public sealed partial class App : PrismApplication
{
    public App()
    {
        string ProfilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RMMVCookTool", "JITProfile");
        if (!Directory.Exists(ProfilePath)) Directory.CreateDirectory(ProfilePath);
        ProfileOptimization.SetProfileRoot(ProfilePath);
        ProfileOptimization.StartProfile("MVCookToolUI.Profile");
    }
    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterDialog<Views.ProjectSettingsView, ViewModels.ProjectSettingsViewModel>("ProjectSettings");
        containerRegistry.RegisterDialog<Views.MetadataEditorView, ViewModels.MetadataEditorViewModel>("MetadataEditor");
        // register other needed services here
    }

    protected override Window CreateShell()
    {
        MainWindow w = Container.Resolve<MainWindow>();
        return w;
    }
}
