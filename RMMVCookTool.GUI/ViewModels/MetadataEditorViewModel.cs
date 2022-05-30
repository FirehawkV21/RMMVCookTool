using Prism.Mvvm;
using Prism.Services.Dialogs;
using RMMVCookTool.Core.ProjectTemplate;
using System.Text.Json;

namespace RMMVCookTool.GUI.ViewModels;
public class MetadataEditorViewModel : BindableBase, IDialogAware
{
    #region Variables
    public string Title => Resources.GamePackageMetadataEditorTitleText;

    public event Action<IDialogResult> RequestClose;

    private string projectLocation;
    private string gameId;
    private string indexFileLocation;
    private string gameVersion;
    private bool enableNodeJs;
    private string chromiumFlags;
    private string jsFlags;
    private string gameName;
    private string gameIconLocation;
    private string windowTitle;
    private bool isWindowResizable;
    private int windowModeIndex;
    private int windowStartLocation;
    private int windowHeight;
    private int windowWidth;
    private int minHeight;
    private int minWidth;
    #endregion

    #region Properties
    internal ProjectMetadata ProjectMetadata { get; set; }
    public string GameId { get => gameId; set {
            ProjectMetadata.GameName = value;
            SetProperty(ref gameId, value);
        } }
    public string IndexFileLocation { get => indexFileLocation; set {
            ProjectMetadata.MainFile = value;
            SetProperty(ref indexFileLocation, value);
        } }
    public string GameVersion { get => gameVersion; set {
            ProjectMetadata.GameVersion = value;
            SetProperty(ref gameVersion, value);
        } }
    public bool EnableNodeJs { get => enableNodeJs; set {
            ProjectMetadata.UseNodeJs = value;
            SetProperty(ref enableNodeJs, value);
        } }
    public string ChromiumFlags { get => chromiumFlags; set {
            ProjectMetadata.ChromiumFlags = value;
            SetProperty(ref chromiumFlags, value);
        } }
    public string JsFlags { get => jsFlags; set {
            ProjectMetadata.JsFlags = value;
            SetProperty(ref jsFlags, value);
        } }
    public string GameName { get => gameName; set {
            ProjectMetadata.GameTitle = value;
            SetProperty(ref gameName, value);
        } }
    public string GameIconLocation { get => gameIconLocation; set {
            ProjectMetadata.WindowProperties.WindowIcon = value;
            SetProperty(ref gameIconLocation, value);
        } }
    public string WindowTitle { get => windowTitle; set {
            ProjectMetadata.WindowProperties.WindowTitle = value;
            SetProperty(ref windowTitle, value);
        } }
    public bool IsWindowResizable { get => isWindowResizable; set {
            ProjectMetadata.WindowProperties.IsResizable = value;
            SetProperty(ref isWindowResizable, value);
        } }
    public int WindowModeIndex { get => windowModeIndex; set {
            switch (value)
            {
                case 2:
                    ProjectMetadata.WindowProperties.StartAtFullScreen = false;
                    ProjectMetadata.WindowProperties.RunInKioskMode = true;
                    break;
                case 1:
                    ProjectMetadata.WindowProperties.StartAtFullScreen = true;
                    ProjectMetadata.WindowProperties.RunInKioskMode = false;
                    break;
                default:
                    ProjectMetadata.WindowProperties.StartAtFullScreen = false;
                    ProjectMetadata.WindowProperties.RunInKioskMode = false;
                    break;
            }
            SetProperty(ref windowModeIndex, value);
        } }
    public int WindowStartLocation { get => windowStartLocation; set {
            ProjectMetadata.WindowProperties.ScreenPosition = value switch
            {
                2 => "mouse",
                1 => "center",
                _ => "none",
            };
            SetProperty(ref windowStartLocation, value);
        } }
    public int WindowHeight { get => windowHeight; set {
            ProjectMetadata.WindowProperties.WindowHeight = (uint)value;
            SetProperty(ref windowHeight, value);
        } }
    public int WindowWidth { get => windowWidth; set {
            ProjectMetadata.WindowProperties.WindowWidth = (uint)value;
            SetProperty(ref windowWidth, value);
        } }
    public int MinHeight { get => minHeight; set {
            ProjectMetadata.WindowProperties.MinimumHeight = (uint)value;
            SetProperty(ref minHeight, value);
        } }
    public int MinWidth { get => minWidth; set {
            ProjectMetadata.WindowProperties.MinimumWidth = (uint)value;
            SetProperty(ref minWidth, value);
        } }
    #endregion

    public bool CanCloseDialog() => true;
    public void OnDialogClosed()
    {
        //Method intentionally left blank
    }
    public void OnDialogOpened(IDialogParameters parameters) 
    {
        projectLocation = parameters.GetValue<string>("location");
        if (File.Exists(Path.Combine(projectLocation, "package.json")))
        {
            string importFile = File.ReadAllText(Path.Combine(projectLocation, "package.json"));
            ProjectMetadata = JsonSerializer.Deserialize(importFile, ProjectMetadataSerializer.Default.ProjectMetadata);
        }
        else ProjectMetadata = new();
    }

    public void SaveJsonFile()
    {
        try
        {
            string output = JsonSerializer.Serialize(ProjectMetadata, ProjectMetadataSerializer.Default.ProjectMetadata);
            File.WriteAllText(Path.Combine(projectLocation, "package.json"), output);
            MessageDialog.ThrowCompleteMessage(Resources.SaveCompleteText);
        }
        catch (FileFormatException ex)
        {
            MessageDialog.ThrowErrorMessage(ex);
        }
        catch (PathTooLongException ex)
        {
            MessageDialog.ThrowErrorMessage(ex);
        }

        catch (IOException ex)
        {
            MessageDialog.ThrowErrorMessage(ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            MessageDialog.ThrowErrorMessage(ex);
        }
    }
    public void CloseDialog()
    {
        ButtonResult result = ButtonResult.OK;
        RequestClose.Invoke(new DialogResult(result));
    }
}
