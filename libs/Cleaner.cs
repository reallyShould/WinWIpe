using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace WinWipe
{
    internal class Cleaner
    {
        // internal classes
        SystemAdd SysAdd = new SystemAdd();

        // VARIABLES
        public Dictionary<string, string> browserCache = new Dictionary<string, string>();

        // FOR RECYCLE BIN
        enum RecycleFlags : uint
        {
            SHERB_NOCONFIRMATION = 0x00000001,
            SHERB_NOPROGRESSUI = 0x00000002,
            SHERB_NOSOUND = 0x00000004
        }
        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        static extern uint SHEmptyRecycleBin(IntPtr hwnd, string pszRootPath, RecycleFlags dwFlags);


        public void init()
        {
            SysAdd.init();
            browserCache = new Dictionary<string, string>()
            {
                { "chrome.exe", $"{SysAdd.activeDisk}:\\Users\\{SysAdd.user_name}\\AppData\\Local\\Google\\Chrome\\User Data\\Default\\Cache" },
                { "firefox.exe", GetFirefoxCache(SysAdd.user_name) },
                //{ "firefox.exe", $"{SysAdd.activeDisk}:\\Users\\{SysAdd.user_name}\\AppData\\Local\\Mozilla\\Firefox\\Profiles" },
                { "opera.exe", $"{SysAdd.activeDisk}:\\Users\\{SysAdd.user_name}\\AppData\\Local\\Opera Software\\Opera Stable\\Cache" },
                { "yandex.exe", $"{SysAdd.activeDisk}:\\Users\\{SysAdd.user_name}\\AppData\\Local\\Yandex\\YandexBrowser\\User Data\\Default\\Cache" }
            };
        }

        // return path to firefox cache
        public string GetFirefoxCache(string username)
        {
            string path = $"{SysAdd.activeDisk}:\\Users\\{username}\\AppData\\Local\\Mozilla\\Firefox\\Profiles";
            if (Directory.Exists(path))
            {
                var tmp = Directory.GetDirectories(path);
                string output = "NONE";
                foreach (var dir in tmp)
                {
                    var timeOfUsed = Directory.GetLastWriteTime(dir).Date;
                    DateTime timeNow = DateTime.Now.Date;
                    if (DateTime.Equals(timeNow, timeOfUsed))
                    {
                        output = dir + "\\cache2\\entries";
                        break;
                    }
                    else
                    {
                        output = "NONE";
                    }
                }
                return output;
            }
            else
            {
                return "NONE";
            }
        }

        // Remove folder with files
        public void FullRemove(string dir, TextBox LogsTextBoxXAML, ScrollViewer LogScrollXAML, Label FinalLabelXAML, Dispatcher d)
        {
            try
            {
                foreach (var file in Directory.GetFiles(dir))
                {
                    d.Invoke(() =>
                    {
                        try
                        {
                            SysAdd.AddToFinal(file, FinalLabelXAML);
                            File.Delete(file);
                            SysAdd.AddLog($"[OK] {file}", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher);
                        }
                        catch (Exception ex)
                        {
                            SysAdd.RemoveFromFinal(file, FinalLabelXAML, d);
                            SysAdd.AddLog($"{SysAdd.outError(ex)} {file}", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher);
                        }
                    });
                }

                foreach (var subdir in Directory.GetDirectories(dir))
                {
                    d.Invoke(() => FullRemove(subdir, LogsTextBoxXAML, LogScrollXAML, FinalLabelXAML, d));
                }

                try
                {
                    Directory.Delete(dir);
                    SysAdd.AddLog($"[OK] Dir {dir} deleted", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher);
                }
                catch (Exception ex)
                {
                    SysAdd.AddLog($"{SysAdd.outError(ex)} {dir}", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher);
                }
            }
            catch (Exception ex)
            {
                SysAdd.AddLog($"{SysAdd.outError(ex)} {dir}", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher);
            }
        }


        public async void CleanTemporary(TextBox LogsTextBoxXAML, ScrollViewer LogScrollXAML, Label FinalLabelXAML, Dispatcher d)
        {
            SysAdd.AddLog($"\t\tCleaning temporary files", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher);
            try
            {
                await Task.Run(() => FullRemove($"{SysAdd.activeDisk}:\\Users\\{SysAdd.user_name}\\AppData\\Local\\Temp", LogsTextBoxXAML, LogScrollXAML, FinalLabelXAML, d));
                await Task.Run(() => FullRemove($"{SysAdd.activeDisk}:\\Windows\\Temp", LogsTextBoxXAML, LogScrollXAML, FinalLabelXAML, d));

            }
            catch (Exception ex)
            {
                SysAdd.AddLog($"{SysAdd.outError(ex)} {ex.Message}", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher);
            }
        }


        public void CleanRecycleBin(TextBox LogsTextBoxXAML, ScrollViewer LogScrollXAML, Button CleanButtonXAML)
        {
            try
            {
                CleanButtonXAML.IsEnabled = false;
                uint result = SHEmptyRecycleBin(IntPtr.Zero, null, RecycleFlags.SHERB_NOCONFIRMATION | RecycleFlags.SHERB_NOPROGRESSUI | RecycleFlags.SHERB_NOSOUND);
                if (result != 0)
                {
                    SysAdd.AddLog("[Error] Recycle bin error", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher);
                }
                else
                {
                    SysAdd.AddLog("[OK] Recycle bin clean", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher);
                }
            }
            finally
            {
                CleanButtonXAML.IsEnabled = true;
            }
        }


        public async void CleanOldUpdates(TextBox LogsTextBoxXAML, ScrollViewer LogScrollXAML)
        {
            SysAdd.AddLog($"\t\tCleaning old updates", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher);
            try
            {
                Process proc = await Task.Run(() => Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = "rd /s /q c:windows.old",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }));
                SysAdd.AddLog($"[OK] Done", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher);
            }
            catch (Exception err) { SysAdd.AddLog($"[Error] {err}", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher); }
        }


        public async void CleanWebCache(TextBox LogsTextBoxXAML, ScrollViewer LogScrollXAML, Label FinalLabelXAML, Dispatcher d)
        {
            foreach (var browser in browserCache.Keys)
            {
                if (SysAdd.installedSoftware.Contains(browser))
                {
                    SysAdd.AddLog($"\t\tCleaning web cache", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher);
                    await Task.Run(() => FullRemove(browserCache[browser], LogsTextBoxXAML, LogScrollXAML, FinalLabelXAML, d));
                }
            }
        }


        public void CleanDownloads(List<string> item, TextBox LogsTextBoxXAML, ScrollViewer LogScrollXAML, Label FinalLabelXAML, Dispatcher d)
        {
            SysAdd.AddLog($"\t\tCleaning downloads", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher);
            foreach (var file in Directory.GetFiles($"{SysAdd.activeDisk}:\\Users\\{SysAdd.user_name}\\Downloads"))
            {
                foreach (var im in item)
                {
                    if (file.EndsWith(im))
                    {
                        try
                        {
                            SysAdd.AddToFinal(file, FinalLabelXAML);
                            File.Delete(file);
                            SysAdd.AddLog($"[OK] File {file} deleted", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher);
                        }
                        catch (Exception ex)
                        {
                            SysAdd.RemoveFromFinal(file, FinalLabelXAML, d);
                            SysAdd.AddLog($"{SysAdd.outError(ex)} {file}", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher);
                        }
                    }
                }
            }
        }


        public void CleanCustomFolder(string customFolder, TextBox LogsTextBoxXAML, ScrollViewer LogScrollXAML, Label FinalLabelXAML, Dispatcher d)
        {
            SysAdd.AddLog($"\t\tCleaning custom folder", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher);
            try
            {
                FullRemove(customFolder, LogsTextBoxXAML, LogScrollXAML, FinalLabelXAML, d);
            }
            catch (Exception ex) 
            { 
                SysAdd.AddLog($"{SysAdd.outError(ex)} {ex.Message}", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher); 
            }
        }

        public void CleanCrushDumps(TextBox LogsTextBoxXAML, ScrollViewer LogScrollXAML, Label FinalLabelXAML, Dispatcher d)
        {
            string CrushDumps = $"C:\\Users\\{SysAdd.user_name}\\AppData\\Local\\CrashDumps";
            SysAdd.AddLog($"\t\tCleaning CrushDumps folder", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher);
            try
            {
                if (Directory.Exists(CrushDumps))
                {
                    FullRemove(CrushDumps, LogsTextBoxXAML, LogScrollXAML, FinalLabelXAML, d);
                }
                else
                {
                    SysAdd.AddLog($"[Not Found] CrushDumps not exist", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher);
                }
            }
            catch (Exception ex)
            {
                SysAdd.AddLog($"{SysAdd.outError(ex)} {ex.Message}", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher);
            }
        }

        public void CleanSteamCache(TextBox LogsTextBoxXAML, ScrollViewer LogScrollXAML, Label FinalLabelXAML, Dispatcher d)
        {
            SysAdd.AddLog($"\t\tCleaning steam cache", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher);
            try
            {
                string SteamCache = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Valve\Steam", "InstallPath", null);
                SteamCache += "\\steam\\cached";
                FullRemove(SteamCache, LogsTextBoxXAML, LogScrollXAML, FinalLabelXAML, d);
            }
            catch (Exception ex)
            {
                SysAdd.AddLog($"{SysAdd.outError(ex)} {ex.Message}", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher);
            }
        }

        public void writeLogs()
        {
            if (SysAdd.log.ToString().Length > 1)
            {
                string newLog = $"{SysAdd.defaultLogDir}\\{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}_" +
                                    $"{DateTime.Now.Hour}-{DateTime.Now.Minute}-{DateTime.Now.Second}.log";

                using (StreamWriter sw = File.CreateText(newLog))
                {
                    sw.WriteLine(SysAdd.log.ToString());
                }
                SysAdd.log.Clear();
            }
        }
    }
}
