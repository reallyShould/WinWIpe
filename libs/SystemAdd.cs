using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Threading;

namespace WinWipe
{
    internal class SystemAdd
    {
        // Variables
        public string user_name = Environment.UserName;
        public StringBuilder log = new StringBuilder();
        public long fullSize;
        public List<string> installedSoftware = new List<string>();
        public bool admin = false;
        public string defaultLogDir;
        public string defaultLogFile;
        public char activeDisk;

        // Actions
        public void init()
        {
            activeDisk = getActiveDisk();

            defaultLogDir = $"{activeDisk}:\\Users\\{user_name}\\AppData\\Roaming\\WinWipe";
            defaultLogFile = $"{activeDisk}:\\Users\\{user_name}\\AppData\\Roaming\\WinWipe\\clean.log";

            fullSize = 0;
            installedSoftware = GetInstalledSoftware();

            // ADMIN CHECK
            try
            {
                Directory.CreateDirectory($"{activeDisk}:\\Windows\\FolderForTest");
                Directory.Delete($"{activeDisk}:\\Windows\\FolderForTest");
                admin = true;
            }
            catch { admin = false; }

            // CREATE LOG FOLDER
            if (!Directory.Exists(defaultLogDir))
                Directory.CreateDirectory(defaultLogDir);

        }

        private char getActiveDisk()
        {
            char disk = 'C';
            List<char> disks = "ABDEFG".ToList();
            foreach (var i in disks) 
            { 
                if (Directory.Exists($"{i}:\\Users\\{user_name}"))
                {
                    disk = i;
                    break;
                }
            }
            return disk;
        }

        public void AddLog(string message, TextBox LogsTextBoxXAML, ScrollViewer LogScrollXAML, Dispatcher d)
        {
            d.Invoke(new Action(() =>
            {
                if (LogsTextBoxXAML.Text.Length > 10000)
                {
                    LogsTextBoxXAML.Clear();
                }
            }));
            if (string.IsNullOrEmpty(message))
            {
                d.Invoke(new Action(() => LogsTextBoxXAML.Clear()));
            }
            else if (message == "sep")
            {
                d.Invoke(new Action(() => AddLog("============", LogsTextBoxXAML, LogScrollXAML, d)));
            }
            else
            {
                d.Invoke(new Action(() =>
                {
                    LogsTextBoxXAML.AppendText($"{message}\n");
                }));
                log.Append($"({DateTime.Now}) {message}\n");
            }
            LogScrollXAML.ScrollToEnd();
        }

        public List<string> GetInstalledSoftware()
        {
            List<string> soft = new List<string>();
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
                            string softPath = key.OpenSubKey(subKeyName)?.GetValue("")?.ToString();
                            if (softPath != null)
                                soft.Add($"{subKeyName}");
                        }
                        catch { }
                    }
                }
            }
            return soft;
        }

        public String BytesToString(long byteCount)
        {
            string[] suf = { " B", " KB", " MB", " GB", " TB", " PB", " EB" };
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

        public void AddToFinal(string file, Label FinalLabelXAML)
        {
            FileInfo lng = new FileInfo(file);
            try
            {
                fullSize += lng.Length;
            }
            catch { }
            FinalLabelXAML.Content = $"Final: {BytesToString(fullSize)}";
        }

        public void RemoveFromFinal(string file, Label FinalLabelXAML, Dispatcher d)
        {
            d.Invoke(() =>
            {
                FileInfo lng = new FileInfo(file);
                fullSize -= lng.Length;
                FinalLabelXAML.Content = $"Final: {BytesToString(fullSize)}";
            });
        }

        public void ResetFinal(Label FinalLabelXAML)
        {
                fullSize = 0;
                FinalLabelXAML.Content = $"Final: {BytesToString(fullSize)}";
        }

        public string outError(Exception ex)
        {
            if (ex is DirectoryNotFoundException || ex is FileNotFoundException) return "[Not Found]";
            else if (ex is UnauthorizedAccessException) return "[Access Denied]";
            else return $"[Error] {ex.Message}";
        }
    }
}
