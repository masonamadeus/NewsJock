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
        string selASsounder { get; set; }
        string selASclip { get; set; }

        public AudioConfigWindow()
        {
            InitializeComponent();
            GetDevices();
            latency = Settings.Default.DSLatency;


            ASIODevices.ItemsSource = asioD;

            DSDevices.ItemsSource = dsD;
            DSClips.ItemsSource = dsD;
            DSSounders.ItemsSource = dsD;
            DSlatency.ItemsSource = new List<int> { 0, 50, 100, 150, 200, 250, 300, 350, 400, 450, 500 };

            if (asioD.Count != 0)
            {
                ASIODevices.Text = Settings.Default.ASIODevice;
            }
            else
            {
                ASIOmenu.Visibility = Visibility.Collapsed;
            }
            DSDevices.SelectedItem = dsD.Find(item => item.Guid == Settings.Default.DSDevice.Guid);
            DSClips.SelectedItem = dsD.Find(item => item.Guid == Settings.Default.DSClips.Guid);
            DSSounders.SelectedItem = dsD.Find(item => item.Guid == Settings.Default.DSSounders.Guid);
            ASIODevices.SelectedItem = asioD.Find(item => item == Settings.Default.ASIODevice);
        }
        List<DirectSoundDeviceInfo> dsD;
        List<String> asioD;
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
            if (selASsounder != null)
            {
                Settings.Default.ASIOSounders = selASsounder;
            }
            if (selASclip != null)
            {
                Settings.Default.ASIOClips = selASclip;
            }

            Settings.Default.Save();
            this.DialogResult = true;
            this.Close();
        }

        private void audioCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void ASIODevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selAS = ASIODevices.SelectedItem as String;
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

    }
}
