using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using Ookii.Dialogs.Wpf;
using RMMVCookTool.Core;
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using TextBox = System.Windows.Controls.TextBox;

namespace RMMVCookTool.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            SetupWorkers();
        }
        private readonly BackgroundWorker _compilerWorker = new BackgroundWorker();
        private string _previousPath;
        private int _compilerStatusReport;
        private StringBuilder _stringBuffer = new StringBuilder();
        private int currentFile;
        private int currentProject;
        private StringBuilder _nextFile = new StringBuilder();
        public static ObservableCollection<CompilerProject> ProjectList { get; } = new ObservableCollection<CompilerProject>();

        #region Compiler Worker
        private void SetupWorkers()
        {
            _compilerWorker.WorkerReportsProgress = true;
            _compilerWorker.WorkerSupportsCancellation = true;
            _compilerWorker.DoWork += StartCompiler;
            _compilerWorker.ProgressChanged += CompilerReport;
            _compilerWorker.RunWorkerCompleted += CompilerFinisher;
        }

        private void StartCompiler(object sender, DoWorkEventArgs e)
        {
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
                    ProjectList[currentProject].GameFilesLocation = JsonProcessor.FindGameFolder(Path.Combine(ProjectList[currentProject].ProjectLocation, "package.json"));
                    if (ProjectList[currentProject].GameFilesLocation == "Null" || ProjectList[currentProject].GameFilesLocation == "Unknown")
                    {
                        MessageDialog.ThrowErrorMessage(Properties.Resources.CannotFindGameFolderTitle, Properties.Resources.CannotFindGameFolderMessage);
                    }
                    else
                    {

                        _compilerStatusReport = 1;
                        _compilerWorker.ReportProgress(currentProject + 1);
                        CompilerUtilities.CleanupBin(ProjectList[currentProject].FileMap);
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
                        if (ProjectList[currentProject].CompressFilesToPackage)
                        {
                            _compilerStatusReport = 4;
                            _compilerWorker.ReportProgress(1);
                            ProjectList[currentProject].CompressFiles();
                        }

                        if (ProjectList[currentProject].RemoveFilesAfterCompression)
                        {
                            _compilerStatusReport = 5;
                            _compilerWorker.ReportProgress(1);
                            ProjectList[currentProject].DeleteFiles();
                        }
                        _compilerStatusReport = 6;
                        _compilerWorker.ReportProgress(currentProject + 1);
                    }
                }
            }
            catch (PathTooLongException exceptionOutput)
            {
                Dispatcher.Invoke(() =>
                {
                    TaskbarInfoHolder.ProgressState = TaskbarItemProgressState.Error;
                    TotalWorkProgressBar.Foreground = Brushes.DarkRed;
                    CurrentWorkloadBar.Foreground = Brushes.DarkRed;
                    CurrentWorkloadLabel.Content = Properties.Resources.FailedText;
                });
                MessageDialog.ThrowErrorMessage(exceptionOutput);
            }
            catch (UnauthorizedAccessException exceptionOutput)
            {
                Dispatcher.Invoke(() =>
                {
                    TaskbarInfoHolder.ProgressState = TaskbarItemProgressState.Error;
                    TotalWorkProgressBar.Foreground = Brushes.DarkRed;
                    CurrentWorkloadBar.Foreground = Brushes.DarkRed;
                    CurrentWorkloadLabel.Content = Properties.Resources.FailedText;
                });
                MessageDialog.ThrowErrorMessage(exceptionOutput);
            }

            catch (ArgumentException exceptionOutput)
            {
                Dispatcher.Invoke(() =>
                {
                    TaskbarInfoHolder.ProgressState = TaskbarItemProgressState.Error;
                    TotalWorkProgressBar.Foreground = Brushes.DarkRed;
                    CurrentWorkloadBar.Foreground = Brushes.DarkRed;
                    CurrentWorkloadLabel.Content = Properties.Resources.FailedText;
                });
                MessageDialog.ThrowErrorMessage(exceptionOutput);
            }
            catch (FileNotFoundException exceptionOutput)
            {
                Dispatcher.Invoke(() =>
                {
                    TaskbarInfoHolder.ProgressState = TaskbarItemProgressState.Error;
                    TotalWorkProgressBar.Foreground = Brushes.DarkRed;
                    CurrentWorkloadBar.Foreground = Brushes.DarkRed;
                    CurrentWorkloadLabel.Content = Properties.Resources.FailedText;
                });
                MessageDialog.ThrowErrorMessage(exceptionOutput);
            }
            catch (DirectoryNotFoundException exceptionOutput)
            {
                Dispatcher.Invoke(() =>
                {
                    TaskbarInfoHolder.ProgressState = TaskbarItemProgressState.Error;
                    TotalWorkProgressBar.Foreground = Brushes.DarkRed;
                    CurrentWorkloadBar.Foreground = Brushes.DarkRed;
                    CurrentWorkloadLabel.Content = Properties.Resources.FailedText;
                });
                MessageDialog.ThrowErrorMessage(exceptionOutput);
            }
            catch (IOException exceptionOutput)
            {
                Dispatcher.Invoke(() =>
                {
                    TaskbarInfoHolder.ProgressState = TaskbarItemProgressState.Error;
                    TotalWorkProgressBar.Foreground = Brushes.DarkRed;
                    CurrentWorkloadBar.Foreground = Brushes.DarkRed;
                    CurrentWorkloadLabel.Content = Properties.Resources.FailedText;
                });
                MessageDialog.ThrowErrorMessage(exceptionOutput);
            }
        }

        private void CompilerReport(object sender, ProgressChangedEventArgs e)
        {
            if ((_compilerStatusReport > 0 && _compilerStatusReport < 3) && currentFile > ProjectList[currentProject].FileMap.Count) _stringBuffer.Insert(0, ProjectList[currentProject].FileMap.ElementAt(currentFile));
            switch (_compilerStatusReport)
            {
                case 6:
                    CurrentWorkloadBar.Value = 0;
                    TaskbarInfoHolder.ProgressValue = 0;
                    TotalWorkProgressBar.Value += 1;
                    break;
                case 5:
                    CurrentWorkloadBar.Value += 1;
                    TaskbarInfoHolder.ProgressValue = CurrentWorkloadBar.Value / CurrentWorkloadBar.Maximum;
                    break;
                case 4:
                    TaskbarInfoHolder.ProgressValue = CurrentWorkloadBar.Value / CurrentWorkloadBar.Maximum;
                    CurrentWorkloadLabel.Content = Properties.Resources.PackaginStatusText;
                    break;
                case 3:
                    CurrentWorkloadBar.Value += 1;
                    TaskbarInfoHolder.ProgressValue = CurrentWorkloadBar.Value / CurrentWorkloadBar.Maximum;
                    if (currentFile < ProjectList[currentProject].FileMap.Count - 1)
                    {
                        _nextFile.Insert(0, ProjectList[currentProject].FileMap.ElementAt(currentFile + 1));
                        CurrentWorkloadLabel.Content = Properties.Resources.CompileText + _nextFile + "...";
                    }
                    _stringBuffer.Clear();
                    _nextFile.Clear();
                    break;
                case 2:
                    CurrentWorkloadLabel.Content = Properties.Resources.CompileText + _stringBuffer + "...";
                    _stringBuffer.Clear();
                    break;
                case 1:
                    CurrentWorkloadBar.Maximum = ProjectList[currentProject].FileMap.Count +  ((ProjectList[currentProject].CompressFilesToPackage) ? ((ProjectList[currentProject].RemoveFilesAfterCompression) ? 2 : 1) : 0);
                    CurrentWorkloadLabel.Content =
                        Properties.Resources.BinRemovalStatusText + ProjectList[currentProject].ProjectLocation + "...";
                    break;
                case 0:
                    TotalProgressLabel.Content = Properties.Resources.CompileText1 + ProjectList[currentProject].ProjectLocation +
                                                 Properties.Resources.FolderText;
                    break;

            }
        }

        private void CompilerFinisher(object sender, RunWorkerCompletedEventArgs e)
        {
            //if (e.Error != null)
            //{

            //}
            if (e.Cancelled)
            {
                TaskbarInfoHolder.ProgressState = TaskbarItemProgressState.Paused;
                CurrentWorkloadBar.Foreground = Brushes.YellowGreen;
                TotalWorkProgressBar.Foreground = Brushes.YellowGreen;
                MessageDialog.ThrowWarningMessage(Properties.Resources.AbortedText, Properties.Resources.TaskCancelledMessage, "");
            }
            else
            {
                MessageDialog.ThrowCompleteMessage(Properties.Resources.CompilationCompleteText);
                TotalProgressLabel.Content = Properties.Resources.DoneText;
                CurrentWorkloadLabel.Content = Properties.Resources.DoneText;
            }
            CompileButton.Visibility = Visibility.Visible;
            CancelCompileButton.Visibility = Visibility.Hidden;
            UnlockSettings(true);
            TaskbarInfoHolder.ProgressValue = 0;
            TaskbarInfoHolder.ProgressState = TaskbarItemProgressState.None;
            TotalWorkProgressBar.Value = 0;
            CurrentWorkloadBar.Value = 0;
            TotalWorkProgressBar.Value = 0;
            TotalWorkProgressBar.Foreground = Brushes.ForestGreen;
            CurrentWorkloadBar.Foreground = Brushes.ForestGreen;
        }

        #endregion

        #region Methods

        private void UnlockSettings(bool setting)
        {
            NwjsLocation.IsEnabled = setting;
            BrowseSdkButton.IsEnabled = setting;
            DefaultProjectSettingsButton.IsEnabled = setting;
            FolderList.IsEnabled = setting;
            AddProjectButton.IsEnabled = setting;
            RemoveProjectButton.IsEnabled = setting;
            ProjectSettingsButton.IsEnabled = setting;
            EditMetadataButton.IsEnabled = setting;
        }
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
             FolderList.ItemsSource = ProjectList;
             var assembly = Assembly.GetExecutingAssembly();
             var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
             var version = fvi.FileVersion;
             ProgramVersionLabel.Content = ProgramVersionLabel.Content + @" (" + version + @")";

            byte[] loader = Encoding.ASCII.GetBytes(Properties.Resources.Manual);
            using MemoryStream stream = new MemoryStream(loader);
            UserManualBox.Selection.Load(stream, DataFormats.Rtf);
        }

        private void BrowseSDKButton_Click(object sender, RoutedEventArgs e)
        {
            var pickSdkFolder = new VistaFolderBrowserDialog
            {
                Description = Properties.Resources.SDKPickerText,
                UseDescriptionForTitle = true
            };
            var pickerResult = pickSdkFolder.ShowDialog();
            if (pickerResult != true) return;
            AppSettings.Default.SDKLocation = pickSdkFolder.SelectedPath;
            NwjsLocation.Text = pickSdkFolder.SelectedPath;
            AppSettings.Default.Save();
        }

        private void NwjsLocation_Drop(object sender, DragEventArgs e)
        {
            TextBox nwjsBox = sender as TextBox;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                NwjsLocation.Text = Path.GetFullPath((string) e.Data.GetData(DataFormats.FileDrop));
            }
        }

        private void NwjsLocation_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                TextBox nwjsBox = sender as TextBox;
                DragDrop.DoDragDrop(nwjsBox, nwjsBox.Text, DragDropEffects.Copy);
            }
        }

        private void NwjsLocation_DragEnter(object sender, DragEventArgs e)
        {
            if (NwjsLocation.Text != null) _previousPath = NwjsLocation.Text;
            TextBox nwjsBox = sender as TextBox;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string data = (string) e.Data.GetData(DataFormats.FileDrop);
                if (Path.IsPathFullyQualified(data))
                {
                    NwjsLocation.Text = Path.GetFullPath(data);
                }
            }
        }

        private void NwjsLocation_DragLeave(object sender, DragEventArgs e)
        {
            TextBox nwjsBox = sender as TextBox;
            if (NwjsLocation.Text != null)
            {
                NwjsLocation.Text = _previousPath;
            }
        }

        private void NwjsLocation_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                string data = (string)e.Data.GetData(DataFormats.StringFormat);

                // If the string can be converted into a Brush, allow copying.
                if (Path.IsPathFullyQualified(data))
                {
                    e.Effects = DragDropEffects.Copy | DragDropEffects.Move;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ProjectSettingsWindow defSettingsWindow = new ProjectSettingsWindow();
            defSettingsWindow.Show();
        }

        private void AddProjectButton_Click(object sender, RoutedEventArgs e)
        {
            var pickJsFolder =
                new VistaFolderBrowserDialog
                {
                    Description = Properties.Resources.ProjectPickerText,
                    UseDescriptionForTitle = true
                };
            var pickerResult = pickJsFolder.ShowDialog();
            if (pickerResult != true) return;
            if (pickJsFolder.SelectedPath != null)
            {
                ProjectList.Add(new CompilerProject(pickJsFolder.SelectedPath, AppSettings.Default.FileExtension,
                    AppSettings.Default.DeleteSourceCode, AppSettings.Default.PackageCode,
                    AppSettings.Default.RemoveFilesAfterPackaging, AppSettings.Default.CompressionMode));

            }

        }

        private void RemoveProjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (FolderList.SelectedIndex != -1) ProjectList.RemoveAt(FolderList.SelectedIndex);
        }

        private void ProjectSettingsButton_Click(object sender, RoutedEventArgs e)
        {

            if (FolderList.SelectedItems.Count == 0)
            {
                MessageDialog.ThrowWarningMessage(Properties.Resources.WarningText, Properties.Resources.ProjectNotSelectedMessage,
                    Properties.Resources.ProjectMessageNotSelected_Details);
            }
            else
            {
                var temp = FolderList.SelectedIndex;
                ProjectSettingsWindow settingsWindow = new ProjectSettingsWindow(temp);
                settingsWindow.ShowDialog();
                ICollectionView view = CollectionViewSource.GetDefaultView(ProjectList);
                view.Refresh();
            }
        }

        private void EditMetadataButton_Click(object sender, RoutedEventArgs e)
        {
            if (FolderList.SelectedItems.Count == 0)
                MessageDialog.ThrowWarningMessage(Properties.Resources.WarningText, Properties.Resources.ProjectNotSelectedMessage,
                    Properties.Resources.ProjectMessageNotSelected_Details);
            else
            {
                var jsonEditorGui = new JsonEditor(ProjectList[FolderList.SelectedIndex].ProjectLocation);
                jsonEditorGui.ShowDialog();
            }
        }

        private void CompileButton_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(Path.Combine(NwjsLocation.Text, "nwjc.exe")))
            {
                MessageDialog.ThrowErrorMessage(Properties.Resources.ErrorText, Properties.Resources.CompilerMissingText);
            }
            else if (FolderList.Items.Count == 0)
            {
                MessageDialog.ThrowErrorMessage(Properties.Resources.ErrorText, Properties.Resources.NoJSFilesPresent);
            }
            else
            {
                UnlockSettings(false);
                TotalWorkProgressBar.Value = 0;
                TotalWorkProgressBar.Maximum = FolderList.Items.Count;
                CompileButton.Visibility = Visibility.Hidden;
                CancelCompileButton.Visibility = Visibility.Visible;
                _compilerWorker.RunWorkerAsync();
            }
        }

        private void CancelCompileButton_Click(object sender, RoutedEventArgs e)
        {
            _compilerWorker.CancelAsync();
        }
    }
}
