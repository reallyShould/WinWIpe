using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace WinWipe
{
    internal class Cleaner
    {
        SystemAdd SysAdd = new SystemAdd();

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
            browserCache = new Dictionary<string, string>()
            {
                { "chrome.exe", $"C:\\Users\\{SysAdd.user_name}\\AppData\\Local\\Google\\Chrome\\User Data\\Default\\Cache" },
                { "firefox.exe", GetFirefoxCache(SysAdd.user_name) },
                { "opera.exe", $"C:\\Users\\{SysAdd.user_name}\\AppData\\Local\\Opera Software\\Opera Stable\\Cache" },
                { "yandex.exe", $"C:\\Users\\{SysAdd.user_name}\\AppData\\Local\\Yandex\\YandexBrowser\\User Data\\Default\\Cache" }
            };
            SysAdd.init();
        }

        public string GetFirefoxCache(string username)
        {
            string path = $"C:\\Users\\{username}\\AppData\\Local\\Mozilla\\Firefox\\Profiles";
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
                    }
                }
                return output;
            }
            else
            {
                return "NONE";
            }
        }

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
                            SysAdd.AddLog($"[OK] File {file} deleted", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher);
                        }
                        catch (Exception ex)
                        {
                            SysAdd.RemoveFromFinal(file, FinalLabelXAML, d);
                            if (ex is DirectoryNotFoundException)
                            {
                                SysAdd.AddLog($"[Not Found] File \"{file}\" is not exist", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher);
                            }
                            else if (ex is UnauthorizedAccessException)
                            {
                                SysAdd.AddLog($"[Access Denied] File \"{file}\" access denied", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher);
                            }
                            else
                            {
                                SysAdd.AddLog($"[Error] {ex.Message}", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher);
                            }
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
                    SysAdd.AddLog($"[Error] {ex.Message}", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher);
                }
        }
            catch (Exception ex)
            {
                if (ex is DirectoryNotFoundException)
                {
                    SysAdd.AddLog($"[Not Found] Directory \"{dir}\" is not exist", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher);
                }
                else if (ex is UnauthorizedAccessException)
                {
                    SysAdd.AddLog($"[Access Denied] Directory \"{dir}\" access denied", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher);
                }
                else
                {
                    SysAdd.AddLog($"[Error] {ex.Message}", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher);
                }
            }
        }

        public async void CleanTemporary(TextBox LogsTextBoxXAML, ScrollViewer LogScrollXAML, Label FinalLabelXAML, Dispatcher d)
        {
            SysAdd.AddLog($"\t\tCleaning temporary files", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher);
            try
            {
                await Task.Run(() => FullRemove($"C:\\Users\\{SysAdd.user_name}\\AppData\\Local\\Temp", LogsTextBoxXAML, LogScrollXAML, FinalLabelXAML, d));
                await Task.Run(() => FullRemove($"C:\\Windows\\Temp", LogsTextBoxXAML, LogScrollXAML, FinalLabelXAML, d));

            }
            catch (Exception err)
            {
                SysAdd.AddLog($"[Error] {err}", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher);
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
            SysAdd.AddLog($"\t\tCleaning downloads ", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher);
            foreach (var file in Directory.GetFiles($"C:\\Users\\{SysAdd.user_name}\\Downloads"))
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
                            SysAdd.AddLog($"[Error] {ex.Message}", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher);
                        }
                    }
                }
            }
            SysAdd.AddLog("[OK] Done", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher);
        }

        public void CleanCustomFolder(string customFolder, TextBox LogsTextBoxXAML, ScrollViewer LogScrollXAML, Label FinalLabelXAML, Dispatcher d)
        {
            SysAdd.AddLog($"\t\tCleaning custom folder", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher);
            try
            {
                FullRemove(customFolder, LogsTextBoxXAML, LogScrollXAML, FinalLabelXAML, d);
            }
            catch (Exception err) { SysAdd.AddLog($"[Error] {err}", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher); }
        }
    }
}
