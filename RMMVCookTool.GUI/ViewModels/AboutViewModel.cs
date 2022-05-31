using Prism.Commands;
using Prism.Mvvm;
using System.Diagnostics;
using System.Reflection;

namespace RMMVCookTool.GUI.ViewModels;
public class AboutViewModel : BindableBase
{
    private string programVersion = "";
    private bool docsAvailable = true;
    public string ProgramVersionText {
        get => programVersion;
        set => SetProperty(ref programVersion, value);
    }
    public bool AreDocsAvailable { get => docsAvailable; set => SetProperty(ref docsAvailable, value); }
    public DelegateCommand OpenDocsCommand { get; private set; }
    private static readonly string ReadmeFile =
    Directory.GetParent(Assembly.GetExecutingAssembly().Location) + "\\Docs\\Manual.pdf";

    private static readonly string GreekReadme =
        Directory.GetParent(Assembly.GetExecutingAssembly().Location) + "\\Docs\\Manual.el.pdf";

    public AboutViewModel()
    {
        OpenDocsCommand = new DelegateCommand(OpenReadme, CheckFile).ObservesProperty(() => AreDocsAvailable);
        Assembly assembly = Assembly.GetExecutingAssembly();
        FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
        string version = fvi.FileVersion;
        ProgramVersionText = Resources.ProgramVersionLabelUiText + @" (" + version + @")";
        string docsFile = (System.Threading.Thread.CurrentThread.CurrentCulture.Name == "el-GR") ? GreekReadme : ReadmeFile;
        if (!File.Exists(docsFile)) AreDocsAvailable = false;
    }
    
    private bool CheckFile()
    {
        return AreDocsAvailable;
    }

    private void OpenReadme() {
        using Process fileopener = new();
        fileopener.StartInfo.FileName = "explorer";
        fileopener.StartInfo.Arguments = "\"" + Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString(), "Docs") + "\"";
        fileopener.Start();
    }
}
