using System;
using System.IO;
using System.Windows;
using Ookii.Dialogs.Wpf;
using System.Text.Json;

namespace RMMVCookTool.GUI.ProjectMetadataManager
{
    /// <summary>
    ///     Interaction logic for JsonEditor.xaml
    /// </summary>
    public partial class JsonEditor : Window
    {
        private readonly string _projectLocation;
        private ProjectMetadata projectMetadata { get; set; }

        public JsonEditor(string projectIn)
        {
            _projectLocation = projectIn;
            if (File.Exists(Path.Combine(_projectLocation, "package.json")))
            {
                var setup = new JsonSerializerOptions
                {
                    AllowTrailingCommas = true
                };
                var importFile = File.ReadAllText(Path.Combine(_projectLocation, "package.json"));
                projectMetadata = JsonSerializer.Deserialize<ProjectMetadata>(importFile, setup);
            }
            else projectMetadata = new();
            InitializeComponent();
        }

        #region Editor UI Logic
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            #region Prepare UI
            GameIdTextBox.Text = projectMetadata.GameName;
            FileLocationTextBox.Text = projectMetadata.MainFile;
            VersionTextBox.Text = projectMetadata.GameVersion;
            EnableNodeJSCheckBox.IsChecked = projectMetadata.UseNodeJs;
            ChromeFlagsTextBox.Text = projectMetadata.ChromiumFlags;
            JsFlagsTextBox.Text = projectMetadata.JsFlags;
            GameNameTextBox.Text = projectMetadata.GameName;
            IconLocationTextBox.Text = projectMetadata.WindowProperties.WindowIcon;
            WindowTitleTextBox.Text = projectMetadata.WindowProperties.WindowTitle;
            WindowIdTextBox.Text = projectMetadata.WindowProperties.WindowId;
            ResizableWindowCheckBox.IsChecked = projectMetadata.WindowProperties.IsResizable;
            if (projectMetadata.WindowProperties.StartAtFullScreen) WindowModeList.SelectedIndex = 1;
            else if (projectMetadata.WindowProperties.RunInKioskMode) WindowModeList.SelectedIndex = 2;
            else WindowModeList.SelectedIndex = 0;
            WindowLocationList.SelectedIndex = (projectMetadata.WindowProperties.ScreenPosition) switch
            {
                "mouse" => 2,
                "center" => 1,
                _ => 0,
            };
            HeightNumber.Value = projectMetadata.WindowProperties.WindowHeight;
            WidthNumber.Value = projectMetadata.WindowProperties.WindowWidth;
            MinimumHeightNumber.Value = projectMetadata.WindowProperties.MinimumHeight;
            MinimumWidthNumber.Value = projectMetadata.WindowProperties.MinimumWidth;
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
            projectMetadata.GameName = GameIdTextBox.Text;
        }

        private void VersionTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            projectMetadata.GameVersion = VersionTextBox.Text;
        }

        private void FileLocationTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            projectMetadata.MainFile = FileLocationTextBox.Text;
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
            projectMetadata.UseNodeJs = EnableNodeJSCheckBox.IsChecked == true;
        }

        private void ChromeFlagsTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            projectMetadata.ChromiumFlags = ChromeFlagsTextBox.Text;
        }

        private void JsFlagsTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            projectMetadata.JsFlags = JsFlagsTextBox.Text;
        }

        private void GameNameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            projectMetadata.GameTitle = GameNameTextBox.Text;
        }

        private void IconLocationTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            projectMetadata.WindowProperties.WindowIcon = IconLocationTextBox.Text;
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
                IconLocationTextBox.Text = projectMetadata.WindowProperties.WindowIcon = stringBuffer;
            }
            else
            {
                MessageDialog.ThrowErrorMessage(Properties.Resources.ErrorText, Properties.Resources.FileOutsideOfProjectError);
            }
        }

        private void WindowTitleTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            projectMetadata.WindowProperties.WindowTitle = WindowTitleTextBox.Text;
        }

        private void WindowIdTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            projectMetadata.WindowProperties.WindowId = WindowIdTextBox.Text;
        }

        private void ResizableWindowCheckBox_CheckChanged(object sender, RoutedEventArgs e)
        {
            projectMetadata.WindowProperties.IsResizable = ResizableWindowCheckBox.IsChecked == true;
        }

        private void WindowModeList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            switch (WindowModeList.SelectedIndex)
            {
                case 2:
                    projectMetadata.WindowProperties.StartAtFullScreen = false;
                    projectMetadata.WindowProperties.RunInKioskMode = true;
                    break;
                case 1:
                    projectMetadata.WindowProperties.StartAtFullScreen = true;
                    projectMetadata.WindowProperties.RunInKioskMode = false;
                    break;
                default:
                    projectMetadata.WindowProperties.StartAtFullScreen = false;
                    projectMetadata.WindowProperties.RunInKioskMode = false;
                    break;
            }
        }

        private void WindowLocationList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            projectMetadata.WindowProperties.ScreenPosition = WindowLocationList.SelectedIndex switch
            {
                2 => "mouse",
                1 => "center",
                _ => "none",
            };
        }

        private void HeightNumber_ValueChanged(object sender, RoutedPropertyChangedEventArgs<uint> e)
        {
            projectMetadata.WindowProperties.WindowHeight = HeightNumber.Value;
        }

        private void MinimumHeightNumber_ValueChanged(object sender, RoutedPropertyChangedEventArgs<uint> e)
        {
            projectMetadata.WindowProperties.MinimumHeight = MinimumHeightNumber.Value;
        }

        private void WidthNumber_ValueChanged(object sender, RoutedPropertyChangedEventArgs<uint> e)
        {
            projectMetadata.WindowProperties.WindowWidth = WidthNumber.Value;
        }

        private void MinimumWidthNumber_ValueChanged(object sender, RoutedPropertyChangedEventArgs<uint> e)
        {
            projectMetadata.WindowProperties.MinimumWidth = MinimumWidthNumber.Value;
        }
        #endregion

        #region Save and Exit
        private void SaveJsonButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var setup = new JsonSerializerOptions
                {
                    WriteIndented = MinifyJsonFileCheckBox.IsChecked == false
                };
                string output = JsonSerializer.Serialize(projectMetadata, setup);
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