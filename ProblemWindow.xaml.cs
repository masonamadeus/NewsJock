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
    /// Interaction logic for ProblemWindow.xaml
    /// </summary>
    public partial class ProblemWindow : Window
    {
        public Exception issue { get; set; }

        public enum FunnyErrors {  }

        public bool tryLife { get; set; }
        public ProblemWindow(Exception exception, bool optionalShutdown)
        {
            InitializeComponent();
            Mouse.OverrideCursor = null;
            issue = exception;
            tryLife = optionalShutdown;
            Code.Text = issue.ToString();
            Description.Text = issue.Message;
            if (!tryLife)
            {
                btnTry.Visibility = Visibility.Collapsed;
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnTry_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void Code_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Code.SelectAll();
            Code.Copy();
            MessageBox.Show("Copied all of that nonsense to the clipboard.\nNow you can just paste it wherever :)");
        }
    }
}
