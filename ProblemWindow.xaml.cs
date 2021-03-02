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

        public string[] funnyErrors = new string[] { 
            "Pretty Big Error",
            "What did you DO!?",
            "Uh Oh, It Broke",
            "10000% Your Fault",
            "I Am SO Sorry",
            "Oh Good. It's Broken",
            "I Hope That's Not Important.",
            "BREAKING NEWS: NewsJock Broke!",
            "Tom? Is that you??",
            "That's Not Good",
            "Oops, All Errors",
            "This Isn't Supposed To Happen"
        };

        public bool tryLife { get; set; }
        public ProblemWindow(Exception exception, bool optionalShutdown)
        {
            InitializeComponent();
            Random r = new Random();
            int number = r.Next(0, funnyErrors.Length - 1);
            Mouse.OverrideCursor = null;
            issue = exception;
            tryLife = optionalShutdown;
            lblError.Content = funnyErrors[number];
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
