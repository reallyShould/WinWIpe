﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VENIK;
using WinForms = System.Windows.Forms;

namespace WpfApp1
{
    /// Palette - https://colorhunt.co/palette/0926351b42425c83749ec8b9
    /// TODO
    /// System info
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
        static public string user_name = Environment.UserName;

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

            //INIT
            SelectScrollXAML.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            LogScrollXAML.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            LogsTextBoxXAML.Text = start_message;
            this.Title = $"VENIK {version} [{user_name}]";
            if(customFolder == null)
            {
                CustomFolderCheckerXAML.IsEnabled = false;
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
                Dispatcher.Invoke(new Action(() => LogsTextBoxXAML.AppendText($"{message}\n")));
            }
            Dispatcher.Invoke(new Action(() => LogScrollXAML.ScrollToEnd()));
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
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Ошибка при получении информации о браузере {subKeyName}: {ex.Message}"); //
                        }
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
            uint result = SHEmptyRecycleBin(IntPtr.Zero, null, RecycleFlags.SHERB_NOCONFIRMATION | RecycleFlags.SHERB_NOPROGRESSUI | RecycleFlags.SHERB_NOSOUND);
            if (result != 0)
            {
                //add_log("[Error] Recycle bin error");
            }
            else
            {
                //add_log("[OK] Recycle bin clean");
            }
        }


        //BUTTONS EVENTS
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
            CleanButtonXAML.IsEnabled = false;

            // FIX
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
                    catch(Exception err) { Debug.WriteLine($"Warning: {err}\ncheckBox: {checkBox.Name}"); }
                }
            }
            //////////////////////////////////////////////////////////
            
            
            /*List<string> installedBrowsers = GetInstalledSoftware();

            Console.WriteLine("Установленные браузеры:");
            foreach (var browser in installedBrowsers)
            {
                Debug.WriteLine(browser);
            }*/

            // окно появляется раньше чем заканчивается прога

            Done doneWindow = new Done();
            doneWindow.Owner = this;
            doneWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            doneWindow.Show();
            CleanButtonXAML.IsEnabled = true;
        }
    }
}
