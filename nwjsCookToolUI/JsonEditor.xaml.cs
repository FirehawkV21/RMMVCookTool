﻿using System;
using System.IO;
using System.Windows;
using CompilerCore;
using Newtonsoft.Json.Linq;
using Ookii.Dialogs.Wpf;

namespace nwjsCookToolUI
{
    /// <summary>
    ///     Interaction logic for JsonEditor.xaml
    /// </summary>
    public partial class JsonEditor : Window
    {
        private readonly string _projectLocation;

        public JsonEditor(string projectIn)
        {
            InitializeComponent();
            _projectLocation = projectIn;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists(Path.Combine(_projectLocation, "package.json")))
            {
                JsonProcessor.ReadJson(Path.Combine(_projectLocation, "package.json"));
                var projectMetadata = JObject.Parse(JsonProcessor.JsonString);
                GameIdTextBox.Text = (string) projectMetadata["name"];
                FileLocationTextBox.Text = (string) projectMetadata["main"];
                VersionTextBox.Text = (string) projectMetadata["version"];
                EnableNodeJSCheckBox.IsChecked =
                    (bool?) projectMetadata["nodejs"] == null || (bool) projectMetadata["nodejs"];
                ChromeFlagsTextBox.Text = (string) projectMetadata["chromium-flags"];
                JsFlagsTextBox.Text = (string) projectMetadata["js-flags"];
                GameNameTextBox.Text = (string) projectMetadata["app_name"];
                IconLocationTextBox.Text = (string) projectMetadata["window"]["icon"];
                WindowTitleTextBox.Text = (string) projectMetadata["window"]["title"];
                WindowIdTextBox.Text = (string) projectMetadata["window"]["id"];
                ResizableWindowCheckBox.IsChecked = (bool?) projectMetadata["window"]["resizeable"] == null ||
                                                    (bool) projectMetadata["window"]["resizeable"];
                if ((bool?) projectMetadata["window"]["fullscreen"] == true) WindowModeList.SelectedIndex = 1;
                else if ((bool?) projectMetadata["window"]["kiosk"] == true) WindowModeList.SelectedIndex = 2;
                else WindowModeList.SelectedIndex = 0;
                switch ((string) projectMetadata["window"]["position"])
                {
                    case "mouse":
                        WindowLocationList.SelectedIndex = 2;
                        break;
                    case "center":
                        WindowLocationList.SelectedIndex = 1;
                        break;
                    default:
                        WindowLocationList.SelectedIndex = 0;
                        break;
                }

                HeightNumber.Value = (int) projectMetadata["window"]["height"];
                WidthNumber.Value = (int) projectMetadata["window"]["width"];
                MinimumHeightNumber.Value = (int) projectMetadata["window"]["min_height"];
                MinimumWidthNumber.Value = (int) projectMetadata["window"]["min_width"];
            }
            else
            {
                FileLocationTextBox.Text = "index.html";
                GameIdTextBox.Text = "NewGame";
                GameNameTextBox.Text = "My New Game";
                JsFlagsTextBox.Text = "--expose-gc";
                ChromeFlagsTextBox.Text =
                    "--enable-gpu-rasterization --enable-gpu-memory-buffer-video-frames --enable-native-gpu-memory-buffers --enable-zero-copy --enable-gpu-async-worker-context";
                EnableNodeJSCheckBox.IsChecked = true;
                ResizableWindowCheckBox.IsChecked = true;
                HeightNumber.Value = 816;
                WidthNumber.Value = 624;
            }
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
                MessageBox.Show(
                    Properties.Resources.FileOutsideOfProjectError,
                    Properties.Resources.ErrorText, MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                FileLocationTextBox.Text = stringBuffer;
            }
            else
            {
                MessageBox.Show(
                    Properties.Resources.FileOutsideOfProjectError,
                    Properties.Resources.ErrorText, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveJsonButton_Click(object sender, RoutedEventArgs e)
        {
            bool isFullscreen, isKioskMode;
            string windowPosition;
            switch (WindowModeList.SelectedIndex)
            {
                case 2:
                    isFullscreen = false;
                    isKioskMode = true;
                    break;
                case 1:
                    isFullscreen = true;
                    isKioskMode = false;
                    break;
                default:
                    isFullscreen = false;
                    isKioskMode = false;
                    break;
            }

            switch (WindowLocationList.SelectedIndex)
            {
                case 2:
                    windowPosition = "mouse";
                    break;
                case 1:
                    windowPosition = "center";
                    break;
                default:
                    windowPosition = "none";
                    break;
            }

            try
            {
                JsonProcessor.BuildJson(
                    GameNameTextBox.Text,
                    GameIdTextBox.Text,
                    VersionTextBox.Text,
                    FileLocationTextBox.Text,
                    EnableNodeJSCheckBox.IsChecked == true,
                    ChromeFlagsTextBox.Text,
                    JsFlagsTextBox.Text,
                    WindowIdTextBox.Text,
                    IconLocationTextBox.Text,                    
                    WindowTitleTextBox.Text,
                    (int) WidthNumber.Value,                    
                    (int) HeightNumber.Value,
                    (int) MinimumWidthNumber.Value,
                    (int) MinimumHeightNumber.Value,
                    ResizableWindowCheckBox.IsChecked == true,
                    isFullscreen,
                    isKioskMode,
                    windowPosition,
                    _projectLocation);
                MessageBox.Show(nwjsCookToolUI.Properties.Resources.SaveCompleteText, Properties.Resources.DoneText,
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (FileFormatException)
            {
                MessageBox.Show(nwjsCookToolUI.Properties.Resources.FileStreamErrorText, Properties.Resources.ErrorText, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (PathTooLongException)
            {
                MessageBox.Show(nwjsCookToolUI.Properties.Resources.ProjectPathTooLongErrorText,
                    Properties.Resources.ErrorText, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            catch (IOException)
            {
                MessageBox.Show(nwjsCookToolUI.Properties.Resources.IOErrorText,
                    Properties.Resources.ErrorText, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show(
                    nwjsCookToolUI.Properties.Resources.FileAccessError,
                    Properties.Resources.ErrorText, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}