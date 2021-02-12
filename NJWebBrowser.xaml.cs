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

namespace NewsBuddy
{
    /// <summary>
    /// Interaction logic for NJWebBrowser.xaml
    /// </summary>
    public partial class NJWebBrowser : Window
    {
        public NJWebBrowser()
        {
            InitializeComponent();

        }

        private void btnGo_Click(object sender, RoutedEventArgs e)
        {
            if (addressBar.Text.StartsWith(@"http://",StringComparison.OrdinalIgnoreCase))
            {
                string addr = addressBar.Text;
                addr = addr.Remove(0, 7);
                addressBar.Text = addr;
            }
            if (addressBar.Text.StartsWith(@"https://", StringComparison.OrdinalIgnoreCase))
            {
                string addr2 = addressBar.Text;
                addr2 = addr2.Remove(0, 8);
                addressBar.Text = addr2;
            }
            webView.Navigate(@"https://" + addressBar.Text);

        }

    }
}
