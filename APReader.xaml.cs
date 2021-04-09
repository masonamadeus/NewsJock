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
        private List<APObject> stories;
        private bool isLoaded = false;
        public APReader()
        {
            InitializeComponent();
            ingest = new APingestor();
            ingest.GetFeed();
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
            }
            this.Owner = Application.Current.MainWindow;
            isLoaded = true;

        }

        private void list_APStories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isLoaded)
            {
                APObject selected = list_APStories.SelectedItem as APObject;
                if (selected != null)
                {
                    StringBuilder builder = new StringBuilder();
                    if (selected.GetStory(ingest))
                    {
                        foreach (XmlNode node in selected.story.ChildNodes)
                        {
                            builder.AppendLine(node.InnerText);
                        }
                        text_CurrentStory.Text = builder.ToString();
                    }

                }
            }
           
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ingest.Dispose();
        }

        private void btn_RefreshAP_Click(object sender, RoutedEventArgs e)
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
            }
        }
    }
}
