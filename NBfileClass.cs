using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Documents;

namespace NewsBuddy
{

    public class NBfile // maybe derive from : EventArgs
    {

        public string NBName { get; set; }

        public string replaceName { get; set; }

        public string NBPath { get; set; }

        public bool NBisSounder = false;

        MainWindow homeBase = Application.Current.Windows[0] as MainWindow;

        public TextPointer textPointer;

        public NBfileLocator locator;


        public void NBPlay(bool isSounder)
        {
            if (File.Exists(NBPath))
            {
                if (isSounder)
                {
                    if (homeBase.SoundersPlayer.Source != null)
                    {
                        homeBase.SoundersPlayer.Stop();

                        if (homeBase.SoundersPlayer.Source != new Uri(NBPath))
                        {
                            homeBase.SoundersPlayer.Source = new Uri(NBPath);
                            homeBase.SoundersPlayer.Play();
                        }
                        else
                        {
                            homeBase.SoundersPlayer.Source = null;
                        }

                    }
                    else
                    {
                        homeBase.SoundersPlayer.Source = new Uri(NBPath);
                        homeBase.SoundersPlayer.Play();
                    }
                }
                else
                {
                    if (homeBase.ClipsPlayer.Source != null)
                    {
                        homeBase.ClipsPlayer.Stop();
                        if (homeBase.ClipsPlayer.Source != new Uri(NBPath))
                        {
                            homeBase.ClipsPlayer.Source = new Uri(NBPath);
                            homeBase.ClipsPlayer.Play();
                        }
                        else
                        {
                            homeBase.ClipsPlayer.Source = null;
                        }

                    }
                    else
                    {
                        homeBase.ClipsPlayer.Source = new Uri(NBPath);
                        homeBase.ClipsPlayer.Play();
                    }
                }
            }
            else
            {
                MessageBox.Show("Error. The file is missing. It may have been moved, renamed, or deleted.");
            }
            
        }

        public NButton NBbutton()
        {
            var bc = new BrushConverter();
            NButton NBbutton = new NButton();
            NBbutton.Content = NBName;
            if (NBisSounder)
            {
                NBbutton.Background = (Brush)bc.ConvertFrom("#05B8CC");
            } else
            {
                NBbutton.Background = (Brush)bc.ConvertFrom("#6DFF9A");
            }
            //NBbutton.Style = (Style)Application.Current.FindResource("btnNB");
            NBbutton.Click += (sender, args) =>
            {
                NBPlay(NBisSounder);
            };
            NBbutton.file = this;

            return NBbutton;
        }

        public string GetIDs(NBfile nb, int index)
        {
            string repName;


            repName = String.Format("%@!${0}", index) + nb.NBPath + String.Format("$@!%{0}", index);


            return repName;
        }

        public string GetIDc(NBfile nb, int index)
        {
            string repName;

            repName = String.Format("%@!#{0}", index) + nb.NBPath + String.Format("#@!%{0}", index);

            return repName;
        }

    }

}
