using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Resources;
using System.Diagnostics;
using Ookii.Dialogs.Wpf;

namespace NewsBuddy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        FileSystemWatcher fs;

        public string dirClipsPath = Settings.Default.ClipsDirectory;
        public string dirSoundersPath = Settings.Default.SoundersDirectory;
        public string dirSharePath = Settings.Default.SharedDirectory;
        public string dirScriptsPath = Settings.Default.ScriptsDirectory;
        public string dirTemplatesPath = Settings.Default.TemplatesDirectory;
        public string[] audioExtensions = new[] { ".mp3", ".wav", ".wma", ".m4a", ".flac" };
        public string scriptExtension = ".xaml";
        NJWebBrowser browser;
        bool flashBox = true;
        public MainWindow()
        {
            InitializeComponent();

            // Debugger messages


            if (!Debugger.IsAttached)
            {
                AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
                    ExceptionCatcher(args.ExceptionObject as Exception, "AppDomain.CurrentDomain.UnhandledException", false);
                TaskScheduler.UnobservedTaskException += (sender, args) =>
                    ExceptionCatcher(args.Exception, "TaskScheduler.UnobservedTaskException", false);
                Dispatcher.UnhandledException += (sender, args) =>
                    ExceptionCatcher(args.Exception, "Dispatcher.UnhandledException", true);
            }




            this.Height = Settings.Default.WindowHeight;
            this.Width = Settings.Default.WindowWidth;



            CheckDirectories();
            CleanUpScripts();

            // init the list of tab items
            _tabItems = new List<TabItem>();

            // make the first tab item

            _tabAdd = new TabItem();
            _tabAdd.FontFamily = Application.Current.FindResource("FA") as FontFamily;
            _tabAdd.Header = "";

            Frame addTab = new Frame();
            addTab.Source = new Uri("/TabAdder.xaml", UriKind.Relative);
            _tabAdd.Content = addTab;
            _tabItems.Add(_tabAdd);
            this.AddTabItem(true);

            DynamicTabs.DataContext = _tabItems;
            DynamicTabs.SelectedIndex = 0;

            SoundersPlayer.BeginInit();
            ClipsPlayer.BeginInit();


            Trace.WriteLine("Started Running");
        }

        void ExceptionCatcher(Exception e, string exceptionType, bool promptForShutdown)
        {
            ProblemWindow pw = new ProblemWindow(e, promptForShutdown);
            pw.Owner = this;
            if (!(bool)pw.ShowDialog())
            {
                MessageBox.Show("Good Luck.");
            }
        }

        private Cursor nbDropCur = null;
        protected override void OnGiveFeedback(GiveFeedbackEventArgs e)
        {
            try
            {
                if (e.Effects == DragDropEffects.Copy)
                {
                    if (nbDropCur == null)
                    {
                        Stream cursor = Application.GetResourceStream(new Uri("pack://application:,,,/resources/buttondrop.cur")).Stream;
                        nbDropCur = new Cursor(cursor);

                    }
                    Mouse.SetCursor(nbDropCur);
                    e.UseDefaultCursors = false;
                    e.Handled = true;
                }
                else
                {
                    base.OnGiveFeedback(e);
                }
            }
            catch
            {
                base.OnGiveFeedback(e);
            }


        }

        void CheckDirectories()
        {
            if (!Directory.Exists(dirClipsPath) || !Directory.Exists(dirScriptsPath) ||
                !Directory.Exists(dirSoundersPath) || !Directory.Exists(dirTemplatesPath)
                || !Directory.Exists(dirSharePath))
            {
                flashBox = false;
                DirConfig dlg = new DirConfig();
                //dlg.Owner = this;
                if ((bool)dlg.ShowDialog())
                {
                    
                    DisplayDirectories();
                    MonitorDirectory(dirClipsPath);
                    MonitorDirectory(dirSoundersPath);
                    MonitorDirectory(dirScriptsPath);
                    MonitorDirectory(dirSharePath);
                   
                }
                else
                {
                    
                    MessageBox.Show("Error configuring directories. Try launching NewsJock again.");
                }


            }
            else
            {
                DisplayDirectories();
                MonitorDirectory(dirClipsPath);
                MonitorDirectory(dirSoundersPath);
                MonitorDirectory(dirScriptsPath);
                MonitorDirectory(dirSharePath);
            }
        }



        #region Drag&Drop Controls

        private void listSounders_MouseMove(object sender, MouseEventArgs e)
        {
            ListBox sdrBx = sender as ListBox;
            if (sdrBx != null && e.LeftButton == MouseButtonState.Pressed)
            {
                DataObject passer = new DataObject();
                NBfile nbS = new NBfile();
                nbS = listSounders.SelectedItem as NBfile;
                if (nbS != null)
                {
                    nbS.NBisSounder = true;
                    passer.SetData("NBfile", nbS);
                    DragDrop.DoDragDrop(sdrBx, passer, DragDropEffects.Copy);
                }
                else
                {
                    MessageBox.Show("Audio file still loading.\nTry again in a moment", "File Not Ready", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
        }

        private void listClips_MouseMove(object sender, MouseEventArgs e)
        {
            ListBox lstbx = sender as ListBox;
            if (lstbx != null && e.LeftButton == MouseButtonState.Pressed)
            {
                DataObject passer = new DataObject();
                NBfile nbC;// = new NBfile();
                nbC = listClips.SelectedItem as NBfile;
                if (nbC != null)
                {
                    nbC.NBisSounder = false;
                    passer.SetData("NBfile", nbC);
                    DragDrop.DoDragDrop(lstbx, passer, DragDropEffects.Copy);
                }
                else
                {
                    MessageBox.Show("Audio file not ready.\nTry again in a moment.", "File Not Ready", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
        }

        #endregion

        #region Directory Controls

        List<NBfile> sounders = new List<NBfile>();
        List<NBfile> clips = new List<NBfile>();
        List<ScriptFile> scripts = new List<ScriptFile>();

        public void DisplayDirectories()
        {
            Settings.Default.Reload();

            listSounders.ItemsSource = null;
            listClips.ItemsSource = null;
            listScripts.ItemsSource = null;
            sounders.Clear();
            clips.Clear();
            scripts.Clear();

            dirClipsPath = Settings.Default.ClipsDirectory;
            dirSoundersPath = Settings.Default.SoundersDirectory;
            dirScriptsPath = Settings.Default.ScriptsDirectory;
            dirSharePath = Settings.Default.SharedDirectory;
            dirTemplatesPath = Settings.Default.TemplatesDirectory;

            if (Directory.Exists(dirClipsPath) && Directory.Exists(dirSoundersPath) && Directory.Exists(dirScriptsPath) && Directory.Exists(dirTemplatesPath))
            {

                object[] AllClips = new DirectoryInfo(dirClipsPath).GetFiles()
                .Where(cf => audioExtensions.Contains(cf.Extension.ToLower()))
                .ToArray();

                object[] AllSounders = new DirectoryInfo(dirSoundersPath).GetFiles()
                .Where(sf => audioExtensions.Contains(sf.Extension.ToLower()))
                .ToArray();

                object[] ShareSounders = new DirectoryInfo(dirSharePath).GetFiles()
                .Where(ssf => audioExtensions.Contains(ssf.Extension.ToLower()))
                .ToArray();

                object[] AllScripts = new DirectoryInfo(dirScriptsPath).GetFiles()
                .Where(sf => scriptExtension.Contains(sf.Extension.ToLower()))
                .ToArray();

                foreach (object c in AllClips)
                {
                    NBfile newFile = new NBfile
                    {
                        NBPath = c.ToString(),
                        NBName = System.IO.Path.GetFileNameWithoutExtension(c.ToString())
                    };

                    clips.Add(newFile);
                }
                foreach (object s in AllSounders)
                {
                    NBfile newFile = new NBfile
                    {
                        NBPath = s.ToString(),
                        NBName = System.IO.Path.GetFileNameWithoutExtension(s.ToString()),
                        NBisSounder = true
                    };

                    sounders.Add(newFile);
                }
                foreach (object Ss in ShareSounders)
                {
                    NBfile newFile = new NBfile
                    {
                        NBPath = Ss.ToString(),
                        NBName = System.IO.Path.GetFileNameWithoutExtension(Ss.ToString()),
                        NBisSounder = true
                    };

                    sounders.Add(newFile);
                }
                foreach (object sc in AllScripts)
                {
                    ScriptFile newScript = new ScriptFile
                    {
                        SCpath = sc.ToString(),
                        SCname = System.IO.Path.GetFileNameWithoutExtension(sc.ToString()),

                    };
                    Trace.WriteLine("Added Script: " + sc.ToString());
                    scripts.Add(newScript);
                }

                listSounders.ItemsSource = sounders;
                listClips.ItemsSource = clips;
                listScripts.ItemsSource = scripts;
            }
            else
            {
                MessageBox.Show("Whoops. Something ain't right. Click the gear on the top right of the main window, and check the paths to your files!", "Directories Don't Exist", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        void MonitorDirectory(string dirPath)
        {
            Trace.WriteLine("Monitoring Started for " + dirPath);
            fs = new FileSystemWatcher(dirPath, "*.*");

            fs.EnableRaisingEvents = true;
            fs.IncludeSubdirectories = true;

            //fs.Created += new FileSystemEventHandler(ReloadDir);
            fs.Changed += new FileSystemEventHandler(ReloadDir);
            fs.Renamed += new RenamedEventHandler(ReloadDir);
            //fs.Deleted += new FileSystemEventHandler(ReloadDir);
        }


        void ReloadDir(Object sender, FileSystemEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                DisplayDirectories();


            });
            Trace.WriteLine("Reload called. Change detected");

        }

        private long GetDirectorySize(string folderPath)
        {
            try
            {
                DirectoryInfo d = new DirectoryInfo(folderPath);
                return d.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly).Sum(fi => fi.Length);
            }
            catch
            {
                return 0;
            }
        }

        void CleanUpScripts()
        {
            if (flashBox)
            {
                MessageBox.Show("Loaded.", "Tidying Up");
            }
            
            
            FileInfo[] allScripts = new DirectoryInfo(dirScriptsPath).GetFiles(
            "*.xaml", SearchOption.AllDirectories);
            List<FileInfo> veryOldScripts = new List<FileInfo>();

            foreach (var af in allScripts)
            {
                if ((DateTime.Today - af.LastAccessTime).TotalDays > 730)
                {
                    veryOldScripts.Add(af);
                }
            }
            if (Settings.Default.WarnDirSize)
            {
                if (GetDirectorySize(dirClipsPath) > 1000000000)
                {
                    MessageBox.Show(this, "You have more than a gigabyte of clips in your Clips directory.\n" +
                       "To avoid taking up too much space, it may be wise to go delete old clips now.",
                       "Over 1Gb Clips", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                if (GetDirectorySize(dirSoundersPath) > 1000000000)
                {
                    MessageBox.Show(this, "You have more than a gigabyte of files in your Sounders directory.\n" +
                        "To avoid taking up too much space, it may be wise to go delete old Sounders now.",
                        "Over 1Gb Sounders", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                if (GetDirectorySize(dirSharePath) > 1000000000)
                {
                    MessageBox.Show(this, "You have more than a gigabyte of files in your Shared Sounders directory.\n" +
                        "To avoid taking up too much space, it may be wise to go delete old Shared Sounders now.",
                        "Over 1Gb Shared Sounders", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                if (GetDirectorySize(dirScriptsPath) > 1000000000)
                {
                    MessageBox.Show(this, "You have more than a gigabyte of files in your Shared Sounders directory.\n" +
                        "To avoid taking up too much space, it may be wise to go delete old Shared Sounders now.",
                        "Over 1Gb Shared Sounders", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            


            if (veryOldScripts.Count > 0)
            {
                MessageBoxResult result = MessageBox.Show("You have scripts in your drive that are over two years old. To avoid taking up too much space, would you like me to delete them?", "Wicked Old Scripts Detected", MessageBoxButton.YesNo, MessageBoxImage.Question);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        for (int it = 0; it < veryOldScripts.Count; it++)
                        {

                            FileInfo vosc = veryOldScripts[it];
                            Trace.WriteLine("got rid of " + vosc.Name);
                            try
                            {
                                File.Delete(vosc.FullName);
                            }
                            catch
                            {
                                Trace.WriteLine("failed to remove old script " + vosc.Name);
                            }
                        }

                        break;
                    case MessageBoxResult.No:
                        break;
                    case MessageBoxResult.None:
                        break;
                }

            }


            if (Settings.Default.CleanUpToggle)
            {
                Trace.WriteLine("cleaning up scripts");
                FileInfo[] allTLScripts = new DirectoryInfo(dirScriptsPath).GetFiles(
                    "*.xaml", SearchOption.TopDirectoryOnly);
                List<FileInfo> oldScripts = new List<FileInfo>();


                foreach (var f in allTLScripts)
                {
                    if ((DateTime.Today - f.LastAccessTime).TotalDays > Settings.Default.CleanUpDays)
                    {
                        Trace.WriteLine("Gonna get rid of file: " + f.Name);
                        oldScripts.Add(f);
                    }
                    else { return; }
                }

                if (oldScripts.Count > 0)
                {

                    string destPath = System.IO.Path.Combine(dirScriptsPath, "Old Scripts - " + DateTime.Now.ToString("MMM"));
                    Directory.CreateDirectory(destPath);


                    for (int i = 0; i < oldScripts.Count; i++)
                    {

                        FileInfo sc = oldScripts[i];
                        string newPath = System.IO.Path.Combine(destPath, sc.Name);
                        Trace.WriteLine("got rid of " + sc.Name);
                        try
                        {
                            File.Move(sc.FullName, newPath);
                        }
                        catch
                        {
                            Trace.WriteLine("failed to remove old script " + sc.Name);
                        }


                    }

                } 
            }
        }


        #endregion

        #region Tab Controls

        private List<TabItem> _tabItems;
        TabItem _tabAdd;

        private readonly Random _random = new Random();
        private string RandomID()
        {
            var builder = new StringBuilder(10);
            char offset = 'a';
            const int letters = 26;
            for (var i = 0; i < 10; i++)
            {
                var @char = (char)_random.Next(offset, offset + letters);
                builder.Append(@char);
            }
            return builder.ToString();
        }
        private TabItem AddTabItem(bool isDefault, string uri = "/EmptyScript.xaml")
        {
            int count = _tabItems.Count;
            string tabName = String.Format("Script {0}", _tabItems.Count);

            if (File.Exists(uri) && !isDefault)
            {
                tabName = System.IO.Path.GetFileNameWithoutExtension(uri);
            }

            TabItem tab = new TabItem();

            tab.Header = tabName;

            tab.Name = RandomID() + _tabItems.Count.ToString();
            tab.HeaderTemplate = DynamicTabs.FindResource("TabHeader") as DataTemplate;

            Frame newContent = new Frame();

            if (File.Exists(uri) && !isDefault)
            {
                newContent.NavigationService.Navigate(new Page1(true, uri));
            }
            else
            {
                newContent.NavigationService.Navigate(new Page1(false));
            }

            tab.Content = newContent;


            _tabItems.Insert(count - 1, tab);

            return tab;
        }


        public void AddNewTabFromFrame(string uri)
        {
            DynamicTabs.DataContext = null;
            TabItem newTab = this.AddTabItem(false, uri);
            DynamicTabs.SelectedItem = newTab;
            DynamicTabs.DataContext = _tabItems;
        }

        private void btnDelTab_Click(object sender, RoutedEventArgs e)
        {
            string tabName = (sender as Button).CommandParameter.ToString();

            var item = DynamicTabs.Items.Cast<TabItem>().Where(i => i.Name.Equals(tabName)).SingleOrDefault();

            TabItem tab = item as TabItem;

            if (tab != null)
            {
                if (_tabItems.Count < 3)
                {
                    MessageBox.Show("Cannot remove this tab, sorry.", "Cannot Remove");
                }
                else if (MessageBox.Show(string.Format("Are you sure you want to close '{0}'?", tab.Header.ToString()),
                  "Close Tab", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    TabItem selectedTab = DynamicTabs.SelectedItem as TabItem;
                    DynamicTabs.DataContext = null;
                    _tabItems.Remove(tab);


                    if (selectedTab == null || selectedTab.Equals(tab))
                    {
                        DynamicTabs.SelectedItem = _tabItems[0];
                    }

                    DynamicTabs.DataContext = _tabItems;

                }

            }


        }

        public void ChangeTabName(string uri)
        {
            TabItem current = DynamicTabs.SelectedItem as TabItem;

            if (current != null)
            {
                DynamicTabs.DataContext = null;
                current.Header = System.IO.Path.GetFileNameWithoutExtension(uri);
                DynamicTabs.DataContext = _tabItems;
                try
                {
                    DynamicTabs.SelectedItem = current;
                }
                catch
                {
                    DynamicTabs.SelectedItem = _tabItems[0];
                }
            }

        }


        #endregion

        #region Menu Controls
        private void sVolSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SoundersPlayer.Volume = (double)sVolSlider.Value;
        }

        private void cVolSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ClipsPlayer.Volume = (double)cVolSlider.Value;
        }



        private void mnNJSettings_Click(object sender, RoutedEventArgs e)
        {
            DirConfig dlf = new DirConfig();
            dlf.ShowDialog();
            DisplayDirectories();
        }

        private void mnResetSettings_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Reset();
            DisplayDirectories();
        }

        private void mnWebSettings_Click(object sender, RoutedEventArgs e)
        {
        }



        #endregion

        private void mnSilence_Click(object sender, RoutedEventArgs e)
        {
            SoundersPlayer.Stop();
            SoundersPlayer.Source = null;
            ClipsPlayer.Stop();
            ClipsPlayer.Source = null;
        }


        private void lblSounders_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Process.Start("explorer.exe", Settings.Default.SoundersDirectory);
        }

        private void lblClips_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Process.Start("explorer.exe", Settings.Default.ClipsDirectory);
        }

        private void lblScripts_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Process.Start("explorer.exe", Settings.Default.ScriptsDirectory);
        }

        DispatcherTimer sounderTimer;
        DispatcherTimer clipTimer;
        private void TimerSounders(object sender, EventArgs e)
        {
            
            if (SoundersPlayer.Source != null && SoundersPlayer.NaturalDuration.HasTimeSpan)
            {
                sounderTimer = new DispatcherTimer();
                sounderTimer.Tick += new EventHandler(sndrTick);
                sounderTimer.Interval = TimeSpan.FromMilliseconds(250);
                sounderTimer.Start();
            }
            else
            {
                Trace.WriteLine("Sounders Player Not PLaying for some reason");
            }

        }

        private void sndrTick(object sender, EventArgs e)
        {
            try
            {
                if (SoundersPlayer.NaturalDuration.HasTimeSpan && SoundersPlayer.Position != SoundersPlayer.NaturalDuration.TimeSpan && SoundersPlayer.Source != null)
                {
                    int sndDur = (int)Math.Ceiling(SoundersPlayer.NaturalDuration.TimeSpan.TotalSeconds);
                    int sndPos = (int)Math.Ceiling(SoundersPlayer.Position.TotalSeconds);
                    string sndRem = ((sndDur - sndPos) / 60).ToString() + ":" + ((sndDur - sndPos) % 60).ToString("00");

                    sndrTimeLeft.Text = sndRem;
                    lblSounders.Content = System.IO.Path.GetFileNameWithoutExtension(SoundersPlayer.Source.LocalPath);
                    SoundersControl.Visibility = Visibility.Visible;
                }
                else
                {
                    sounderTimer.Stop();
                    SoundersControl.Visibility = Visibility.Collapsed;
                    lblSounders.Content = "Sounders";
                    sndrTimeLeft.Text = "0:00";
                    SoundersPlayer.Source = null;
                }
            }
            catch
            {
                sounderTimer.Stop();
                SoundersControl.Visibility = Visibility.Collapsed;
                lblSounders.Content = "Sounders";
                sndrTimeLeft.Text = "0:00";
                
                SoundersPlayer.Source = null;
                Trace.WriteLine("Sounders Player Display Failed.");
            }
           

        }

        private void TimerClips(object sender, EventArgs e)
        {
            
            if (ClipsPlayer.Source != null && ClipsPlayer.NaturalDuration.HasTimeSpan)
            {
                clipTimer = new DispatcherTimer();
                clipTimer.Tick += new EventHandler(clipTick);
                clipTimer.Interval = TimeSpan.FromMilliseconds(250);
                clipTimer.Start();
            }

        }

        private void clipTick(object sender, EventArgs e)
        {
            try
            {
                if (ClipsPlayer.NaturalDuration.HasTimeSpan && ClipsPlayer.Position != ClipsPlayer.NaturalDuration.TimeSpan && ClipsPlayer.Source != null)
                {
                    int clpDur = (int)Math.Ceiling(ClipsPlayer.NaturalDuration.TimeSpan.TotalSeconds);
                    int clpPos = (int)Math.Ceiling(ClipsPlayer.Position.TotalSeconds);
                    string clpRem = ((clpDur - clpPos) / 60).ToString() + ":" + ((clpDur - clpPos) % 60).ToString("00");

                    lblClips.Content = System.IO.Path.GetFileNameWithoutExtension(ClipsPlayer.Source.LocalPath);
                    ClipsControl.Visibility = Visibility.Visible;

                    clipTimeLeft.Text = clpRem;
                }
                else
                {
                    clipTimer.Stop();
                    ClipsControl.Visibility = Visibility.Collapsed;
                    lblClips.Content = "Clips";
                    clipTimeLeft.Text = "0:00";
                    ClipsPlayer.Source = null;
                }
            }
            catch
            {
                clipTimer.Stop();
                ClipsControl.Visibility = Visibility.Collapsed;
                lblClips.Content = "Clips";
                clipTimeLeft.Text = "0:00";
                ClipsPlayer.Source = null;
                Trace.WriteLine("Clips Timer Display Failed");
            }
           
        }

        private void listScripts_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ScriptFile chosen = listScripts.SelectedItem as ScriptFile;
            if (chosen != null)
            {

                this.Dispatcher.Invoke(() =>
                {
                    AddNewTabFromFrame(chosen.SCpath);
                });

                listScripts.SelectedItem = null;
            }
        }

        private void btnStopClips_Click(object sender, RoutedEventArgs e)
        {
            //ClipsPlayer.Stop();
            ClipsPlayer.Source = null;
        }

        private void btnStopSounders_Click(object sender, RoutedEventArgs e)
        {
            //SoundersPlayer.Stop();
            SoundersPlayer.Source = null;
        }



        private void HandlePreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }

        private void expandVisible_Click(object sender, RoutedEventArgs e)
        {
            var bt = sender as System.Windows.Controls.Primitives.ToggleButton;
            if (bt != null)
            {
                if (bt.IsChecked == true)
                {
                    bt.Content = " Hide";
                }
                else
                {
                    bt.Content = " Show";
                }
            }

        }

        private void mnRefresh_Click(object sender, RoutedEventArgs e)
        {
            CheckDirectories();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Settings.Default.WindowHeight = this.Height;
            Settings.Default.WindowWidth = this.Width;
            Settings.Default.Save();
        }

        private void Browser_Click(object sender, RoutedEventArgs e)
        {
            if (browser != null)
            {
                try
                {
                    browser.Focus();
                }
                catch
                {
                    MessageBox.Show("Browser Unresponsive");
                }
            }
            else
            {
                browser = new NJWebBrowser();
                browser.Show();
            }

        }

        private void SoundersPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Trace.WriteLine("Sounders Media Failed: " + e.ErrorException.Message);
            if (SoundersPlayer.Source != null)
            {
                
                SoundersPlayer.Source = null;
            }
        }

        private void ClipsPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Trace.WriteLine("Clips Media Failed: " + e.ErrorException.Message);
            if (ClipsPlayer.Source != null)
            {
                ClipsPlayer.Source = null;
            }
        }
    }

}
