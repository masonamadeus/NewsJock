using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ookii.Dialogs.Wpf;

namespace NewsBuddy
{

    public partial class Page1 : Page
    {
        List<NBfileLocator> NBlocks = new List<NBfileLocator>();

        public Page1()
        {
            InitializeComponent();
            rtbScript.AddHandler(RichTextBox.DragOverEvent, new DragEventHandler(Script_DragOver), true);
            rtbScript.AddHandler(RichTextBox.DropEvent, new DragEventHandler(Script_Drop), true);
            rtbScript.IsDocumentEnabled = true;
            selFontSize.ItemsSource = new List<Double>() { 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32, 34, 36, 38, 50, 60, 72 };
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
            NBfileLocator NBloc = new NBfileLocator();


            passNB = e.Data.GetData("NBfile") as NBfile;

            int caretOffset = rtbScript.Document.ContentStart.GetOffsetToPosition(rtbScript.CaretPosition);

            passNB.insertOffset = caretOffset;

            NBloc.offset = caretOffset;
            NBloc.file = passNB;

            Trace.WriteLine("added NB file with index: " + NBloc.offset);

            NBlocks.Add(NBloc);

            InlineUIContainer nb = new InlineUIContainer(passNB.NBbutton(), rtbScript.CaretPosition);

            e.Handled = true;
        }

        private void selFontSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!rtbScript.Selection.IsEmpty)
            {
                rtbScript.Selection.ApplyPropertyValue(Inline.FontSizeProperty, selFontSize.Text);
            }
            
        }

        private void rtbScript_SelectionChanged(object sender, RoutedEventArgs e)
        {
            object temp = rtbScript.Selection.GetPropertyValue(Inline.FontWeightProperty);
            btnBold.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontWeights.Bold));
            temp = rtbScript.Selection.GetPropertyValue(Inline.FontStyleProperty);
            btnItal.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontStyles.Italic));
            temp = rtbScript.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
            btnUndr.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(TextDecorations.Underline));

            temp = rtbScript.Selection.GetPropertyValue(Inline.FontSizeProperty);

            double size;

            if (Double.TryParse(temp.ToString(), out size))
            { selFontSize.Text = temp.ToString(); }
        }

        private void mnuSave_Click(object sender, RoutedEventArgs e)
        {
            VistaSaveFileDialog svD = new VistaSaveFileDialog();
            svD.Filter = "xaml files (*.xaml)|*.xaml";
            svD.DefaultExt = "xaml";
            svD.InitialDirectory = Settings.Default.ScriptsDirectory;
            if ((bool)svD.ShowDialog())
            {
                int prevOffset = 0;

                for (int i = 0; i < NBlocks.Count; i++)
                {
                    TextPointer newCaret;

                    NBfileLocator nbL = NBlocks[i];

                    int offset = nbL.offset;

                    rtbScript.CaretPosition = rtbScript.Document.ContentStart;

                    newCaret = rtbScript.Document.ContentStart;

                    newCaret = newCaret.GetPositionAtOffset(offset + prevOffset);

                    rtbScript.CaretPosition = newCaret;

                    string repname = nbL.GetID(nbL.file, i);

                    rtbScript.CaretPosition.InsertTextInRun(repname);

                    prevOffset += repname.Length;

                    Trace.WriteLine("Inserted " + System.IO.Path.GetFileNameWithoutExtension(repname) + " with index "+i+" at offset " + nbL.offset + ".");
                }


                string fileName = svD.FileName;
                TextRange range;
                FileStream fs;

                range = new TextRange(rtbScript.Document.ContentStart, rtbScript.Document.ContentEnd);
                fs = new FileStream(fileName, FileMode.Create);
                range.Save(fs, DataFormats.XamlPackage);
                fs.Close();

               // ResetNBfiles();
            }


        }

        private void mnuOpen_Click(object sender, RoutedEventArgs e)
        {
            TextRange range;
            FileStream fs;
            VistaOpenFileDialog opn = new VistaOpenFileDialog();
            if ((bool)opn.ShowDialog())
            {
                if (opn.CheckFileExists)
                {
                    range = new TextRange(rtbScript.Document.ContentStart, rtbScript.Document.ContentEnd);
                    fs = new FileStream(opn.FileName, FileMode.OpenOrCreate);
                    range.Load(fs, DataFormats.XamlPackage);
                    fs.Close();
                    ResetNBfiles();
                }
            }

        }


        /*
        public void ResetNBfiles()
        {

            bool foundAllSounders = false;
            bool foundAllClips = false;
            int soundersIndex = 0;
            TextRange searchRange = new TextRange(rtbScript.Document.ContentStart, rtbScript.Document.ContentEnd);

            while (!foundAllSounders)
            {
                searchRange = new TextRange(rtbScript.Document.ContentStart, rtbScript.Document.ContentEnd);
                TextRange foundKeyS1 = FindTextInRange(searchRange, String.Format("%${0}",soundersIndex));
                if (foundKeyS1 != null)
                {
                    searchRange = new TextRange(rtbScript.Document.ContentStart, rtbScript.Document.ContentEnd);
                    TextRange foundKeyS2 = FindTextInRange(searchRange, String.Format("$%{0}", soundersIndex));

                    if (foundKeyS2 != null)
                    {
                        Trace.WriteLine("foundKeyS1 = " + foundKeyS1.Text);
                        Trace.WriteLine("foundKeyS2 = " + foundKeyS2.Text);

                        TextPointer pathStartS = foundKeyS1.End;
                        TextPointer pathEndS = foundKeyS2.Start;
                        foundKeyS1.Text = "";
                        foundKeyS2.Text = "";

                        TextRange NBKeyS = new TextRange(pathStartS, pathEndS);

                        string _NBPathS = NBKeyS.Text;

                        rtbScript.CaretPosition = pathEndS;
                        NBfile replS = new NBfile();
                        replS.NBPath = _NBPathS;
                        replS.NBName = System.IO.Path.GetFileNameWithoutExtension(_NBPathS);
                        replS.NBisSounder = true;

                        NBKeyS.Text = "";
                        soundersIndex += 1;
                        InlineUIContainer nb = new InlineUIContainer(replS.NBbutton(), rtbScript.CaretPosition); ;



                        Trace.WriteLine("NBKey = " + _NBPathS);
                        Trace.WriteLine("NBfile Path = " + replS.NBPath);
                        Trace.WriteLine("NBfile Name = " + replS.NBName);

                        ResetNBfiles();
                    } else { MessageBox.Show("Key2 was blank. wtf."); }
                  
                } else { return; }


            }


        }

        */



        public void ResetNBfiles()
        {
            List<NBfile> newNBs = new List<NBfile>();

            bool foundAllSounders = false;
            bool foundAllClips = false;
            int soundersIndex = 0;

            TextRange searchRange = new TextRange(rtbScript.Document.ContentStart, rtbScript.Document.ContentEnd);

            while (!foundAllSounders)
            {

                searchRange = new TextRange(rtbScript.Document.ContentStart, rtbScript.Document.ContentEnd);
                TextRange foundKeyS1 = FindTextInRange(searchRange, String.Format("%${0}", soundersIndex));
                if (foundKeyS1 != null)
                {
                    searchRange = new TextRange(rtbScript.Document.ContentStart, rtbScript.Document.ContentEnd);
                    TextRange foundKeyS2 = FindTextInRange(searchRange, String.Format("$%{0}", soundersIndex));

                    if (foundKeyS2 != null)
                    {
                        Trace.WriteLine("foundKeyS1 = " + foundKeyS1.Text);
                        Trace.WriteLine("foundKeyS2 = " + foundKeyS2.Text);

                        TextPointer pathStartS = foundKeyS1.End;
                        TextPointer pathEndS = foundKeyS2.Start;
                        TextPointer nextIns = foundKeyS2.End;


                        TextRange NBKeyS = new TextRange(pathStartS, pathEndS);

                        string _NBPathS = NBKeyS.Text;
                        int _NBoffsetS = rtbScript.Document.ContentStart.GetOffsetToPosition(nextIns);

                        NBfile replS = new NBfile();
                        replS.NBPath = _NBPathS;
                        replS.NBName = System.IO.Path.GetFileNameWithoutExtension(_NBPathS);
                        replS.NBisSounder = true;
                        replS.insertOffset = _NBoffsetS;
                        newNBs.Add(replS);
                        soundersIndex += 1;

                        Trace.WriteLine("NBKey = " + _NBPathS);
                        Trace.WriteLine("NBfile Path = " + replS.NBPath);
                        Trace.WriteLine("NBfile Name = " + replS.NBName);
                        Trace.WriteLine(soundersIndex.ToString());

                    }
                    else { MessageBox.Show("Key2 was blank. wtf."); }

                }
                else { foundAllSounders = true; }

                
            }

            foreach (NBfile nb in newNBs)
            {
                TextPointer newPos;
                rtbScript.CaretPosition = rtbScript.Document.ContentStart;
                newPos = rtbScript.CaretPosition;
                newPos = newPos.GetPositionAtOffset(nb.insertOffset);
                rtbScript.CaretPosition = newPos;
                InlineUIContainer newNBbutton = new InlineUIContainer(nb.NBbutton(), newPos);
            }

        }







        //NOT MY CODE

        public TextRange FindTextInRange(TextRange searchRange, string searchText)
        {
            int offset = searchRange.Text.IndexOf(searchText, StringComparison.OrdinalIgnoreCase);
            if (offset < 0)
                return null;  // Not found

            var start = GetTextPositionAtOffset(searchRange.Start, offset);
            TextRange result = new TextRange(start, GetTextPositionAtOffset(start, searchText.Length));

            return result;
        }

        TextPointer GetTextPositionAtOffset(TextPointer position, int characterCount)
        {
            while (position != null)
            {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    int count = position.GetTextRunLength(LogicalDirection.Forward);
                    if (characterCount <= count)
                    {
                        return position.GetPositionAtOffset(characterCount);
                    }

                    characterCount -= count;
                }

                TextPointer nextContextPosition = position.GetNextContextPosition(LogicalDirection.Forward);
                if (nextContextPosition == null)
                    return position;

                position = nextContextPosition;
            }
            return position;
        }
    }
}
