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
        public StringBuilder log = new StringBuilder();

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
    }
}
