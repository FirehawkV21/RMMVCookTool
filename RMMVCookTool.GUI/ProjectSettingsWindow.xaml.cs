using System;
using System.Windows;

namespace RMMVCookTool.GUI
{
    /// <summary>
    /// Interaction logic for ProjectSettingsWindow.xaml
    /// </summary>
    public partial class ProjectSettingsWindow : Window
    {
        private readonly int pickedProject;
        private readonly bool isEditingProjectSettings;
        public ProjectSettingsWindow()
        {
            InitializeComponent();
        }

        public ProjectSettingsWindow(int index)
        {
            pickedProject = index;
            isEditingProjectSettings = true;
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            if (!isEditingProjectSettings)
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
                RemoveSourceFilesAfterCompilingCheckbox.IsChecked = MainWindow.ProjectList[pickedProject].RemoveSourceCodeAfterCompiling;
                CompressFilesToPackageCheckbox.IsChecked = MainWindow.ProjectList[pickedProject].CompressFilesToPackage;
                RemoveFilesAfterPackagingCheckbox.IsChecked = MainWindow.ProjectList[pickedProject].RemoveFilesAfterCompression;
                CompressionLevelBox.SelectedIndex = MainWindow.ProjectList[pickedProject].CompressionModeLevel;
                FileExtensionTextBox.Text = MainWindow.ProjectList[pickedProject].FileExtension;
            }
        }

        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isEditingProjectSettings)
            {
                AppSettings.Default.DeleteSourceCode = RemoveSourceFilesAfterCompilingCheckbox.IsChecked == true;
                AppSettings.Default.PackageCode = CompressFilesToPackageCheckbox.IsChecked == true;
                AppSettings.Default.RemoveFilesAfterPackaging = RemoveFilesAfterPackagingCheckbox.IsChecked == true;
                AppSettings.Default.CompressionMode = CompressionLevelBox.SelectedIndex;
                AppSettings.Default.FileExtension = FileExtensionTextBox.Text;
                AppSettings.Default.Save();
            }
            else
            {
                MainWindow.ProjectList[pickedProject].RemoveSourceCodeAfterCompiling = RemoveSourceFilesAfterCompilingCheckbox.IsChecked == true;
                MainWindow.ProjectList[pickedProject].CompressFilesToPackage = CompressFilesToPackageCheckbox.IsChecked == true;
                MainWindow.ProjectList[pickedProject].RemoveFilesAfterCompression = RemoveFilesAfterPackagingCheckbox.IsChecked == true;
                MainWindow.ProjectList[pickedProject].CompressionModeLevel = CompressionLevelBox.SelectedIndex;
                MainWindow.ProjectList[pickedProject].FileExtension = FileExtensionTextBox.Text;
            }
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
