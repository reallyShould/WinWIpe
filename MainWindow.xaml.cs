using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WinForms = System.Windows.Forms;

namespace WpfApp1
{
    /// Palette - https://colorhunt.co/palette/0926351b42425c83749ec8b9
    /// TODO
    /// System info
    /// Win logs
    /// Web chache 
    /// Fix scrollbar
    /// Status bar
    /// Cleaned memory info

    public partial class MainWindow : Window
    {
        //FOR RECYCLE BIN
        enum RecycleFlags : uint
        {
            SHERB_NOCONFIRMATION = 0x00000001,
            SHERB_NOPROGRESSUI = 0x00000002,
            SHERB_NOSOUND = 0x00000004
        }
        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        static extern uint SHEmptyRecycleBin(IntPtr hwnd, string pszRootPath, RecycleFlags dwFlags);

        //MAIN VARIABLES
        static public string version = "0.1";
        public string customFolder = null;
        public int counter = 0;
        public string start_message = $"=================\nVENIK by reallyShould\nVersion {version}\n=================\n";
        public string user_name = Environment.UserName;

        //DOWNLOADS LISTS
        List<string> apps = new List<string>() { ".exe", ".msi" };
        List<string> audio = new List<string>() { ".mp3", ".wav", ".ogg", ".aac", ".flac", ".alac", ".dsd" };
        List<string> video = new List<string>() { ".mp4", ".mov", ".wmv", ".avi", ".mkv", ".avchd" };
        List<string> images = new List<string>() { ".svg", ".png", ".jpeg", ".jpg", ".gif", ".raw", ".tiff" };
        List<string> archives = new List<string>() { ".zip", ".rar", ".tar", ".7z", ".cab", ".arj", ".lzh" };
        List<string> torrents = new List<string>() { ".torrent" };

        Dictionary<string, CheckBox> checkersDownloads = new Dictionary<string, CheckBox>();
        Dictionary<string, List<string>>  checkersDes = new Dictionary<string, List<string>>();


        public MainWindow()
        {
            InitializeComponent();

            //INIT
            selectScroll.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            statusBar.Value = 100; //
            logs.Text = start_message;
            this.Title = $"VENIK {version} [{user_name}]";
            if(customFolder == null)
            {
                clear_custom_folder_chk.IsEnabled = false;
            }

            //DICTS FOR DOWNLOADS
            checkersDownloads = new Dictionary<string, CheckBox>()
            {
                { "clear_downloads_exe", clear_downloads_exe },
                { "clear_downloads_music", clear_downloads_music },
                { "clear_downloads_video", clear_downloads_video },
                { "clear_downloads_images", clear_downloads_images },
                { "clear_downloads_torrents", clear_downloads_torrents },
                { "clear_downloads_archive", clear_downloads_archive }
            };
            checkersDes = new Dictionary<string, List<string>>()
            {
                { "clear_downloads_exe", apps },
                { "clear_downloads_music", audio },
                { "clear_downloads_video", video },
                { "clear_downloads_images", images },
                { "clear_downloads_torrents", torrents },
                { "clear_downloads_archive", archives }
            };
        }

        //ACTIONS
        // Try add to other script

        private void clean_downloads(List<string> item)
        {
            foreach (var file in Directory.GetFiles($"C:\\Users\\{user_name}\\Downloads"))
            {
                foreach (var im in item)
                {
                    if (file.EndsWith(im))
                    {
                        try 
                        {
                            File.Delete(file);
                            add_log($"[OK] File {file} deleted");
                        }
                        catch (Exception ex)
                        {
                            add_log($"[Error] {ex.Message}");
                        }
                    }
                }
            }
            add_log("[OK] Done");
        }

        private void clean_custom_folder(string dir)
        {
            foreach (var file in Directory.GetFiles(dir))
            {
                try
                {
                    File.Delete(file);
                    add_log($"[OK] File {file} deleted");
                }
                catch (Exception ex)
                {
                    add_log($"[Error] {ex.Message}");
                    continue;
                }
            }
            foreach (var subdir in Directory.GetDirectories(dir))
            {
                clean_custom_folder(subdir);
            }
            try { 
                Directory.Delete(dir);
                add_log($"[OK] Dir {dir} deleted");
            }
            catch (Exception ex) 
            {
                add_log($"[Error] {ex.Message}");
            }
        }
 
        private void clean_recycle()
        {
            uint result = SHEmptyRecycleBin(IntPtr.Zero, null, RecycleFlags.SHERB_NOCONFIRMATION | RecycleFlags.SHERB_NOPROGRESSUI | RecycleFlags.SHERB_NOSOUND);
            if (result != 0)
            {
                add_log("[Error] Recycle bin error");
            }
            else
            {
                add_log("[OK] Recycle bin clean");
            }
        }

        private void add_log(string message)
        {
            if (string.IsNullOrEmpty(message)) 
            { 
                Dispatcher.Invoke(new Action(() => logs.Text = string.Empty));
            }
            else if (message == "sep")
            {
                add_log("============");
            }
            else 
            { 
                Dispatcher.Invoke(new Action(() => logs.AppendText($"{message}\n")));
            }
            Dispatcher.Invoke(new Action(() => logScroll.ScrollToEnd()));
        }

        //BUTTONS EVENTS
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
            // FIX
            add_log("");
            add_log("Clean start");
            add_log("sep");

            // FIX
            if (clear_custom_folder_chk.IsChecked == true)
            {
                add_log("Custom folder cleaning\n");
                Task.Run(() => clean_custom_folder(customFolder));
            }
            if (clear_tmp_chk.IsChecked == true)
            {
                Task.Run(() => clean_custom_folder($"C:\\Users\\{user_name}\\AppData\\Local\\Temp"));
            }

            if (clear_recycle.IsChecked == true)
            {
                add_log("Recycle bin cleaning\n");
                Task.Run(() => clean_recycle());
            }

            //CLEAN DOWNLOADS
            foreach (FrameworkElement checkBox in stackPanel.Children)
            {
                if (checkBox is CheckBox || checkersDownloads.ContainsKey(checkBox.Name))
                {
                    try
                    {
                        if (checkersDownloads[checkBox.Name].IsChecked == true)
                        {
                            Dispatcher.Invoke(new Action(() => clean_downloads(checkersDes[checkBox.Name])));
                        }
                    }
                    catch { }
                }
            }
        }
    }
}
