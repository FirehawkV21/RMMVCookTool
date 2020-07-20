using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RMMVCookTool.GUI
{
    /// <summary>
    /// Interaction logic for ProjectSettingsWindow.xaml
    /// </summary>
    public partial class ProjectSettingsWindow : Window
    {
        private int pickedProject;
        private bool isEditingProjectSettings;
        public ProjectSettingsWindow()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            if (!isEditingProjectSettings)
            {
                RemoveSourceFilesAfterCompilingCheckbox.IsChecked = AppSettings.Default.DeleteSourceCode;
                CompressFilesToPackageCheckbox.IsChecked = AppSettings.Default.PackageCode;
                RemoveFilesAfterPackagingCheckbox.IsChecked = AppSettings.Default.RemoveFilesAfterPackaging;
                CompressionLevelBox.SelectedIndex = AppSettings.Default.CompressionMode;
                FileExtensionTextBox.Text = AppSettings.Default.FileExtension;
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
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
