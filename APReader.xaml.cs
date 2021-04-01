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

namespace NewsBuddy
{
    /// <summary>
    /// Interaction logic for APReader.xaml
    /// </summary>
    public partial class APReader : Window
    {
        public APingestor ingest;
        private List<APObject> stories;
        public APReader()
        {
            InitializeComponent();
            ingest = new APingestor();
            ingest.GetFeed();
            list_APStories.ItemsSource = ingest.GetItems().Distinct().ToList();

        }
    }
}
