using Prism.Mvvm;
using System.Diagnostics;
using System.Reflection;

namespace RMMVCookTool.GUI.ViewModels;
public class AboutViewModel : BindableBase
{
    private string programVersion = "";
    public string ProgramVersionText {
        get => programVersion;
        set => SetProperty(ref programVersion, value);
    }

    public AboutViewModel()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
        string version = fvi.FileVersion;
        ProgramVersionText = Resources.ProgramVersionLabelUiText + @" (" + version + @")";
    }
}
