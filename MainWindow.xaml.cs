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

        //MAIN VARIABLES
        static public string version = "1.0";
        public string customFolder = null;
        public int counter = 0;
        public string start_message = $"=================\nWinWipe by reallyShould\nVersion {version}\n=================\n";


        List<string> defaultBrowsers = new List<string>() { "chrome.exe", "firefox.exe", "opera.exe", "yandex.exe" };

        //DOWNLOADS LISTS
        List<string> apps = new List<string>() { ".exe", ".msi" };
        List<string> audio = new List<string>() { ".mp3", ".wav", ".ogg", ".aac", ".flac", ".alac", ".dsd" };
        List<string> video = new List<string>() { ".mp4", ".mov", ".wmv", ".avi", ".mkv", ".avchd" };
        List<string> images = new List<string>() { ".svg", ".png", ".jpeg", ".jpg", ".gif", ".raw", ".tiff" };
        List<string> archives = new List<string>() { ".zip", ".rar", ".tar", ".7z", ".cab", ".arj", ".lzh" };
        List<string> torrents = new List<string>() { ".torrent" };
        List<string> word = new List<string>() { ".doc", ".docx" };

        Dictionary<string, CheckBox> checkers = new Dictionary<string, CheckBox>();
        Dictionary<string, Action> checkersDes = new Dictionary<string, Action>();
        Dictionary<string, CheckBox> checkersDownloads = new Dictionary<string, CheckBox>();
        Dictionary<string, List<string>>  checkersDownloadsDes = new Dictionary<string, List<string>>();


        public MainWindow()
        {
            InitializeComponent();

            SysAdd.init();
            cleaner.init();

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

            checkersDes = new Dictionary<string, Action>()
            {
                { "TmpCheckerXAML", () => cleaner.CleanTemporary(LogsTextBoxXAML, LogScrollXAML, FinalLabelXAML, Application.Current.Dispatcher) },
                { "RecycleCheckerXAML", () => cleaner.CleanRecycleBin(LogsTextBoxXAML, LogScrollXAML, CleanButtonXAML) },
                { "UpdatesCheckerXAML", () => cleaner.CleanOldUpdates(LogsTextBoxXAML, LogScrollXAML) },
                { "CustomFolderCheckerXAML", () => cleaner.CleanCustomFolder(customFolder, LogsTextBoxXAML, LogScrollXAML, FinalLabelXAML, Application.Current.Dispatcher) },
                { "WebCacheCheckerXAML", () => cleaner.CleanWebCache(LogsTextBoxXAML, LogScrollXAML, FinalLabelXAML, Application.Current.Dispatcher) }
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
                Dispatcher.Invoke(new Action(() => SysAdd.AddLog("Custom folder is " + customFolder, LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher)));
            }
        }

        private void btn_clean(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() => SysAdd.ResetFinal(FinalLabelXAML)));
            CleanButtonXAML.IsEnabled = false;

            // FIX IT PLS
            // WTF ADAM? FIX IT!!!!!
            Dispatcher.Invoke(new Action(() => SysAdd.AddLog("", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher)));
            Dispatcher.Invoke(new Action(() => SysAdd.AddLog("Clean start", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher)));
            Dispatcher.Invoke(new Action(() => SysAdd.AddLog("sep", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher)));

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
                            Dispatcher.Invoke(new Action(() => cleaner.CleanDownloads(checkersDownloadsDes[checkBox.Name], LogsTextBoxXAML, LogScrollXAML, FinalLabelXAML, Application.Current.Dispatcher)));
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
