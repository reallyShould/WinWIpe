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
using WinForms = System.Windows.Forms;

namespace WinWipe
{
    /// <summary>
    /// Interaction logic for Grammer.xaml
    /// </summary>
    public partial class Grammer : Window
    {
        SystemAdd SysAdd = new SystemAdd();

        private string grammerTestToken = "6556579523:AAH8DEEY0f0AWlUxVlEpW2RhAsarHtqFb3I";
        private string grammerURL = "https://github.com/reallyShould/WinWipe/releases/download/1.0/Grammer.exe";
        private string grammerPath;
        private string grammerStart;
        private bool installed = false;

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
            {
                installed = true;
            }
            PathTextBoxXAML.Text = grammerPath;
            if (installed)
            {
                InstallButtonXAML.IsEnabled = false;
            } 
            else
            {
                InstallButtonXAML.IsEnabled = true;
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
                writer.Write(TokenTextBoxXAML);
            }
            writer.Close();
            // add to autorun 

            Directory.SetCurrentDirectory(grammerPath);
            Process.Start(grammerStart);
            Init();
        }
    }
}
