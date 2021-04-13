﻿using System;
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
using System.Linq;
using System.Xml;

namespace NewsBuddy
{
    /// <summary>
    /// Interaction logic for APReader.xaml
    /// </summary>
    public partial class APReader : Window
    {
        public APingestor ingest;
       // private List<APObject> stories;
        private bool isLoaded = false;
        public MainWindow mainWindow { get; set; }
        public APReader()
        {
            InitializeComponent();
            editorGrid.Visibility = Visibility.Collapsed;
            mainWindow = Application.Current.Windows[0] as MainWindow;
            ingest = new APingestor();
            RefreshAP(new object(), new RoutedEventArgs());
            isLoaded = true;

        }

        private void list_APStories_SelectionChanged()
        {
            if (isLoaded)
            {
                APObject selected = list_APStories.SelectedItem as APObject;
                if (selected != null)
                {
                    if (selected.HasStory)
                    {
                        if (selected.story.isEditMode)
                        {
                            btn_ToggleMode.Content = "Revert to Original Story";
                        }
                        else
                        {
                            btn_ToggleMode.Content = "Switch to Story Editor Mode";
                        }
                        frame_Story.Content=selected.story;
                        editorGrid.Visibility = Visibility.Visible;
                        scrl_Story.ScrollToVerticalOffset(0);
                    }
                    else
                    {
                        editorGrid.Visibility = Visibility.Collapsed;
                    }
                }
            }
           
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ingest.Dispose();
        }

        private void RefreshAP(object sender, RoutedEventArgs e)
        {
            list_APStories.Items.Clear();
            ingest.GetFeed();
            if (ingest.isAuthorized)
            {
                foreach (APObject obj in ingest.GetItems())
                {
                    if (obj.isAssocParent)
                    {
                        TreeViewItem assocParent = new TreeViewItem()
                        {
                            Header = obj.headline
                        };
                        foreach (APObject assoc in obj.associations)
                        {
                            assocParent.Items.Add(assoc);
                        }

                        list_APStories.Items.Add(assocParent);
                        
                    }
                    else
                    {
                        list_APStories.Items.Add(obj);
                    }
                }
            }
            else
            {
                list_APStories.Items.Add(new TextBlock()
                {
                    Text = "Unauthorized. Check your API Key in NewsJock Settings."
                });
                frame_Story.Content = new TextBlock()
                {
                    Text = "API Unauthorized. Check your API key settings in NewsJock Settings. \nIf you need help: contact your supervisor.\nIf you ARE the supervisor, contact AP and get an API key."
                };
            }
        }

        private void list_APStories_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.Invoke(() => 
            {
                list_APStories_SelectionChanged();
            });
        }

        private void btn_ToggleMode_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            APStory story = frame_Story.Content as APStory;
            story.ToggleModes();
            if (story.isEditMode)
            {
                btn.Visibility = Visibility.Visible;
                btn.Content = "Revert to Original Story";
            }
            else
            {
                btn.Visibility = Visibility.Visible;
                btn.Content = "Switch to Story Editor Mode";
            }

        }

        private void btn_SendToScript_Click(object sender, RoutedEventArgs e)
        {
            APStory story = frame_Story.Content as APStory;
            mainWindow.currentScript.InsertChunksFromIngestor(story.GetChunks());
        }

        private void btn_SendToNewScript_Click(object sender, RoutedEventArgs e)
        {
            
            APStory story = frame_Story.Content as APStory;
            mainWindow.AddNewTabFromChunks(story.GetChunks());

        }
    }
}