using System;
using System.Collections.Generic;
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
            else if (Directory.Exists(ProjectLocation.Text))
            {
                MessageBox.Show("Can't operate on an non-existant folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\nCannot operate in a non-existant folder.\n-----";
            }
            else
            {
                MessageBox.Show(
                    "The compiler tool will be hidden until the compiler finishes.\n You will be notified when it's done.");
                //cookToolUi.Visibility = Visibility.Collapsed;
                cookToolUi.Hide();
                Thread.Sleep(1000);
                try
                {
                    string[] foldermap = {"libs", "plugins"};
                    string[] filemap = Directory.GetFiles(ProjectLocation.Text + "\\www\\js\\", "*.js");
                    CompilerInfo.FileName = NwjsLocation.Text;
                    OutputArea.Text = "\n" + OutputArea.Text + DateTime.Now + "Compiling scripts in the js folder...\n-----";
                    StatusLabel.Content = "Compiling scripts in the js folder...";

                    Compileset(filemap, FileExtensionTextbox.Text, RemoveCompiledJsCheckbox.IsChecked == true);
                    Array.Clear(filemap, 0, filemap.Length);
                    foreach (var foldername in foldermap)
                    {
                        OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "Compiling scripts in the" +
                                          foldername +
                                          " folder...\n";
                        StatusLabel.Content = "Compiling scripts in the " + foldername + " folder...";
                        filemap = Directory.GetFiles(ProjectLocation.Text + "\\www\\js\\" + foldername + "\\", "*.js");
                        Compileset(filemap, FileExtensionTextbox.Text, RemoveCompiledJsCheckbox.IsChecked == true);
                        Array.Clear(filemap, 0, filemap.Length);

                    }

                    if (PackageNwCheckbox.IsChecked == true)
                    {
                        OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now +
                                          "\n Copying files to a temporary area...\n";
                        StatusLabel.Content = "Packaging...";
                        //Directory.CreateDirectory(ProjectLocation.Text + "\\Package\\");
                        DirectoryCopy(ProjectLocation.Text + "\\www\\",
                            Path.Combine(Path.GetTempPath(), "nwjspackage") + "\\www\\", true);
                        File.Copy(ProjectLocation.Text + "\\package.json",
                            Path.Combine(Path.GetTempPath(), "nwjspackage") + "\\package.json", true);
                        OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\n Creating package...\n";
                        ZipFile.CreateFromDirectory(Path.Combine(Path.GetTempPath(), "nwjspackage"),
                            ProjectLocation.Text + "\\package.nw");
                        Directory.Delete(Path.Combine(Path.GetTempPath(), "nwjspackage"), true);

                    }

                    OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\n Compilation complete!\n";
                    MessageBox.Show("Compilation complete!", "Done!", MessageBoxButton.OK, MessageBoxImage.Information);
                    StatusLabel.Content = "Done!";
                }

                catch (Exception exceptionOutput)
                {
                    OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\n" + exceptionOutput + "\n";
                    MessageBox.Show("Ack! An error occured! See the output in the About tab.", "Failure!",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }

                cookToolUi.Show();
            }

            //cookToolUi.Visibility = Visibility.Visible;
        }

        private void Compileset(IEnumerable<string> filemap, string extension, bool removejs)
        {
            foreach (var file in filemap)
            {
                string filebuffer = file.Replace(".js", "");
                OutputArea.Text = OutputArea.Text + "\n" + DateTime.Now + "\nCompiling " + file + "...";
                CompilerInfo.Arguments = "\"" + file + "\"" + " " + "\"" + filebuffer + "." + extension + "\"";
                CompilerInfo.CreateNoWindow = true;
                CompilerInfo.WindowStyle = ProcessWindowStyle.Hidden;
                CompilerProcess.StartInfo = CompilerInfo;
                CompilerProcess.Start();
                CompilerProcess.WaitForExit();
                if (removejs) File.Delete(file);
                OutputArea.Text = OutputArea.Text + "\n Compiled on " + DateTime.Now + ".\n";

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
    }
    }

