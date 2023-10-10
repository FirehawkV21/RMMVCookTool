using Prism.Commands;
using Prism.Mvvm;
using Prism.Dialogs;
using RMMVCookTool.Core.ProjectTemplate;
using System.Text.Json;

namespace RMMVCookTool.GUI.ViewModels;
public sealed class MetadataEditorViewModel : BindableBase, IDialogAware
{
    #region Variables
    public string Title => Resources.GamePackageMetadataEditorTitleText;
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
    private string windowId;
    private bool isWindowResizable;
    private int windowModeIndex;
    private int windowStartLocation;
    private uint windowHeight;
    private uint windowWidth;
    private uint minHeight;
    private uint minWidth;
    #endregion

    #region Properties
    internal ProjectMetadata ProjectMetadata { get; set; }
    public string GameId {
        get => gameId; set {
            ProjectMetadata.GameName = value;
            SetProperty(ref gameId, value);
        }
    }
    public string IndexFileLocation {
        get => indexFileLocation; set {
            ProjectMetadata.MainFile = value;
            SetProperty(ref indexFileLocation, value);
        }
    }
    public string GameVersion {
        get => gameVersion; set {
            ProjectMetadata.GameVersion = value;
            SetProperty(ref gameVersion, value);
        }
    }
    public bool EnableNodeJs {
        get => enableNodeJs; set {
            ProjectMetadata.UseNodeJs = value;
            SetProperty(ref enableNodeJs, value);
        }
    }
    public string ChromiumFlags {
        get => chromiumFlags; set {
            ProjectMetadata.ChromiumFlags = value;
            SetProperty(ref chromiumFlags, value);
        }
    }
    public string JsFlags {
        get => jsFlags; set {
            ProjectMetadata.JsFlags = value;
            SetProperty(ref jsFlags, value);
        }
    }
    public string GameName {
        get => gameName; set {
            ProjectMetadata.GameTitle = value;
            SetProperty(ref gameName, value);
        }
    }
    public string GameIconLocation {
        get => gameIconLocation; set {
            ProjectMetadata.WindowProperties.WindowIcon = value;
            SetProperty(ref gameIconLocation, value);
        }
    }
    public string WindowTitle {
        get => windowTitle; set {
            ProjectMetadata.WindowProperties.WindowTitle = value;
            SetProperty(ref windowTitle, value);
        }
    }
    public string WindowId {
        get => windowId; set {
            ProjectMetadata.WindowProperties.WindowId = value;
            SetProperty(ref windowId, value);
        }
    }
    public bool IsWindowResizable {
        get => isWindowResizable; set {
            ProjectMetadata.WindowProperties.IsResizable = value;
            SetProperty(ref isWindowResizable, value);
        }
    }
    public int WindowModeIndex {
        get => windowModeIndex; set {
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
        }
    }
    public int WindowStartLocation {
        get => windowStartLocation; set {
            ProjectMetadata.WindowProperties.ScreenPosition = value switch
            {
                2 => "mouse",
                1 => "center",
                _ => "none",
            };
            SetProperty(ref windowStartLocation, value);
        }
    }
    public uint WindowHeight {
        get => windowHeight; set {
            SetProperty(ref windowHeight, value);
            ProjectMetadata.WindowProperties.WindowHeight = value;
        }
    }
    public uint WindowWidth {
        get => windowWidth; set {
            SetProperty(ref windowWidth, value);
            ProjectMetadata.WindowProperties.WindowWidth = value;
        }
    }
    public uint MinHeight {
        get => minHeight; set {
            SetProperty(ref minHeight, value);
            ProjectMetadata.WindowProperties.MinimumHeight = value;
        }
    }
    public uint MinWidth {
        get => minWidth; set {
            SetProperty(ref minWidth, value);
            ProjectMetadata.WindowProperties.MinimumWidth = value;
        }
    }
    #endregion

    #region Commands
    public DelegateCommand FindHtmlFileCommand { get; private set; }
    public DelegateCommand FindIconFileCommand { get; private set; }
    public DelegateCommand SaveCommand { get; private set; }
    public DelegateCommand CloseCommand { get; private set; }

    public DialogCloseListener RequestClose { get; }
    #endregion

    public MetadataEditorViewModel()
    {
        SaveCommand = new DelegateCommand(SaveJsonFile);
        CloseCommand = new DelegateCommand(CloseDialog);
        FindHtmlFileCommand = new DelegateCommand(FindHtmlFile);
        FindIconFileCommand = new DelegateCommand(FindIconFile);
    }

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
        ConvertValues();

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

    private void ConvertValues()
    {
        GameId = ProjectMetadata.GameName;
        IndexFileLocation = ProjectMetadata.MainFile;
        GameVersion = ProjectMetadata.GameVersion;
        EnableNodeJs = ProjectMetadata.UseNodeJs;
        ChromiumFlags = ProjectMetadata.ChromiumFlags;
        JsFlags = ProjectMetadata.JsFlags;
        GameName = ProjectMetadata.GameTitle;
        GameIconLocation = ProjectMetadata.WindowProperties.WindowIcon;
        WindowTitle = ProjectMetadata.WindowProperties.WindowTitle;
        WindowId = ProjectMetadata.WindowProperties.WindowId;
        IsWindowResizable = ProjectMetadata.WindowProperties.IsResizable;
        if (ProjectMetadata.WindowProperties.StartAtFullScreen) WindowModeIndex = 1;
        else if (ProjectMetadata.WindowProperties.RunInKioskMode) WindowModeIndex = 2;
        else WindowModeIndex = 0;
        WindowStartLocation = (ProjectMetadata.WindowProperties.ScreenPosition) switch
        {
            "mouse" => 2,
            "center" => 1,
            _ => 0,
        };
        WindowHeight = ProjectMetadata.WindowProperties.WindowHeight;
        WindowWidth = ProjectMetadata.WindowProperties.WindowWidth;
        MinHeight = ProjectMetadata.WindowProperties.MinimumHeight;
        MinWidth = ProjectMetadata.WindowProperties.MinimumWidth;
    }

    private void FindHtmlFile()
    {
        VistaOpenFileDialog htmlFilePicker = new()
        {
            Title = Resources.ProjectPickerText,
            Filter = Resources.HTMLFileText,
            InitialDirectory = projectLocation,
            Multiselect = false
        };
        bool? pickerResult = htmlFilePicker.ShowDialog();
        if (pickerResult != true) return;
        if (htmlFilePicker.FileName.Contains(projectLocation))
        {
            string stringBuffer = htmlFilePicker.FileName.Replace(projectLocation + "\\", "");
            stringBuffer = stringBuffer.Replace("\\", "/");
            IndexFileLocation = stringBuffer;

        }
        else
        {
            MessageDialog.ThrowErrorMessage(Resources.ErrorText, Resources.FileOutsideOfProjectError);
        }
    }

    private void FindIconFile()
    {
        VistaOpenFileDialog iconFilePicker = new()
        {
            Title = Resources.ProjectPickerText,
            Filter = Resources.PNGFileText,
            InitialDirectory = projectLocation,
            Multiselect = false
        };
        bool? pickerResult = iconFilePicker.ShowDialog();
        if (pickerResult != true) return;
        if (iconFilePicker.FileName.Contains(projectLocation))
        {
            string stringBuffer = iconFilePicker.FileName.Replace(projectLocation + "\\", "");
            stringBuffer = stringBuffer.Replace("\\", "/");
            GameIconLocation = ProjectMetadata.WindowProperties.WindowIcon = stringBuffer;
        }
        else
        {
            MessageDialog.ThrowErrorMessage(Resources.ErrorText, Resources.FileOutsideOfProjectError);
        }
    }
}
