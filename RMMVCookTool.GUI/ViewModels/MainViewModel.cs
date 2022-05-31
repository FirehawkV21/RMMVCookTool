using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using RMMVCookTool.Core;
using RMMVCookTool.Core.Compiler;
using RMMVCookTool.Core.CompilerSettings;
using RMMVCookTool.Core.Utilities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Media;

namespace RMMVCookTool.GUI.ViewModels;

public class MainViewModel : BindableBase
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
    private SolidColorBrush currentBrush;
    private bool settingsAccessible = true;
    private string sdkLocation;
    private Visibility compilerButtonVisible = Visibility.Visible;
    private Visibility cancelButtonVisible = Visibility.Hidden;
    private readonly CompilerSettingsManager SettingsManager;
    private readonly IDialogService dialogManager;

    public string CurrentProgressText { get => currentProgressText; set => SetProperty(ref currentProgressText, value); }
    public string CurrentProjectText { get => currentProjectText; set => SetProperty(ref currentProjectText, value); }
    public ObservableCollection<CompilerProject> ProjectList { get => projectList; set => SetProperty(ref projectList, value); }
    public int SelectedProjectIndex { get => selectedProjectIndex; set => SetProperty(ref selectedProjectIndex, value); }
    public int CurrentFileCounter { get => currentFile; set => SetProperty(ref currentFile, value); }
    public int MaxFileCounter { get => maxFiles; set => SetProperty(ref maxFiles, value); }
    public int CurrentProjectCounter { get => currentProject; set => SetProperty(ref maxProject, value); }
    public int MaxProjectCounter { get => maxProject; set => SetProperty(ref maxProject, value); }
    public SolidColorBrush CurrentStateBrush { get => currentBrush; set => SetProperty(ref currentBrush, value); }
    public bool AreSettingsAccessible { get => settingsAccessible; set => SetProperty(ref settingsAccessible, value); }
    public string SdkLocation { get => sdkLocation; set => SetProperty(ref sdkLocation, value); }
    public Visibility IsCompilerButtonVisible { get => compilerButtonVisible; set => SetProperty(ref compilerButtonVisible, value); }
    public Visibility IsCancelButtonVisible { get => cancelButtonVisible; set => SetProperty(ref cancelButtonVisible, value); }
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
        IsCancelButtonVisible = Visibility.Hidden;
        SettingsManager = new();
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
            for (currentProject = 0; currentProject < ProjectList.Count; currentProject++)
            {
                if (_compilerWorker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                _compilerStatusReport = 0;
                _compilerWorker.ReportProgress(currentProject + 1);
                ProjectList[currentProject].CompilerInfo.Value.FileName = Path.Combine(AppSettings.Default.SDKLocation, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "nwjc.exe" : "nwjc");
                ProjectList[currentProject].GameFilesLocation = CompilerUtilities.GetProjectFilesLocation(Path.Combine(ProjectList[currentProject].ProjectLocation, "package.json"));
                if (ProjectList[currentProject].GameFilesLocation is "Null" or "Unknown")
                {
                    CompilerUtilities.RecordToLog("Missing info for the game files location. Aborting session.", 0);
                    MessageDialog.ThrowErrorMessage(Resources.CannotFindGameFolderTitle, Resources.CannotFindGameFolderMessage);
                }
                else
                {

                    _compilerStatusReport = 1;
                    _compilerWorker.ReportProgress(currentProject + 1);
                    CompilerUtilities.CleanupBin(ProjectList[currentProject].FileMap);
                    CompilerUtilities.RemoveDebugFiles(ProjectList[currentProject].GameFilesLocation);
                    _compilerStatusReport = 2;
                    _compilerWorker.ReportProgress(1);
                    _compilerStatusReport = 3;
                    for (currentFile = 0; currentFile < ProjectList[currentProject].FileMap.Count; currentFile++)
                    {
                        if (_compilerWorker.CancellationPending)
                        {
                            e.Cancel = true;
                            break;
                        }
                        _compilerWorker.ReportProgress(currentProject + 1);
                        ProjectList[currentProject].CompileFile(currentFile);
                    }
                    if (e.Cancel) break;
                    if (ProjectList[currentProject].Setup.CompressProjectFiles)
                    {
                        _compilerStatusReport = 4;
                        _compilerWorker.ReportProgress(1);
                        ProjectList[currentProject].CompressFiles();
                    }
                    CompilerUtilities.RecordToLog("The project " + ProjectList[currentProject].ProjectLocation + "is ready.", 0);
                    _compilerStatusReport = 6;
                    _compilerWorker.ReportProgress(currentProject + 1);
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
        if ((_compilerStatusReport > 0 && _compilerStatusReport < 3) && currentFile > ProjectList[currentProject].FileMap.Count) _stringBuffer.Insert(0, ProjectList[currentProject].FileMap.ElementAt(currentFile));
        switch (_compilerStatusReport)
        {
            case 6:
                CurrentFileCounter = 0;
                CurrentProjectCounter += 1;
                break;
            case 5:
                CurrentFileCounter += 1;
                break;
            case 4:
                CurrentProgressText = Resources.PackaginStatusText;
                CompilerUtilities.RecordToLog("Packaging project " + ProjectList[currentProject].ProjectLocation + "...", 0);
                break;
            case 3:
                CompilerUtilities.RecordToLog("Compiled " + ProjectList[currentProject].FileMap.ElementAt(currentFile), 0);
                CurrentFileCounter += 1;
                if (currentFile < ProjectList[currentProject].FileMap.Count - 1)
                {
                    _nextFile.Insert(0, ProjectList[currentProject].FileMap.ElementAt(currentFile + 1));
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
                MaxFileCounter = ProjectList[currentProject].FileMap.Count + ((ProjectList[currentProject].Setup.CompressProjectFiles) ? 1 : 0);
                CurrentProgressText =
                    Resources.BinRemovalStatusText + ProjectList[currentProject].ProjectLocation + "...";
                CompilerUtilities.RecordToLog("Removing binary files...", 0);
                break;
            case 0:
                CurrentProjectText = Resources.CompileText1 + ProjectList[currentProject].ProjectLocation +
                                             Resources.FolderText;
                CompilerUtilities.RecordToLog("Preparing for the project " + ProjectList[currentProject].ProjectLocation + "...", 0);
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
        AreSettingsAccessible = true;
        CurrentProjectCounter = 0;
        MaxProjectCounter = 0;
        CurrentFileCounter = 0;
        MaxFileCounter = 0;
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
            CurrentProjectCounter = 0;
            currentFile = 0;
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
                WriteSettings(ref tempSettings, param);
                isUpdated = true;
            }
        });
        if (isUpdated) settings = tempSettings;
        return isUpdated;
    }

    private static void WriteSettings(ref ProjectSettings settings, in DialogParameters param)
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
