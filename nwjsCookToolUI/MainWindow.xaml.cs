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
        private static BackgroundWorker CompilerWorker;
        private static BackgroundWorker MapCompilerWorker;

        public MainWindow()
        {
            InitializeComponent();
            PrepareWorkers();
        }

        private void PrepareWorkers()
        {
            CompilerWorker.WorkerReportsProgress = true;
            CompilerWorker.WorkerSupportsCancellation = true;
            CompilerWorker.DoWork += StartCompiler;
            CompilerWorker.ProgressChanged += CompilerReport;

            MapCompilerWorker.WorkerReportsProgress = true;
            MapCompilerWorker.WorkerSupportsCancellation = true;
            MapCompilerWorker.DoWork += StartMapCompiler;
            MapCompilerWorker.ProgressChanged += CompilerReport;
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

        public void LockSettings(bool lockSetting)
        {
            RemoveCompiledJsCheckbox.IsEnabled = lockSetting;
            PackageNwCheckbox.IsEnabled = lockSetting;
            FileExtensionTextbox.IsEnabled = lockSetting;
            BrowseSdkButton.IsEnabled = lockSetting;
            RemoveFilesCheckBox.IsEnabled = lockSetting && PackageNwCheckbox.IsChecked == true;
        }

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

        private void CompileButton_Click(object sender, RoutedEventArgs e)
        {
            CompileButton.IsEnabled = false;
            TestProjectButton.IsEnabled = false;
            ProjectLocation.IsEnabled = false;
            FindProjectButton.IsEnabled = false;
            LockSettings(false);
            MainProgress.Foreground = Brushes.ForestGreen;
            if (!File.Exists(Path.Combine(NwjsLocation.Text, "nwjc.exe")))
            {
                MessageBox.Show(Properties.Resources.CompilerMissingText, Properties.Resources.ErrorText, MessageBoxButton.OK, MessageBoxImage.Error);
                OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\n"+ Properties.Resources.CompilerMissingText + "\n-----";
                CompileButton.IsEnabled = true;
                TestProjectButton.IsEnabled = true;
                ProjectLocation.IsEnabled = true;
                FindProjectButton.IsEnabled = true;
            }
            else if (!Directory.Exists(ProjectLocation.Text))
            {
                MessageBox.Show(Properties.Resources.NonExistantFolderText, Properties.Resources.ErrorText, MessageBoxButton.OK,
                    MessageBoxImage.Error);
                OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now +
                                  "\n"+ Properties.Resources.NonExistantFolderText +"\n-----";
                CompileButton.IsEnabled = true;
                TestProjectButton.IsEnabled = true;
                ProjectLocation.IsEnabled = true;
                FindProjectButton.IsEnabled = true;
                LockSettings(true);
            }
            else
            {
                MainProgress.Value = 0;
                MainProgress.Maximum = PackageNwCheckbox.IsChecked == true ? 4 : 3;
                var compilerWorker = new BackgroundWorker();
                compilerWorker.RunWorkerAsync();
            }
        }


        private void StartCompiler(object sender, DoWorkEventArgs e)
        {
            var compilerInput = Dispatcher.Invoke(() => ProjectLocation.Text);
            Dispatcher.Invoke(() => StatusLabel.Content = Properties.Resources.CompileJsFolderProgressText);
            try
            {
                var folderMap = "js";
                CoreCode.FileFinder(Path.Combine(compilerInput, "www", folderMap), "*.js");
                Dispatcher.Invoke(() =>
                    OutputArea.Text += "\n" + DateTime.Now +
                                       Properties.Resources.BinRemovalText);
                Dispatcher.Invoke(() => StatusLabel.Content = Properties.Resources.BinRemovalProgressText);
                CoreCode.CleanupBin();
                CoreCode.CompilerInfo.FileName = Dispatcher.Invoke(() => Path.Combine(NwjsLocation.Text, "nwjc.exe"));
                Dispatcher.Invoke(() => MainProgress.Maximum = CoreCode.FileMap.Length);
                if (Dispatcher.Invoke(() => PackageNwCheckbox.IsChecked == true))
                    Dispatcher.Invoke(() => MainProgress.Maximum += 1);
                if(Dispatcher.Invoke(() => RemoveFilesCheckBox.IsChecked == true)) Dispatcher.Invoke(() => MainProgress.Maximum += 1);
                foreach (var fileName in CoreCode.FileMap)
                {
                    Dispatcher.Invoke(
                        () => OutputArea.Text += "\n" + DateTime.Now + Properties.Resources.CompilingText + fileName + "...\n");
                    Dispatcher.Invoke(() =>
                        StatusLabel.Content = StatusLabel.Content = Properties.Resources.CompileText + fileName + "...");
                    Dispatcher.Invoke(() => CoreCode.CompilerWorkerTask(fileName, FileExtensionTextbox.Text,
                        RemoveCompiledJsCheckbox.IsChecked == true));
                    Dispatcher.Invoke(() => OutputArea.Text += Properties.Resources.CompiledOutputText + DateTime.Now + "\n");
                    Dispatcher.Invoke(() => MainProgress.Value += 1);
                }

                Array.Clear(CoreCode.FileMap, 0, CoreCode.FileMap.Length);


                if (Dispatcher.Invoke(() => PackageNwCheckbox.IsChecked == true))
                {
                    Dispatcher.Invoke(() => StatusLabel.Content = Properties.Resources.PackaginStatusText);
                    Dispatcher.Invoke(() => OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now +
                                                              Properties.Resources.FileCopyText);
                    Dispatcher.Invoke(() => CoreCode.PreparePack(compilerInput));
                    Dispatcher.Invoke(() => 
                        OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + Properties.Resources.PackageCreationText);
                    CoreCode.CompressFiles(compilerInput);                   
                    Dispatcher.Invoke(() => MainProgress.Value += 1);
                    if (Dispatcher.Invoke(() => RemoveFilesCheckBox.IsChecked == true))
                    {
                        Dispatcher.Invoke(() => MainProgress.Value += 1);
                        Dispatcher.Invoke(() => OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\nRemoving files...\n");
                        CoreCode.DeleteFiles(compilerInput);
                    }
                }

                Dispatcher.Invoke(() =>
                    OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\n "+ Properties.Resources.CompilationCompleteText + "\n");
                MessageBox.Show(Properties.Resources.CompilationCompleteText, Properties.Resources.DoneText, MessageBoxButton.OK, MessageBoxImage.Information);
                Dispatcher.Invoke(() => StatusLabel.Content = Properties.Resources.DoneText);
            }

            catch (Exception exceptionOutput)
            {
                Dispatcher.Invoke(() =>
                    OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\n" + exceptionOutput + "\n");
                Dispatcher.Invoke(() => StatusLabel.Content = Properties.Resources.FailedText);
                MessageBox.Show(Properties.Resources.ErrorOccuredText, Properties.Resources.FailedText,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Dispatcher.Invoke(() => MainProgress.Value = 0);
                Dispatcher.Invoke(() => MainProgress.Foreground = Brushes.ForestGreen);
                Array.Clear(CoreCode.FileMap, 0, CoreCode.FileMap.Length);
            }

            Dispatcher.Invoke(() => CompileButton.IsEnabled = true);
            Dispatcher.Invoke(() => TestProjectButton.IsEnabled = true);
            Dispatcher.Invoke(() => ProjectLocation.IsEnabled = true);
            Dispatcher.Invoke(() => FindProjectButton.IsEnabled = true);
            Dispatcher.Invoke(() => LockSettings(true));
        }

        private void StartMapCompiler(object sender, DoWorkEventArgs e)
        {
            try
            {
                for (var i = 0; i < FolderList.Items.Count; i++)
                {
                    var i1 = i;
                    Dispatcher.Invoke(() =>
                        OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + Properties.Resources.CompileText1 +
                                          FolderList.Items[i1] + Properties.Resources.FolderText + "\n");

                    Dispatcher.Invoke(() =>
                        MapStatusLabel.Content = Properties.Resources.CompileText1 + FolderList.Items[i1] + Properties.Resources.FolderText);
                    CoreCode.FileFinder(FolderList.Items[i1].ToString(), "*.js");
                    Dispatcher.Invoke(() =>
                        OutputArea.Text += "\n" + DateTime.Now +
                                           Properties.Resources.BinRemovalText);
                    Dispatcher.Invoke(() => CurrentWorkloadBar.Maximum = CoreCode.FileMap.Length);

                    Dispatcher.Invoke(() =>
                        CurrentWorkloadLabel.Content = Properties.Resources.BinRemovalStatusText + FolderList.Items[i1] + "...");
                    CoreCode.CleanupBin();
                    foreach (var file in CoreCode.FileMap)
                    {
                        Dispatcher.Invoke(() => CurrentWorkloadLabel.Content = Properties.Resources.CompileText + file + "...");
                        Thread.Sleep(200);
                        Dispatcher.Invoke(() => CoreCode.CompilerWorkerTask(file, FileExtensionTextbox.Text,
                            RemoveCompiledJsCheckbox.IsChecked == true));
                        Thread.Sleep(200);
                        Dispatcher.Invoke(() => CurrentWorkloadBar.Value += 1);
                    }

                    Array.Clear(CoreCode.FileMap, 0, CoreCode.FileMap.Length);
                    Dispatcher.Invoke(() => CurrentWorkloadBar.Value = 0);
                    Dispatcher.Invoke(() => MapProgress.Value += 1);
                }

                Dispatcher.Invoke(() =>
                    OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\n" + Properties.Resources.CompilationCompleteText + "\n");
                MessageBox.Show(Properties.Resources.CompilationCompleteText, Properties.Resources.DoneText, MessageBoxButton.OK, MessageBoxImage.Information);
                Dispatcher.Invoke(() => MapStatusLabel.Content = Properties.Resources.DoneText);
                Dispatcher.Invoke(() => CurrentWorkloadLabel.Content = Properties.Resources.DoneText);
            }
            catch (Exception exceptionOutput)
            {
                Dispatcher.Invoke(() => MapProgress.Foreground = Brushes.DarkRed);
                Dispatcher.Invoke(() =>
                    OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\n" + exceptionOutput + "\n");
                Dispatcher.Invoke(() => MapStatusLabel.Content = Properties.Resources.FailedText);
                Dispatcher.Invoke(() => CurrentWorkloadBar.Foreground = Brushes.DarkRed);
                Dispatcher.Invoke(() => CurrentWorkloadLabel.Content = Properties.Resources.FailedText);

                Dispatcher.Invoke(() =>
                    OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\n" + exceptionOutput + "\n");
                MessageBox.Show(Properties.Resources.ErrorOccuredText, Properties.Resources.FailedText,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Dispatcher.Invoke(() => CurrentWorkloadBar.Value = 0);
                Dispatcher.Invoke(() => MapProgress.Value = 0);
                Dispatcher.Invoke(() => MapProgress.Foreground = Brushes.ForestGreen);
                Dispatcher.Invoke(() => CurrentWorkloadBar.Foreground = Brushes.ForestGreen);
                Array.Clear(CoreCode.FileMap, 0, CoreCode.FileMap.Length);
            }

            Dispatcher.Invoke(() => MapCompileButton.IsEnabled = true);
            Dispatcher.Invoke(() => AddToMapButton.IsEnabled = true);
            Dispatcher.Invoke(() => RemoveFromMapButton.IsEnabled = true);
        }

        private void CompilerReport(object sender, ProgressChangedEventArgs e)
        {
            MainProgress.Value += 1;
        }

        private void CookToolUi_Loaded(object sender, RoutedEventArgs e)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            var version = fvi.FileVersion;
            ProgramVersionLabel.Content = ProgramVersionLabel.Content + @" (" + version + @")";
        }

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
            MapCompileButton.IsEnabled = false;
            AddToMapButton.IsEnabled = false;
            RemoveFromMapButton.IsEnabled = false;
            MainProgress.Foreground = Brushes.ForestGreen;
            if (!File.Exists(Path.Combine(NwjsLocation.Text, "nwjc.exe")))
            {
                MessageBox.Show(Properties.Resources.CompilerMissingText, Properties.Resources.ErrorText, MessageBoxButton.OK, MessageBoxImage.Error);
                OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\n"+ Properties.Resources.CompilerMissingText +  "\n-----";
                MapCompileButton.IsEnabled = true;
                AddToMapButton.IsEnabled = true;
                RemoveFromMapButton.IsEnabled = true;
            }
            else if (FolderList.Items.Count == 0)
            {
                MessageBox.Show(Properties.Resources.NoJSFilesPresent, Properties.Resources.ErrorText,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now +
                                  "\n"+ Properties.Resources.NonExistantFolderText + "\n-----";
                MapCompileButton.IsEnabled = true;
                AddToMapButton.IsEnabled = true;
                RemoveFromMapButton.IsEnabled = true;
            }
            else
            {
                CoreCode.CompilerInfo.FileName = Dispatcher.Invoke(() => Path.Combine(NwjsLocation.Text, "nwjc.exe"));
                MapProgress.Value = 0;
                MapProgress.Maximum = FolderList.Items.Count;
                MapCompilerWorker.RunWorkerAsync();
            }
        }

        private void FileExtensionTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.Default.Save();
        }

        private void Checkbox_CheckChanged(object sender, RoutedEventArgs e)
        {
            Settings.Default.Save();
        }

        private void TestProjectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                CoreCode.RunTest(Settings.Default.SDKLocation, ProjectLocation.Text);
            }
            catch (Exception nwjsException)
            {
                MessageBox.Show(Properties.Resources.ErrorOccuredText, Properties.Resources.ErrorText, MessageBoxButton.OK, MessageBoxImage.Error);
                OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + nwjsException;
            }

        }

        private void ReplacementCodeButton_Click(object sender, RoutedEventArgs e)
        {
            string ci = CultureInfo.InstalledUICulture.ToString() == "el-GR" || CultureInfo.InstalledUICulture.ToString() == "el-CY" ? "el" : "en";
            string fileLocation = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), ci, "ReplacementCode.txt");
            if (File.Exists(fileLocation)) Process.Start(fileLocation);
            else
                MessageBox.Show(Properties.Resources.FileUnavailableText, Properties.Resources.InfoText, MessageBoxButton.OK,
                    MessageBoxImage.Information);
        }

        private void RemoveFilesCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.Save();
        }

        private void CancelTaskButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}