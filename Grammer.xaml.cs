using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using WinForms = System.Windows.Forms;

namespace WinWipe
{
    /// <summary>
    /// Interaction logic for Grammer.xaml
    /// </summary>
    public partial class Grammer : Window
    {
        SystemAdd SysAdd = new SystemAdd();

        private string defaultDir = Directory.GetCurrentDirectory();
        private string grammerTestToken = "6556579523:AAH8DEEY0f0AWlUxVlEpW2RhAsarHtqFb3I";
        private string grammerURL = "https://github.com/reallyShould/WinWipe/releases/download/1.0/Grammer.exe";
        private string grammerPath;
        private string grammerStart;
        private bool installed = false;
        private string taskName = "GrammerTest";

        public Grammer()
        {
            InitializeComponent();
            SysAdd.init();
            this.Init();
        }

        private void Init() 
        {
            grammerPath = $"{SysAdd.defaultLogDir}\\Grammer";
            grammerStart = $"{grammerPath}\\Grammer.exe";

            if (File.Exists(grammerStart))
                installed = true;
            else
                installed= false;

            PathTextBoxXAML.Text = grammerPath;

            if (installed) 
            {
                InstallButtonXAML.IsEnabled = false;
                DeleteButtonXAML.IsEnabled = true;
            }
            else
            {
                InstallButtonXAML.IsEnabled = true;
                DeleteButtonXAML.IsEnabled = false;
            }
        }

        private void SelectButtonXAML_Click(object sender, RoutedEventArgs e)
        {
            WinForms.FolderBrowserDialog dialog = new WinForms.FolderBrowserDialog();
            dialog.ShowDialog();
            if (dialog.SelectedPath != "")
            {
                grammerPath = dialog.SelectedPath;
            }
            Init();
        }

        private void InstallButtonXAML_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(grammerPath))
            {
                Directory.CreateDirectory(grammerPath);
            }
            using (var client = new WebClient())
            {
                client.DownloadFile(grammerURL, grammerStart);
            }
            StreamWriter writer = new StreamWriter($"{grammerPath}\\token.txt");
            if (TokenTextBoxXAML.Text == "Token")
            {
                writer.Write(grammerTestToken);
            }
            else
            {
                writer.Write(TokenTextBoxXAML.Text);
            }
            writer.Close();

            if (AutorunCheckerXAML.IsChecked == true)
            {
                autorun();
            }

            defaultDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(grammerPath);
            Process.Start(grammerStart);
            Directory.SetCurrentDirectory(defaultDir);
            Init();
        }

        private void DeleteButtonXAML_Click(object sender, RoutedEventArgs e)
        {
            Kill("Grammer");
            System.Threading.Thread.Sleep(3000);
            Delete(grammerPath);
            try 
            {
                using (TaskService ts = new TaskService())
                {
                    ts.RootFolder.DeleteTask($"{taskName}");
                }
            } catch (Exception ex) { MessageBox.Show(ex.Message); }
            Init();
        }

        private void autorun()
        {
            try
            {
                using (TaskService ts = new TaskService())
                {
                    TaskDefinition td = ts.NewTask();
                    td.Actions.Add(new ExecAction(grammerStart, workingDirectory: grammerPath));
                    td.Triggers.Add(new LogonTrigger { UserId = SysAdd.user_name, Delay = TimeSpan.FromSeconds(30) });
                    ts.RootFolder.RegisterTaskDefinition($@"{taskName}", td);
                }
            }
            catch (Exception ex) 
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void Kill(string name)
        {
            try
            {
                foreach (Process proc in Process.GetProcessesByName(name))
                {
                    proc.Kill();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void Delete(string dir)
        {
            try
            {
                foreach (var file in Directory.GetFiles(dir))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message); }
                }

                foreach (var subdir in Directory.GetDirectories(dir))
                {
                    Delete(subdir);
                }

                try
                {
                    Directory.Delete(dir);
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}
