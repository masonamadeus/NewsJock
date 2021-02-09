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
        //List<InlineUIContainer> NBbuttons = new List<InlineUIContainer>();

        public Page1(bool fromTemplate, string uri = "")
        {
            InitializeComponent();
            rtbScript.AddHandler(RichTextBox.DragOverEvent, new DragEventHandler(Script_DragOver), true);
            rtbScript.AddHandler(RichTextBox.DropEvent, new DragEventHandler(Script_Drop), true);
            rtbScript.IsDocumentEnabled = true;
            selFontSize.ItemsSource = new List<Double>() { 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32, 34, 36, 38, 50, 60, 72 };

            if (fromTemplate)
            {
                OpenFromTemplate(uri);
            }

            MonitorDirectories(Settings.Default.ClipsDirectory);
            MonitorDirectories(Settings.Default.SoundersDirectory);
        }

        FileSystemWatcher fsP;

        private void MonitorDirectories(string dirPath)
        {
            fsP = new FileSystemWatcher(dirPath, "*.*");

            fsP.EnableRaisingEvents = true;
            fsP.IncludeSubdirectories = true;

            fsP.Renamed += new RenamedEventHandler(NBrenamed);
            fsP.Deleted += new FileSystemEventHandler(NBdeleted);
        }

        private void Script_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("NBfile"))
            {
                e.Effects = DragDropEffects.Copy;
            }
            e.Handled = false;

        }

        protected override void OnGiveFeedback(GiveFeedbackEventArgs e)
        {
            base.OnGiveFeedback(e);
            if (e.Effects.HasFlag(DragDropEffects.Copy))
            {
                Mouse.SetCursor(Cursors.IBeam);
            }
        }

        private void Script_Drop(object sender, DragEventArgs e)
        {
            //RichTextBox rtb = sender as RichTextBox;
            if (e.Data.GetDataPresent("NBfile"))
            {
                NBfile passNB = new NBfile();

                passNB = e.Data.GetData("NBfile") as NBfile;

                NButton nButton = passNB.NBbutton();
                nButton.file = passNB;

                InlineUIContainer nb = new InlineUIContainer(nButton, rtbScript.CaretPosition)
                {
                    Tag = passNB
                };

                e.Handled = true;
            }

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
            try
            {
                object temp = rtbScript.Selection.GetPropertyValue(Inline.FontWeightProperty);
                btnBold.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontWeights.Bold));
                temp = rtbScript.Selection.GetPropertyValue(Inline.FontStyleProperty);
                btnItal.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontStyles.Italic));
                temp = rtbScript.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
                btnUndr.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(TextDecorations.Underline));

                temp = rtbScript.Selection.GetPropertyValue(Paragraph.TextAlignmentProperty);
                btnAleft.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(TextAlignment.Left));
                btnAcenter.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(TextAlignment.Center));
                btnAright.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(TextAlignment.Right));

                temp = rtbScript.Selection.GetPropertyValue(Inline.FontSizeProperty);
                Paragraph startPara = rtbScript.Selection.Start.Paragraph;
                Paragraph endPara = rtbScript.Selection.End.Paragraph;
                if (startPara != null && (startPara.Parent is ListItem) && (endPara.Parent is ListItem) && object.ReferenceEquals(((ListItem)startPara.Parent).List, ((ListItem)endPara.Parent).List))
                {
                    TextMarkerStyle markerStyle = ((ListItem)startPara.Parent).List.MarkerStyle;
                    if (markerStyle == TextMarkerStyle.Disc)
                    {
                        btnBullet.IsChecked = true;
                        btnNumber.IsChecked = false;
                    }
                    else if (markerStyle == TextMarkerStyle.Decimal)
                    {
                        btnNumber.IsChecked = true;
                        btnBullet.IsChecked = false;
                    }
                }
                else
                {
                    btnNumber.IsChecked = false;
                    btnBullet.IsChecked = false;
                }

                double size;

                if (Double.TryParse(temp.ToString(), out size))
                { selFontSize.Text = temp.ToString(); }

            } catch
            {
                return;
            }

        }

        private void mnuSave_Click(object sender, RoutedEventArgs e)
        {
            VistaSaveFileDialog svD = new VistaSaveFileDialog
            {
                Filter = "xaml files (*.xaml)|*.xaml",
                DefaultExt = "xaml",
                InitialDirectory = Settings.Default.ScriptsDirectory,
                RestoreDirectory = true,
                Title = "Save Script"
            };

            if ((bool)svD.ShowDialog())
            {
                List<NBfile> NBs = new List<NBfile>();
                List<Inline> oldInlines = new List<Inline>();


                int clipIndex = 0;
                int sounderIndex = 0;


                // FIRST WE GO THROUGH, FIND ALL THE LOCATIONS OF THE EXISTING NB BUTTONS AND ADD THEIR FILES TO THE LIST NBs.
                // WE ALSO ADD THE INLINES TO THEIR OWN LIST SO WE CAN DELETE THEM NEXT.
                foreach (Paragraph para in rtbScript.Document.Blocks)
                {
                    foreach (Inline inline in para.Inlines)
                    {
                        if (inline.Tag is NBfile)
                        {
                            NBfile inlNB = inline.Tag as NBfile;
                            NBfile newNB = new NBfile
                            {
                                NBName = inlNB.NBName,
                                NBPath = inlNB.NBPath,
                                NBisSounder = inlNB.NBisSounder,

                                //THIS WILL GET THE INSERTION POSITION FOR THE REPLACEMENT NAME.
                                textPointer = inline.ElementStart.GetNextInsertionPosition(LogicalDirection.Forward)
                            };

                            NBs.Add(newNB);
                            oldInlines.Add(inline);
                        }
                    }
                }


                for (int i = 0; i < oldInlines.Count; i++)
                {
                    List<Object> blocks = new List<Object>();

                    Inline NBbutt = oldInlines[i];

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
                            paragraph.Inlines.Remove(NBbutt);
                        }
                    }
                }

                // then go through and do the replacement. we're doing it in this order so that text pointers dont get too frigged off.

                for (int ii = 0; ii < NBs.Count; ii++)
                {
                    NBfile nb = NBs[ii];

                    if (nb.NBisSounder)
                    {
                        string repname = nb.GetIDs(nb, sounderIndex);

                        rtbScript.CaretPosition = nb.textPointer;

                        rtbScript.CaretPosition.InsertTextInRun(repname);

                        Trace.WriteLine("Inserted Sounder " + System.IO.Path.GetFileNameWithoutExtension(repname) + " from index " + ii + " with ID Number " + sounderIndex);

                        sounderIndex += 1;
                    }
                    else
                    {
                        string repname = nb.GetIDc(nb, clipIndex);

                        rtbScript.CaretPosition = nb.textPointer;

                        rtbScript.CaretPosition.InsertTextInRun(repname);

                        Trace.WriteLine("Inserted Clip " + System.IO.Path.GetFileNameWithoutExtension(repname) + " from index " + ii + " with ID Number" + clipIndex);

                        clipIndex += 1;
                    }

                }

                // now its time to actually save the stupid thing out. savey savey save save.

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
            opn.InitialDirectory = Settings.Default.ScriptsDirectory;
            opn.RestoreDirectory = true;
            opn.Title = "Open a Script";
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

        public void OpenFromTemplate(string uri)
        {
            TextRange range = new TextRange(rtbScript.Document.ContentStart, rtbScript.Document.ContentEnd);
            FileStream fs;
            if (File.Exists(uri))
            {
                fs = new FileStream(uri, FileMode.OpenOrCreate);
                range.Load(fs, DataFormats.XamlPackage);
                fs.Close();
                RestoreNBfiles(true);
            } else { MessageBox.Show("This file is busted. Sorry!"); }
        }

        public void RestoreNBfiles(bool recreate)
        {
            List<NBfile> newNBs = new List<NBfile>();
            newNBs.Clear();

            bool foundAllSounders = false;
            bool foundAllClips = false;
            int soundersIndex = 0;
            int clipsIndex = 0;


            // SOUNDERS DISCOVERY SECTION */ /*

            while (!foundAllSounders)
            {
                TextRange foundKeyS1 = FindStringRangeFromPosition(rtbScript.Document.ContentStart, String.Format("%@!${0}", soundersIndex));
                if (foundKeyS1 != null)
                {
                    TextRange foundKeyS2 = FindStringRangeFromPosition(rtbScript.Document.ContentStart, String.Format("$@!%{0}", soundersIndex));

                    if (foundKeyS2 != null)
                    {

                        TextPointer pathStartS = foundKeyS1.End;
                        TextPointer pathEndS = foundKeyS2.Start;
                        TextPointer nextInsS = foundKeyS2.End;

                        TextRange NBKeyS = new TextRange(pathStartS, pathEndS);
                        string _NBPathS = NBKeyS.Text;

                        Trace.WriteLine("foundKeyS1 = " + foundKeyS1.Text);
                        Trace.WriteLine("foundKeyS2 = " + foundKeyS2.Text);

                        NBfile replS = new NBfile();
                        replS.NBPath = _NBPathS;
                        replS.NBName = System.IO.Path.GetFileNameWithoutExtension(_NBPathS);
                        replS.NBisSounder = true;
                        replS.textPointer = nextInsS.GetNextInsertionPosition(LogicalDirection.Backward);


                        newNBs.Add(replS);

                        Trace.WriteLine("NBKey = " + _NBPathS);
                        Trace.WriteLine("NBfile Path = " + replS.NBPath);
                        Trace.WriteLine("NBfile Name = " + replS.NBName);
                        Trace.WriteLine("Checked Sounders Index: " + soundersIndex.ToString());

                        soundersIndex += 1;

                    }
                    else { MessageBox.Show("Key2 was blank in the Sounders discovery secion. wtf? This is a bug. Email Mason about it."); }
                }
                else { foundAllSounders = true; Trace.WriteLine("Found All Sounders."); }
            }

            // CLIPS DISCOVERY SECTION */ /*


            while (!foundAllClips)
            {
                TextRange foundKeyC1 = FindStringRangeFromPosition(rtbScript.Document.ContentStart, String.Format("%@!#{0}", clipsIndex));
                if (foundKeyC1 != null)
                {
                    TextRange foundKeyC2 = FindStringRangeFromPosition(rtbScript.Document.ContentStart, String.Format("#@!%{0}", clipsIndex));

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
                        replC.textPointer = nextInsC.GetNextInsertionPosition(LogicalDirection.Backward);

                        //InlineUIContainer newNBC = new InlineUIContainer(replC.NBbutton(), nextInsC.GetNextInsertionPosition(LogicalDirection.Forward));
                        //newNBC.Tag = replC;

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




            // SOUNDERS DELETION SECTION */ 


            for (int i = soundersIndex; i >= 0; i--)
            {

                TextRange foundKeySx1 = FindStringRangeFromPosition(rtbScript.Document.ContentStart, String.Format("%@!${0}", i));
                if (foundKeySx1 != null)
                {
                    TextRange foundKeySx2 = FindStringRangeFromPosition(rtbScript.Document.ContentStart, String.Format("$@!%{0}", i));
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



            // CLIPS DELETION SECTION */ /*


            for (int y = clipsIndex; y >= 0; y--)
            {

                TextRange foundKeyCx1 = FindStringRangeFromPosition(rtbScript.Document.ContentStart, String.Format("%@!#{0}", y));
                if (foundKeyCx1 != null)
                {
                    TextRange foundKeyCx2 = FindStringRangeFromPosition(rtbScript.Document.ContentStart, String.Format("#@!%{0}", y));
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
                    if (File.Exists(nb.NBPath))
                    {
                        NButton nButton = nb.NBbutton();
                        nButton.file = nb;

                        InlineUIContainer newNBbutton = new InlineUIContainer(nButton, nb.textPointer);
                        newNBbutton.Tag = nb;
                    }
                    else
                    {
                        var bc = new BrushConverter();

                        InlineUIContainer newFailure = new InlineUIContainer(new Button() { Content = "ERROR", Background = (Brush)bc.ConvertFrom("#ff0000") }, nb.textPointer);
                    }


                }

            }

        }

        //  SEARCH FUNCTIONALITY VERSION 2
        public static TextRange FindStringRangeFromPosition(TextPointer position, string matchStr, bool isCaseSensitive = false)
        {
            int curIdx = 0;
            TextPointer startPointer = null;
            StringComparison stringComparison = isCaseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;
            while (position != null)
            {
                if (position.GetPointerContext(LogicalDirection.Forward) != TextPointerContext.Text)
                {
                    if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.EmbeddedElement)
                    {
                        var inlineUIelement = position.Parent;
                        //handle inlineUIelement.Child contents here...
                    }
                    position = position.GetNextContextPosition(LogicalDirection.Forward);
                    continue;
                }
                var runStr = position.GetTextInRun(LogicalDirection.Forward);
                if (string.IsNullOrEmpty(runStr))
                {
                    position = position.GetNextContextPosition(LogicalDirection.Forward);
                    continue;
                }
                //only concerned with current character of match string
                int runIdx = runStr.IndexOf(matchStr[curIdx].ToString(), stringComparison);
                if (runIdx == -1)
                {
                    //if no match found reset search
                    curIdx = 0;
                    if (startPointer == null)
                    {
                        position = position.GetNextContextPosition(LogicalDirection.Forward);
                    }
                    else
                    {
                        //when no match somewhere after first character reset search to the position AFTER beginning of last partial match
                        position = startPointer.GetPositionAtOffset(1, LogicalDirection.Forward);
                        startPointer = null;
                    }
                    continue;
                }
                if (curIdx == 0)
                {
                    //beginning of range found at runIdx
                    startPointer = position.GetPositionAtOffset(runIdx, LogicalDirection.Forward);
                }
                if (curIdx == matchStr.Length - 1)
                {
                    //each character has been matched
                    var endPointer = position.GetPositionAtOffset(runIdx, LogicalDirection.Forward);
                    //for edge cases of repeating characters these loops ensure start is not early and last character isn't lost 
                    if (isCaseSensitive)
                    {
                        while (endPointer != null && !new TextRange(startPointer, endPointer).Text.Contains(matchStr))
                        {
                            endPointer = endPointer.GetPositionAtOffset(1, LogicalDirection.Forward);
                        }
                    }
                    else
                    {
                        while (endPointer != null && !new TextRange(startPointer, endPointer).Text.ToLower().Contains(matchStr.ToLower()))
                        {
                            endPointer = endPointer.GetPositionAtOffset(1, LogicalDirection.Forward);
                        }
                    }
                    if (endPointer == null)
                    {
                        return null;
                    }
                    while (startPointer != null && new TextRange(startPointer, endPointer).Text.Length > matchStr.Length)
                    {
                        startPointer = startPointer.GetPositionAtOffset(1, LogicalDirection.Forward);
                    }
                    if (startPointer == null)
                    {
                        return null;
                    }
                    return new TextRange(startPointer, endPointer);
                }
                else
                {
                    //prepare loop for next match character
                    curIdx++;
                    //iterate position one offset AFTER match offset
                    position = position.GetPositionAtOffset(runIdx + 1, LogicalDirection.Forward);
                }
            }
            return null;
        }

        public void NBrenamed2(object sender, RenamedEventArgs e)
        {
            foreach (Paragraph para in rtbScript.Document.Blocks)
            {
                foreach (Inline inline in para.Inlines)
                {
                    if (inline.Tag is NBfile)
                    {
                        NBfile nb = inline.Tag as NBfile;
                        if (nb.NBPath == e.OldFullPath)
                        {
                            nb.NBPath = e.FullPath;
                            nb.NBName = System.IO.Path.GetFileNameWithoutExtension(e.FullPath);
                        }
                    }
                }
            }
        }

        void NBrenamed(object sender, RenamedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                NBrenamed2(sender, e);
            });
        }
        void NBdeleted(object sender, FileSystemEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                NBdeleted2(sender, e);
            });
        }

        public void NBdeleted2(object sender, FileSystemEventArgs e)
        {
            List<Inline> deletedNBs = new List<Inline>();


            foreach (Paragraph para in rtbScript.Document.Blocks)
            {

                foreach (Inline inline in para.Inlines)
                {
                    if (inline.Tag is NBfile)
                    {
                        NBfile nb = inline.Tag as NBfile;
                        if (nb.NBPath == e.FullPath)
                        {
                            deletedNBs.Add(inline);
                        }
                    }
                }
            }

            for (int d = 0; d < deletedNBs.Count; d++)
            {
                Inline del = deletedNBs[d];
                List<Paragraph> blocks = new List<Paragraph>();
                foreach (Paragraph block in rtbScript.Document.Blocks)
                {
                    blocks.Add(block);
                }

                for (int dd = 0; dd < blocks.Count; dd++)
                {
                    Paragraph bl = blocks[dd];
                    if (bl.Inlines.Contains(del))
                    {
                        bl.Inlines.Remove(del);
                    }
                }

            }
        }

        private void mnuSaveTemplate_Click(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine(Settings.Default.TemplatesDirectory);
            VistaSaveFileDialog svD = new VistaSaveFileDialog
            {
                Filter = "xaml files (*.xaml)|*.xaml",
                DefaultExt = "xaml",
                InitialDirectory = Settings.Default.TemplatesDirectory,
                RestoreDirectory = true,
                Title = "Save Template"
            };

            if ((bool)svD.ShowDialog())
            {
                List<NBfile> NBs = new List<NBfile>();
                List<Inline> oldInlines = new List<Inline>();


                int clipIndex = 0;
                int sounderIndex = 0;


                // FIRST WE GO THROUGH, FIND ALL THE LOCATIONS OF THE EXISTING NB BUTTONS AND ADD THEIR FILES TO THE LIST NBs.
                // WE ALSO ADD THE INLINES TO THEIR OWN LIST SO WE CAN DELETE THEM NEXT.
                foreach (Paragraph para in rtbScript.Document.Blocks)
                {
                    foreach (Inline inline in para.Inlines)
                    {
                        if (inline.Tag is NBfile)
                        {
                            NBfile inlNB = inline.Tag as NBfile;
                            NBfile newNB = new NBfile
                            {
                                NBName = inlNB.NBName,
                                NBPath = inlNB.NBPath,
                                NBisSounder = inlNB.NBisSounder,

                                //THIS WILL GET THE INSERTION POSITION FOR THE REPLACEMENT NAME.
                                textPointer = inline.ElementStart.GetNextInsertionPosition(LogicalDirection.Forward)
                            };

                            NBs.Add(newNB);
                            oldInlines.Add(inline);
                        }
                    }
                }


                for (int i = 0; i < oldInlines.Count; i++)
                {
                    List<Object> blocks = new List<Object>();

                    Inline NBbutt = oldInlines[i];

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
                            paragraph.Inlines.Remove(NBbutt);
                        }
                    }
                }

                // then go through and do the replacement. we're doing it in this order so that text pointers dont get too frigged off.

                for (int ii = 0; ii < NBs.Count; ii++)
                {
                    NBfile nb = NBs[ii];

                    if (nb.NBisSounder)
                    {
                        string repname = nb.GetIDs(nb, sounderIndex);

                        rtbScript.CaretPosition = nb.textPointer;

                        rtbScript.CaretPosition.InsertTextInRun(repname);

                        Trace.WriteLine("Inserted Sounder " + System.IO.Path.GetFileNameWithoutExtension(repname) + " from index " + ii + " with ID Number " + sounderIndex);

                        sounderIndex += 1;
                    }
                    else
                    {
                        string repname = nb.GetIDc(nb, clipIndex);

                        rtbScript.CaretPosition = nb.textPointer;

                        rtbScript.CaretPosition.InsertTextInRun(repname);

                        Trace.WriteLine("Inserted Clip " + System.IO.Path.GetFileNameWithoutExtension(repname) + " from index " + ii + " with ID Number" + clipIndex);

                        clipIndex += 1;
                    }

                }

                // now its time to actually save the stupid thing out. savey savey save save.

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
    }
    
}
