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
        List<NBfile> newNBs = new List<NBfile>();
        List<InlineUIContainer> NBbuttons = new List<InlineUIContainer>();

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

            passNB.textPointer = rtbScript.CaretPosition.GetInsertionPosition(LogicalDirection.Forward);

            NBloc.offset = caretOffset;
            NBloc.textPointer = rtbScript.CaretPosition.GetInsertionPosition(LogicalDirection.Forward);
            NBloc.file = passNB;

            Trace.WriteLine("added NB file with index: " + NBloc.offset);
            NButton nButton = passNB.NBbutton();
            nButton.locator = NBloc;

            NBlocks.Add(NBloc);

            InlineUIContainer nb = new InlineUIContainer(nButton, rtbScript.CaretPosition);
            nb.Name = String.Format("NBblock{0}", NBlocks.Count-1);
            nb.Unloaded += NBbutton_Deleted;
            
            NBbuttons.Add(nb);

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
                
                int clipIndex = 0;
                int sounderIndex = 0;


                for (int i = 0; i < NBbuttons.Count; i++)
                {
                    List<Object> blocks = new List<Object>();
                    InlineUIContainer NBbutt = NBbuttons[i];
                    foreach (var block in rtbScript.Document.Blocks)
                    {
                        blocks.Add(block);
                    }

                    for (int x = 0; x < blocks.Count; x++)
                    {
                        Object block = blocks[x];
                        var paragraph = block as Paragraph;

                        if (paragraph.Inlines.Contains(NBbutt))
                        {
                            // THIS IS THE KEY TO TRACKING THE POSITION OF NBUTTONS AS THEY MOVE! ITERATE THROUG THE
                            // FLOWDOCUMENT AND FIND THEM LIKE THIS, THEN SET THEIR TEXTPOINTER TO THE INSERTION POINT BEFORE
                            // THE PARAGRAPH, BECAUSE THAT HAS TO BE THE END OF THE PREVIOUS ELEMENT MOTHERFUUUUCKEEERRR. RIGHT?
                            // OR MAYBE JUST GO TO THE VERY OUSIDE OF THE INLINES OR SOMETHING.
                            (NBbutt.Child as NButton).file.locator.textPointer = paragraph.ContentStart.GetNextInsertionPosition(LogicalDirection.Backward);
                            //child.file.textPointer = paragraph.ContentStart.GetNextInsertionPosition(LogicalDirection.Backward);
                            paragraph.Inlines.Remove(NBbutt);
                        }
                    }

                }

                for (int i = 0; i < NBlocks.Count; i++)
                {
                    //TextPointer newCaret;

                    NBfileLocator nbL = NBlocks[i];

                    //int offset = nbL.offset;

                    //rtbScript.CaretPosition = rtbScript.Document.ContentStart;

                    //newCaret = rtbScript.Document.ContentStart;

                    //newCaret = newCaret.GetPositionAtOffset(offset + prevOffset);


                    // change back to newCaret if textPointer doesn't work.
                    
                    if (nbL.file.NBisSounder)
                    {
                        string repname = nbL.GetIDs(nbL.file, sounderIndex);

                        rtbScript.CaretPosition = nbL.textPointer;

                        rtbScript.CaretPosition.InsertTextInRun(repname);

                        Trace.WriteLine("Inserted Sounder " + System.IO.Path.GetFileNameWithoutExtension(repname) + " from index " + i + " with ID Number " + sounderIndex);

                        sounderIndex += 1;
                    } else
                    {
                        string repname = nbL.GetIDc(nbL.file, clipIndex);

                        rtbScript.CaretPosition = nbL.textPointer;

                        rtbScript.CaretPosition.InsertTextInRun(repname);

                        Trace.WriteLine("Inserted Clip " + System.IO.Path.GetFileNameWithoutExtension(repname) + " from index " + i + " with ID Number" + clipIndex);

                        clipIndex += 1;
                    }

                }



                string fileName = svD.FileName;
                TextRange range;
                FileStream fs;

                range = new TextRange(rtbScript.Document.ContentStart, rtbScript.Document.ContentEnd);
                fs = new FileStream(fileName, FileMode.Create);
                range.Save(fs, DataFormats.XamlPackage);
                fs.Close();

                RestoreNBfiles(true);

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
                    RestoreNBfiles(true);
                }
            }

        }


        public void RestoreNBfiles(bool recreate)
        {
            newNBs.Clear();

            bool foundAllSounders = false;
            bool foundAllClips = false;
            int soundersIndex = 0;
            int clipsIndex = 0;

            TextRange searchRange = new TextRange(rtbScript.Document.ContentStart, rtbScript.Document.ContentEnd);

// SOUNDERS DISCOVERY SECTION

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
                        TextPointer nextInsS = foundKeyS2.End;

                        TextRange NBKeyS = new TextRange(pathStartS, pathEndS);
                        string _NBPathS = NBKeyS.Text;

                        NBfile replS = new NBfile();
                        replS.NBPath = _NBPathS;
                        replS.NBName = System.IO.Path.GetFileNameWithoutExtension(_NBPathS);
                        replS.NBisSounder = true;
                        replS.textPointer = nextInsS.GetInsertionPosition(LogicalDirection.Forward);

                        newNBs.Add(replS);

                        Trace.WriteLine("NBKey = " + _NBPathS);
                        Trace.WriteLine("NBfile Path = " + replS.NBPath);
                        Trace.WriteLine("NBfile Name = " + replS.NBName);
                        Trace.WriteLine("Checked Sounders Index: "+soundersIndex.ToString());

                        soundersIndex += 1;

                    }
                    else { MessageBox.Show("Key2 was blank in the Sounders discovery secion. wtf? This is a bug. Email Mason about it."); }
                }
                else { foundAllSounders = true; Trace.WriteLine("Found All Sounders."); }             
            }

// CLIPS DISCOVERY SECTION


            while (!foundAllClips)
            {
                searchRange = new TextRange(rtbScript.Document.ContentStart, rtbScript.Document.ContentEnd);
                TextRange foundKeyC1 = FindTextInRange(searchRange, String.Format("%#{0}", clipsIndex));
                if (foundKeyC1 != null)
                {
                    searchRange = new TextRange(rtbScript.Document.ContentStart, rtbScript.Document.ContentEnd);
                    TextRange foundKeyC2 = FindTextInRange(searchRange, String.Format("#%{0}", clipsIndex));

                    if (foundKeyC2 != null)
                    {
                        Trace.WriteLine("foundKeyC1 = " + foundKeyC1.Text);
                        Trace.WriteLine("foundKeyC2 = " + foundKeyC2.Text);

                        TextPointer pathStartC = foundKeyC1.End;
                        TextPointer pathEndC = foundKeyC2.Start;
                        TextPointer nextInsC = foundKeyC2.End;

                        TextRange NBKeyC = new TextRange(pathStartC, pathEndC);
                        string _NBPathC = NBKeyC.Text;

                        NBfile replC = new NBfile();
                        replC.NBPath = _NBPathC;
                        replC.NBName = System.IO.Path.GetFileNameWithoutExtension(_NBPathC);
                        replC.NBisSounder = false;
                        replC.textPointer = nextInsC.GetInsertionPosition(LogicalDirection.Forward);

                        newNBs.Add(replC);

                        
                        Trace.WriteLine("NBKey = " + _NBPathC);
                        Trace.WriteLine("NBfile Path = " + replC.NBPath);
                        Trace.WriteLine("NBfile Name = " + replC.NBName);
                        Trace.WriteLine("Checked Clips Index: " + clipsIndex.ToString());

                        clipsIndex += 1;

                    }
                    else { MessageBox.Show("Key2 was blank in the Clips discovery secion. wtf? This is a bug. Email Mason about it."); }
                }
                
                else { foundAllClips = true; Trace.WriteLine("Found All Clips"); }
            }




            // SOUNDERS DELETION SECTION

            for (int i = soundersIndex; i >= 0; i--)
            {

                searchRange = new TextRange(rtbScript.Document.ContentStart, rtbScript.Document.ContentEnd);
                TextRange foundKeySx1 = FindTextInRange(searchRange, String.Format("%${0}", i));
                if (foundKeySx1 != null)
                {
                    searchRange = new TextRange(rtbScript.Document.ContentStart, rtbScript.Document.ContentEnd);
                    TextRange foundKeySx2 = FindTextInRange(searchRange, String.Format("$%{0}", i));
                    if (foundKeySx2 != null)
                    {
                        Trace.WriteLine("foundKeySx1 = " + foundKeySx1.Text);
                        Trace.WriteLine("foundKeySx2 = " + foundKeySx2.Text);

                        TextPointer pathStartSx = foundKeySx1.Start;
                        TextPointer pathEndSx = foundKeySx2.End;

                        TextRange wipeOutSounders = new TextRange(pathStartSx, pathEndSx);

                        wipeOutSounders.Text = "";

                        Trace.WriteLine("Deleted sounders index: " + i.ToString());

                    }
                    else { MessageBox.Show("Key2 was blank in the Sounders deletion section. wtf? This is a bug. Email Mason about it."); }
                }
                else { Trace.WriteLine("Sounders Deletion Completed."); }
            }

            

// CLIPS DELETION SECTION


            for (int y = clipsIndex; y >= 0; y--)
            {

                searchRange = new TextRange(rtbScript.Document.ContentStart, rtbScript.Document.ContentEnd);
                TextRange foundKeyCx1 = FindTextInRange(searchRange, String.Format("%#{0}", y));
                if (foundKeyCx1 != null)
                {
                    searchRange = new TextRange(rtbScript.Document.ContentStart, rtbScript.Document.ContentEnd);
                    TextRange foundKeyCx2 = FindTextInRange(searchRange, String.Format("#%{0}", y));
                    if (foundKeyCx2 != null)
                    {
                        Trace.WriteLine("foundKeyCx1 = " + foundKeyCx1.Text);
                        Trace.WriteLine("foundKeyCx2 = " + foundKeyCx2.Text);

                        TextPointer pathStartCx = foundKeyCx1.Start;
                        TextPointer pathEndCx = foundKeyCx2.End;

                        TextRange wipeOutClips = new TextRange(pathStartCx, pathEndCx);

                        wipeOutClips.Text = "";

                        Trace.WriteLine("Deleted clips index: " + y.ToString());

                    }
                    else { MessageBox.Show("Key2 was blank in the Clips deletion section. wtf? This is a bug. Email Mason about it."); }
                }
                else { Trace.WriteLine("Clips Deletion Completed."); }
            }


            // RECREATION SECTION


            if (recreate)
            {
                foreach (NBfile nb in newNBs)
                {
                    
                    NBfileLocator nbLoc = new NBfileLocator();

                    nbLoc.textPointer = nb.textPointer;
                    nbLoc.file = nb;
                    NButton nButton = nb.NBbutton();
                    nButton.locator = nbLoc;
                    NBlocks.Add(nbLoc);

                    InlineUIContainer newNBbutton = new InlineUIContainer(nButton, nb.textPointer);
                    NBbuttons.Add(newNBbutton);
                    newNBbutton.Unloaded += NBbutton_Deleted;
                }

            }

        }


        // NEED TO FIGURE OUT HOW TO MAKE THIS NOT EFFECT BUTTONS WHEN THEY ARE MOVED IN THE TEXT.
        void NBbutton_Deleted(object sender, RoutedEventArgs e)
        {
            if (rtbScript.IsLoaded)
            {
                Dispatcher.Invoke((Action)delegate ()
                {
                    InlineUIContainer item = sender as InlineUIContainer;
                    if (NBbuttons.Contains(item))
                    {
                        NButton child = item.Child as NButton;
                        Trace.WriteLine("Removed NButton " + child.locator.file.NBName);
                        NBlocks.Remove(child.locator);
                        NBbuttons.Remove(item);
                    }
                    else { Trace.WriteLine("Error in deleting NB Button."); }
                });
            } else { Trace.WriteLine("Did not remove NBbutton " + e.OriginalSource.ToString()); }

        }


        // SEARCH FUNCTIONALITY
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



        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < NBbuttons.Count; i++)
            {
                InlineUIContainer Nbutt = NBbuttons[i];
                Nbutt.Unloaded -= NBbutton_Deleted;
                Trace.WriteLine("Unsubscribed button " + i + " from the 'unload' handler.");
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < NBbuttons.Count; i++)
            {
                InlineUIContainer Nbutt = NBbuttons[i];
                Nbutt.Unloaded += NBbutton_Deleted;
                Trace.WriteLine("Subscribed button " + i + " to the 'unload' handler.");
            }
        }
    }
}
