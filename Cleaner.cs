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
    internal class Cleaner
    {


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
    }
}
