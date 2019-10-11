using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CompilerCore;
using nwjsCookToolUI.Properties;
using Ookii.Dialogs.Wpf;

namespace nwjsCookToolUI
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Variables
        private static int _currentFile;
        private static int _compilerStatusReport;
        private static string[] _projectList;
        //private static string _errorOutput;
        private readonly BackgroundWorker _compilerWorker = new BackgroundWorker();
        private readonly BackgroundWorker _mapCompilerWorker = new BackgroundWorker();
        private int _currentProject;
        private string[] FileMap;        
        #endregion Variables

        public MainWindow()
        {
            InitializeComponent();
            SetupWorkers();
        }

        #region Methods
        private void SetupWorkers()
        {
            _compilerWorker.WorkerReportsProgress = true;
            _compilerWorker.WorkerSupportsCancellation = true;
            _compilerWorker.DoWork += StartCompiler;
            _compilerWorker.ProgressChanged += CompilerReport;
            _compilerWorker.RunWorkerCompleted += CompilerFinisher;

            _mapCompilerWorker.WorkerReportsProgress = true;
            _mapCompilerWorker.WorkerSupportsCancellation = true;
            _mapCompilerWorker.DoWork += StartMapCompiler;
            _mapCompilerWorker.ProgressChanged += MapCompilerReport;
            _mapCompilerWorker.RunWorkerCompleted += MapCompilerFinisher;

        }

        /// <summary>
        /// Locks or unlocks the settings.
        /// </summary>
        /// <param name="unlockSetting">Lock (false) or unlock (true).</param>
        private void UnlockSettings(bool unlockSetting)
        {
            RemoveCompiledJsCheckbox.IsEnabled = unlockSetting;
            PackageNwCheckbox.IsEnabled = unlockSetting;
            FileExtensionTextbox.IsEnabled = unlockSetting;
            BrowseSdkButton.IsEnabled = unlockSetting;
            editJsonButton.IsEnabled = unlockSetting;
            SafeModeCheckBox.IsEnabled = unlockSetting && PackageNwCheckbox.IsChecked == true;
            RemoveFilesCheckBox.IsEnabled = unlockSetting && PackageNwCheckbox.IsChecked == true;
        }
        #endregion Methods

        #region Settings Code Set
        private void CookToolUi_Loaded(object sender, RoutedEventArgs e)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            var version = fvi.FileVersion;
            ProgramVersionLabel.Content = ProgramVersionLabel.Content + @" (" + version + @")";
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
            Settings.Default.SDKLocation = pickSdkFolder.SelectedPath;
            NwjsLocation.Text = pickSdkFolder.SelectedPath;
            Settings.Default.Save();
        }

        private void ReplacementCodeButton_Click(object sender, RoutedEventArgs e)
        {
            var ci = CultureInfo.InstalledUICulture.ToString() == "el-GR" || CultureInfo.InstalledUICulture.ToString() == "el-CY" ? "el" : "en";
            var fileLocation = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), ci, "ReplacementCode.txt");
            if (File.Exists(fileLocation)) Process.Start(fileLocation);
            else
                MessageBox.Show(Properties.Resources.FileUnavailableText, Properties.Resources.InfoText, MessageBoxButton.OK,
                    MessageBoxImage.Information);
        }

        private void Checkbox_CheckChanged(object sender, RoutedEventArgs e)
        {
            Settings.Default.Save();
        }

        private void FileExtensionTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.Default.Save();
        }
        #endregion Settings Code Set

        #region Quick Compile Code Set
        #region Quick Compile Tab UI Logic
        private void FindProjectButton_Click(object sender, RoutedEventArgs e)
        {
            var pickProjectFolder =
                new VistaFolderBrowserDialog
                {
                    Description = Properties.Resources.ProjectPickerText,
                    UseDescriptionForTitle = true
                };
            var pickerResult = pickProjectFolder.ShowDialog();
            if (pickerResult != true) return;
            ProjectLocation.Text = pickProjectFolder.SelectedPath;
        }

        private void EditJsonButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProjectLocation.Text == null || !Directory.Exists(ProjectLocation.Text))
                MessageBox.Show(Properties.Resources.ProjectLocationNotValidText, Properties.Resources.ErrorText,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                var JsonEditorGui = new JsonEditor(ProjectLocation.Text);
                JsonEditorGui.Show();
            }
        }

        private void CompileButton_Click(object sender, RoutedEventArgs e)
        {
            _compilerStatusReport = 0;
            CompileButton.Visibility = Visibility.Hidden;
            CancelTaskButton.Visibility = Visibility.Visible;
            TestProjectButton.IsEnabled = false;
            ProjectLocation.IsEnabled = false;
            FindProjectButton.IsEnabled = false;
            UnlockSettings(false);
            MainProgress.Foreground = Brushes.ForestGreen;
            if (!File.Exists(Path.Combine(NwjsLocation.Text, "nwjc.exe")))
            {
                MessageBox.Show(Properties.Resources.CompilerMissingText, Properties.Resources.ErrorText, MessageBoxButton.OK, MessageBoxImage.Error);
                OutputArea.Text = OutputArea.Text + "\n" + "[" +DateTime.Now + "]"+ Properties.Resources.CompilerMissingText;
                CompileButton.Visibility = Visibility.Visible;
                CancelTaskButton.Visibility = Visibility.Hidden;
                TestProjectButton.IsEnabled = true;
                ProjectLocation.IsEnabled = true;
                FindProjectButton.IsEnabled = true;
                UnlockSettings(true);
            }
            else if (!Directory.Exists(ProjectLocation.Text))
            {
                MessageBox.Show(Properties.Resources.NonExistantFolderText, Properties.Resources.ErrorText, MessageBoxButton.OK,
                    MessageBoxImage.Error);
                OutputArea.Text = OutputArea.Text + "\n" + "[" + DateTime.Now +
                                  "]"+ Properties.Resources.NonExistantFolderText;
                CompileButton.Visibility = Visibility.Visible;
                CancelTaskButton.Visibility = Visibility.Hidden;
                TestProjectButton.IsEnabled = true;
                ProjectLocation.IsEnabled = true;
                FindProjectButton.IsEnabled = true;
                UnlockSettings(true);
            }
            else
            {
                Array.Resize(ref _projectList, 1);
                _projectList[0] = ProjectLocation.Text;
                MainProgress.Value = 0;
                //MainProgress.Maximum = PackageNwCheckbox.IsChecked == true ? 4 : 3;
                TaskbarInfoHolder.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
                TaskbarInfoHolder.ProgressValue = 0;
                _compilerWorker.RunWorkerAsync();
            }
        }

        private void TestProjectButton_Click(object sender, RoutedEventArgs e)
        {
                try
                {
                    CoreCode.RunTest(Settings.Default.SDKLocation, ProjectLocation.Text);
                }
                catch (FileNotFoundException nwjsException){
                    MessageBox.Show(Properties.Resources.MissingNwjsExecutableError, Properties.Resources.ErrorText, MessageBoxButton.OK, MessageBoxImage.Error);
                    OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + nwjsException;
                }
                catch (Win32Exception nwjsException)
                {
                    MessageBox.Show(Properties.Resources.Win32ApiErrorText, Properties.Resources.ErrorText, MessageBoxButton.OK, MessageBoxImage.Error);
                    OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + nwjsException;
                }
                catch (InvalidOperationException nwjsException)
                {
                    MessageBox.Show(Properties.Resources.InvalidOperationErrorText, Properties.Resources.ErrorText, MessageBoxButton.OK, MessageBoxImage.Error);
                    OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + nwjsException;
                }
        }

        private void CancelTaskButton_Click(object sender, RoutedEventArgs e)
        {
            _compilerWorker.CancelAsync();
        }
        #endregion

        #region Quick Compile Code
        private void StartCompiler(object sender, DoWorkEventArgs e)
        {
            var compilerInput = _projectList[0];
            try
            {
                const string folderMap = "js";
                FileMap = CoreCode.FileFinder(Path.Combine(compilerInput, "www", folderMap), "*.js");
                _compilerWorker.ReportProgress(_currentFile + 1);
                CoreCode.CleanupBin(FileMap);
                CoreCode.CompilerInfo.FileName = Path.Combine(Settings.Default.SDKLocation, "nwjc.exe");
                if (Settings.Default.PackageCode)
                    Dispatcher.Invoke(() => MainProgress.Maximum = FileMap.Length + ((Settings.Default.PackageCode) ? ((Settings.Default.CompressionEngineSafeMode) ? 2 : 1) : 0));
                _compilerStatusReport = 1;
                for(_currentFile = 0; _currentFile < FileMap.Length; _currentFile++)
                {
                    if (_compilerWorker.CancellationPending)
                    {
                        e.Cancel = true;
                        break;
                    }
                    CoreCode.CompilerWorkerTask(FileMap[_currentFile], Settings.Default.FileExtension, Settings.Default.DeleteSourceCode);
                    _compilerWorker.ReportProgress(_currentFile + 1);
                    _compilerStatusReport = 2;
                    Thread.Sleep(200);
                }

                if (!_compilerWorker.CancellationPending)
                {
                    if (Settings.Default.PackageCode)
                    {
                        if (Settings.Default.CompressionEngineSafeMode)
                        {
                            _compilerStatusReport = 3;
                            _compilerWorker.ReportProgress(_currentFile + 1);
                            CoreCode.PreparePack(compilerInput);
                        }
                        _compilerStatusReport = 4;
                        _compilerWorker.ReportProgress(_currentFile + 2);
                        CoreCode.CompressFiles(compilerInput, compilerInput, Settings.Default.CompressionMode, Settings.Default.CompressionEngineSafeMode);
                        if (Settings.Default.DeleteSourceCode)
                        {
                            _compilerStatusReport = 5;
                            _compilerWorker.ReportProgress(_currentFile + 3);
                            CoreCode.DeleteFiles(compilerInput);
                        }
                    }
                }

                if (_compilerWorker.CancellationPending) return;
                _compilerStatusReport = 6;
                _compilerWorker.ReportProgress(_currentFile + 1);
            }
            catch (PathTooLongException exceptionOutput)
            {
                Dispatcher.Invoke(() =>
                {
                    TaskbarInfoHolder.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error;
                    OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\n" + exceptionOutput + "\n";
                    StatusLabel.Content = Properties.Resources.FailedText;
                });
                MessageBox.Show(Properties.Resources.PathTooLongErrorText, Properties.Resources.FailedText, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (UnauthorizedAccessException exceptionOutput)
            {
                Dispatcher.Invoke(() =>
                {
                    TaskbarInfoHolder.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error;
                    OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\n" + exceptionOutput + "\n";
                    StatusLabel.Content = Properties.Resources.FailedText;
                });
                MessageBox.Show(Properties.Resources.UnauthorizedAccessErrorText, Properties.Resources.FailedText, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            catch (ArgumentException exceptionOutput)
            {
                Dispatcher.Invoke(() =>
                {
                    TaskbarInfoHolder.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error;
                    OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\n" + exceptionOutput + "\n";
                    StatusLabel.Content = Properties.Resources.FailedText;
                });
                MessageBox.Show(Properties.Resources.ArgumentExceptionErrorText, Properties.Resources.FailedText, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (FileNotFoundException exceptionOutput)
            {
                Dispatcher.Invoke(() =>
                {
                    TaskbarInfoHolder.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error;
                    OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\n" + exceptionOutput + "\n";
                    StatusLabel.Content = Properties.Resources.FailedText;
                });
                MessageBox.Show(Properties.Resources.FineNotFoundErrorText, Properties.Resources.FailedText, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (DirectoryNotFoundException exceptionOutput)
            {
                Dispatcher.Invoke(() =>
                {
                    TaskbarInfoHolder.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error;
                    OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\n" + exceptionOutput + "\n";
                    StatusLabel.Content = Properties.Resources.FailedText;
                });
                MessageBox.Show(
                    Properties.Resources.DirectoryNotFoundErrorText,
                    Properties.Resources.FailedText, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (IOException exceptionOutput)
            {
                Dispatcher.Invoke(() =>
                {
                    TaskbarInfoHolder.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error;
                    OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\n" + exceptionOutput + "\n";
                    StatusLabel.Content = Properties.Resources.FailedText;
                });
                MessageBox.Show(Properties.Resources.IOExceptionErrorText, Properties.Resources.FailedText, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CompilerReport(object sender, ProgressChangedEventArgs e)
        {
            switch (_compilerStatusReport)
            {
                case 6:

                    MainProgress.Value += MainProgress.Maximum;
                    TaskbarInfoHolder.ProgressValue = 1;
                    break;
                case 5:
                    MainProgress.Value += 1;
                    TaskbarInfoHolder.ProgressValue = MainProgress.Value / MainProgress.Maximum;
                    TaskbarInfoHolder.ProgressValue = (double)_currentFile / FileMap.Length;
                    OutputArea.Text = OutputArea.Text + "\n[" + DateTime.Now + "]Removing files...";
                    break;
                case 4:
                    TaskbarInfoHolder.ProgressValue = MainProgress.Value / MainProgress.Maximum;
                    StatusLabel.Content = Properties.Resources.PackaginStatusText;
                    OutputArea.Text = OutputArea.Text + "\n[" + DateTime.Now + "]" + Properties.Resources.PackageCreationText;
                    break;
                case 3:
                    StatusLabel.Content = Properties.Resources.CopyToTempLocationStatusText;
                    OutputArea.Text = OutputArea.Text + "\n[" + DateTime.Now + "]"+ Properties.Resources.FileCopyText;
                    break;
                case 2:
                    if (_currentFile < FileMap.Length)
                    {
                        MainProgress.Value = _currentFile;
                        TaskbarInfoHolder.ProgressValue = MainProgress.Value / MainProgress.Maximum;
                        OutputArea.Text += "\n[" + DateTime.Now + "]" + Properties.Resources.FileText + FileMap[_currentFile] + Properties.Resources.CompiledOutputText;
                        OutputArea.Text +=
                            "\n[" + DateTime.Now + "]" + Properties.Resources.CompilingText + FileMap[_currentFile] +
                            "...";
                        StatusLabel.Content = Properties.Resources.CompileText + FileMap[_currentFile] + "...";
                    }
                    break;
                case 1:
                    OutputArea.Text += "\n[" + DateTime.Now + "]" + Properties.Resources.CompilingText + FileMap[_currentFile] +
                                       "...";
                    StatusLabel.Content = Properties.Resources.CompileText + FileMap[_currentFile] + "...";
                    break;
                case 0:
                    OutputArea.Text += Properties.Resources.StartTaskPointText;
                    StatusLabel.Content = Properties.Resources.BinRemovalProgressText;
                    OutputArea.Text += "[" + DateTime.Now +
                                       "]"+Properties.Resources.BinRemovalText;
                    break;
                default:
                    OutputArea.Text += "Unknown Status Code";
                    StatusLabel.Content = "Unknown Status Code";
                    OutputArea.Text += "[" + DateTime.Now +
                                       "]" + "Unknown Status Code";
                    break;
            }
        }

        private void CompilerFinisher(object sender, RunWorkerCompletedEventArgs e)
        {
            //if (e.Error != null)
            //{
            //    OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\n" + _errorOutput + "\n";
            //    StatusLabel.Content = Properties.Resources.FailedText;
            //}
            if (e.Cancelled)
            {
                TaskbarInfoHolder.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Paused;
                MainProgress.Foreground = Brushes.YellowGreen;
                OutputArea.Text += "[" + DateTime.Now + "]" + Properties.Resources.TaskCancelledOutputText + "\n";
                MessageBox.Show(Properties.Resources.TaskCancelledMessage, Properties.Resources.AbortedText, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(Properties.Resources.CompilationCompleteText, Properties.Resources.DoneText,
                    MessageBoxButton.OK, MessageBoxImage.Information);
                StatusLabel.Content = Properties.Resources.DoneText;
                OutputArea.Text = OutputArea.Text + "[" + DateTime.Now + "]" + Properties.Resources.CompilationCompleteText + "\n";
            }
            MainProgress.Value = 0;
            MainProgress.Foreground = Brushes.ForestGreen;
            Array.Clear(FileMap, 0, FileMap.Length);
            CompileButton.IsEnabled = true;
            TestProjectButton.IsEnabled = true;
            FindProjectButton.IsEnabled = true;
            UnlockSettings(true);
            CompileButton.Visibility = Visibility.Visible;
            CancelTaskButton.Visibility = Visibility.Hidden;
            OutputArea.Text += Properties.Resources.TaskEndPointText;
            TaskbarInfoHolder.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
            TaskbarInfoHolder.ProgressValue = 0;

        }
        #endregion
        #endregion Quick Compile Code Set

        #region Batch Compile Code Set
        private void AddToMapButton_Click(object sender, RoutedEventArgs e)
        {
            var pickJsFolder =
                new VistaFolderBrowserDialog
                {
                    Description = Properties.Resources.ProjectPickerText,
                    UseDescriptionForTitle = true
                };
            var pickerResult = pickJsFolder.ShowDialog();
            if (pickerResult != true) return;
            if (pickJsFolder.SelectedPath != null) FolderList.Items.Add(pickJsFolder.SelectedPath);
        }

        private void RemoveFromMapButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var s in FolderList.SelectedItems.OfType<string>().ToList())
                FolderList.Items.Remove(s);
        }

        private void MapCompileButton_Click(object sender, RoutedEventArgs e)
        {
            MapCompileButton.Visibility = Visibility.Hidden;
            CancelMapCompileButton.Visibility = Visibility.Visible;
            AddToMapButton.IsEnabled = false;
            RemoveFromMapButton.IsEnabled = false;
            MapProgress.Foreground = Brushes.ForestGreen;
            CurrentWorkloadBar.Foreground = Brushes.ForestGreen;
            MapProgress.Value = 0;

            if (!File.Exists(Path.Combine(NwjsLocation.Text, "nwjc.exe")))
            {
                MessageBox.Show(Properties.Resources.CompilerMissingText, Properties.Resources.ErrorText, MessageBoxButton.OK, MessageBoxImage.Error);
                OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\n"+ Properties.Resources.CompilerMissingText +  "\n-----";
                MapCompileButton.Visibility = Visibility.Visible;
                CancelMapCompileButton.Visibility = Visibility.Hidden;
                AddToMapButton.IsEnabled = true;
                RemoveFromMapButton.IsEnabled = true;
            }
            else if (FolderList.Items.Count == 0)
            {
                MessageBox.Show(Properties.Resources.NoJSFilesPresent, Properties.Resources.ErrorText,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now +
                                  "\n"+ Properties.Resources.NonExistantFolderText + "\n-----";
                MapCompileButton.Visibility = Visibility.Visible;
                CancelMapCompileButton.Visibility = Visibility.Hidden;
                AddToMapButton.IsEnabled = true;
                RemoveFromMapButton.IsEnabled = true;
            }
            else
            {
                OutputArea.Text += Properties.Resources.StartTaskPointText;
                CoreCode.CompilerInfo.FileName = Path.Combine(Settings.Default.SDKLocation, "nwjc.exe");
                MapProgress.Value = 0;
                MapProgress.Maximum = FolderList.Items.Count;
                Array.Resize(ref _projectList, FolderList.Items.Count);
                TaskbarInfoHolder.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
                TaskbarInfoHolder.ProgressValue = 0;
                _mapCompilerWorker.RunWorkerAsync();
            }
        }

        private void StartMapCompiler(object sender, DoWorkEventArgs e)
        {
            try
            {
                for (_currentProject = 0; _currentProject < _projectList.Length; _currentProject++)
                {
                    _compilerStatusReport = 0;
                    if (_mapCompilerWorker.CancellationPending)
                    {
                        e.Cancel = true;
                        break;
                    }                    
                    FileMap = CoreCode.FileFinder(FolderList.Items[_currentProject].ToString(), "*.js");
                    _mapCompilerWorker.ReportProgress(_currentProject + 1);
                    Thread.Sleep(200);
                    _compilerStatusReport = 1;
                    _mapCompilerWorker.ReportProgress(_currentProject + 1);
                    CoreCode.CleanupBin(FileMap);
                    _compilerStatusReport = 2;
                    for (_currentFile = 0; _currentFile < FileMap.Length; _currentFile++)
                    {
                        if (_mapCompilerWorker.CancellationPending)
                        {
                            e.Cancel = true;
                            break;
                        }
                        _mapCompilerWorker.ReportProgress(_currentProject + 1);
                        CoreCode.CompilerWorkerTask(FileMap[_currentFile], Settings.Default.FileExtension,
                            Settings.Default.DeleteSourceCode);

                    }
                    if (e.Cancel) break;
                    Array.Clear(FileMap, 0, FileMap.Length);
                    _compilerStatusReport = 3;
                    _mapCompilerWorker.ReportProgress(_currentProject + 1);
                    Thread.Sleep(200);
                }
            }
            catch (PathTooLongException exceptionOutput)
            {
                Dispatcher.Invoke(() =>
                {
                    TaskbarInfoHolder.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error;
                    MapProgress.Foreground = Brushes.DarkRed;
                    OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\n" + exceptionOutput + "\n";
                    CurrentWorkloadBar.Foreground = Brushes.DarkRed;
                    CurrentWorkloadLabel.Content = Properties.Resources.FailedText;
                });
                MessageBox.Show(Properties.Resources.PathTooLongErrorText, Properties.Resources.FailedText, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (UnauthorizedAccessException exceptionOutput)
            {
                Dispatcher.Invoke(() =>
                {
                    TaskbarInfoHolder.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error;
                    MapProgress.Foreground = Brushes.DarkRed;
                    OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\n" + exceptionOutput + "\n";
                    CurrentWorkloadBar.Foreground = Brushes.DarkRed;
                    CurrentWorkloadLabel.Content = Properties.Resources.FailedText;
                });
                MessageBox.Show(Properties.Resources.UnauthorizedAccessErrorText, Properties.Resources.FailedText, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            catch (ArgumentException exceptionOutput)
            {
                Dispatcher.Invoke(() =>
                {
                    TaskbarInfoHolder.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error;
                    MapProgress.Foreground = Brushes.DarkRed;
                    OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\n" + exceptionOutput + "\n";
                    CurrentWorkloadBar.Foreground = Brushes.DarkRed;
                    CurrentWorkloadLabel.Content = Properties.Resources.FailedText;
                });
                MessageBox.Show(Properties.Resources.ArgumentExceptionErrorText, Properties.Resources.FailedText, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (FileNotFoundException exceptionOutput)
            {
                Dispatcher.Invoke(() =>
                {
                    TaskbarInfoHolder.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error;
                    MapProgress.Foreground = Brushes.DarkRed;
                    OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\n" + exceptionOutput + "\n";
                    CurrentWorkloadBar.Foreground = Brushes.DarkRed;
                    CurrentWorkloadLabel.Content = Properties.Resources.FailedText;
                });
                MessageBox.Show(Properties.Resources.FineNotFoundErrorText, Properties.Resources.FailedText, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (DirectoryNotFoundException exceptionOutput)
            {
                Dispatcher.Invoke(() =>
                {
                    TaskbarInfoHolder.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error;
                    MapProgress.Foreground = Brushes.DarkRed;
                    OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\n" + exceptionOutput + "\n";
                    CurrentWorkloadBar.Foreground = Brushes.DarkRed;
                    CurrentWorkloadLabel.Content = Properties.Resources.FailedText;
                });
                MessageBox.Show(
                    Properties.Resources.DirectoryNotFoundErrorText,
                    Properties.Resources.FailedText, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (IOException exceptionOutput)
            {
                Dispatcher.Invoke(() =>
                {
                    TaskbarInfoHolder.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error;
                    MapProgress.Foreground = Brushes.DarkRed;
                    OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\n" + exceptionOutput + "\n";
                    CurrentWorkloadBar.Foreground = Brushes.DarkRed;
                    CurrentWorkloadLabel.Content = Properties.Resources.FailedText;
                });
                MessageBox.Show(Properties.Resources.IOExceptionErrorText, Properties.Resources.FailedText, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MapCompilerReport(object sender, ProgressChangedEventArgs e)
        {
            switch (_compilerStatusReport)
            {
                case 3:
                    CurrentWorkloadBar.Value = 0;
                    TaskbarInfoHolder.ProgressValue = 0;
                    MapProgress.Value += 1;
                    OutputArea.Text += Properties.Resources.ProjectCompilationEndPointText;
                    break;
                case 2:
                    CurrentWorkloadBar.Value += 1;
                    TaskbarInfoHolder.ProgressValue = CurrentWorkloadBar.Value / CurrentWorkloadBar.Maximum;
                    CurrentWorkloadLabel.Content = Properties.Resources.CompileText + FileMap[_currentFile] + "...";
                    OutputArea.Text += "\n[" + DateTime.Now + "]" + Properties.Resources.FileText + FileMap[_currentFile] + Properties.Resources.CompiledOutputText;
                    OutputArea.Text +=
                        "\n[" + DateTime.Now + "]" + Properties.Resources.CompilingText + FileMap[_currentFile] +
                        "...";
                    break;
                case 1:
                    CurrentWorkloadBar.Maximum = FileMap.Length;
                    OutputArea.Text += "\n[" + DateTime.Now + "]" + Properties.Resources.BinRemovalText;
                    CurrentWorkloadLabel.Content =
                        Properties.Resources.BinRemovalStatusText + FolderList.Items[_currentProject] + "...";
                    break;
                case 0:
                    if (_currentProject < _projectList.Length)
                    {
                        OutputArea.Text += "[" + DateTime.Now  +"]"+ Properties.Resources.CompileText1 +
                                           FolderList.Items[_currentProject] + Properties.Resources.FolderText + Properties.Resources.ProjectCompilationStartPointText;
                        MapStatusLabel.Content = Properties.Resources.CompileText1 + FolderList.Items[_currentProject] +
                                                 Properties.Resources.FolderText;
                    }
                    break;

            }
        }

        private void MapCompilerFinisher(object sender, RunWorkerCompletedEventArgs e)
        {
            //if (e.Error != null)
            //{

            //}
            if (e.Cancelled)
            {
                TaskbarInfoHolder.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Paused;
                CurrentWorkloadBar.Foreground = Brushes.YellowGreen;
                MapProgress.Foreground = Brushes.YellowGreen;
                OutputArea.Text += "\n" + DateTime.Now + "\n" + Properties.Resources.TaskCancelledOutputText + "\n";
                MessageBox.Show(Properties.Resources.TaskCancelledMessage, Properties.Resources.AbortedText, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                OutputArea.Text = OutputArea.Text + "[" + DateTime.Now + "]" +
                                  Properties.Resources.CompilationCompleteText + "\n";
                MessageBox.Show(Properties.Resources.CompilationCompleteText, Properties.Resources.DoneText,
                    MessageBoxButton.OK, MessageBoxImage.Information);
                MapStatusLabel.Content = Properties.Resources.DoneText;
                CurrentWorkloadLabel.Content = Properties.Resources.DoneText;
            }
            Array.Clear(FileMap, 0, FileMap.Length);
            Array.Clear(_projectList, 0, _projectList.Length);
            MapCompileButton.Visibility = Visibility.Visible;
            CancelMapCompileButton.Visibility = Visibility.Hidden;
            AddToMapButton.IsEnabled = true;
            RemoveFromMapButton.IsEnabled = true;
            UnlockSettings(true);
            OutputArea.Text += Properties.Resources.TaskEndPointText;
            TaskbarInfoHolder.ProgressValue = 0;
            TaskbarInfoHolder.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
            MapProgress.Value = 0;
            CurrentWorkloadBar.Value = 0;
            MapProgress.Value = 0;
            MapProgress.Foreground = Brushes.ForestGreen;
            CurrentWorkloadBar.Foreground = Brushes.ForestGreen;
        }

        private void CancelMapCompileButton_Click(object sender, RoutedEventArgs e)
        {
            _mapCompilerWorker.CancelAsync();
        }
        #endregion Batch Compile Code Set

    }
}