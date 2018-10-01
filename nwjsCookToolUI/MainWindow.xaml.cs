using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CompilerCore;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using nwjsCookToolUI.Properties;

namespace nwjsCookToolUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
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

            var pickSdkFolder = new Microsoft.Win32.OpenFileDialog
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
                new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog
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
                MessageBox.Show("Can't operate on an non-existent folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\nCannot operate in a non-existent folder.\n-----";
            }
            else
            {
                MainProgress.Value = 0;
                MainProgress.Maximum = PackageNwCheckbox.IsChecked == true ? 4 : 3;
                BackgroundWorker compilerWorker = new BackgroundWorker();
                compilerWorker.WorkerReportsProgress = true;
                compilerWorker.DoWork += StartCompiler;
                compilerWorker.ProgressChanged += CompilerReport;
                compilerWorker.RunWorkerAsync();
            }

            CompileButton.IsEnabled = true;

            //cookToolUi.Visibility = Visibility.Visible;
        }



        private void StartCompiler(object sender, DoWorkEventArgs e)
        {
            string compilerInput = Dispatcher.Invoke(() => ProjectLocation.Text);
            var packageOutput = compilerInput + "\\package.nw";
            Dispatcher.Invoke(() => StatusLabel.Content = "Compiling scripts in the js folder...");
            Thread.Sleep(400);
            try
            {
                CoreCode.FileMap = Directory.GetFiles(compilerInput + "\\www\\js\\", "*.js");
                string[] folderMap = { "libs", "plugins" };
                CoreCode.CompilerInfo.FileName = Dispatcher.Invoke(() => NwjsLocation.Text);
                Dispatcher.Invoke(() => OutputArea.Text = OutputArea.Text = "\n" + OutputArea.Text + DateTime.Now + "Compiling scripts in the js folder...\n-----");
                Thread.Sleep(200);


                Dispatcher.Invoke(() => CoreCode.CompilerWorkerTask(CoreCode.FileMap, FileExtensionTextbox.Text, RemoveCompiledJsCheckbox.IsChecked == true));
                Array.Clear(CoreCode.FileMap, 0, CoreCode.FileMap.Length);
                Dispatcher.Invoke(() => MainProgress.Value += 1);
                foreach (var folderName in folderMap)
                {
                    Dispatcher.Invoke(() => OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "Compiling scripts in the " + folderName + " folder...\n");
                    Thread.Sleep(200);
                    Dispatcher.Invoke(() => StatusLabel.Content = StatusLabel.Content = "Compiling scripts in the " + folderName + " folder...");
                    Thread.Sleep(200);
                    CoreCode.FileMap = Directory.GetFiles(compilerInput + "\\www\\js\\" + folderName + "\\", "*.js");
                    Dispatcher.Invoke(() => CoreCode.CompilerWorkerTask(CoreCode.FileMap, FileExtensionTextbox.Text, RemoveCompiledJsCheckbox.IsChecked == true));
                    Array.Clear(CoreCode.FileMap, 0, CoreCode.FileMap.Length);
                    Dispatcher.Invoke(() => MainProgress.Value += 1);
                }

                
                if (Dispatcher.Invoke(() => PackageNwCheckbox.IsChecked == true))
                {
                    Dispatcher.Invoke(() => StatusLabel.Content = "Packaging...");
                    Thread.Sleep(200);
                    Dispatcher.Invoke(() => OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now +
                                      "\n Copying files to a temporary area...\n");
                    Thread.Sleep(200);
                    //Directory.CreateDirectory(ProjectLocation.Text + "\\Package\\");
                    if (Directory.Exists(_tempFolderLocation)) Directory.Delete(_tempFolderLocation, true);
                    Dispatcher.Invoke(() => CoreCode.DirectoryCopy(compilerInput + "\\www\\",
                        _tempFolderLocation + "\\www\\", true));
                    File.Copy(compilerInput + "\\package.json",
                        _tempFolderLocation + "\\package.json", true);
                    Dispatcher.Invoke(() => OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\n Creating package...\n");
                    if (File.Exists(packageOutput)) File.Delete(packageOutput);
                    ZipFile.CreateFromDirectory(_tempFolderLocation,
                        packageOutput);
                    Directory.Delete(_tempFolderLocation, true);
                    Dispatcher.Invoke(() => MainProgress.Value += 1);
                }

                Dispatcher.Invoke(() => OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\n Compilation complete!\n");
                MessageBox.Show("Compilation complete!", "Done!", MessageBoxButton.OK, MessageBoxImage.Information);
                Dispatcher.Invoke(() => StatusLabel.Content = "Done!");
                
            }

            catch (Exception exceptionOutput)
            {
                Dispatcher.Invoke(() => MainProgress.Foreground = Brushes.DarkRed);
                Dispatcher.Invoke(() => MainProgress.Value = 0);
                Dispatcher.Invoke(() => OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\n" + exceptionOutput + "\n");
                Dispatcher.Invoke(() => StatusLabel.Content = "Failed!");
                MessageBox.Show("Ack! An error occured! See the output in the About tab.", "Failure!",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartMapCompiler(object sender, DoWorkEventArgs e)
        {

            try
            {
                for (int i=0; i < FolderList.Items.Count; i++)
                {
                    var i1 = i;
                    Dispatcher.Invoke(() =>
                        OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "Compiling scripts in the " +
                                          FolderList.Items[i1] + " folder...\n");
                    Thread.Sleep(200);
                    
                    Dispatcher.Invoke(() =>
                        MapStatusLabel.Content =
                            StatusLabel.Content = "Compiling scripts in the " + FolderList.Items[i1] + " folder...");
                    Thread.Sleep(200);
                    CoreCode.FileMap = Directory.GetFiles(FolderList.Items[i1].ToString(), "*.js");
                    Dispatcher.Invoke(() => CoreCode.CompilerWorkerTask(CoreCode.FileMap, FileExtensionTextbox.Text,
                        RemoveCompiledJsCheckbox.IsChecked == true));
                    Array.Clear(CoreCode.FileMap, 0, CoreCode.FileMap.Length);
                    Dispatcher.Invoke(() => MapProgress.Value += 1);
                }
                Dispatcher.Invoke(() => OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\n Compilation complete!\n");
                MessageBox.Show("Compilation complete!", "Done!", MessageBoxButton.OK, MessageBoxImage.Information);
                Dispatcher.Invoke(() => StatusLabel.Content = "Done!");
        }
            catch (Exception exceptionOutput)
            {
                Dispatcher.Invoke(() => MainProgress.Foreground = Brushes.DarkRed);
                Dispatcher.Invoke(() => MainProgress.Value = 0);
                Dispatcher.Invoke(() => OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\n" + exceptionOutput + "\n");
                Dispatcher.Invoke(() => StatusLabel.Content = "Failed!");
                MessageBox.Show("Ack! An error occured! See the output in the About tab.", "Failure!",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

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
                new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog
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
            foreach (string s in FolderList.SelectedItems.OfType<string>().ToList())
                FolderList.Items.Remove(s);
        }

        private void MapCompileButton_Click(object sender, RoutedEventArgs e)
        {
            
            CompileButton.IsEnabled = false;
                MainProgress.Foreground = Brushes.ForestGreen;
                if (!File.Exists(NwjsLocation.Text))
                {
                    MessageBox.Show("The nwjs Compiler is missing.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\nMissing nwjs Compiler executable.\n-----";
                }
                else if (FolderList.Items.Count == 0)
                {
                    MessageBox.Show("Please add the folders you want the JavaScript files to be compiled.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\nCannot operate in a non-existent folder.\n-----";
                }
                else
                {
                    CoreCode.CompilerInfo.FileName = Dispatcher.Invoke(() => NwjsLocation.Text);
                MapProgress.Value = 0;
                    MapProgress.Maximum = FolderList.Items.Count;
                    BackgroundWorker compilerWorker = new BackgroundWorker();
                    compilerWorker.WorkerReportsProgress = true;
                    compilerWorker.DoWork += StartMapCompiler;
                    compilerWorker.ProgressChanged += CompilerReport;
                    compilerWorker.RunWorkerAsync();
                }

                CompileButton.IsEnabled = true;

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

