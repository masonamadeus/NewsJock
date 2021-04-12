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

    
    }
}
