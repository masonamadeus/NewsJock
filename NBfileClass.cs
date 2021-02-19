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

        public NJAudioPlayer player;


        public void NBPlayNA(bool isSounder)
        {
            if (File.Exists(NBPath))
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
                if (Settings.Default.DSSeparate & Settings.Default.AudioOutType == 0)
                {
                    if (NBisSounder)
                    {
                        player = new NJAudioPlayer(NBPath, Settings.Default.DSSounders.Guid);
                    }
                    else
                    {
                        player = new NJAudioPlayer(NBPath, Settings.Default.DSClips.Guid);
                    }
                }
                else if (Settings.Default.AudioOutType == 1)
                {
                    Trace.WriteLine("ASIO not implemented yet");
                }
                else
                {
                    player = new NJAudioPlayer(NBPath);
                }
                Trace.WriteLine("Player made for " + NBName);
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
        }



    }

}
