using Prism.Commands;
using Prism.Mvvm;
using System.Diagnostics;
using System.Reflection;

namespace RMMVCookTool.GUI.ViewModels;
public sealed class AboutViewModel : BindableBase
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
    Path.Combine(AppContext.BaseDirectory, "Docs", "Manual.pdf");

    private static readonly string GreekReadme =
        Path.Combine(AppContext.BaseDirectory, "Docs", "Manual.el.pdf");

    public AboutViewModel()
    {
        OpenDocsCommand = new DelegateCommand(OpenReadme, CheckFile).ObservesProperty(() => AreDocsAvailable);
        string version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        ProgramVersionText = Resources.ProgramVersionLabelUiText + @" (" + version + @")";
        string docsFile = (System.Threading.Thread.CurrentThread.CurrentCulture.Name == "el-GR") ? GreekReadme : ReadmeFile;
        if (!File.Exists(docsFile)) AreDocsAvailable = false;
    }

    private bool CheckFile() => AreDocsAvailable;

    private void OpenReadme() {
        using Process fileopener = new();
        fileopener.StartInfo.FileName = (System.Threading.Thread.CurrentThread.CurrentCulture.Name == "el-GR") ? GreekReadme : ReadmeFile;
        fileopener.StartInfo.UseShellExecute = true;
        fileopener.Start();
    }
}
