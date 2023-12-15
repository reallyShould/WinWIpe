using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WinForms = System.Windows.Forms;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string customFolder = null;
        public int counter = 0;
        public string start_message = "=================\nVENIK by reallyShould\nVersion 0.0.1\n=================\n";

        List<string> apps = new List<string>() { ".exe", ".msi" };
        List<string> audio = new List<string>() { ".mp3", ".wav", ".ogg", ".aac", ".flac", ".alac", ".dsd" };
        List<string> video = new List<string>() { ".mp4", ".mov", ".wmv", ".avi", ".mkv", ".avchd" };
        List<string> images = new List<string>() { ".svg", ".png", ".jpeg", ".jpg", ".gif", ".raw", ".tiff" };
        List<string> archives = new List<string>() { ".zip", ".rar", ".tar", ".7z", ".cab", ".arj", ".lzh" };
        List<string> torrents = new List<string>() { ".torrent" };


        public MainWindow()
        {
            InitializeComponent();
            selectScroll.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            statusBar.Value = 100;
            logs.Text = start_message;
            if(customFolder == null)
            {
                clear_custom_folder_chk.IsEnabled = false;
            }
        }

        //ACTIONS

        private void clean_custom_folder(string dir)
        {
            try
            {
                foreach (var file in Directory.GetFiles(dir))
                {
                    File.Delete(file);
                    add_log($"File {file} deleted");
                }
                foreach (var subdir in Directory.GetDirectories(dir))
                {
                    clean_custom_folder(subdir);
                }
                Directory.Delete(dir);
                add_log($"Dir {dir} deleted");
            }
            catch (Exception ex)
            {
                add_log($"Error: {ex.Message}");
            }
        }

        //BUTTONS EVENTS
        private void add_log(string message)
        {
            if (logs.Text == "Logs") { logs.Text += "\n"; }
            if (string.IsNullOrEmpty(message)) 
            { 
                logs.Text = string.Empty; 
            }
            else if (message == "sep")
            {
                add_log("============");
            }
            else 
            { 
                logs.Text = logs.Text + message + "\n"; 
            }
            logScroll.ScrollToEnd();
        }

        private void btn_change_custom_folder(object sender, RoutedEventArgs e)
        {
            WinForms.FolderBrowserDialog dialog = new WinForms.FolderBrowserDialog();
            dialog.ShowDialog();
            if (dialog.SelectedPath != "")
            {
                customFolder = dialog.SelectedPath;
                clear_custom_folder_chk.IsEnabled = true;
                clear_custom_folder_chk.IsChecked = true;
                add_log("Custom folder is " + customFolder);
            }
        }

        private void btn_clean(object sender, RoutedEventArgs e)
        {
            add_log("");
            add_log("Clean start");
            add_log("sep");
            //action
            if (clear_custom_folder_chk.IsChecked == true)
            {
                add_log("Custom folder cleaning\n");
                clean_custom_folder(customFolder);
            }
            add_log("sep");
            add_log("Clean end");
        }
    }
}
