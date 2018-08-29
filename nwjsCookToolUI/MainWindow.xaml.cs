using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Windows;
using nwjsCookToolUI.Properties;

namespace nwjsCookToolUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private static readonly Process CompilerProcess = new Process();
        private static readonly ProcessStartInfo CompilerInfo = new ProcessStartInfo();
        private string[] _filemap;
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
            if (!File.Exists(NwjsLocation.Text))
            {
                MessageBox.Show("The nwjs Compiler is missing.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\nMissing nwjs Compiler executable.\n-----";
            }
            else if (!Directory.Exists(ProjectLocation.Text))
            {
                MessageBox.Show("Can't operate on an non-existant folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\nCannot operate in a non-existant folder.\n-----";
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

            //cookToolUi.Visibility = Visibility.Visible;
        }

        private void Compileset(IEnumerable<string> filemap, string extension, bool removejs)
        {
            foreach (var file in filemap)
            {
                string filebuffer = file.Replace(".js", "");
                OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\nCompiling " + file + "...";
                Thread.Sleep(200);
                CompilerInfo.Arguments = "\"" + file + "\"" + " " + "\"" + filebuffer + "." + extension + "\"";
                CompilerInfo.CreateNoWindow = true;
                CompilerInfo.WindowStyle = ProcessWindowStyle.Hidden;
                CompilerProcess.StartInfo = CompilerInfo;
                CompilerProcess.Start();
                CompilerProcess.WaitForExit();
                if (removejs) File.Delete(file);
                OutputArea.Text = OutputArea.Text + "\n Compiled on " + DateTime.Now + ".\n";
                Thread.Sleep(200);

            }
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, true);
                }
            }
        }

        private void StartCompiler(object sender, DoWorkEventArgs e)
        {
            Dispatcher.Invoke(() => StatusLabel.Content = "Compiling scripts in the js folder...");
            Thread.Sleep(400);
            try
            {
                string compilerInput = Dispatcher.Invoke(() => ProjectLocation.Text);
                _filemap = Directory.GetFiles(compilerInput + "\\www\\js\\", "*.js");
                string[] foldermap = { "libs", "plugins" };
                CompilerInfo.FileName = Dispatcher.Invoke(() => NwjsLocation.Text);
                Dispatcher.Invoke(() => OutputArea.Text = OutputArea.Text = "\n" + OutputArea.Text + DateTime.Now + "Compiling scripts in the js folder...\n-----");
                Thread.Sleep(200);


                Dispatcher.Invoke(() => Compileset(_filemap, FileExtensionTextbox.Text, RemoveCompiledJsCheckbox.IsChecked == true));
                Array.Clear(_filemap, 0, _filemap.Length);
                Dispatcher.Invoke(() => MainProgress.Value += 1);
                foreach (var foldername in foldermap)
                {
                    Dispatcher.Invoke(() => OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "Compiling scripts in the " + foldername + " folder...\n");
                    Thread.Sleep(200);
                    Dispatcher.Invoke(() => StatusLabel.Content = StatusLabel.Content = "Compiling scripts in the " + foldername + " folder...");
                    Thread.Sleep(200);
                    _filemap = Directory.GetFiles(compilerInput + "\\www\\js\\" + foldername + "\\", "*.js");
                    Dispatcher.Invoke(() => Compileset(_filemap, FileExtensionTextbox.Text, RemoveCompiledJsCheckbox.IsChecked == true));
                    Array.Clear(_filemap, 0, _filemap.Length);
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
                    Dispatcher.Invoke(() => DirectoryCopy(compilerInput + "\\www\\",
                        Path.Combine(Path.GetTempPath(), "nwjspackage") + "\\www\\", true));
                    File.Copy(compilerInput + "\\package.json",
                        Path.Combine(Path.GetTempPath(), "nwjspackage") + "\\package.json", true);
                    Dispatcher.Invoke(() => OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\n Creating package...\n");
                    ZipFile.CreateFromDirectory(Path.Combine(Path.GetTempPath(), "nwjspackage"),
                        compilerInput + "\\package.nw");
                    Directory.Delete(Path.Combine(Path.GetTempPath(), "nwjspackage"), true);
                    Dispatcher.Invoke(() => MainProgress.Value += 1);
                }

                Dispatcher.Invoke(() => OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\n Compilation complete!\n");
                MessageBox.Show("Compilation complete!", "Done!", MessageBoxButton.OK, MessageBoxImage.Information);
                Dispatcher.Invoke(() => StatusLabel.Content = "Done!");
                
            }

            catch (Exception exceptionOutput)
            {
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
    }
    }

