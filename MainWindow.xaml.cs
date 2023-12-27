using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
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
    /// https://colorhunt.co/palette/0926351b42425c83749ec8b9

    public partial class MainWindow : Window
    {
        enum RecycleFlags : uint
        {
            SHERB_NOCONFIRMATION = 0x00000001,
            SHERB_NOPROGRESSUI = 0x00000002,
            SHERB_NOSOUND = 0x00000004
        }
        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        static extern uint SHEmptyRecycleBin(IntPtr hwnd, string pszRootPath, RecycleFlags dwFlags);


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
            foreach (var file in Directory.GetFiles(dir))
            {
                try
                {
                    File.Delete(file);
                    this.Dispatcher.Invoke(new Action(() => add_log($"[OK] File {file} deleted")));
                }
                catch (Exception ex)
                {
                    this.Dispatcher.Invoke(new Action( () => add_log($"[Error] {ex.Message}") ));
                    continue;
                }
            }
            foreach (var subdir in Directory.GetDirectories(dir))
            {
                clean_custom_folder(subdir);
            }
            try { 
                Directory.Delete(dir);
                this.Dispatcher.Invoke(new Action(() => add_log($"[OK] Dir {dir} deleted")));
            }
            catch (Exception ex) 
            {
                this.Dispatcher.Invoke(new Action(() => add_log($"[Error] {ex.Message}")));
            }
        }
 

        private void clean_recycle()
        {
            uint result = SHEmptyRecycleBin(IntPtr.Zero, null, 0);
            if (result != 0)
            {
                add_log("[Error] Recycle bin error");
            }
            else
            {
                add_log("[OK] Recycle bin clean");
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
                Task.Run(() => clean_custom_folder(customFolder));
            }
            if (clear_tmp_chk.IsChecked == true)
            {
                Task.Run(() => clean_custom_folder("C:\\Users\\reallyShould\\AppData\\Local\\Temp"));
            }

            if (clear_recycle.IsChecked == true)
            {
                add_log("Recycle bin cleaning\n");
                Task.Run(() => clean_recycle());
            }
        }
    }
}
