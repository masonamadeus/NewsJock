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
        public APReader()
        {
            InitializeComponent();
            ingest = new APingestor();
            RefreshAP(new object(), new RoutedEventArgs());
            isLoaded = true;
            list_APStories.SelectedItem = list_APStories.Items[0];

        }

        private void list_APStories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isLoaded)
            {
                APObject selected = list_APStories.SelectedItem as APObject;
                if (selected != null)
                {
                    if (selected.GetStory())
                    {
                        frame_Story.Content=selected.story;
                        scrl_Story.ScrollToVerticalOffset(0);
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
            ingest.GetFeed();
            list_APStories.Items.Clear();
            if (ingest.isAuthorized)
            {
                foreach (APObject obj in ingest.GetItems())
                {
                    list_APStories.Items.Add(obj);
                    list_APStories.Items.Add(new Separator());
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

      
    }
}
