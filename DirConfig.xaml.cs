﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Windows.Data;
using System.Diagnostics;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Ookii.Dialogs.Wpf;

namespace NewsBuddy
{
    /// <summary>
    /// Interaction logic for DirConfig.xaml
    /// </summary>
    public partial class DirConfig : Window
    {
        public DirConfig()
        {
            InitializeComponent();
            selDirClip.Text = Settings.Default.ClipsDirectory;
            selDirSounder.Text = Settings.Default.SoundersDirectory;
            selDirScript.Text = Settings.Default.ScriptsDirectory;
            bxCleanDays.ItemsSource = new List<double> { 1, 2, 3, 4, 5, 6, 7, 10, 14, 30 };

        }



        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.ClipsDirectory = selDirClip.Text;
            Settings.Default.SoundersDirectory = selDirSounder.Text;
            Settings.Default.ScriptsDirectory = selDirScript.Text;
            string templateDir = selDirScript.Text + @"\Templates";

            Settings.Default.TemplatesDirectory = templateDir;

            Trace.WriteLine(Settings.Default.TemplatesDirectory);
            Trace.WriteLine(Settings.Default.ScriptsDirectory);
            Trace.WriteLine(Settings.Default.SoundersDirectory);
            Trace.WriteLine(Settings.Default.ClipsDirectory);

            Settings.Default.Save();

            Directory.CreateDirectory(Settings.Default.ClipsDirectory);
            Directory.CreateDirectory(Settings.Default.SoundersDirectory);
            Directory.CreateDirectory(Settings.Default.ScriptsDirectory);
            Directory.CreateDirectory(Settings.Default.TemplatesDirectory);
            
     
            this.Close();

        }

        private void srchSounder_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog srch = new VistaFolderBrowserDialog();
            srch.Description = "Please select your 'Sounders' folder.";
            srch.UseDescriptionForTitle = true;
            if ((bool)srch.ShowDialog(this))
            selDirSounder.Text = srch.SelectedPath;

        }

        private void srchClip_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog srch = new VistaFolderBrowserDialog();
            srch.Description = "Please select your 'Clips' folder.";
            srch.UseDescriptionForTitle = true;
            if ((bool)srch.ShowDialog(this))
                selDirClip.Text = srch.SelectedPath;
        }

        private void srchScript_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog srch = new VistaFolderBrowserDialog();
            srch.Description = "Please select your 'Scripts' folder.";
            srch.UseDescriptionForTitle = true;
            if ((bool)srch.ShowDialog(this))
                selDirScript.Text = srch.SelectedPath;
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
