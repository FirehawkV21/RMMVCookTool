using System;
using System.Windows;

namespace RMMVCookTool.GUI
{
    /// <summary>
    /// Interaction logic for ProjectSettingsWindow.xaml
    /// </summary>
    public partial class ProjectSettingsWindow : Window
    {
        private readonly int _pickedProject;
        private readonly bool _isEditingProjectSettings;
        public ProjectSettingsWindow()
        {
            InitializeComponent();
        }

        public ProjectSettingsWindow(int index)
        {
            _pickedProject = index;
            _isEditingProjectSettings = true;
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            if (!_isEditingProjectSettings)
            {
                Title = "Default Project Settings";
                RemoveSourceFilesAfterCompilingCheckbox.IsChecked = AppSettings.Default.DeleteSourceCode;
                CompressFilesToPackageCheckbox.IsChecked = AppSettings.Default.PackageCode;
                RemoveFilesAfterPackagingCheckbox.IsChecked = AppSettings.Default.RemoveFilesAfterPackaging;
                CompressionLevelBox.SelectedIndex = AppSettings.Default.CompressionMode;
                FileExtensionTextBox.Text = AppSettings.Default.FileExtension;
            }
            else
            {
                RemoveSourceFilesAfterCompilingCheckbox.IsChecked = MainWindow.ProjectList[_pickedProject].RemoveSourceCodeAfterCompiling;
                CompressFilesToPackageCheckbox.IsChecked = MainWindow.ProjectList[_pickedProject].CompressFilesToPackage;
                RemoveFilesAfterPackagingCheckbox.IsChecked = MainWindow.ProjectList[_pickedProject].RemoveFilesAfterCompression;
                CompressionLevelBox.SelectedIndex = MainWindow.ProjectList[_pickedProject].CompressionModeLevel;
                FileExtensionTextBox.Text = MainWindow.ProjectList[_pickedProject].FileExtension;
            }
        }

        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isEditingProjectSettings)
            {
                AppSettings.Default.DeleteSourceCode = RemoveSourceFilesAfterCompilingCheckbox.IsChecked == true;
                AppSettings.Default.PackageCode = CompressFilesToPackageCheckbox.IsChecked == true;
                AppSettings.Default.RemoveFilesAfterPackaging = RemoveFilesAfterPackagingCheckbox.IsChecked == true;
                AppSettings.Default.CompressionMode = CompressionLevelBox.SelectedIndex;
                AppSettings.Default.FileExtension = FileExtensionTextBox.Text;
                MessageDialog.ThrowCompleteMessage(Properties.Resources.DefaultSettingsUpdatedText, Properties.Resources.DefaultSettingsUpdatedMessage);
                AppSettings.Default.Save();
            }
            else
            {
                MainWindow.ProjectList[_pickedProject].RemoveSourceCodeAfterCompiling = RemoveSourceFilesAfterCompilingCheckbox.IsChecked == true;
                MainWindow.ProjectList[_pickedProject].CompressFilesToPackage = CompressFilesToPackageCheckbox.IsChecked == true;
                MainWindow.ProjectList[_pickedProject].RemoveFilesAfterCompression = RemoveFilesAfterPackagingCheckbox.IsChecked == true;
                MainWindow.ProjectList[_pickedProject].CompressionModeLevel = CompressionLevelBox.SelectedIndex;
                MainWindow.ProjectList[_pickedProject].FileExtension = FileExtensionTextBox.Text;
                MessageDialog.ThrowCompleteMessage(Properties.Resources.SProjectSettingsUpdatedText);
            }
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
