using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace VENIK
{
    public partial class Log_Viewer : Window
    {
        string defaultLogDir;

        public Log_Viewer()
        {
            InitializeComponent();
            LogsScrollViewerXAML.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;

            defaultLogDir = new MainWindow().defaultLogDir;

            UpdateList();
        }

        private void UpdateList()
        {
            LogsListBoxXAML.Items.Clear();
            if (Directory.GetFiles(defaultLogDir).Length != 0)
            {
                foreach (var logInfo in Directory.GetFiles(defaultLogDir))
                {
                    if (logInfo.EndsWith(".log"))
                    {
                        FileInfo log = new FileInfo(logInfo);
                        LogsListBoxXAML.Items.Add(log.Name);
                    }
                }
            }
        }

        private void LogsListBoxXAML_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Dispatcher.Invoke(new System.Action(() =>
            {
                try
                {
                    string fullPath = $"{defaultLogDir}\\{LogsListBoxXAML.SelectedItem}";
                    StreamReader sr = new StreamReader(fullPath);
                    LogsTextBoxXAML.Text = sr.ReadToEnd();
                    sr.Close();
                }
                catch { }
            }));
        }

        private void RefreshLogButtonXAML_Click(object sender, RoutedEventArgs e)
        {
            UpdateList();
        }

        private void DeleteLogButtonXAML_Click(object sender, RoutedEventArgs e)
        {
            string fullPath = $"{defaultLogDir}\\{LogsListBoxXAML.SelectedItem}";
            File.Delete(fullPath);
            UpdateList();
        }

        private void CleanLogButtonXAML_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.GetFiles(defaultLogDir).Length != 0)
            {
                foreach (var logFile in Directory.GetFiles(defaultLogDir))
                {
                    if (logFile.EndsWith(".log"))
                    {
                        File.Delete(logFile);
                    }
                }
                UpdateList();
            }
        }
    }
}
