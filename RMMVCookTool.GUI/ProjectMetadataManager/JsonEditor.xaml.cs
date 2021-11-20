using RMMVCookTool.Core.ProjectTemplate;
using System.Text.Json;

namespace RMMVCookTool.GUI.ProjectMetadataManager
{
    /// <summary>
    ///     Interaction logic for JsonEditor.xaml
    /// </summary>
    public partial class JsonEditor : Window
    {
        private readonly string _projectLocation;
        internal ProjectMetadata ProjectMetadata { get; set; }

        public JsonEditor(string projectIn)
        {
            _projectLocation = projectIn;
            if (File.Exists(Path.Combine(_projectLocation, "package.json")))
            {
                var importFile = File.ReadAllText(Path.Combine(_projectLocation, "package.json"));
                ProjectMetadata = JsonSerializer.Deserialize(importFile, ProjectMetadataSerializer.Default.ProjectMetadata);
            }
            else ProjectMetadata = new();
            InitializeComponent();
        }

        #region Editor UI Logic
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            #region Prepare UI
            GameIdTextBox.Text = ProjectMetadata.GameName;
            FileLocationTextBox.Text = ProjectMetadata.MainFile;
            VersionTextBox.Text = ProjectMetadata.GameVersion;
            EnableNodeJSCheckBox.IsChecked = ProjectMetadata.UseNodeJs;
            ChromeFlagsTextBox.Text = ProjectMetadata.ChromiumFlags;
            JsFlagsTextBox.Text = ProjectMetadata.JsFlags;
            GameNameTextBox.Text = ProjectMetadata.GameName;
            IconLocationTextBox.Text = ProjectMetadata.WindowProperties.WindowIcon;
            WindowTitleTextBox.Text = ProjectMetadata.WindowProperties.WindowTitle;
            WindowIdTextBox.Text = ProjectMetadata.WindowProperties.WindowId;
            ResizableWindowCheckBox.IsChecked = ProjectMetadata.WindowProperties.IsResizable;
            if (ProjectMetadata.WindowProperties.StartAtFullScreen) WindowModeList.SelectedIndex = 1;
            else if (ProjectMetadata.WindowProperties.RunInKioskMode) WindowModeList.SelectedIndex = 2;
            else WindowModeList.SelectedIndex = 0;
            WindowLocationList.SelectedIndex = (ProjectMetadata.WindowProperties.ScreenPosition) switch
            {
                "mouse" => 2,
                "center" => 1,
                _ => 0,
            };
            HeightNumber.Value = ProjectMetadata.WindowProperties.WindowHeight;
            WidthNumber.Value = ProjectMetadata.WindowProperties.WindowWidth;
            MinimumHeightNumber.Value = ProjectMetadata.WindowProperties.MinimumHeight;
            MinimumWidthNumber.Value = ProjectMetadata.WindowProperties.MinimumWidth;
            #endregion
            #region Event Registration
            GameIdTextBox.TextChanged += GameIdTextBox_TextChanged;
            FileLocationTextBox.TextChanged += FileLocationTextBox_TextChanged;
            VersionTextBox.TextChanged += VersionTextBox_TextChanged;
            ChromeFlagsTextBox.TextChanged += ChromeFlagsTextBox_TextChanged;
            EnableNodeJSCheckBox.Checked += EnableNodeJSCheckBox_CheckChanged;
            EnableNodeJSCheckBox.Unchecked += EnableNodeJSCheckBox_CheckChanged;
            GameNameTextBox.TextChanged += GameNameTextBox_TextChanged;
            IconLocationTextBox.TextChanged += IconLocationTextBox_TextChanged;
            WindowTitleTextBox.TextChanged += WindowTitleTextBox_TextChanged;
            WindowIdTextBox.TextChanged += WindowIdTextBox_TextChanged;
            ResizableWindowCheckBox.Checked += ResizableWindowCheckBox_CheckChanged;
            ResizableWindowCheckBox.Unchecked += ResizableWindowCheckBox_CheckChanged;
            WindowModeList.SelectionChanged += WindowModeList_SelectionChanged;
            WindowLocationList.SelectionChanged += WindowLocationList_SelectionChanged;
            HeightNumber.ValueChanged += HeightNumber_ValueChanged;
            WidthNumber.ValueChanged += WidthNumber_ValueChanged;
            MinimumHeightNumber.ValueChanged += MinimumHeightNumber_ValueChanged;
            MinimumWidthNumber.ValueChanged += MinimumWidthNumber_ValueChanged;
            #endregion

        }
        #region Main Logic
        private void GameIdTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ProjectMetadata.GameName = GameIdTextBox.Text;
        }

        private void VersionTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ProjectMetadata.GameVersion = VersionTextBox.Text;
        }

        private void FileLocationTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ProjectMetadata.MainFile = FileLocationTextBox.Text;
        }

        private void BrowseForHtmlFileButton_Click(object sender, RoutedEventArgs e)
        {
            var htmlFilePicker =
                new VistaOpenFileDialog
                {
                    Title = Properties.Resources.ProjectPickerText,
                    Filter = Properties.Resources.HTMLFileText,
                    InitialDirectory = _projectLocation,
                    Multiselect = false
                };
            var pickerResult = htmlFilePicker.ShowDialog();
            if (pickerResult != true) return;
            if (htmlFilePicker.FileName.Contains(_projectLocation))
            {
                var stringBuffer = htmlFilePicker.FileName.Replace(_projectLocation + "\\", "");
                stringBuffer = stringBuffer.Replace("\\", "/");
                FileLocationTextBox.Text = stringBuffer;

            }
            else
            {
                MessageDialog.ThrowErrorMessage(Properties.Resources.ErrorText, Properties.Resources.FileOutsideOfProjectError);
            }
        }

        private void EnableNodeJSCheckBox_CheckChanged(object sender, RoutedEventArgs e)
        {
            ProjectMetadata.UseNodeJs = EnableNodeJSCheckBox.IsChecked == true;
        }

        private void ChromeFlagsTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ProjectMetadata.ChromiumFlags = ChromeFlagsTextBox.Text;
        }

        private void JsFlagsTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ProjectMetadata.JsFlags = JsFlagsTextBox.Text;
        }

        private void GameNameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ProjectMetadata.GameTitle = GameNameTextBox.Text;
        }

        private void IconLocationTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ProjectMetadata.WindowProperties.WindowIcon = IconLocationTextBox.Text;
        }

        private void BrowseForIconButton_Click(object sender, RoutedEventArgs e)
        {
            var iconFilePicker =
                new VistaOpenFileDialog
                {
                    Title = Properties.Resources.ProjectPickerText,
                    Filter = Properties.Resources.PNGFileText,
                    InitialDirectory = _projectLocation,
                    Multiselect = false
                };
            var pickerResult = iconFilePicker.ShowDialog();
            if (pickerResult != true) return;
            if (iconFilePicker.FileName.Contains(_projectLocation))
            {
                var stringBuffer = iconFilePicker.FileName.Replace(_projectLocation + "\\", "");
                stringBuffer = stringBuffer.Replace("\\", "/");
                IconLocationTextBox.Text = ProjectMetadata.WindowProperties.WindowIcon = stringBuffer;
            }
            else
            {
                MessageDialog.ThrowErrorMessage(Properties.Resources.ErrorText, Properties.Resources.FileOutsideOfProjectError);
            }
        }

        private void WindowTitleTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ProjectMetadata.WindowProperties.WindowTitle = WindowTitleTextBox.Text;
        }

        private void WindowIdTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ProjectMetadata.WindowProperties.WindowId = WindowIdTextBox.Text;
        }

        private void ResizableWindowCheckBox_CheckChanged(object sender, RoutedEventArgs e)
        {
            ProjectMetadata.WindowProperties.IsResizable = ResizableWindowCheckBox.IsChecked == true;
        }

        private void WindowModeList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            switch (WindowModeList.SelectedIndex)
            {
                case 2:
                    ProjectMetadata.WindowProperties.StartAtFullScreen = false;
                    ProjectMetadata.WindowProperties.RunInKioskMode = true;
                    break;
                case 1:
                    ProjectMetadata.WindowProperties.StartAtFullScreen = true;
                    ProjectMetadata.WindowProperties.RunInKioskMode = false;
                    break;
                default:
                    ProjectMetadata.WindowProperties.StartAtFullScreen = false;
                    ProjectMetadata.WindowProperties.RunInKioskMode = false;
                    break;
            }
        }

        private void WindowLocationList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ProjectMetadata.WindowProperties.ScreenPosition = WindowLocationList.SelectedIndex switch
            {
                2 => "mouse",
                1 => "center",
                _ => "none",
            };
        }

        private void HeightNumber_ValueChanged(object sender, RoutedPropertyChangedEventArgs<uint> e)
        {
            ProjectMetadata.WindowProperties.WindowHeight = HeightNumber.Value;
        }

        private void MinimumHeightNumber_ValueChanged(object sender, RoutedPropertyChangedEventArgs<uint> e)
        {
            ProjectMetadata.WindowProperties.MinimumHeight = MinimumHeightNumber.Value;
        }

        private void WidthNumber_ValueChanged(object sender, RoutedPropertyChangedEventArgs<uint> e)
        {
            ProjectMetadata.WindowProperties.WindowWidth = WidthNumber.Value;
        }

        private void MinimumWidthNumber_ValueChanged(object sender, RoutedPropertyChangedEventArgs<uint> e)
        {
            ProjectMetadata.WindowProperties.MinimumWidth = MinimumWidthNumber.Value;
        }
        #endregion

        #region Save and Exit
        private void SaveJsonButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string output = JsonSerializer.Serialize(ProjectMetadata, ProjectMetadataSerializer.Default.ProjectMetadata);
                File.WriteAllText(Path.Combine(_projectLocation, "package.json"), output);
                MessageDialog.ThrowCompleteMessage(Properties.Resources.SaveCompleteText);
            }
            catch (FileFormatException ex)
            {
                MessageDialog.ThrowErrorMessage(ex);
            }
            catch (PathTooLongException ex)
            {
                MessageDialog.ThrowErrorMessage(ex);
            }

            catch (IOException ex)
            {
                MessageDialog.ThrowErrorMessage(ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageDialog.ThrowErrorMessage(ex);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion
        #endregion
    }
}