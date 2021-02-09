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
        public string dirScriptsPath = Settings.Default.ScriptsDirectory;
        public string dirTemplatesPath = Settings.Default.TemplatesDirectory;
        public string[] audioExtensions = new[] { ".mp3", ".wav", ".wma", ".m4a", ".flac" };
        public string scriptExtension = ".xaml";
        readonly string dflt = "Choose a Folder...";


        public MainWindow()
        {
            InitializeComponent();
            CheckDirectories();
            CleanUpScripts();
            DisplayDirectories();

            // init the list of tab items
            _tabItems = new List<TabItem>();

            // make the first tab item

            _tabAdd = new TabItem();
            _tabAdd.Header = "+";

            Frame addTab = new Frame();
            addTab.Source = new Uri("/TabAdder.xaml", UriKind.Relative);
            _tabAdd.Content = addTab;
            _tabItems.Add(_tabAdd);
            this.AddTabItem(true);

            DynamicTabs.DataContext = _tabItems;
            DynamicTabs.SelectedIndex = 0;

            Trace.WriteLine("Started Running");
        }

        void CheckDirectories()
        {
            if (dirClipsPath == dflt || dirScriptsPath == dflt || dirSoundersPath == dflt || dirTemplatesPath == dflt)
            {
                DirConfig dlg = new DirConfig();
                dlg.ShowDialog();
                DisplayDirectories();

            }
            else
            {
                MonitorDirectory(dirClipsPath);
                MonitorDirectory(dirSoundersPath);
                MonitorDirectory(dirScriptsPath);
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
                    MessageBox.Show("File Not Ready, Try Again in a Sec...");
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
                    passer.SetData("NBfile", nbC);
                    DragDrop.DoDragDrop(lstbx, passer, DragDropEffects.Copy);
                }
                else
                {
                    MessageBox.Show("File Not Ready, Try Again in a Sec...");
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
            dirTemplatesPath = Settings.Default.TemplatesDirectory;

            if (Directory.Exists(dirClipsPath) && Directory.Exists(dirSoundersPath) && Directory.Exists(dirScriptsPath) && Directory.Exists(dirTemplatesPath))
            {

                object[] AllClips = new DirectoryInfo(dirClipsPath).GetFiles()
                .Where(cf => audioExtensions.Contains(cf.Extension.ToLower()))
                .ToArray();

                object[] AllSounders = new DirectoryInfo(dirSoundersPath).GetFiles()
                .Where(sf => audioExtensions.Contains(sf.Extension.ToLower()))
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
                        NBName = System.IO.Path.GetFileNameWithoutExtension(s.ToString())
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
                MessageBox.Show("Whoops. Something ain't right. Go to 'Edit > Settings > NewsJock Settings' and check the paths to your files!", "Directories Don't Exist", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        void MonitorDirectory(string dirPath)
        {
            Trace.WriteLine("Monitoring Started");
            fs = new FileSystemWatcher(dirPath, "*.*");

            fs.EnableRaisingEvents = true;
            fs.IncludeSubdirectories = true;

            fs.Created += new FileSystemEventHandler(ReloadDir);
            fs.Changed += new FileSystemEventHandler(ReloadDir);
            fs.Renamed += new RenamedEventHandler(ReloadDir);
            fs.Deleted += new FileSystemEventHandler(ReloadDir);
        }


        void ReloadDir(Object sender, FileSystemEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                DisplayDirectories();


            });
            Trace.WriteLine("Reload called. Change detected");

        }

        void CleanUpScripts()
        {
            if (Settings.Default.CleanUpToggle)
            {
                Trace.WriteLine("cleaning up scripts");
                FileInfo[] allScripts = new DirectoryInfo(dirScriptsPath).GetFiles(
                    "*.xaml",SearchOption.TopDirectoryOnly);
                List<FileInfo> oldScripts = new List<FileInfo>();

                foreach(var f in allScripts)
                {
                    if ((DateTime.Today - f.LastAccessTime).TotalDays > Settings.Default.CleanUpDays)
                    {
                        Trace.WriteLine("Gonna get rid of file: " + f.Name);
                        oldScripts.Add(f);
                    }
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
                        } catch
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
            tab.Name = string.Format("Script{0}", count);
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
                else if (MessageBox.Show(string.Format("Are you sure you want to remove '{0}'?", tab.Header.ToString()),
                  "Remove Tab", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
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

        DispatcherTimer sounderTimer;
        DispatcherTimer clipTimer;
        private void TimerSounders(object sender, EventArgs e)
        {
            sounderTimer = new DispatcherTimer();
            sounderTimer.Tick += new EventHandler(sndrTick);
            sounderTimer.Interval = TimeSpan.FromMilliseconds(250);
            sounderTimer.Start();
        }

        private void sndrTick(object sender, EventArgs e)
        {
            if (SoundersPlayer.NaturalDuration.HasTimeSpan)
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
            }

        }

        private void TimerClips(object sender, EventArgs e)
        {
            clipTimer = new DispatcherTimer();
            clipTimer.Tick += new EventHandler(clipTick);
            clipTimer.Interval = TimeSpan.FromMilliseconds(250);
            clipTimer.Start();
        }

        private void clipTick(object sender, EventArgs e)
        {
            if (ClipsPlayer.NaturalDuration.HasTimeSpan)
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
            ClipsPlayer.Stop();
            ClipsPlayer.Source = null;
        }

        private void btnStopSounders_Click(object sender, RoutedEventArgs e)
        {
            SoundersPlayer.Stop();
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



    }



}
