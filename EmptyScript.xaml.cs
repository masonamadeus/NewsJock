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
using System.Windows.Markup;
using System.ComponentModel;
using Microsoft.Win32;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ookii.Dialogs.Wpf;
using System.Linq;

namespace NewsBuddy
{

    public partial class Page1 : Page
    {
        FileSystemWatcher fsP;
        public string scriptUri { get; set; }
        public bool isChanged { get; set; }

        private string fileFilter = "NewsJock Scripts (*.njs)|*.njs | XAML Files (*.xaml)|*.xaml";


        public Page1(bool fromTemplate, string uri = "")
        {
            InitializeComponent();

            rtbScript.AddHandler(RichTextBox.DragOverEvent, new DragEventHandler(Script_DragOver), true);
            rtbScript.AddHandler(RichTextBox.DropEvent, new DragEventHandler(Script_Drop), true);
            rtbScript.IsDocumentEnabled = true;
            selFontSize.ItemsSource = new List<Double>() { 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32, 34, 36, 38, 50, 60, 72 };
            selFontSize.Focusable = false;

            if (fromTemplate)
            {
                scriptUri = uri;
                OpenXamlFromTemplate(uri);
            }

            isChanged = false;

            MonitorDirectories(Settings.Default.ClipsDirectory);
            MonitorDirectories(Settings.Default.SoundersDirectory);
        }

        public Page1(List<APChunk> chunks)
        {
            InitializeComponent();

            rtbScript.AddHandler(RichTextBox.DragOverEvent, new DragEventHandler(Script_DragOver), true);
            rtbScript.AddHandler(RichTextBox.DropEvent, new DragEventHandler(Script_Drop), true);
            rtbScript.IsDocumentEnabled = true;
            selFontSize.ItemsSource = new List<Double>() { 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32, 34, 36, 38, 50, 60, 72 };
            selFontSize.Focusable = false;
            TextRange doc = new TextRange(rtbScript.Document.ContentStart, rtbScript.Document.ContentEnd);
            doc.Text = "";
            rtbScript.CaretPosition = rtbScript.Document.ContentStart.DocumentStart;

            isChanged = false;

            InsertChunksFromIngestor(chunks);

            MonitorDirectories(Settings.Default.ClipsDirectory);
            MonitorDirectories(Settings.Default.SoundersDirectory);
        }

        #region Saving & Loading

        public void SaveTemplateXaml(object sender, RoutedEventArgs e)
        {
            SaveFileDialog svD = new SaveFileDialog
            {
                Filter = fileFilter,
                DefaultExt = "xaml",
                InitialDirectory = Settings.Default.TemplatesDirectory,
                RestoreDirectory = true,
                Title = "Save Template"
            };

            if ((bool)svD.ShowDialog())
            {
                ToggleLoading(true);
                if (scriptUri == null)
                {
                    scriptUri = svD.FileName;
                }
                string fileName = svD.FileName;
                //StartNewWorker(new NJSaveFile(rtbScript.Document, svD.FileName), true);


                FileStream fs = new FileStream(fileName, FileMode.Create);
                XamlWriter.Save(rtbScript.Document, fs);
                fs.Close();
                ((MainWindow)Application.Current.MainWindow).ChangeTabName(scriptUri);
                isChanged = false;

            }

            ToggleLoading(false);
        }

        public void SaveAsXaml(object sender, RoutedEventArgs e)
        {
            SaveFileDialog svD = new SaveFileDialog
            {
                Filter = fileFilter,
                DefaultExt = "xaml",
                InitialDirectory = Settings.Default.ScriptsDirectory,
                RestoreDirectory = true,
                Title = "Save Script"
            };

            if ((bool)svD.ShowDialog())
            {
                ToggleLoading(true);

                scriptUri = svD.FileName;
                //StartNewWorker(new NJSaveFile(rtbScript.Document, svD.FileName), true);

                // now its time to actually save the stupid thing out. savey savey save save.
                string fileName = svD.FileName;
                FileStream fs;
                fs = new FileStream(fileName, FileMode.Create);
                XamlWriter.Save(rtbScript.Document, fs);
                fs.Close();
                ((MainWindow)Application.Current.MainWindow).ChangeTabName(scriptUri);

                isChanged = false;

            }
            ToggleLoading(false);
        }

        public void SaveXaml(object sender, RoutedEventArgs e)
        {
            string dir = "no";

            if (scriptUri != null && File.Exists(scriptUri))
            {
                dir = System.IO.Path.GetDirectoryName(scriptUri);
            }

            if (dir != "no" && dir != Settings.Default.TemplatesDirectory && File.Exists(scriptUri))
            {
                //StartNewWorker(new NJSaveFile(rtbScript.Document, scriptUri), true);

                ToggleLoading(true);
                string fileName = scriptUri;
                FileStream fs;
                fs = new FileStream(fileName, FileMode.Create);
                XamlWriter.Save(rtbScript.Document, fs);
                fs.Close();
                isChanged = false;

            }
            else
            {
                SaveFileDialog svD = new SaveFileDialog
                {
                    Filter = fileFilter,
                    DefaultExt = "njs",
                    InitialDirectory = Settings.Default.ScriptsDirectory,
                    RestoreDirectory = true,
                    Title = "Save Script"
                };

                if ((bool)svD.ShowDialog())
                {
                    ToggleLoading(true);

                    scriptUri = svD.FileName;
                    //StartNewWorker(new NJSaveFile(rtbScript.Document, svD.FileName), true);


                    // now its time to actually save the stupid thing out. savey savey save save.
                    string fileName = svD.FileName;
                    FileStream fs;
                    fs = new FileStream(fileName, FileMode.Create);
                    XamlWriter.Save(rtbScript.Document, fs);
                    fs.Close();
                    ((MainWindow)Application.Current.MainWindow).ChangeTabName(scriptUri);
                    isChanged = false;

                }
            }
            ToggleLoading(false);
        }

        public void OpenXaml(object sender, RoutedEventArgs e)
        {
            OpenFileDialog opn = new OpenFileDialog();
            opn.InitialDirectory = Settings.Default.ScriptsDirectory;
            opn.RestoreDirectory = true;
            opn.Title = "Open a Script";
            opn.Filter = fileFilter;
            if ((bool)opn.ShowDialog())
            {

                ToggleLoading(true);
                try
                {
                    if (opn.CheckFileExists)
                    {
                        //StartNewWorker(new NJSaveFile(rtbScript.Document, opn.FileName), false);

                        FileStream fs;
                        fs = new FileStream(opn.FileName, FileMode.Open);
                        FlowDocument loaded = XamlReader.Load(fs) as FlowDocument;
                        fs.Close();
                        scriptUri = opn.FileName;
                        rtbScript.Document = loaded;
                        RestoreNBXaml();
                        isChanged = false;
                    }
                }
                catch
                {
                    MessageBox.Show("The selected file could not be opened.", "Error Opening File", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            ToggleLoading(false);
        }

        public void OpenXamlFromTemplate(string uri)
        {

            if (File.Exists(uri))
            {
                //StartNewWorker(new NJSaveFile(rtbScript.Document, uri), false);
                try
                {
                    ToggleLoading(true);
                    FileStream fs = new FileStream(uri, FileMode.Open);
                    FlowDocument loaded = XamlReader.Load(fs) as FlowDocument;
                    fs.Close();
                    rtbScript.Document = loaded;
                    scriptUri = uri;
                    RestoreNBXaml();
                    isChanged = false;
                }
                catch
                {
                    MessageBox.Show("The selected file could not be opened.", "Error Opening File", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
            else { MessageBox.Show("This file is busted. Sorry!"); }

            ToggleLoading(false);
        }


        #endregion

        #region Script Interaction

        /// <summary>
        /// Recieves a list of APChunks and parses them into the rich text document.
        /// </summary>
        /// <param name="chunks"></param>
        public void InsertChunksFromIngestor(List<APChunk> chunks)
        {
            List<Paragraph> newParas = new List<Paragraph>();
            for (int x = 0; x < chunks.Count; x++)
            {
                APChunk chunk = chunks[x];
                Paragraph text = new Paragraph();
                if (chunk.chunk_fontWeight == FontWeights.Bold || chunk.chunk_fontStyle == FontStyles.Italic)
                {
                    text.Inlines.Add(chunk.chunk_text);
                }
                else
                {
                    text.Inlines.Add("     " + chunk.chunk_text);
                }
                text.FontStyle = chunk.chunk_fontStyle;
                text.FontWeight = chunk.chunk_fontWeight;
                text.FontSize = chunk.chunk_fontSize;
                text.TextAlignment = chunk.chunk_textAlignment;
                newParas.Add(text);
            }

            if (!rtbScript.Selection.IsEmpty)
            {
                rtbScript.Selection.Text = "";
            }

            if (rtbScript.CaretPosition == null)
            {
                rtbScript.CaretPosition = rtbScript.Document.ContentStart;
            }

            for (int i = 0; i < newParas.Count; i++)
            {
                if (rtbScript.CaretPosition.Paragraph == null)
                {
                    rtbScript.Document.Blocks.Add(new Paragraph());
                }
                if (rtbScript.CaretPosition.Paragraph.Parent is ListItem)
                {
                    ListItem nextItem = rtbScript.CaretPosition.Paragraph.Parent as ListItem;
                    nextItem.Blocks.InsertBefore(rtbScript.CaretPosition.Paragraph, newParas[i]);
                }
                else
                {
                    rtbScript.Document.Blocks.InsertBefore(rtbScript.CaretPosition.Paragraph, newParas[i]);

                }

            }

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
                isChanged = true;
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
                if (startPara != null && endPara != null && (startPara.Parent is ListItem) && (endPara.Parent is ListItem) && object.ReferenceEquals(((ListItem)startPara.Parent).List, ((ListItem)endPara.Parent).List))
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

            }
            catch (NullReferenceException _)
            {
                return;
            }
            catch
            {
                return;
            }

        }

        // used to determine if need to prompt on close
        private void rtbScript_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!isChanged)
            {
                isChanged = true;
            }
        }

        private void rtbScript_Pasting(object sender, DataObjectPastingEventArgs e)
        {

            if (e.FormatToApply == "Bitmap")
            {
                try
                {
                    e.FormatToApply = "FileName";
                }
                catch
                {
                    e.CancelCommand();
                }
            }
            else
            if (e.DataObject.GetDataPresent("Text"))
            {
                e.FormatToApply = "Text";
                Trace.WriteLine("Pasting as text");
            }

            // Can remove later
            if (e.DataObject.GetDataPresent("NBcount"))
            {
                int count = (int)e.DataObject.GetData("NBcount");
                Trace.WriteLine("NBcount is present.");
                for (int gg = 0; gg < count; gg++)
                {
                    string datacall = (string)e.DataObject.GetData(String.Format("NB{0}", gg));
                    bool sounder = true;
                    if (datacall.Contains(Settings.Default.ClipsDirectory))
                    {
                        sounder = false;
                    }

                    Trace.WriteLine("Gonna Paste " + datacall);
                    NBfile passNB = new NBfile(datacall, sounder);
                    NButton nButton = passNB.NBbutton();
                    nButton.file = passNB;

                    InlineUIContainer nb = new InlineUIContainer(nButton, rtbScript.CaretPosition)
                    {
                        Tag = passNB
                    };
                }


            }

            if (Debugger.IsAttached)
            {
                foreach (string s in e.DataObject.GetFormats())
                {
                    Trace.WriteLine(s);
                }
            }

        }

        private void rtbScript_Copying(object sender, DataObjectCopyingEventArgs e)
        {

            List<NBfile> foundNBfiles = new List<NBfile>();

            List<Inline> found = DetectNBs(rtbScript.Document.Blocks);

            for (int xy = 0; xy < found.Count; xy++)
            {
                TextPointer examine = found[xy].ContentStart;
                if (rtbScript.Selection.Contains(examine))
                {
                    Trace.WriteLine("Found NB in selection");
                    foundNBfiles.Add((NBfile)found[xy].Tag);

                }
            }

            if (foundNBfiles.Count > 0)
            {
                e.DataObject.SetData("NBcount", foundNBfiles.Count);
                for (int ix = 0; ix < foundNBfiles.Count; ix++)
                {
                    NBfile tempNB = foundNBfiles[ix];
                    e.DataObject.SetData(String.Format("NB{0}", ix), tempNB.NBPath);
                }
            }


        }

        private void rtbScript_Loaded(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("Script Reloaded.");
            this.Dispatcher.Invoke(() => {
                RestoreNBXaml();
            });
        }


        #endregion

        #region Utility Methods


        /// <summary>
        /// Restores all sound cue buttons to a working state.
        /// </summary>
        public void RestoreNBXaml()
        {
            List<Inline> nbInlines = DetectNBs(rtbScript.Document.Blocks);

            for (int i = 0; i < nbInlines.Count; i++)
            {
                Inline inl = nbInlines[i];
                InlineUIContainer nbi = inl as InlineUIContainer;
                NBfile nb = inl.Tag as NBfile;
                nbi.Child = nb.NBbutton();
            }
            Trace.WriteLine("Restoring NBs");
        }

        /// <summary>
        /// If true, overrides the mouse cursor to the spinning circle. If false, returns mouse cursor to normal state.
        /// </summary>
        /// <param name="show"></param>
        public void ToggleLoading(bool show)
        {
            if (show)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                rtbScript.IsEnabled = false;
                progBar.Visibility = Visibility.Visible;
                topMenu.Visibility = Visibility.Hidden;
            }
            else
            {
                rtbScript.IsEnabled = true;
                Mouse.OverrideCursor = null;
                progBar.Visibility = Visibility.Collapsed;
                topMenu.Visibility = Visibility.Visible;
            }
        }

        private void MonitorDirectories(string dirPath)
        {
            fsP = new FileSystemWatcher(dirPath, "*.*");

            fsP.EnableRaisingEvents = true;
            fsP.IncludeSubdirectories = true;

            fsP.Renamed += new RenamedEventHandler(NBrenamed);
            fsP.Deleted += new FileSystemEventHandler(NBdeleted);
        }

        void NBrenamed(object sender, RenamedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                List<Inline> foundInlines = DetectNBs(rtbScript.Document.Blocks);

                for (int line = 0; line < foundInlines.Count; line++)
                {
                    Inline examine = foundInlines[line];

                    NBfile nb = examine.Tag as NBfile;
                    if (nb.NBPath == e.OldFullPath)
                    {
                        nb.NBPath = e.FullPath;
                        nb.NBName = System.IO.Path.GetFileNameWithoutExtension(e.FullPath);
                    }

                }
            });
        }

        void NBdeleted(object sender, FileSystemEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                List<Inline> deletedNBs = new List<Inline>();
                List<Inline> foundInlines = DetectNBs(rtbScript.Document.Blocks);


                for (int line = 0; line < foundInlines.Count; line++)
                {
                    Inline examine = foundInlines[line];

                    NBfile nb = examine.Tag as NBfile;
                    if (nb.NBPath == e.FullPath)
                    {
                        deletedNBs.Add(examine);
                    }

                }

                for (int d = 0; d < deletedNBs.Count; d++)
                {
                    Inline del = deletedNBs[d];

                    List<Paragraph> blocks = DetectParagraphs(rtbScript.Document.Blocks);

                    for (int dd = 0; dd < blocks.Count; dd++)
                    {

                        Paragraph bl = blocks[dd];
                        if (bl.Inlines.Contains(del))
                        {
                            bl.Inlines.Remove(del);
                        }

                    }

                }
            });
        }

        // Returns a list of every paragraph chunk of text, recursively searching through lists.
        private List<Paragraph> DetectParagraphs(BlockCollection blocks, int depth = 0)
        {
            Trace.WriteLine(String.Format("[{0}]Detect Paragraph Loop Started", depth));
            List<Paragraph> paragraphs = new List<Paragraph>();
            foreach (var block in blocks)
            {
                if (block is Paragraph)
                {
                    Trace.WriteLine(String.Format("[{0}]Found Paragraph", depth));
                    paragraphs.Add((Paragraph)block);
                }
                if (block is List)
                {
                    Trace.WriteLine(String.Format("[{0}]Found List", depth));
                    foreach (ListItem listItems in ((List)block).ListItems)
                    {
                        Trace.WriteLine(String.Format("[{0}]Starting loop for list item", depth));
                        paragraphs.AddRange(DetectParagraphs(listItems.Blocks, depth + 1));
                    }
                }
            }
            return paragraphs;

        }

        // Returns a list of every inline containing an NB file. Recursively searches lists.
        private List<Inline> DetectNBs(BlockCollection blocks, int depth = 0)
        {
            Trace.WriteLine(String.Format("[{0}]DetectNBs Loop Started", depth));
            List<Inline> inlines = new List<Inline>();
            foreach (var block in blocks)
            {
                if (block is Paragraph)
                {
                    Trace.WriteLine(String.Format("[{0}]Found Paragraph", depth));
                    foreach (Inline inl in ((Paragraph)block).Inlines)
                    {
                        if (inl.Tag is NBfile)
                        {
                            Trace.WriteLine(String.Format("[{0}]Found NB File", depth));
                            inlines.Add(inl);
                        }

                    }
                }
                if (block is List)
                {
                    Trace.WriteLine(String.Format("[{0}]Found List", depth));
                    foreach (ListItem listItems in ((List)block).ListItems)
                    {
                        Trace.WriteLine(String.Format("[{0}]Starting loop for list item", depth));
                        inlines.AddRange(DetectNBs(listItems.Blocks, depth + 1));
                    }
                }
            }
            return inlines;
        }


        #endregion

        #region User Interface
        void DarkMode_Toggle(object sender, RoutedEventArgs e)
        {
            if (btnDark.IsChecked == true)
            {
                rtbScript.Background = Brushes.Black;
                rtbScript.Foreground = Brushes.White;
            }
            else if (btnDark.IsChecked == false)
            {
                rtbScript.Background = Brushes.White;
                rtbScript.Foreground = Brushes.Black;
            }
        }

        #endregion











    }

}
