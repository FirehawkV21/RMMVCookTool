namespace RMMVCookTool.GUI;

/// <summary>
/// Interaction logic for ProjectSettingsWindow.xaml
/// </summary>
public partial class ProjectSettingsWindow : Window
{
    private readonly int _pickedProject;
    private readonly bool _isEditingProjectSettings;
    public ProjectSettingsWindow() => InitializeComponent();

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
            Title = Properties.Resources.DefaultProjectSettingsUiText;
            RemoveSourceFilesAfterCompilingCheckbox.IsChecked = AppSettings.Default.DeleteSourceCode;
            CompressFilesToPackageCheckbox.IsChecked = AppSettings.Default.PackageCode;
            RemoveFilesAfterPackagingCheckbox.IsChecked = AppSettings.Default.RemoveFilesAfterPackaging;
            CompressionLevelBox.SelectedIndex = AppSettings.Default.CompressionMode;
            FileExtensionTextBox.Text = AppSettings.Default.FileExtension;
        }
        else
        {

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

        }
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => Close();

    private void CompressFilesToPackageCheckbox_Unchecked(object sender, RoutedEventArgs e)
    {
        RemoveFilesAfterPackagingCheckbox.IsChecked = false;
        CompressionLevelBox.SelectedIndex = 0;
    }

}
