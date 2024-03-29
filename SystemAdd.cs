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
using System.Windows.Threading;
using WinForms = System.Windows.Forms;

namespace WinWipe
{
    internal class SystemAdd
    {
        // Variables
        public StringBuilder log = new StringBuilder();

        // Actions
        public void AddLog(string message, TextBox LogsTextBoxXAML, ScrollViewer LogScrollXAML)
        {
            if (LogsTextBoxXAML.Text.Length > 10000)
            {
                LogsTextBoxXAML.Clear();
            }

            if (string.IsNullOrEmpty(message))
            {
                LogsTextBoxXAML.Clear();
            }
            else if (message == "sep")
            {
                AddLog("============", LogsTextBoxXAML, LogScrollXAML);
            }
            else
            {
                LogsTextBoxXAML.AppendText($"{message}\n");
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
    }
}
