using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using NAudio.Wave;
using System.Linq;
using System.Diagnostics;

namespace NewsBuddy
{
    /// <summary>
    /// Interaction logic for AudioConfigWindow.xaml
    /// </summary>
    /// 

    public partial class AudioConfigWindow : Window
    {

        DirectSoundDeviceInfo selDS { get; set; }
        DirectSoundDeviceInfo selDSsounder { get; set; }
        DirectSoundDeviceInfo selDSclip { get; set; }

        int latency { get; set; }

        string selAS { get; set; }

        int selASout { get; set; }
        int selASsounder { get; set; }
        int selASclip { get; set; }

        public AudioConfigWindow()
        {
            InitializeComponent();
            GetDevices();
            GetASIOChannels();
            latency = Settings.Default.DSLatency;


            ASIODevices.ItemsSource = asioD;
            ASIOChannel.ItemsSource = asioChannels;

            DSDevices.ItemsSource = dsD;
            DSClips.ItemsSource = dsD;
            DSSounders.ItemsSource = dsD;
            DSlatency.ItemsSource = new List<int> { 0, 5, 10, 20, 25, 50, 100, 150, 200, 250, 300, 350, 400, 450, 500 };

            if (Settings.Default.DSDevice != null)
            {
                DSDevices.SelectedItem = dsD.Find(item => item.Guid == Settings.Default.DSDevice.Guid);

            }
            if (Settings.Default.DSClips != null)
            {
                DSClips.SelectedItem = dsD.Find(item => item.Guid == Settings.Default.DSClips.Guid);

            }
            if (Settings.Default.DSSounders != null)
            {
                DSSounders.SelectedItem = dsD.Find(item => item.Guid == Settings.Default.DSSounders.Guid);

            }
            if (Settings.Default.ASIODevice != null)
            {
                ASIODevices.SelectedItem = asioD.Find(item => item == Settings.Default.ASIODevice);
            }
            try
            {
                ASIOSounders.SelectedItem = asioChannels.Find(item => item.index == Settings.Default.ASIOSounders);
                ASIOClips.SelectedItem = asioChannels.Find(item => item.index == Settings.Default.ASIOClips);
            }
            catch
            {
                ASIOSounders.SelectedItem = null;
                ASIOClips.SelectedItem = null;
            }
        }
        List<DirectSoundDeviceInfo> dsD;
        List<String> asioD;
        List<ASIOOutputInfo> asioChannels;
        public void GetDevices()
        {
            asioD = AsioOut.GetDriverNames().ToList();

            dsD = DirectSoundOut.Devices.ToList();

        }

        private void audioOK_Click(object sender, RoutedEventArgs e)
        {
            if (selDS != null)
            {
                Settings.Default.DSDevice = selDS;
            }
            if (selDSsounder != null)
            {
                 Settings.Default.DSSounders = selDSsounder;
            }
            if (selDSclip != null)
            {
                Settings.Default.DSClips = selDSclip;
            }
            if (selAS != null)
            {
                Settings.Default.ASIODevice = selAS;
            }
            if (ASIOSounders.Visibility == Visibility.Visible)
            {
                Settings.Default.ASIOSounders = selASsounder;
            }
            if (ASIOClips.Visibility == Visibility.Visible)
            {
                Settings.Default.ASIOClips = selASclip;
            }
            if (ASIOChannel.Visibility == Visibility.Visible)
            {
                Settings.Default.ASIOOutput = selASout;
                Settings.Default.ASIOClips = selASout;
                Settings.Default.ASIOSounders = selASout;
            }
            Settings.Default.Save();
            this.DialogResult = true;
            this.Close();
        }

        private void audioCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Settings.Default.Reload();
            this.Close();
        }

        private void ASIODevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selAS = ASIODevices.SelectedItem as String;
            Settings.Default.ASIODevice = selAS;
            //Settings.Default.Save();
            //Settings.Default.Reload();
            GetASIOChannels();
           // Trace.WriteLine(selAS);
        }

        private void DSDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selDS = DSDevices.SelectedItem as DirectSoundDeviceInfo;
            //Trace.WriteLine(selDS.Description);
            //Trace.WriteLine(selDS.Guid);
        }

        private void DSSounders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selDSsounder = DSSounders.SelectedItem as DirectSoundDeviceInfo;
            //Trace.WriteLine(selDSsounder.Description);
            //Trace.WriteLine(selDSsounder.Guid);
        }

        private void DSClips_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selDSclip = DSClips.SelectedItem as DirectSoundDeviceInfo;
            //Trace.WriteLine(selDSclip.Description);
            //Trace.WriteLine(selDSclip.Guid);
        }

        private void DriverDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DriverDropdown.SelectedIndex == 1)
            {
                GetASIOChannels();
            }
        }

        private void GetASIOChannels()
        {
            if (Settings.Default.ASIODevice != null)
            {
                try
                {
                    if (asioChannels == null)
                    {
                        asioChannels = new List<ASIOOutputInfo>();
                    }
                    if (ASIOSounders != null & ASIOClips != null & ASIOChannel != null)
                    {
                        ASIOSounders.ItemsSource = null;
                        ASIOClips.ItemsSource = null;
                        ASIOChannel.ItemsSource = null;
                    }

                    asioChannels.Clear();
                    AsioOut asio;
                    int outputs = 0;
                    if (Settings.Default.ASIOSplit)
                    {
                        asio = new AsioOut(Settings.Default.ASIODevice);

                        outputs = asio.DriverOutputChannelCount;
                        for (int i = 0; i <= outputs; i++)
                        {
                            ASIOOutputInfo inf = new ASIOOutputInfo();
                            inf.index = i;
                            inf.name = asio.AsioOutputChannelName(i);
                            asioChannels.Add(inf);
                        }

                    }
                    else
                    {
                        asio = new AsioOut(Settings.Default.ASIODevice);

                        outputs = asio.NumberOfOutputChannels;
                        for (int i = 0; i <= outputs; i++)
                        {
                            ASIOOutputInfo inf = new ASIOOutputInfo();
                            inf.index = i;
                            inf.name = asio.AsioOutputChannelName(i);
                            asioChannels.Add(inf);
                        }
                    }

                    asio.Dispose();

                    if (ASIOSounders != null & ASIOClips != null & ASIOChannel != null)
                    {
                        ASIOSounders.ItemsSource = asioChannels;
                        ASIOClips.ItemsSource = asioChannels;
                        ASIOChannel.ItemsSource = asioChannels;
                    }
                    
                }
                catch
                {
                    Trace.WriteLine("No ASIO Driver.");
                }

            }
        }

        private void ASIOSounders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ASIOSounders.SelectedItem is ASIOOutputInfo)
            {
                selASsounder = ((ASIOOutputInfo)ASIOSounders.SelectedItem).index;

            }
        }

        private void ASIOClips_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ASIOClips.SelectedItem is ASIOOutputInfo)
            {
                selASclip = ((ASIOOutputInfo)ASIOClips.SelectedItem).index;
            }
        }

        private void SeparateOutputs_Click(object sender, RoutedEventArgs e)
        {
            GetASIOChannels();
            ASIOSounders.ItemsSource = asioChannels;
            ASIOClips.ItemsSource = asioChannels;
        }

        private void StackPanel_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            GetASIOChannels();
            ASIOSounders.ItemsSource = asioChannels;
            ASIOClips.ItemsSource = asioChannels;
        }

        private void ASIOMono_Click(object sender, RoutedEventArgs e)
        {

                GetASIOChannels();
            ASIOSounders.ItemsSource = asioChannels;
            ASIOClips.ItemsSource = asioChannels;
        }

        private void ASIOChannel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ASIOChannel.SelectedItem is ASIOOutputInfo)
            {
                selASout = ((ASIOOutputInfo)ASIOChannel.SelectedItem).index;
            }
        }
    }

    public class ASIOOutputInfo
    {
        public int index { get; set; }
        public string name { get; set; }
    }
}
