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
using System.Linq;
using System.Xml;

namespace NewsBuddy
{
    /// <summary>
    /// Interaction logic for APReader.xaml
    /// </summary>
    public partial class APReader : Window
    {
        public APingestor ingest;
       // private List<APObject> stories;
        private bool isLoaded = false;
        public MainWindow mainWindow { get; set; }
        public APReader()
        {
            InitializeComponent();

            lbl_EditorDisclaimer.Visibility = Visibility.Collapsed;
            btn_DeleteChecked.Visibility = Visibility.Collapsed;
            btn_DeleteUnChecked.Visibility = Visibility.Collapsed;

            foreach (var btn in editorGrid.Children)
            {
                if (btn is Button)
                {
                    ((Button)btn).IsEnabled = false;
                }
            }

            mainWindow = Application.Current.Windows[0] as MainWindow;
            ingest = new APingestor();

            RefreshAP(new object(), new RoutedEventArgs());

            frame_Story.Content = new APReaderDocs();

            isLoaded = true;

        }

        private void list_APStories_SelectionChanged()
        {
            if (isLoaded)
            {
                APObject selected = list_APStories.SelectedItem as APObject;
                if (selected != null)
                {
                    if (selected.HasStory)
                    {
                        SetControls(selected.story.isEditMode);
                        if (selected.story.isEditMode)
                        {
                            btn_ToggleMode.Content = "Revert to Original Story";
                        }
                        else
                        {
                            btn_ToggleMode.Content = "Switch to Story Editor Mode";
                        }
                        frame_Story.Content=selected.story;
                        foreach (var btn in editorGrid.Children)
                        {
                            if (btn is Button)
                            {
                                ((Button)btn).IsEnabled = true;
                            }
                        }
                        editorGrid.Visibility = Visibility.Visible;
                        scrl_Story.ScrollToVerticalOffset(0);
                    }
                    else
                    {
                        foreach (var btn in editorGrid.Children)
                        {
                            if (btn is Button)
                            {
                                ((Button)btn).IsEnabled = false;
                            }
                        }
                        frame_Story.Content = new APReaderDocs();
                        //editorGrid.Visibility = Visibility.Collapsed;
                    }
                }
            }
           
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ingest.Dispose();
        }

        private void RefreshAP(object sender, RoutedEventArgs e)
        {
            list_APStories.Items.Clear();
            ingest.GetFeed();
            if (ingest.isAuthorized)
            {
                foreach (APObject obj in ingest.Items)
                {
                    if (obj.isAssocParent)
                    {
                        TreeViewItem assocParent = new TreeViewItem()
                        {
                            Header = obj.headline
                        };
                        foreach (APObject assoc in obj.associations)
                        {
                            assocParent.Items.Add(assoc);
                        }

                        list_APStories.Items.Add(assocParent);
                        
                    }
                    else
                    {
                        list_APStories.Items.Add(obj);
                    }
                }
            }
            else
            {
                list_APStories.Items.Add(new TextBlock()
                {
                    Text = "Unauthorized. Check your API Key\nin NewsJock Settings."
                });
                frame_Story.Content = "If you do not have an AP API Key, contact your administrator.";
            }
        }

        private void list_APStories_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.Invoke(() => 
            {
                list_APStories_SelectionChanged();
            });
        }

        private void btn_ToggleMode_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            APStory story = frame_Story.Content as APStory;
            story.ToggleModes();
            SetControls(story.isEditMode);
            if (story.isEditMode)
            {
                btn.Visibility = Visibility.Visible;
                btn.Content = "Revert to Original Story";
            }
            else
            {
                btn.Visibility = Visibility.Visible;
                btn.Content = "Switch to Story Editor Mode";
            }

        }

        private void SetControls(bool editMode)
        {
            if (editMode)
            {
                Grid.SetRow(scrl_Story, 2);
                Grid.SetRowSpan(scrl_Story, 2);
                Grid.SetRowSpan(editorGrid, 2);

                Grid.SetRow(btn_ToggleMode, 1);
                Grid.SetRowSpan(btn_ToggleMode, 1);
                Grid.SetRowSpan(btn_SendToNewScript, 1);
                Grid.SetRowSpan(btn_SendToScript, 1);

                lbl_EditorDisclaimer.Visibility = Visibility.Visible;
                btn_DeleteChecked.Visibility = Visibility.Visible;
                btn_DeleteUnChecked.Visibility = Visibility.Visible;
            }
            else
            {
                Grid.SetRow(scrl_Story, 1);
                Grid.SetRowSpan(scrl_Story, 3);
                Grid.SetRowSpan(editorGrid, 1);

                Grid.SetRow(btn_ToggleMode, 2);
                Grid.SetRowSpan(btn_ToggleMode, 2);
                Grid.SetRowSpan(btn_SendToNewScript, 2);
                Grid.SetRowSpan(btn_SendToScript, 2);

                lbl_EditorDisclaimer.Visibility = Visibility.Collapsed;
                btn_DeleteChecked.Visibility = Visibility.Collapsed;
                btn_DeleteUnChecked.Visibility = Visibility.Collapsed;
            }
        }

        private void btn_SendToScript_Click(object sender, RoutedEventArgs e)
        {
            APStory story = frame_Story.Content as APStory;
            mainWindow.currentScript.InsertChunksFromIngestor(story.GetChunks());
        }

        private void btn_SendToNewScript_Click(object sender, RoutedEventArgs e)
        {
            
            APStory story = frame_Story.Content as APStory;
            mainWindow.AddNewTabFromChunks(story.GetChunks());

        }

        private void btn_DeleteChecked_Click(object sender, RoutedEventArgs e)
        {
            APStory story = frame_Story.Content as APStory;
            story.DeleteChecked();
        }

        private void btn_DeleteUnChecked_Click(object sender, RoutedEventArgs e)
        {
            APStory story = frame_Story.Content as APStory;
            story.DeleteUnChecked();
        }

        private void btnTopicsSettings_Click(object sender, RoutedEventArgs e)
        {
            foreach (var btn in editorGrid.Children)
            {
                if (btn is Button)
                {
                    ((Button)btn).IsEnabled = false;
                }
            }
            frame_Story.Content = new APTopicSettings();
        }
    }
}
