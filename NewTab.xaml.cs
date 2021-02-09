using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NewsBuddy
{
    /// <summary>
    /// Interaction logic for NewTab.xaml
    /// </summary>
    public partial class NewTab : Window
    {
        MainWindow homeBase = Application.Current.Windows[0] as MainWindow;
        public NewTab()
        {
            InitializeComponent();
            
            string[] templates = Directory.GetFiles(Settings.Default.TemplatesDirectory);
            ListBoxItem blank = new ListBoxItem();
            blank.Content = "Blank Script";
            blank.Tag = "/EmptyScript.xaml";
            TemplatesList.Items.Add(blank);
            TemplatesList.Items.Add(new Separator());
            foreach (string file in templates)
            {
                ListBoxItem item = new ListBoxItem();
                item.Content = System.IO.Path.GetFileNameWithoutExtension(file);
                item.Tag = file;
                TemplatesList.Items.Add(item);
            }
        }

        public string chosenFile;


        private void TemplatesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBoxItem chosen = TemplatesList.SelectedItem as ListBoxItem;
            chosenFile = chosen.Tag.ToString();
           homeBase.AddNewTabFromFrame(chosenFile);

        }

        private void Window_Deactivated(object sender, EventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
