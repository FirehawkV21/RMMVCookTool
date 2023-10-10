using Prism.Commands;
using Prism.Mvvm;
using Prism.Dialogs;
using RMMVCookTool.Core;
using RMMVCookTool.Core.Compiler;
using RMMVCookTool.Core.CompilerSettings;
using RMMVCookTool.Core.Utilities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace RMMVCookTool.GUI.ViewModels;

public sealed class MainViewModel : BindableBase
{
    #region Variables
    private readonly BackgroundWorker _compilerWorker = new();
    private int _compilerStatusReport;
    private readonly StringBuilder _stringBuffer = new();
    private int currentFile;
    private int maxFiles;
    private int currentProject;
    private int maxProject;
    private string currentProjectText;
    private string currentProgressText;
    private ObservableCollection<CompilerProject> projectList;
    private int selectedProjectIndex;
    private readonly StringBuilder _nextFile = new();
    private SolidColorBrush currentBrush = Brushes.ForestGreen;
    private bool settingsAccessible = true;
    private string sdkLocation;
    private Visibility compilerButtonVisible = Visibility.Visible;
    private Visibility cancelButtonVisible = Visibility.Hidden;
    private readonly CompilerSettingsManager SettingsManager;
    private readonly IDialogService dialogManager;
    private bool isProjectSelected;

    public string CurrentProgressText { get => currentProgressText; set => SetProperty(ref currentProgressText, value); }
    public string CurrentProjectText { get => currentProjectText; set => SetProperty(ref currentProjectText, value); }
    public ObservableCollection<CompilerProject> ProjectList { get => projectList; set => SetProperty(ref projectList, value); }
    public int SelectedProjectIndex { get => selectedProjectIndex; set => SetProperty(ref selectedProjectIndex, value); }
    public int CurrentFileCounter { get => currentFile; set => SetProperty(ref currentFile, value); }
    public int MaxFileCounter { get => maxFiles; set => SetProperty(ref maxFiles, value); }
    public int CurrentProjectCounter { get => currentProject; set => SetProperty(ref currentProject, value); }
    public int MaxProjectCounter { get => maxProject; set => SetProperty(ref maxProject, value); }
    public SolidColorBrush CurrentStateBrush { get => currentBrush; set => SetProperty(ref currentBrush, value); }
    public bool AreSettingsAccessible { get => settingsAccessible; set => SetProperty(ref settingsAccessible, value); }
    public string SdkLocation { get => sdkLocation; set => SetProperty(ref sdkLocation, value); }
    public Visibility IsCompilerButtonVisible { get => compilerButtonVisible; set => SetProperty(ref compilerButtonVisible, value); }
    public Visibility IsCancelButtonVisible { get => cancelButtonVisible; set => SetProperty(ref cancelButtonVisible, value); }
    public bool IsProjectSelected { get => isProjectSelected; set => SetProperty(ref isProjectSelected, value); }
    
    #endregion
    #region Commands
    public DelegateCommand StartCompilerCommand { get; private set; }
    public DelegateCommand CancelCompilerCommand { get; private set; }
    public DelegateCommand AddProjectCommand { get; private set; }
    public DelegateCommand RemoveProjectCommand { get; private set; }
    public DelegateCommand ConfigProjectCommand { get; private set; }
    public DelegateCommand EditProjectFileCommand { get; private set; }
    public DelegateCommand BrowseSDKCommand { get; private set; }
    public DelegateCommand DefaultProjectSettingsCommand { get; private set; }
    public DelegateCommand ProjectSettingsCommand { get; private set; }
    public DelegateCommand EditMetadataCommand { get; private set; }
    public DelegateCommand SelectProjectCheck { get; private set; }
    #endregion
    #region Constructor Code
    public MainViewModel(IDialogService dialogService)
    {
        projectList = new();
        BrowseSDKCommand = new DelegateCommand(FindSdkFolder, CheckSettingsAccess).ObservesProperty(() => AreSettingsAccessible);
        AddProjectCommand = new DelegateCommand(FindProjectFolder, CheckSettingsAccess).ObservesProperty(() => AreSettingsAccessible);
        RemoveProjectCommand = new DelegateCommand(RemoveSelectedProjects, CheckSettingsAccess).ObservesProperty(() => AreSettingsAccessible);
        StartCompilerCommand = new DelegateCommand(StartCompilerWorkload);
        CancelCompilerCommand = new DelegateCommand(CancelCompilerWorkload);
        DefaultProjectSettingsCommand = new DelegateCommand(EditDefaultProjectSettings, CheckSettingsAccess).ObservesProperty(() => AreSettingsAccessible);
        ProjectSettingsCommand = new DelegateCommand(EditProjectSettings, CheckSettingsAccess).ObservesProperty(() => AreSettingsAccessible);
        EditMetadataCommand = new DelegateCommand(EditProjectMetadata, CheckSettingsAccess).ObservesProperty(() => AreSettingsAccessible);
        SelectProjectCheck = new DelegateCommand(CheckSelection, CheckSettingsAccess).ObservesProperty(() => AreSettingsAccessible);
        IsCancelButtonVisible = Visibility.Hidden;
        SettingsManager = new();
        CurrentFileCounter = 0;
        MaxFileCounter = 1;
        CurrentProjectCounter = 0;
        MaxProjectCounter = 1;
        SdkLocation = SettingsManager.Settings.NwjsLocation;
        dialogManager = dialogService;
        SetupWorkers();
    }

    private void SetupWorkers()
    {
        _compilerWorker.WorkerReportsProgress = true;
        _compilerWorker.WorkerSupportsCancellation = true;
        _compilerWorker.DoWork += StartCompiler;
        _compilerWorker.ProgressChanged += CompilerReport;
        _compilerWorker.RunWorkerCompleted += CompilerFinisher;
    }
    #endregion
    #region Compiler Code
    private void StartCompiler(object sender, DoWorkEventArgs e)
    {
        CompilerUtilities.RecordToLog("Starting the session.", 0);
        try
        {
            foreach (CompilerProject project in ProjectList)
            {
                if (_compilerWorker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                CurrentProjectText = Resources.CompileText1 + project.ProjectLocation +
                             Resources.FolderText;
                CompilerUtilities.RecordToLog("Preparing for the project " + project.ProjectLocation + "...", 0);
                project.CompilerInfo.Value.FileName = Path.Combine(SdkLocation, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "nwjc.exe" : "nwjc");
                CurrentProgressText = Resources.CompilerPreparationText;
                project.PullSourceFiles();
                project.GameFilesLocation = CompilerUtilities.GetProjectFilesLocation(Path.Combine(project.ProjectLocation, "package.json"));

                if (project.GameFilesLocation is "Null" or "Unknown")
                {
                    CompilerUtilities.RecordToLog("Missing info for the game files location. Aborting session.", 0);
                    MessageDialog.ThrowErrorMessage(Resources.CannotFindGameFolderTitle, Resources.CannotFindGameFolderMessage);
                }
                else
                {

                    _compilerStatusReport = 1;
                    _compilerWorker.ReportProgress(CurrentProjectCounter + 1);
                    CompilerUtilities.CleanupBin(project.FileMap);
                    CompilerUtilities.RemoveDebugFiles(project.GameFilesLocation);
                    _compilerStatusReport = 2;
                    _compilerWorker.ReportProgress(1);
                    _compilerStatusReport = 3;
                    for (CurrentFileCounter = 0; CurrentFileCounter < project.FileMap.Count; CurrentFileCounter++)
                    {
                        if (_compilerWorker.CancellationPending)
                        {
                            e.Cancel = true;
                            break;
                        }
                        CompilerErrorReport errorCode = project.CompileFile(CurrentFileCounter);
                        if (errorCode.ErrorCode > 0)
                        {
                            MessageDialog.ThrowErrorMessage(Resources.CompilerErrorTitle, Resources.CompilerErrorMessage + errorCode.ErrorMessage);
                            e.Cancel = true;
                            _compilerWorker.CancelAsync();
                        }
                        _compilerWorker.ReportProgress(CurrentProjectCounter + 1);
                    }
                    if (e.Cancel) break;
                    if (project.Setup.CompressProjectFiles)
                    {
                        _compilerStatusReport = 4;
                        _compilerWorker.ReportProgress(1);
                        project.CompressFiles();
                    }
                    _compilerStatusReport = 6;
                    _compilerWorker.ReportProgress(CurrentProjectCounter + 1);
                    CompilerUtilities.RecordToLog("The project " + project.ProjectLocation + "is ready.", 0);
                    CurrentProjectCounter += 1;
                }
            }
        }
        catch (PathTooLongException exceptionOutput)
        {
            BreakDueToError();
            CompilerUtilities.RecordToLog(exceptionOutput);
            MessageDialog.ThrowErrorMessage(exceptionOutput);
        }
        catch (UnauthorizedAccessException exceptionOutput)
        {

            CompilerUtilities.RecordToLog(exceptionOutput);
            MessageDialog.ThrowErrorMessage(exceptionOutput);
        }

        catch (ArgumentException exceptionOutput)
        {
            BreakDueToError();
            CompilerUtilities.RecordToLog(exceptionOutput);
            MessageDialog.ThrowErrorMessage(exceptionOutput);
        }
        catch (FileNotFoundException exceptionOutput)
        {
            BreakDueToError();
            CompilerUtilities.RecordToLog(exceptionOutput);
            MessageDialog.ThrowErrorMessage(exceptionOutput);
        }
        catch (DirectoryNotFoundException exceptionOutput)
        {
            BreakDueToError();
            CompilerUtilities.RecordToLog(exceptionOutput);
            MessageDialog.ThrowErrorMessage(exceptionOutput);
        }
        catch (IOException exceptionOutput)
        {
            BreakDueToError();
            CompilerUtilities.RecordToLog(exceptionOutput);
            MessageDialog.ThrowErrorMessage(exceptionOutput);
        }
    }

    private void CompilerReport(object sender, ProgressChangedEventArgs e)
    {
        if ((_compilerStatusReport > 0 && _compilerStatusReport < 3) && CurrentFileCounter > ProjectList[CurrentProjectCounter].FileMap.Count) _stringBuffer.Insert(0, ProjectList[CurrentProjectCounter].FileMap.ElementAt(CurrentFileCounter));
        switch (_compilerStatusReport)
        {
            case 6:
                CurrentFileCounter = 0;
                break;
            case 5:
                CurrentFileCounter += 1;
                break;
            case 4:
                CurrentProgressText = Resources.PackaginStatusText;
                CompilerUtilities.RecordToLog("Packaging project " + ProjectList[CurrentProjectCounter].ProjectLocation + "...", 0);
                break;
            case 3:
                CompilerUtilities.RecordToLog("Compiled " + ProjectList[CurrentProjectCounter].FileMap.ElementAt(CurrentFileCounter), 0);
                if (CurrentFileCounter < ProjectList[CurrentProjectCounter].FileMap.Count - 1)
                {
                    _nextFile.Insert(0, ProjectList[CurrentProjectCounter].FileMap.ElementAt(CurrentFileCounter + 1));
                    CurrentProgressText = Resources.CompileText + _nextFile + "...";
                    CompilerUtilities.RecordToLog("Compiling " + _nextFile + "...", 0);
                }
                else
                {
                    CompilerUtilities.RecordToLog("Completed the compilation.", 0);
                }
                _stringBuffer.Clear();
                _nextFile.Clear();
                break;
            case 2:
                CurrentProgressText = Resources.CompileText + _stringBuffer + "...";
                CompilerUtilities.RecordToLog("Compiling " + _stringBuffer + "...", 0);
                _stringBuffer.Clear();
                break;
            case 1:
                MaxFileCounter = ProjectList[CurrentProjectCounter].FileMap.Count + ((ProjectList[CurrentProjectCounter].Setup.CompressProjectFiles) ? 1 : 0);
                CurrentProgressText =
                    Resources.BinRemovalStatusText + ProjectList[CurrentProjectCounter].ProjectLocation + "...";
                CompilerUtilities.RecordToLog("Removing binary files...", 0);
                break;

        }
    }

    private void CompilerFinisher(object sender, RunWorkerCompletedEventArgs e)
    {
        if (e.Cancelled)
        {
            CurrentStateBrush = Brushes.YellowGreen;
            CompilerUtilities.RecordToLog("Session cancelled.", 0);
            MessageDialog.ThrowWarningMessage(Resources.AbortedText, Resources.TaskCancelledMessage, "");
        }
        else
        {
            CompilerUtilities.RecordToLog("Session completed.", 0);
            MessageDialog.ThrowCompleteMessage(Resources.CompilationCompleteText);
            CurrentProjectText = Resources.DoneText;
            CurrentProgressText = Resources.DoneText;
        }
        IsCompilerButtonVisible = Visibility.Visible;
        IsCancelButtonVisible = Visibility.Hidden;
        CurrentProjectCounter = 0;
        MaxProjectCounter = 0;
        AreSettingsAccessible = true;
        CurrentStateBrush = Brushes.ForestGreen;
    }

    private void BreakDueToError()
    {
        CurrentProgressText = Resources.FailedText;
        CurrentProjectText = Resources.FailedText;
        CurrentStateBrush = Brushes.DarkRed;
    }
    #endregion

    #region UI Code

    private void FindSdkFolder()
    {
        VistaFolderBrowserDialog pickSdkFolder = new()
        {
            Description = Resources.SDKPickerText,
            UseDescriptionForTitle = true
        };
        bool? pickerResult = pickSdkFolder.ShowDialog();
        if (pickerResult != true) return;
        SettingsManager.Settings.NwjsLocation = SdkLocation = pickSdkFolder.SelectedPath;
        SettingsManager.SaveSettings();
    }

    private void CheckSelection()
    {
        if (AreSettingsAccessible)
            IsProjectSelected = SelectedProjectIndex > -1;
    }

    private void FindProjectFolder()
    {
        VistaFolderBrowserDialog pickJsFolder = new()
        {
            Description = Resources.ProjectPickerText,
            UseDescriptionForTitle = true
        };
        bool? pickerResult = pickJsFolder.ShowDialog();
        if (pickerResult != true) return;
        if (pickJsFolder.SelectedPath != null) ProjectList.Add(new CompilerProject(pickJsFolder.SelectedPath, SettingsManager.Settings.DefaultProjectSettings));
    }

    private void RemoveSelectedProjects()
    {
           if (SelectedProjectIndex >= -1) ProjectList.RemoveAt(SelectedProjectIndex);
    }

    private void StartCompilerWorkload()
    {
        if (!File.Exists(Path.Combine(SdkLocation, "nwjc.exe")))
        {
            MessageDialog.ThrowErrorMessage(Resources.ErrorText, Resources.CompilerMissingText);
        }
        else if (ProjectList.Count == 0)
        {
            MessageDialog.ThrowErrorMessage(Resources.ErrorText, Resources.NoJSFilesPresent);
        }
        else
        {
            IsCompilerButtonVisible = Visibility.Hidden;
            IsCancelButtonVisible = Visibility.Visible;
            AreSettingsAccessible = false;
            SelectedProjectIndex = -1;
            IsProjectSelected = false;
            MaxProjectCounter = ProjectList.Count;
            _compilerWorker.RunWorkerAsync();
        }
    }

    private bool CheckSettingsAccess() => AreSettingsAccessible;
    private void CancelCompilerWorkload() => _compilerWorker.CancelAsync();
    
    private void EditDefaultProjectSettings()
    {
        ProjectSettings tempSettings = SettingsManager.Settings.DefaultProjectSettings;
        bool update = EditProjectSettings(ref tempSettings, Resources.DefaultProjectSettingsUiText);
        if (update)
        {
            SettingsManager.Settings.DefaultProjectSettings = tempSettings;
            SettingsManager.SaveSettings();
            MessageDialog.ThrowCompleteMessage(Resources.DefaultSettingsUpdatedText, Resources.DefaultSettingsUpdatedMessage);
        }
    }

    private void EditProjectSettings()
    {
        if (SelectedProjectIndex == -1)
        {
            MessageDialog.ThrowWarningMessage(Resources.WarningText, Resources.ProjectNotSelectedMessage,
                Resources.ProjectMessageNotSelected_Details);
        }
        else
        {
            ProjectSettings tempSettings = ProjectList[SelectedProjectIndex].Setup;
            bool update = EditProjectSettings(ref tempSettings, Resources.ProjectSettingsUiText);
            if (update)
            {
                ProjectList[SelectedProjectIndex].Setup = tempSettings;
                MessageDialog.ThrowCompleteMessage(Resources.SProjectSettingsUpdatedText);
                    ICollectionView view = CollectionViewSource.GetDefaultView(ProjectList);
                    view.Refresh();
            }
        }
    }

    private bool EditProjectSettings(ref ProjectSettings settings, in string TitleBarText)
    {
        ProjectSettings tempSettings = new();
        bool isUpdated = false;
        DialogParameters param = new()
        {
            { "title", TitleBarText },
            { "fileExtension", settings.FileExtension },
            { "removeSource", settings.RemoveSourceFiles },
            { "compressFiles", settings.CompressProjectFiles },
            { "removeAfterCompression", settings.RemoveFilesAfterCompression },
            { "compressionLevel", settings.CompressionLevel }
        };

        dialogManager.ShowDialog("ProjectSettings", param, r => {
            if (r.Result == ButtonResult.OK)
            {
                WriteSettings(ref tempSettings, r.Parameters);
                isUpdated = true;
            }
        });
        if (isUpdated) settings = tempSettings;
        return isUpdated;
    }

    private static void WriteSettings(ref ProjectSettings settings, in IDialogParameters param)
    {
        settings.FileExtension = param.GetValue<string>("fileExtension");
        settings.RemoveSourceFiles = param.GetValue<bool>("removeSource");
        settings.CompressProjectFiles = param.GetValue<bool>("compressFiles");
        settings.RemoveFilesAfterCompression = param.GetValue<bool>("removeAfterCompression");
        settings.CompressionLevel = param.GetValue<int>("compressionLevel");
    }

    private void EditProjectMetadata()
    {
        if (SelectedProjectIndex == -1)
        {
            MessageDialog.ThrowWarningMessage(Resources.WarningText, Resources.ProjectNotSelectedMessage,
                Resources.ProjectMessageNotSelected_Details);
        }
        else
        {
            dialogManager.ShowDialog("MetadataEditor", new DialogParameters($"location={ProjectList[SelectedProjectIndex].ProjectLocation}"), r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                   //Left intentionally blank.
                }
            });
        }
    }
    #endregion
}
