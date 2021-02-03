using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace NewsBuddy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        FileSystemWatcher fs;
        // these need to go into a user preferences thingy
        public string dirClipsPath = "C:/NBtest/Clips";
        public string dirSoundersPath = "C:/NBtest/Sounders";
        public string[] audioExtensions = new[] { ".mp3", ".wav", ".wma", ".m4a", ".flac"};

        

        public MainWindow()
        {
            InitializeComponent();
            DisplayDirectories();
            MonitorDirectory("C:/NBtest");

            // init the list of tab items
            _tabItems = new List<TabItem>();

            // make the first tab item

            _tabAdd = new TabItem();
            _tabAdd.Header = "+";
            _tabItems.Add(_tabAdd);

            this.AddTabItem();

            DynamicTabs.DataContext = _tabItems;
            DynamicTabs.SelectedIndex = 0;

            Trace.WriteLine("Started Running");
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
                } else {
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
                } else
                {
                    MessageBox.Show("File Not Ready, Try Again in a Sec...");
                }
            }
        }

        #endregion

        #region Directory Controls

        List<NBfile> sounders = new List<NBfile>();
        List<NBfile> clips = new List<NBfile>();

        public void DisplayDirectories()
        {
            listSounders.ItemsSource = null;
            listClips.ItemsSource = null;
            sounders.Clear();
            clips.Clear();

            object[] AllClips = new DirectoryInfo(dirClipsPath).GetFiles()
                .Where(cf => audioExtensions.Contains(cf.Extension.ToLower()))
                .ToArray();

            object[] AllSounders = new DirectoryInfo(dirSoundersPath).GetFiles()
                .Where(sf => audioExtensions.Contains(sf.Extension.ToLower()))
                .ToArray();

            foreach (object c in AllClips)
            {
                NBfile newFile = new NBfile
                {
                    NBPath = c.ToString(),
                    NBName = System.IO.Path.GetFileName(c.ToString())
                };
                clips.Add(newFile);
            }
            foreach (object s in AllSounders)
            {
                NBfile newFile = new NBfile
                {
                    NBPath = s.ToString(),
                    NBName = System.IO.Path.GetFileName(s.ToString())
                };
                sounders.Add(newFile);
            }

            listSounders.ItemsSource = sounders;
            listClips.ItemsSource = clips;
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
            Trace.WriteLine("Reload called. CHange detected");           

        }

        #endregion

        #region Tab Controls

        private List<TabItem> _tabItems;
        TabItem _tabAdd;

        private TabItem AddTabItem()
        {
            int count = _tabItems.Count;
            TabItem tab = new TabItem();

            tab.Header = string.Format("Script {0}", count);
            tab.Name = string.Format("Script{0}", count);
            tab.HeaderTemplate = DynamicTabs.FindResource("TabHeader") as DataTemplate;

            Frame newContent = new Frame();
            newContent.Name = "txt";
            newContent.Source = new Uri("/EmptyScript.xaml", UriKind.Relative);
            tab.Content = newContent;

            _tabItems.Insert(count - 1, tab);

            return tab;
        }

        private void DynamicTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabItem tab = DynamicTabs.SelectedItem as TabItem;
            if (tab != null && tab.Header != null)
            {
                if (tab.Header.Equals(_tabAdd.Header)) 
                {
                    DynamicTabs.DataContext = null;
                    TabItem newTab = this.AddTabItem();
                    DynamicTabs.DataContext = _tabItems;
                    DynamicTabs.SelectedItem = newTab;
                }
            }
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
                    MessageBox.Show("Cannot remove this tab, sorry.","Cannot Remove");
                } else if (MessageBox.Show(string.Format("Are you sure you want to remove '{0}'?",tab.Header.ToString()),
                    "Remove Tab", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    TabItem selectedTab = DynamicTabs.SelectedItem as TabItem;
                    DynamicTabs.DataContext = null;
                    _tabItems.Remove(tab);
                    DynamicTabs.DataContext = _tabItems;

                    if (selectedTab == null || selectedTab.Equals(tab))
                    {
                        selectedTab = _tabItems[1];
                    }

                }
          
            }

           


        }


        #endregion

        private void sVolSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SoundersPlayer.Volume = (double)sVolSlider.Value;
        }

        private void cVolSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ClipsPlayer.Volume = (double)cVolSlider.Value;
        }
    }

}
