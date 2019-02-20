using System.IO;
using System.Windows;
using CompilerCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace nwjsCookToolUI
{
    /// <summary>
    /// Interaction logic for JsonEditor.xaml
    /// </summary>
    public partial class JsonEditor : Window
    {
        private string projectLocation;
        public JsonEditor(string projectIn)
        {
            InitializeComponent();
            projectLocation = projectIn;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists(Path.Combine(projectLocation, "package.json")))
            {
                JsonProcessor.ReadJson(Path.Combine(projectLocation, "package.json"));
                JObject projectMetadata = JObject.Parse(JsonProcessor.JsonString);
                GameIdTextBox.Text = (string)projectMetadata["name"];
                FileLocationTextBox.Text = (string)projectMetadata["main"];
                VersionTextBox.Text = (string)projectMetadata["version"];
                EnableNodeJSCheckBox.IsChecked = ((bool?)projectMetadata["nodejs"] == null) || (bool)projectMetadata["nodejs"];
                ChromeFlagsTextBox.Text = (string)projectMetadata["chromium-args"];
                JsFlagsTextBox.Text = (string)projectMetadata["js-flags"];
                GameNameTextBox.Text = (string)projectMetadata["app_name"];
                IconLocationTextBox.Text = (string)projectMetadata["window"]["icon"];
                WindowTitleTextBox.Text = (string)projectMetadata["window"]["title"];
                ResizableWindowCheckBox.IsChecked = ((bool?)projectMetadata["window"]["resizeable"] == null) || (bool)projectMetadata["window"]["resizeable"];
                if ((bool?)projectMetadata["window"]["fullscreen"] == true) WindowModeList.SelectedIndex = 1;
                else if ((bool?)projectMetadata["window"]["kiosk"] == true) WindowModeList.SelectedIndex = 2;
                else WindowModeList.SelectedIndex = 0;
                switch ((string)projectMetadata["window"]["position"])
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
                HeightNumber.Value = (int)projectMetadata["window"]["height"];
                WidthNumber.Value = (int)projectMetadata["window"]["width"];
                MinimumHeightNumber.Value = (int)projectMetadata["window"]["min_height"];
                MinimumWidthNumber.Value = (int)projectMetadata["window"]["min_width"];
            }
            else
            {

            }
        }
    }
}
