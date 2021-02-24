using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Diagnostics;
using NAudio.Wave;

namespace NewsBuddy
{

    public class NBfile // maybe derive from : EventArgs
    {

        public string NBName { get; set; }

        public string NBPath { get; set; }

        public bool NBisSounder { get; set; }

        MainWindow homeBase = Application.Current.Windows[0] as MainWindow;

        public TextPointer textPointer;

        public NJAudioPlayer player { get; set; }

        public NJFileReader NJF { get; set; }


        public void NBPlayNA(bool isSounder)
        {
            if (File.Exists(NBPath))
            {
                if (Settings.Default.AudioOutType == 1 && ((Settings.Default.ASIOSounders == Settings.Default.ASIOClips) || Settings.Default.ASIOSplit || !Settings.Default.SeparateOutputs))
                {
                    homeBase.PlayAsioMixer(NJF);
                }
                else
                {
                    if (isSounder)
                    {
                        homeBase.PlaySounder(player);
                    }
                    else
                    {
                        homeBase.PlayClip(player);
                    }
                }                                           
            }
        }

        public NButton NBbutton()
        {
            if (!File.Exists(NBPath))
            {
                NButton badNB = new NButton();
                badNB.Background = Brushes.DarkRed;
                badNB.Content = "Couldn't Find: " + NBName;
                return badNB;
            }
            else
            {
                NButton NBbutton = new NButton();
                NBbutton.Content = NBName;
                NBbutton.Loaded += MakePlayer;
                NBbutton.Unloaded += DisposePlayer;

                DisposePlayer(this, new RoutedEventArgs());
                
                if (NBisSounder)
                {
                    NBbutton.Style = (Style)Application.Current.FindResource("btnNBs");
                }
                else
                {
                    NBbutton.Style = (Style)Application.Current.FindResource("btnNBc");
                }


                NBbutton.Click += (sender, args) =>
                {
                    NBPlayNA(NBisSounder);
                };
                NBbutton.MouseDoubleClick += (sender, args) =>
                {
                    NBPlayNA(NBisSounder);
                };
                NBbutton.file = this;

                return NBbutton;
            }
            
        }

        private void MakePlayer(object sender, RoutedEventArgs e)
        {
            if (player == null)
            {
                if (Settings.Default.AudioOutType == 0)
                {
                    if (Settings.Default.SeparateOutputs)
                    {
                        if (Settings.Default.DSSounders != null & Settings.Default.DSClips != null)
                        {
                            if (NBisSounder)
                            {
                                player = new NJAudioPlayer(NBPath, Settings.Default.DSSounders.Guid);
                                Trace.WriteLine("Player made for " + NBName);

                            }
                            else
                            {
                                player = new NJAudioPlayer(NBPath, Settings.Default.DSClips.Guid);
                                Trace.WriteLine("Player made for " + NBName);

                            }
                        }
                        else
                        {
                          
                            MessageBox.Show("Audio Output not selected.\nCheck your settings under Settings > Audio Device Settings", "No Audio Device Selected", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                       
                    } 
                    else
                    {
                        player = new NJAudioPlayer(NBPath);
                        Trace.WriteLine("Player made for " + NBName);

                    }

                }
                else if (Settings.Default.AudioOutType == 1)
                {
                    // if they're separate devices but not splitting channels, and not on the same device channel, new output for each each time.
                    if (Settings.Default.SeparateOutputs & !Settings.Default.ASIOSplit && (Settings.Default.ASIOSounders != Settings.Default.ASIOClips))
                    {
                        if (NBisSounder)
                        {
                            player = new NJAudioPlayer(NBPath, Settings.Default.ASIODevice, Settings.Default.ASIOSounders);
                            Trace.WriteLine("Player made for " + NBName);

                        }
                        else
                        {
                            player = new NJAudioPlayer(NBPath, Settings.Default.ASIODevice, Settings.Default.ASIOClips);
                            Trace.WriteLine("Player made for " + NBName);

                        }
                    }
                    // if they're both using the same device, or using split channels, OR not separated
                    else if ((Settings.Default.ASIOSounders == Settings.Default.ASIOClips) || (Settings.Default.ASIOSplit && Settings.Default.SeparateOutputs) || !Settings.Default.SeparateOutputs)
                    {
                        if (NJF == null)
                        {
                            NJF = new NJFileReader(new AudioFileReader(NBPath), NBisSounder, NBName);
                            Trace.WriteLine("Mixer Player Made for " + NBName);
                        }
                        
                    }
                    
                    

                }
                else
                {
                    MessageBox.Show("Audio Device Error.\nCheck your Settings under:\nSettings > Audio Device Settings", "Audio Device Error");
                }
            } else
            {
                Trace.WriteLine("Player was not null when MakePlayer called: " + NBName);
            }
        }

        private void DisposePlayer(object sender, RoutedEventArgs e)
        {
            if (player != null && !player.IsPlaying())
            {
                player.Dispose();
                Trace.WriteLine("Disposed of player for " + NBName);
                player = null;
            } else
            {
                Trace.WriteLine("Player called for dispose but was null OR playing: " + NBName);
            }
            if (NJF != null && !NJF.isPlaying)
            {
                NJF.reader.Position = 0;
                NJF.reader.Dispose();
                NJF = null;
            }
            else
            {
                Trace.WriteLine("NJF called for dispose but was null or playing." + NBName);
            }

        }



    }

}
