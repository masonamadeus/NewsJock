using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Threading;
using System.Text;

namespace NewsBuddy
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public Mutex mutex;

        public App()
        {
            SingleInstanceCheck();

            SplashScreen splash = new SplashScreen("resources/NewsJockSplashv2.png");
            splash.Show(true);
        }

        public void SingleInstanceCheck()
        {
            bool isOnlyInstance = false;
            mutex = new Mutex(true, @"NewsJock", out isOnlyInstance);
            if (!isOnlyInstance)
            {
                string filesToOpen = "";
                var args = Environment.GetCommandLineArgs();
                if (args != null && args.Length > 1)
                {
                    StringBuilder builder = new StringBuilder();
                    for (int i = 1; i < args.Length; i++)
                    {
                        builder.AppendLine(args[i]);
                    }
                    filesToOpen = builder.ToString();
                }

                var manager = new NamedPipeManager("NewsJock");
                manager.Write(filesToOpen);

                Environment.Exit(0);
            }
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            List<string> paths = new List<string>();
            bool fileFound = false;
            
            foreach (string arg in e.Args)
            {
                if (arg.EndsWith(".xaml") || arg.EndsWith(".njs"))
                {
                    string filePath = arg;
                    if (File.Exists(filePath))
                    {
                        paths.Add(filePath);
                    }
                }
            }

            if (paths.Count > 0)
            {
                fileFound = true;
            }

            MainWindow main = new MainWindow(fileFound);

            foreach (string pth in paths)
            {
                main.AddNewTabFromFrame(pth);
            }

            main.Show();

        }
    }

}
