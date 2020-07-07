using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;

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
    }
}
