using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CompilerCore;
using Microsoft.Win32;
using nwjsCookToolUI.Properties;
using Ookii.Dialogs.Wpf;

namespace nwjsCookToolUI
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly string _tempFolderLocation = Path.Combine(Path.GetTempPath(), "nwjspackage");

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BrowseSDKButton_Click(object sender, RoutedEventArgs e)
        {
            var pickSdkFolder = new OpenFileDialog
            {
                DefaultExt = "nwjc.exe",
                Filter = "nwjs Compiler|nwjc.exe",
                Title = "Select the nwjs Compiler."
            };
            var pickerResult = pickSdkFolder.ShowDialog();
            if (pickerResult != true) return;
            Settings.Default.SDKLocation = pickSdkFolder.FileName;
            NwjsLocation.Text = pickSdkFolder.FileName;
            Settings.Default.Save();
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
            MainProgress.Foreground = Brushes.ForestGreen;
            if (!File.Exists(NwjsLocation.Text))
            {
                MessageBox.Show("The nwjs Compiler is missing.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\nMissing nwjs Compiler executable.\n-----";
            }
            else if (!Directory.Exists(ProjectLocation.Text))
            {
                MessageBox.Show("Can't operate on an non-existent folder.", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now +
                                  "\nCannot operate in a non-existent folder.\n-----";
            }
            else
            {
                MainProgress.Value = 0;
                MainProgress.Maximum = PackageNwCheckbox.IsChecked == true ? 4 : 3;
                var compilerWorker = new BackgroundWorker();
                compilerWorker.WorkerReportsProgress = true;
                compilerWorker.DoWork += StartCompiler;
                compilerWorker.ProgressChanged += CompilerReport;
                compilerWorker.RunWorkerAsync();
            }
        }


        private void StartCompiler(object sender, DoWorkEventArgs e)
        {
            var compilerInput = Dispatcher.Invoke(() => ProjectLocation.Text);
            var packageOutput = Path.Combine(compilerInput, "package.nw");
            Dispatcher.Invoke(() => StatusLabel.Content = "Compiling scripts in the js folder...");
            try
            {
                var folderMap = "js";
                CoreCode.FileFinder(Path.Combine(compilerInput, "www", folderMap), "*.js");
                Dispatcher.Invoke(() =>
                    OutputArea.Text += "\n" + DateTime.Now +
                                       "\nRemoving binary files from the project (if there are)...\n");
                Dispatcher.Invoke(() => StatusLabel.Content = "Removing binary files (if present)...");
                CoreCode.CleanupBin();
                CoreCode.CompilerInfo.FileName = Dispatcher.Invoke(() => NwjsLocation.Text);
                Dispatcher.Invoke(() => MainProgress.Maximum = CoreCode.FileMap.Length);
                if (Dispatcher.Invoke(() => PackageNwCheckbox.IsChecked == true))
                    Dispatcher.Invoke(() => MainProgress.Maximum += 1);
                foreach (var fileName in CoreCode.FileMap)
                {
                    Dispatcher.Invoke(
                        () => OutputArea.Text += "\n" + DateTime.Now + "\nCompiling " + fileName + "...\n");
                    Thread.Sleep(200);
                    Dispatcher.Invoke(() =>
                        StatusLabel.Content = StatusLabel.Content = "Compiling " + fileName + "...");
                    Thread.Sleep(200);
                    Dispatcher.Invoke(() => CoreCode.CompilerWorkerTask(fileName, FileExtensionTextbox.Text,
                        RemoveCompiledJsCheckbox.IsChecked == true));
                    Dispatcher.Invoke(() => OutputArea.Text += "\nCompiled on " + DateTime.Now + "\n");
                    Dispatcher.Invoke(() => MainProgress.Value += 1);
                }

                Array.Clear(CoreCode.FileMap, 0, CoreCode.FileMap.Length);


                if (Dispatcher.Invoke(() => PackageNwCheckbox.IsChecked == true))
                {
                    Dispatcher.Invoke(() => StatusLabel.Content = "Packaging...");
                    Thread.Sleep(200);
                    Dispatcher.Invoke(() => OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now +
                                                              "\n Copying files to a temporary area...\n");
                    Thread.Sleep(200);
                    if (Directory.Exists(_tempFolderLocation)) Directory.Delete(_tempFolderLocation, true);
                    Dispatcher.Invoke(() => CoreCode.DirectoryCopy(Path.Combine(compilerInput, "www"),
                        Path.Combine(_tempFolderLocation, "www"), true));
                    File.Copy(Path.Combine(compilerInput, "package.json"),
                        Path.Combine(_tempFolderLocation, "package.json"), true);
                    Dispatcher.Invoke(() =>
                        OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\n Creating package...\n");
                    if (File.Exists(packageOutput)) File.Delete(packageOutput);
                    ZipFile.CreateFromDirectory(_tempFolderLocation,
                        packageOutput);
                    Directory.Delete(_tempFolderLocation, true);
                    Dispatcher.Invoke(() => MainProgress.Value += 1);
                }

                Dispatcher.Invoke(() =>
                    OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\n Compilation complete!\n");
                MessageBox.Show("Compilation complete!", "Done!", MessageBoxButton.OK, MessageBoxImage.Information);
                Dispatcher.Invoke(() => StatusLabel.Content = "Done!");
            }

            catch (Exception exceptionOutput)
            {
                Dispatcher.Invoke(() =>
                    OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\n" + exceptionOutput + "\n");
                Dispatcher.Invoke(() => StatusLabel.Content = "Failed!");
                MessageBox.Show("Ack! An error occured! See the output in the About tab.", "Failure!",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Dispatcher.Invoke(() => MainProgress.Value = 0);
                Dispatcher.Invoke(() => MainProgress.Foreground = Brushes.ForestGreen);
                Array.Clear(CoreCode.FileMap, 0, CoreCode.FileMap.Length);
            }

            Dispatcher.Invoke(() => CompileButton.IsEnabled = true);
        }

        private void StartMapCompiler(object sender, DoWorkEventArgs e)
        {
            try
            {
                for (var i = 0; i < FolderList.Items.Count; i++)
                {
                    var i1 = i;
                    Dispatcher.Invoke(() =>
                        OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "Compiling scripts in the " +
                                          FolderList.Items[i1] + " folder...\n");

                    Dispatcher.Invoke(() =>
                        MapStatusLabel.Content = "Compiling scripts in the " + FolderList.Items[i1] + " folder...");
                    CoreCode.FileFinder(FolderList.Items[i1].ToString(), "*.js");
                    Dispatcher.Invoke(() =>
                        OutputArea.Text += "\n" + DateTime.Now +
                                           "\nRemoving binary files from the project (if there are)...\n");
                    Dispatcher.Invoke(() => CurrentWorkloadBar.Maximum = CoreCode.FileMap.Length);

                    Dispatcher.Invoke(() =>
                        CurrentWorkloadLabel.Content = "Removing binary files from " + FolderList.Items[i1] + "...");
                    CoreCode.CleanupBin();
                    foreach (var file in CoreCode.FileMap)
                    {
                        Dispatcher.Invoke(() => CurrentWorkloadLabel.Content = "Compiling" + file + "...");
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
                    OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\n Compilation complete!\n");
                MessageBox.Show("Compilation complete!", "Done!", MessageBoxButton.OK, MessageBoxImage.Information);
                Dispatcher.Invoke(() => MapStatusLabel.Content = "Done!");
                Dispatcher.Invoke(() => CurrentWorkloadLabel.Content = "Done!");
            }
            catch (Exception exceptionOutput)
            {
                Dispatcher.Invoke(() => MapProgress.Foreground = Brushes.DarkRed);
                Dispatcher.Invoke(() =>
                    OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\n" + exceptionOutput + "\n");
                Dispatcher.Invoke(() => MapStatusLabel.Content = "Failed!");
                Dispatcher.Invoke(() => CurrentWorkloadBar.Foreground = Brushes.DarkRed);
                Dispatcher.Invoke(() => CurrentWorkloadLabel.Content = "Failed!");

                Dispatcher.Invoke(() =>
                    OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\n" + exceptionOutput + "\n");
                MessageBox.Show("Ack! An error occured! See the output in the About tab.", "Failure!",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Dispatcher.Invoke(() => CurrentWorkloadBar.Value = 0);
                Dispatcher.Invoke(() => MapProgress.Value = 0);
                Dispatcher.Invoke(() => MapProgress.Foreground = Brushes.ForestGreen);
                Dispatcher.Invoke(() => CurrentWorkloadBar.Foreground = Brushes.ForestGreen);
                Array.Clear(CoreCode.FileMap, 0, CoreCode.FileMap.Length);
            }

            Dispatcher.Invoke(() => MapCompileButton.IsEnabled = true);
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
            MainProgress.Foreground = Brushes.ForestGreen;
            if (!File.Exists(NwjsLocation.Text))
            {
                MessageBox.Show("The nwjs Compiler is missing.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\nMissing nwjs Compiler executable.\n-----";
            }
            else if (FolderList.Items.Count == 0)
            {
                MessageBox.Show("Please add the folders you want the JavaScript files to be compiled.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now +
                                  "\nCannot operate in a non-existent folder.\n-----";
            }
            else
            {
                CoreCode.CompilerInfo.FileName = Dispatcher.Invoke(() => NwjsLocation.Text);
                MapProgress.Value = 0;
                MapProgress.Maximum = FolderList.Items.Count;
                var compilerWorker = new BackgroundWorker();
                compilerWorker.WorkerReportsProgress = true;
                compilerWorker.DoWork += StartMapCompiler;
                compilerWorker.ProgressChanged += CompilerReport;
                compilerWorker.RunWorkerAsync();
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
    }
}