using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ookii.Dialogs.Wpf;
using RMMVCookTool.Core;

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
        }

        private string _previousPath;
        public static List<CompilerProject> ProjectList { get; } = new List<CompilerProject>();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
             var assembly = Assembly.GetExecutingAssembly();
             var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
             var version = fvi.FileVersion;
             ProgramVersionLabel.Content = ProgramVersionLabel.Content + @" (" + version + @")";

            byte[] loader = Encoding.ASCII.GetBytes(Properties.Resources.Manual);
            using (MemoryStream stream = new MemoryStream(loader))
            {
                UserManualBox.Selection.Load(stream, DataFormats.Rtf);
            }
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
            if (pickJsFolder.SelectedPath != null) FolderList.Items.Add(pickJsFolder.SelectedPath);
            ProjectList.Add(new CompilerProject(pickJsFolder.SelectedPath, AppSettings.Default.FileExtension, AppSettings.Default.DeleteSourceCode, AppSettings.Default.PackageCode, AppSettings.Default.RemoveFilesAfterPackaging, AppSettings.Default.CompressionMode));
        }

        private void RemoveProjectButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var s in FolderList.SelectedItems.OfType<string>().ToList())
            {
                var temp = FolderList.Items.IndexOf(s);
                FolderList.Items.Remove(s);
                ProjectList.RemoveAt(temp);
            }

        }

        private void ProjectSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var temp = FolderList.SelectedIndex;
            ProjectSettingsWindow SettingsWindow = new ProjectSettingsWindow(temp);
            SettingsWindow.Show();
        }

        private void EditMetadataButton_Click(object sender, RoutedEventArgs e)
        {
            if (!FolderList.SelectedItems.OfType<string>().Any())
                MessageBox.Show("Please select a project.", Properties.Resources.ErrorText,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                var jsonEditorGui = new JsonEditor(FolderList.SelectedItem.ToString());
                jsonEditorGui.Show();
            }
        }
    }
}
