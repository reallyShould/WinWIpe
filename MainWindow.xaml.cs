using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WinForms = System.Windows.Forms;

namespace VENIK
{
    /// Palette - https://colorhunt.co/palette/0926351b42425c83749ec8b9
    /// TODO
    /// exception size
    /// System info
    /// Web chache 
    /// Status bar ???
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
        static public string version = "0.2";
        public string defaultLogDir = "C:\\Users\\reallyShould\\AppData\\Roaming\\VENIK";
        public string defaultLogFile = "C:\\Users\\reallyShould\\AppData\\Roaming\\VENIK\\clean.log";
        StringBuilder log = new StringBuilder();
        public string customFolder = null;
        public long fullSize;
        public int counter = 0;
        public string start_message = $"=================\nVENIK by reallyShould\nVersion {version}\n=================\n";
        static public string user_name = Environment.UserName;
        public bool admin = false;

        List<string> defaultBrowsers = new List<string>() { "chrome.exe", "firefox.exe", "opera.exe", "yandex.exe" };
        Dictionary<string, string> browserChache = new Dictionary<string, string>()
        {
            { "chrome.exe", $"C:\\Users\\{user_name}\\AppData\\Local\\Google\\Chrome\\User Data\\Default\\Cache" },
            { "firefox.exe", $"C:\\Users\\{user_name}\\AppData\\Local\\Mozilla\\Firefox\\Profiles\\zxcvb5678.default\\cache2\\entries" },///////////
            { "opera.exe", $"C:\\Users\\{user_name}\\AppData\\Local\\Opera Software\\Opera Stable\\Cache" },
            { "yandex.exe", $"C:\\Users\\{user_name}\\AppData\\Local\\Yandex\\YandexBrowser\\User Data\\Default\\Cache" }
        };

        //DOWNLOADS LISTS
        List<string> apps = new List<string>() { ".exe", ".msi" };
        List<string> audio = new List<string>() { ".mp3", ".wav", ".ogg", ".aac", ".flac", ".alac", ".dsd" };
        List<string> video = new List<string>() { ".mp4", ".mov", ".wmv", ".avi", ".mkv", ".avchd" };
        List<string> images = new List<string>() { ".svg", ".png", ".jpeg", ".jpg", ".gif", ".raw", ".tiff" };
        List<string> archives = new List<string>() { ".zip", ".rar", ".tar", ".7z", ".cab", ".arj", ".lzh" };
        List<string> torrents = new List<string>() { ".torrent" };

        Dictionary<string, CheckBox> checkers = new Dictionary<string, CheckBox>();
        Dictionary<string, Delegate> checkersDes = new Dictionary<string, Delegate>();
        Dictionary<string, CheckBox> checkersDownloads = new Dictionary<string, CheckBox>();
        Dictionary<string, List<string>>  checkersDownloadsDes = new Dictionary<string, List<string>>();


        public MainWindow()
        {
            InitializeComponent();

            fullSize = 0;

            SelectScrollXAML.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            LogScrollXAML.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            LogsTextBoxXAML.Text = start_message;

            this.Title = $"VENIK {version} [{user_name}] Admin: {admin}";

            try
            {
                Directory.CreateDirectory("C:\\Windows\\FolderForTest");
                Directory.Delete("C:\\Windows\\FolderForTest");
                admin = true;
            }
            catch 
            {
                admin = false;
            }

            //CREATE LOG FOLDER
            Dispatcher.Invoke(new Action(() =>
            {
                if (!Directory.Exists(defaultLogDir))
                    Directory.CreateDirectory(defaultLogDir);
            }));

            if (customFolder == null)
            {
                CustomFolderCheckerXAML.IsEnabled = false;
            }

            //INIT
            List<string> installedSoftware = GetInstalledSoftware();
            Debug.WriteLine("\tSOFT:");
            foreach (var soft in installedSoftware)
            {
                Debug.WriteLine(soft);
            }

            //DICTS FOR MAIN
            checkers = new Dictionary<string, CheckBox>()
            {
                { "TmpCheckerXAML", TmpCheckerXAML },
                { "RecycleCheckerXAML", RecycleCheckerXAML },
                { "UpdatesCheckerXAML", UpdatesCheckerXAML },
                { "CustomFolderCheckerXAML", CustomFolderCheckerXAML }
            };

            checkersDes = new Dictionary<string, Delegate>()
            {
                { "TmpCheckerXAML", new Action(clean_tmp) },
                { "RecycleCheckerXAML", new Action(clean_recycle) },
                { "UpdatesCheckerXAML", new Action(old_updates) },
                { "CustomFolderCheckerXAML", new Action(clean_folder) }
            };

            //DICTS FOR DOWNLOADS
            checkersDownloads = new Dictionary<string, CheckBox>()
            {
                { "ExeCheckerXAML", ExeCheckerXAML },
                { "AudioCheckerXAML", AudioCheckerXAML },
                { "VideoCheckerXAML", VideoCheckerXAML },
                { "ImagesCheckerXAML", ImagesCheckerXAML },
                { "TorrentsCheckerXAML", TorrentsCheckerXAML },
                { "ArchiveCheckerXAML", ArchiveCheckerXAML }
            };

            checkersDownloadsDes = new Dictionary<string, List<string>>()
            {
                { "ExeCheckerXAML", apps },
                { "AudioCheckerXAML", audio },
                { "VideoCheckerXAML", video },
                { "ImagesCheckerXAML", images },
                { "TorrentsCheckerXAML", torrents },
                { "ArchiveCheckerXAML", archives }
            };
        }

        //ADDITIONAL

        private void add_log(string message)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                if (LogsTextBoxXAML.Text.Length > 10000)
                {
                    LogsTextBoxXAML.Clear();
                }
            }));
            if (string.IsNullOrEmpty(message))
            {
                Dispatcher.Invoke(new Action(() => LogsTextBoxXAML.Clear()));
            }
            else if (message == "sep")
            {
                add_log("============");
            }
            else
            {
                Dispatcher.Invoke(new Action(() => 
                { 
                    LogsTextBoxXAML.AppendText($"{message}\n");
                    log.Append($"({DateTime.Now}) {message}\n");
                }));
            }
            Dispatcher.Invoke(new Action(() => LogScrollXAML.ScrollToEnd()));
        }

        private void AddToFinal(string file)
        {
            Dispatcher.Invoke(() =>
            {
                FileInfo lng = new FileInfo(file);
                fullSize += lng.Length;
                FinalLabelXAML.Content = $"Final {BytesToString(fullSize)}";
            });
        }

        static String BytesToString(long byteCount)
        {
            string[] suf = { "Byt", "KB", "MB", "GB", "TB", "PB", "EB" };
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

        private void clean_custom_folder(string dir)
        {
            try
            {
                foreach (var file in Directory.GetFiles(dir))
                {
                    try
                    {
                        File.Delete(file);
                        AddToFinal(file);
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
                    Dispatcher.Invoke(() => clean_custom_folder(subdir));
                }
                try
                {
                    Directory.Delete(dir);
                    add_log($"[OK] Dir {dir} deleted");
                }
                catch (Exception ex)
                {
                    add_log($"[Error] {ex.Message}");
                }
            }
            catch (Exception err) { add_log($"[Error] {err}"); }
        }

        static List<string> GetInstalledSoftware()
        {
            List<string> browsers = new List<string>();
            string registryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\";

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryPath))
            {
                if (key != null)
                {
                    string[] subKeyNames = key.GetSubKeyNames();

                    foreach (var subKeyName in subKeyNames)
                    {
                        try
                        {
                            string browserPath = key.OpenSubKey(subKeyName)?.GetValue("")?.ToString();
                            if (browserPath != null)
                                browsers.Add($"{subKeyName}");
                        }
                        catch { }
                    }
                }
            }
            return browsers;
        }


        //ACTIONS
        // Try add to other script

        private void clean_tmp()
        {
            add_log($"\t\tCleaning temporary files");
            try
            {
                Task.Run(() => clean_custom_folder($"C:\\Windows\\Temp"));
                Task.Run(() => clean_custom_folder($"C:\\Users\\{user_name}\\AppData\\Local\\Temp"));
            }
            catch (Exception err) { add_log($"[Error] {err}"); }
        }

        private void clean_folder()
        {
            add_log($"\t\tCleaning custom folder");
            try
            {
                Task.Run(() => clean_custom_folder(customFolder));
            }
            catch (Exception err) { add_log($"[Error] {err}"); }
        }

        private void old_updates()
        {
            add_log($"\t\tCleaning old updates");
            try
            {
                Process proc = Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = "rd /s /q c:windows.old",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                add_log($"[OK] Done");
            }
            catch (Exception err) { add_log($"[Error] {err}"); }
        }

        private void clean_downloads(List<string> item)
        {
            add_log($"\t\tCleaning downloads ");
            foreach (var file in Directory.GetFiles($"C:\\Users\\{user_name}\\Downloads"))
            {
                foreach (var im in item)
                {
                    if (file.EndsWith(im))
                    {
                        try 
                        {
                            File.Delete(file);
                            AddToFinal(file);
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

 
        private void clean_recycle()
        {
            Task.Run(new Action(() =>
            {
                Dispatcher.Invoke(new Action(() => CleanButtonXAML.IsEnabled = false));
                uint result = SHEmptyRecycleBin(IntPtr.Zero, null, RecycleFlags.SHERB_NOCONFIRMATION | RecycleFlags.SHERB_NOPROGRESSUI | RecycleFlags.SHERB_NOSOUND);
                if (result != 0)
                {
                    add_log("[Error] Recycle bin error");
                }
                else
                {
                    add_log("[OK] Recycle bin clean");
                }
                Dispatcher.Invoke(new Action(() => CleanButtonXAML.IsEnabled = true));
            }));
        }


        //BUTTONS EVENTS

        private void LogsButtonXAML_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    string newLog = $"{defaultLogDir}\\{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}_" +
                                    $"{DateTime.Now.Hour}-{DateTime.Now.Minute}-{DateTime.Now.Second}.log";

                    using (StreamWriter sw = File.CreateText(newLog))
                    {
                        sw.WriteLine(log.ToString());
                    }

                    log.Clear();

                    Log_Viewer log_Viewer = new Log_Viewer();
                    log_Viewer.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    log_Viewer.Owner = this;
                    log_Viewer.Show();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Write error: " + ex.Message);
                }
            }));
        }


        private void CleanLogsButtonXAML_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() => LogsTextBoxXAML.Clear()));
        }

        private void btn_ChangeCustomFolderButtonXAML(object sender, RoutedEventArgs e)
        {
            WinForms.FolderBrowserDialog dialog = new WinForms.FolderBrowserDialog();
            dialog.ShowDialog();
            if (dialog.SelectedPath != "")
            {
                customFolder = dialog.SelectedPath;
                CustomFolderCheckerXAML.IsEnabled = true;
                CustomFolderCheckerXAML.IsChecked = true;
                add_log("Custom folder is " + customFolder);
            }
        }

        private void btn_clean(object sender, RoutedEventArgs e)
        {
            // FIX IT PLS
            add_log("");
            add_log("Clean start");
            add_log("sep");

            //MAIN
            foreach (FrameworkElement checkBox in StackPanelXAML.Children)
            {
                if (checkBox is CheckBox || checkers.ContainsKey(checkBox.Name))
                {
                    try
                    {
                        if (checkers[checkBox.Name].IsChecked == true)
                        {
                            Dispatcher.Invoke(checkersDes[checkBox.Name]);
                        }
                    }
                    catch (Exception err) { Debug.WriteLine($"Warning: {err}\ncheckBox: {checkBox.Name}"); }
                }
            }

            //CLEAN DOWNLOADS
            foreach (FrameworkElement checkBox in StackPanelXAML.Children)
            {
                if (checkBox is CheckBox || checkersDownloads.ContainsKey(checkBox.Name))
                {
                    try
                    {
                        if (checkersDownloads[checkBox.Name].IsChecked == true)
                        {
                            Dispatcher.Invoke(new Action(() => clean_downloads(checkersDownloadsDes[checkBox.Name])));
                        }
                    }
                    catch (Exception err) { Debug.WriteLine($"Warning: {err}\ncheckBox: {checkBox.Name}"); }
                }
            }
        }
    }
}
