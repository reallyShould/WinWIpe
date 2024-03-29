using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WinForms = System.Windows.Forms;

namespace WinWipe
{
    /// TODO
    /// System info

    public partial class MainWindow : Window
    {
        //Refactor 
        SystemAdd SysAdd = new SystemAdd();
        Cleaner cleaner = new Cleaner();

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
        static public string version = "1.0";
        public string customFolder = null;
        public int counter = 0;
        public string start_message = $"=================\nWinWipe by reallyShould\nVersion {version}\n=================\n";


        List<string> defaultBrowsers = new List<string>() { "chrome.exe", "firefox.exe", "opera.exe", "yandex.exe" };
        Dictionary<string, string> browserCache = new Dictionary<string, string>();

        //DOWNLOADS LISTS
        List<string> apps = new List<string>() { ".exe", ".msi" };
        List<string> audio = new List<string>() { ".mp3", ".wav", ".ogg", ".aac", ".flac", ".alac", ".dsd" };
        List<string> video = new List<string>() { ".mp4", ".mov", ".wmv", ".avi", ".mkv", ".avchd" };
        List<string> images = new List<string>() { ".svg", ".png", ".jpeg", ".jpg", ".gif", ".raw", ".tiff" };
        List<string> archives = new List<string>() { ".zip", ".rar", ".tar", ".7z", ".cab", ".arj", ".lzh" };
        List<string> torrents = new List<string>() { ".torrent" };
        List<string> word = new List<string>() { ".doc", ".docx" };

        Dictionary<string, CheckBox> checkers = new Dictionary<string, CheckBox>();
        Dictionary<string, Delegate> checkersDes = new Dictionary<string, Delegate>();
        Dictionary<string, CheckBox> checkersDownloads = new Dictionary<string, CheckBox>();
        Dictionary<string, List<string>>  checkersDownloadsDes = new Dictionary<string, List<string>>();


        public MainWindow()
        {
            InitializeComponent();

            SysAdd.init();
            

            SelectScrollXAML.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            LogScrollXAML.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            LogsTextBoxXAML.Text = start_message;

            Title = $"WinWipe {version} [{SysAdd.user_name}] Admin: {SysAdd.admin}";

            if (customFolder == null)
            {
                CustomFolderCheckerXAML.IsEnabled = false;
            }

            //DICTS FOR MAIN
            checkers = new Dictionary<string, CheckBox>()
            {
                { "TmpCheckerXAML", TmpCheckerXAML },
                { "RecycleCheckerXAML", RecycleCheckerXAML },
                { "UpdatesCheckerXAML", UpdatesCheckerXAML },
                { "CustomFolderCheckerXAML", CustomFolderCheckerXAML },
                { "WebCacheCheckerXAML", WebCacheCheckerXAML }
            };

            checkersDes = new Dictionary<string, Delegate>()
            {
                { "TmpCheckerXAML", new Action(CleanTemporary) },
                { "RecycleCheckerXAML", new Action(CleanRecycleBin) },
                { "UpdatesCheckerXAML", new Action(CleanOldUpdates) },
                { "CustomFolderCheckerXAML", new Action(CleanCustomFolder) },
                { "WebCacheCheckerXAML", new Action(CleanWebCache) }
            };

            browserCache = new Dictionary<string, string>()
            {
                { "chrome.exe", $"C:\\Users\\{SysAdd.user_name}\\AppData\\Local\\Google\\Chrome\\User Data\\Default\\Cache" },
                { "firefox.exe", cleaner.GetFirefoxCache(SysAdd.user_name) },
                { "opera.exe", $"C:\\Users\\{SysAdd.user_name}\\AppData\\Local\\Opera Software\\Opera Stable\\Cache" },
                { "yandex.exe", $"C:\\Users\\{SysAdd.user_name}\\AppData\\Local\\Yandex\\YandexBrowser\\User Data\\Default\\Cache" }
            };

            //DICTS FOR DOWNLOADS
            checkersDownloads = new Dictionary<string, CheckBox>()
            {
                { "ExeCheckerXAML", ExeCheckerXAML },
                { "AudioCheckerXAML", AudioCheckerXAML },
                { "VideoCheckerXAML", VideoCheckerXAML },
                { "ImagesCheckerXAML", ImagesCheckerXAML },
                { "TorrentsCheckerXAML", TorrentsCheckerXAML },
                { "ArchiveCheckerXAML", ArchiveCheckerXAML },
                { "DocumentsCheckerXAML", DocumentsCheckerXAML }
            };

            checkersDownloadsDes = new Dictionary<string, List<string>>()
            {
                { "ExeCheckerXAML", apps },
                { "AudioCheckerXAML", audio },
                { "VideoCheckerXAML", video },
                { "ImagesCheckerXAML", images },
                { "TorrentsCheckerXAML", torrents },
                { "ArchiveCheckerXAML", archives },
                { "DocumentsCheckerXAML", word }
            };
        }

        //ACTIONS

        private void FullRemove(string dir)
        {
            try
            {
                foreach (var file in Directory.GetFiles(dir))
                {
                    try
                    {
                        Dispatcher.Invoke(new Action(() => SysAdd.AddToFinal(file, FinalLabelXAML)));
                        File.Delete(file);
                        Dispatcher.Invoke(new Action(() => SysAdd.AddLog($"[OK] File {file} deleted", LogsTextBoxXAML, LogScrollXAML)));
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.Invoke(new Action(() => SysAdd.RemoveFromFinal(file, FinalLabelXAML)));
                        if (ex is DirectoryNotFoundException) 
                        { 
                            Dispatcher.Invoke(new Action(() => SysAdd.AddLog($"[Not Found] File \"{file}\" is not exist", LogsTextBoxXAML, LogScrollXAML))); 
                        }
                        else if (ex is UnauthorizedAccessException) 
                        { 
                            Dispatcher.Invoke(new Action(() => SysAdd.AddLog($"[Access Denied] File \"{file}\" access denied", LogsTextBoxXAML, LogScrollXAML))); 
                        }
                        else 
                        { 
                            Dispatcher.Invoke(new Action(() => SysAdd.AddLog($"[Error] {ex.Message}", LogsTextBoxXAML, LogScrollXAML)));
                        }
                        continue;
                    }
                }
                foreach (var subdir in Directory.GetDirectories(dir))
                {
                    Dispatcher.Invoke(() => FullRemove(subdir));
                }
                try
                {
                    Directory.Delete(dir);
                    Dispatcher.Invoke(new Action(() => SysAdd.AddLog($"[OK] Dir {dir} deleted", LogsTextBoxXAML, LogScrollXAML)));
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(new Action(() => SysAdd.AddLog($"[Error] {ex.Message}", LogsTextBoxXAML, LogScrollXAML)));
                }
            }
            catch (Exception ex) 
            {
                if (ex is DirectoryNotFoundException) 
                { 
                    Dispatcher.Invoke(new Action(() => SysAdd.AddLog($"[Not Found] Directory \"{dir}\" is not exist", LogsTextBoxXAML, LogScrollXAML))); 
                }
                else if (ex is UnauthorizedAccessException) 
                { 
                    Dispatcher.Invoke(new Action(() => SysAdd.AddLog($"[Access Denied] Directory \"{dir}\" access denied", LogsTextBoxXAML, LogScrollXAML))); 
                }
                else 
                { 
                    Dispatcher.Invoke(new Action(() => SysAdd.AddLog($"[Error] {ex.Message}", LogsTextBoxXAML, LogScrollXAML))); 
                }
            }
        }

        private async void CleanTemporary()
        {
            Dispatcher.Invoke(new Action(() => SysAdd.AddLog($"\t\tCleaning temporary files", LogsTextBoxXAML, LogScrollXAML)));
            try
            {
                await Task.Run(() => FullRemove($"C:\\Windows\\Temp"));
                await Task.Run(() => FullRemove($"C:\\Users\\{SysAdd.user_name}\\AppData\\Local\\Temp"));
            }
            catch (Exception err)
            {
                Dispatcher.Invoke(new Action(() => SysAdd.AddLog($"[Error] {err}", LogsTextBoxXAML, LogScrollXAML)));
            }
        }

        private void CleanRecycleBin()
        {
            try
            {
                CleanButtonXAML.IsEnabled = false;
                uint result = SHEmptyRecycleBin(IntPtr.Zero, null, RecycleFlags.SHERB_NOCONFIRMATION | RecycleFlags.SHERB_NOPROGRESSUI | RecycleFlags.SHERB_NOSOUND);
                if (result != 0)
                {
                    Dispatcher.Invoke(new Action(() => SysAdd.AddLog("[Error] Recycle bin error", LogsTextBoxXAML, LogScrollXAML)));
                }
                else
                {
                    Dispatcher.Invoke(new Action(() => SysAdd.AddLog("[OK] Recycle bin clean", LogsTextBoxXAML, LogScrollXAML)));
                }
            }
            finally
            {
                CleanButtonXAML.IsEnabled = true;
            }
        }

        private async void CleanOldUpdates()
        {
            Dispatcher.Invoke(new Action(() => SysAdd.AddLog($"\t\tCleaning old updates", LogsTextBoxXAML, LogScrollXAML)));
            try
            {
                Process proc = await Task.Run(() => Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = "rd /s /q c:windows.old",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }));
                Dispatcher.Invoke(new Action(() => SysAdd.AddLog($"[OK] Done", LogsTextBoxXAML, LogScrollXAML)));
            }
            catch (Exception err) { Dispatcher.Invoke(new Action(() => SysAdd.AddLog($"[Error] {err}", LogsTextBoxXAML, LogScrollXAML))); }
        }

        private async void CleanWebCache()
        {
            foreach (var browser in browserCache.Keys)
            {
                if (SysAdd.installedSoftware.Contains(browser))
                {
                    await Task.Run(() => FullRemove(browserCache[browser]));
                    Debug.WriteLine(browser);
                }
            }
        }

        private void CleanDownloads(List<string> item)
        {
            Dispatcher.Invoke(new Action(() => SysAdd.AddLog($"\t\tCleaning downloads ", LogsTextBoxXAML, LogScrollXAML)));
            foreach (var file in Directory.GetFiles($"C:\\Users\\{SysAdd.user_name}\\Downloads"))
            {
                foreach (var im in item)
                {
                    if (file.EndsWith(im))
                    {
                        try
                        {
                            Dispatcher.Invoke(new Action(() => SysAdd.AddToFinal(file, FinalLabelXAML)));
                            File.Delete(file);
                            Dispatcher.Invoke(new Action(() => SysAdd.AddLog($"[OK] File {file} deleted", LogsTextBoxXAML, LogScrollXAML)));
                        }
                        catch (Exception ex)
                        {
                            Dispatcher.Invoke(new Action(() => SysAdd.RemoveFromFinal(file, FinalLabelXAML)));
                            Dispatcher.Invoke(new Action(() => SysAdd.AddLog($"[Error] {ex.Message}", LogsTextBoxXAML, LogScrollXAML)));
                        }
                    }
                }
            }
            Dispatcher.Invoke(new Action(() => SysAdd.AddLog("[OK] Done", LogsTextBoxXAML, LogScrollXAML)));
        }

        private void CleanCustomFolder()
        {
            Dispatcher.Invoke(new Action(() => SysAdd.AddLog($"\t\tCleaning custom folder", LogsTextBoxXAML, LogScrollXAML)));
            try
            {
                Task.Run(() => FullRemove(customFolder));
            }
            catch (Exception err) { Dispatcher.Invoke(new Action(() => SysAdd.AddLog($"[Error] {err}", LogsTextBoxXAML, LogScrollXAML))); }
        }

        //BUTTONS EVENTS

        private void LogsButtonXAML_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    string newLog = $"{SysAdd.defaultLogDir}\\{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}_" +
                                    $"{DateTime.Now.Hour}-{DateTime.Now.Minute}-{DateTime.Now.Second}.log";

                    using (StreamWriter sw = File.CreateText(newLog))
                    {
                        sw.WriteLine(SysAdd.log.ToString());
                    }

                    SysAdd.log.Clear();

                    Log_Viewer log_Viewer = new Log_Viewer(SysAdd.defaultLogDir);
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
                Dispatcher.Invoke(new Action(() => SysAdd.AddLog("Custom folder is " + customFolder, LogsTextBoxXAML, LogScrollXAML)));
            }
        }

        private void btn_clean(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() => SysAdd.ResetFinal(FinalLabelXAML)));
            CleanButtonXAML.IsEnabled = false;

            // FIX IT PLS
            // WTF ADAM? FIX IT!!!!!
            Dispatcher.Invoke(new Action(() => SysAdd.AddLog("", LogsTextBoxXAML, LogScrollXAML)));
            Dispatcher.Invoke(new Action(() => SysAdd.AddLog("Clean start", LogsTextBoxXAML, LogScrollXAML)));
            Dispatcher.Invoke(new Action(() => SysAdd.AddLog("sep", LogsTextBoxXAML, LogScrollXAML)));

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
                    catch (Exception err)
                    {
                        Debug.WriteLine($"Warning: {err}\ncheckBox: {checkBox.Name}");
                    }
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
                            Dispatcher.Invoke(new Action(() => CleanDownloads(checkersDownloadsDes[checkBox.Name])));
                        }
                    }
                    catch (Exception err)
                    {
                        Debug.WriteLine($"Warning: {err}\ncheckBox: {checkBox.Name}");
                    }
                }
            }
            CleanButtonXAML.IsEnabled = true;
        }
    }
}
