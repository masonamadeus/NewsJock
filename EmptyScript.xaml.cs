using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NewsBuddy
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Page1 : Page
    {
        MainWindow main = Application.Current.Windows[0] as MainWindow;
        public Page1()
        {
            InitializeComponent();
            rtbScript.AddHandler(RichTextBox.DragOverEvent, new DragEventHandler(Script_DragOver), true);
            rtbScript.AddHandler(RichTextBox.DropEvent, new DragEventHandler(Script_Drop), true);
            rtbScript.IsDocumentEnabled = true;
        }


        private void Script_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("NBfile"))
            {
                e.Effects = DragDropEffects.Copy;
            }
            e.Handled = false;

        }

        private void Script_Drop(object sender, DragEventArgs e)
        {
            RichTextBox rtb = sender as RichTextBox;
            NBfile passNB = new NBfile();
            Button NBbutton = new Button();
            passNB = e.Data.GetData("NBfile") as NBfile;
            InlineUIContainer nb = new InlineUIContainer(passNB.NBbutton(), rtbScript.CaretPosition);
            e.Handled = true;
        }
    }
}
