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

namespace NewsBuddy
{

    public partial class Page1 : Page
    {
        public string scriptUri { get; set; }

        public Page1(bool fromTemplate, string uri = "")
        {
            InitializeComponent();

            rtbScript.AddHandler(RichTextBox.DragOverEvent, new DragEventHandler(Script_DragOver), true);
            rtbScript.AddHandler(RichTextBox.DropEvent, new DragEventHandler(Script_Drop), true);
            rtbScript.IsDocumentEnabled = true;
            selFontSize.ItemsSource = new List<Double>() { 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32, 34, 36, 38, 50, 60, 72 };

            if (fromTemplate)
            {
                scriptUri = uri;
                OpenXamlFromTemplate(uri);
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

            }
            catch (NullReferenceException dump)
            {
                return;
            }
            catch
            {
                return;
            }

        }

        public void SaveTemplateXaml(object sender, RoutedEventArgs e)
        {
            SaveFileDialog svD = new SaveFileDialog
            {
                Filter = "xaml files (*.xaml)|*.xaml",
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

            }
            ToggleLoading(false);
        }

        public void SaveAsXaml(object sender, RoutedEventArgs e)
        {
            SaveFileDialog svD = new SaveFileDialog
            {
                Filter = "xaml files (*.xaml)|*.xaml",
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

            }
            else
            {
                SaveFileDialog svD = new SaveFileDialog
                {
                    Filter = "xaml files (*.xaml)|*.xaml",
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
            if ((bool)opn.ShowDialog())
            {

                ToggleLoading(true);
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
                }
            }

            ToggleLoading(false);
        }

        public void OpenXamlFromTemplate(string uri)
        {

            if (File.Exists(uri))
            {
                //StartNewWorker(new NJSaveFile(rtbScript.Document, uri), false);
                ToggleLoading(true);
                FileStream fs = new FileStream(uri, FileMode.Open);
                FlowDocument loaded = XamlReader.Load(fs) as FlowDocument;
                fs.Close();
                rtbScript.Document = loaded;
                scriptUri = uri;
                RestoreNBXaml();
            }
            else { MessageBox.Show("This file is busted. Sorry!"); }

            ToggleLoading(false);
        }

        void RestoreNBXaml()
        {
            List<Inline> nbInlines = new List<Inline>();

            foreach (var block in rtbScript.Document.Blocks)
            {
                if (block is Paragraph)
                {
                    Paragraph para = block as Paragraph;
                    foreach (Inline inline in para.Inlines)
                    {
                        if (inline.Tag is NBfile)
                        {
                            nbInlines.Add(inline);
                        }
                    }
                }

                if (block is List)
                {
                    List lol = block as List;
                    foreach (ListItem tip in lol.ListItems)
                    {
                        foreach (Paragraph pl in tip.Blocks)
                        {
                            foreach (Inline inl in pl.Inlines)
                            {
                                if (inl.Tag is NBfile)
                                {
                                    nbInlines.Add(inl);
                                }
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < nbInlines.Count; i++)
            {
                Inline inl = nbInlines[i];
                InlineUIContainer nbi = inl as InlineUIContainer;
                NBfile nb = inl.Tag as NBfile;
                nbi.Child = nb.NBbutton();
            }
            ((MainWindow)Application.Current.MainWindow).ChangeTabName(scriptUri);
        }

        public void NBrenamed2(object sender, RenamedEventArgs e)
        {
            foreach (var block in rtbScript.Document.Blocks)
            {
                if (block is Paragraph)
                {
                    Paragraph para = block as Paragraph;
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

                if (block is List)
                {
                    List lol = block as List;
                    foreach (ListItem tip in lol.ListItems)
                    {
                        foreach (Paragraph par in tip.Blocks)
                        {
                            foreach (Inline il in par.Inlines)
                            {
                                if (il.Tag is NBfile)
                                {
                                    NBfile nbb = il.Tag as NBfile;
                                    if (nbb.NBPath == e.OldFullPath)
                                    {
                                        nbb.NBPath = e.FullPath;
                                        nbb.NBName = System.IO.Path.GetFileNameWithoutExtension(e.FullPath);
                                    }
                                }
                            }
                        }
                    }
                }

            }
        }

        public void NBdeleted2(object sender, FileSystemEventArgs e)
        {
            List<Inline> deletedNBs = new List<Inline>();


            foreach (var block in rtbScript.Document.Blocks)
            {
                if (block is Paragraph)
                {
                    Paragraph para = block as Paragraph;
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

            }

            for (int d = 0; d < deletedNBs.Count; d++)
            {
                Inline del = deletedNBs[d];

                List<Paragraph> blocks = new List<Paragraph>();
                foreach (var block in rtbScript.Document.Blocks)
                {
                    if (block is Paragraph)
                    {
                        Paragraph par = block as Paragraph;
                        blocks.Add(par);
                    }
                    if (block is List)
                    {
                        List lool = block as List;
                        foreach (ListItem lit in lool.ListItems)
                        {
                            foreach (Paragraph pp in lit.Blocks)
                            {
                                blocks.Add(pp);
                            }
                        }
                    }

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

        #region Background Worker (in progress)
        private void StartNewWorker(NJSaveFile saveFile, bool isSave)
        {

            Mouse.OverrideCursor = Cursors.Wait;
            progBar.Visibility = Visibility.Visible;
            topMenu.Visibility = Visibility.Collapsed;
            BackgroundWorker worker = new BackgroundWorker();
            if (isSave)
            {
                worker.DoWork += Worker_DoSave;
            }
            else
            {
                worker.DoWork += Worker_DoLoad;
            }
            worker.ProgressChanged += Worker_Progress;
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = false;
            worker.RunWorkerAsync(saveFile);

        }
        private void Worker_DoSave(object sender, DoWorkEventArgs w)
        {
            BackgroundWorker bw = sender as BackgroundWorker;

            NJSaveFile file = (NJSaveFile)w.Argument;
            FileStream fs = new FileStream(file.uri, FileMode.Create);
            XamlWriter.Save(file.document, fs);
            fs.Close();


            List<Inline> nbInlines = new List<Inline>();

            foreach (Paragraph para in file.document.Blocks)
            {
                foreach (Inline inline in para.Inlines)
                {
                    if (inline.Tag is NBfile)
                    {
                        nbInlines.Add(inline);
                    }
                }
            }

            for (int i = 0; i < nbInlines.Count; i++)
            {
                Inline inl = nbInlines[i];
                InlineUIContainer nbi = inl as InlineUIContainer;
                NBfile nb = inl.Tag as NBfile;
                nbi.Child = nb.NBbutton();
            }

            w.Result = file;

        }

        private void Worker_DoLoad(object sender, DoWorkEventArgs w)
        {
            BackgroundWorker bw = sender as BackgroundWorker;
            NJSaveFile file = (NJSaveFile)w.Argument;
            FileStream fs = new FileStream(file.uri, FileMode.Open);
            FlowDocument loaded = XamlReader.Load(fs) as FlowDocument;
            fs.Close();
            file.document = loaded;

            List<Inline> nbInlines = new List<Inline>();

            foreach (Paragraph para in file.document.Blocks)
            {
                foreach (Inline inline in para.Inlines)
                {
                    if (inline.Tag is NBfile)
                    {
                        nbInlines.Add(inline);
                    }
                }
            }

            for (int i = 0; i < nbInlines.Count; i++)
            {
                Inline inl = nbInlines[i];
                InlineUIContainer nbi = inl as InlineUIContainer;
                NBfile nb = inl.Tag as NBfile;
                nbi.Child = nb.NBbutton();
            }
            w.Result = file;

        }

        private void Worker_Progress(object sender, ProgressChangedEventArgs p)
        {
            double per = (p.ProgressPercentage * 100) / 50;
            progBar.IsIndeterminate = false;
            progBar.Value = Math.Round(per, 0);

        }

        private void Worker_Done(object sender, DoWorkEventArgs w)
        {
            NJSaveFile result = (NJSaveFile)w.Result;
            rtbScript.Document = result.document;
            scriptUri = result.uri;
            progBar.Visibility = Visibility.Collapsed;
            topMenu.Visibility = Visibility.Visible;
            Mouse.OverrideCursor = null;
        }

        #endregion

        private void rtbScript_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.FormatToApply == "Bitmap")
            {
                e.FormatToApply = "FileName";
            }
        }
    }

}
