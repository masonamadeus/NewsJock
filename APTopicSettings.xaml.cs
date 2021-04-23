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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace NewsBuddy
{
    /// <summary>
    /// Interaction logic for APTopicSettings.xaml
    /// </summary>
    public partial class APTopicSettings : Page
    {
        public APTopicSettings()
        {
            InitializeComponent();
            if (Settings.Default.APfollowedTopics == null)
            {
                Settings.Default.APfollowedTopics = new List<APTopic>();
                Settings.Default.Save();
            }
            if (Settings.Default.APunfollowedTopics == null)
            {
                Settings.Default.APunfollowedTopics = new List<APTopic>();
                Settings.Default.Save();
            }
            FillList();

            if (Debugger.IsAttached)
            {
                foreach (APTopic i in Settings.Default.APunfollowedTopics)
                {
                    Trace.WriteLine(i.topicName);
                }
            }
            

        }

        private bool nameClear = true;
        private bool idClear = true;

        private Regex ex = new Regex("[^0-9]+");

        private void NumbersOnly(object sender, TextCompositionEventArgs e)
        {
            e.Handled = ex.IsMatch(e.Text);
        }

        private void FillList()
        {
            // if (lst_Topics.Items.Count > 0)
            // {
            //    lst_Topics.Items.Clear();

            // }
            

            if (lst_UnFollowed.Items.Count > 0)
            {
                lst_UnFollowed.Items.Clear();
            }

            foreach (APTopic badTopic in Settings.Default.APunfollowedTopics)
            {
                lst_UnFollowed.Items.Add(badTopic);
            }

            if (lst_Topics.Items.Count > 0)
            {
                lst_Topics.Items.Clear();
            }

            foreach (APTopic topic in Settings.Default.APfollowedTopics)
            {
                lst_Topics.Items.Add(topic);
            }

            if (Settings.Default.APfollowedTopics.Count == 0)
            {
                lst_Topics.Items.Add(new TextBlock() { Text = "No Followed Topics", Tag = "NA" });
            }
        }

        private void UpdateFollowedTopics()
        {
            Settings.Default.APfollowedTopics.Clear();

            for (int x = 0; x < lst_Topics.Items.Count; x++)
            {
                var topic = lst_Topics.Items[x];
                if (topic is APTopic)
                {
                    Settings.Default.APfollowedTopics.Add((APTopic)topic);
                }
                else if (topic is TextBox && (string)((TextBox)topic).Tag == "NA")
                {
                    lst_Topics.Items.Remove(topic);
                }
            }

            Settings.Default.APunfollowedTopics.Clear();

            for (int y = 0; y < lst_UnFollowed.Items.Count; y++)
            {
                var badTopic = lst_UnFollowed.Items[y];
                if (badTopic is APTopic)
                {
                    Settings.Default.APunfollowedTopics.Add((APTopic)badTopic);
                }
            }

            Settings.Default.Save();
            Settings.Default.Reload();
            FillList();
        }

        private void RemoveTopic()
        {
            if (lst_UnFollowed.SelectedItem is APTopic)
            {
                APTopic badTopic = (APTopic)lst_UnFollowed.SelectedItem;
                lst_UnFollowed.Items.Remove(badTopic);
                UpdateFollowedTopics();
            }
        }

        private void AddTopic()
        {
            if (String.Equals(txt_NewTopicName.Text,"Topic Name") || String.Equals(txt_NewTopicID.Text,"Topic ID #")
                || String.Equals(txt_NewTopicName.Text,"") || String.Equals(txt_NewTopicID.Text,""))
            {
                txt_NewTopicID.Text = "Topic ID #";
                txt_NewTopicName.Text = "Topic Name";
                nameClear = true;
                idClear = true;
                return;
            }
            string newName = txt_NewTopicName.Text;
            string newIDs = txt_NewTopicID.Text;
            int newID = Convert.ToInt32(newIDs);

            if (newID == 0)
            {
                txt_NewTopicID.Text = "Topic ID #";
                txt_NewTopicName.Text = "Topic Name";
                nameClear = true;
                idClear = true;
                return;
            }

            Trace.WriteLine(newID + newName);
            APTopic newtopic = new APTopic()
            {
                topicName = newName,
                topicID = newID

            };

            txt_NewTopicID.Text = "Topic ID #";
            txt_NewTopicName.Text = "Topic Name";
            nameClear = true;
            idClear = true;

            lst_Topics.Items.Add(newtopic);

            UpdateFollowedTopics();

        }

        private void btn_AddTopic_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() => { AddTopic(); });
        }

        private void btn_Follow_Click(object sender, RoutedEventArgs e)
        {
            if (lst_UnFollowed.SelectedItem != null && lst_UnFollowed.SelectedItem is APTopic)
            {
                APTopic mover = lst_UnFollowed.SelectedItem as APTopic;
                lst_UnFollowed.Items.Remove(mover);
                lst_Topics.Items.Add(mover);
                UpdateFollowedTopics();
            }
        }

        private void btn_UnFollow_Click(object sender, RoutedEventArgs e)
        {
            if (lst_Topics.SelectedItem != null && lst_Topics.SelectedItem is APTopic)
            {
                APTopic mover = lst_Topics.SelectedItem as APTopic;
                lst_UnFollowed.Items.Add(mover);
                lst_Topics.Items.Remove(mover);
                UpdateFollowedTopics();
            }
        }

        private void lst_UnFollowed_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lst_UnFollowed.SelectedItem != null && lst_UnFollowed.SelectedItem is APTopic)
            {
                APTopic topic = lst_UnFollowed.SelectedItem as APTopic;
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete \"" + topic.topicName + "\"?", "Remove Followed Topic", MessageBoxButton.YesNo, MessageBoxImage.Question);
                switch (result)
                {
                    case MessageBoxResult.Yes: RemoveTopic();
                        break;
                    case MessageBoxResult.No: return;
                        break;
                    case MessageBoxResult.None: return;
                        break;
                }

            }
        }

        private void txt_NewTopicName_GotFocus(object sender, RoutedEventArgs e)
        {
            if (nameClear)
            {
                txt_NewTopicName.Text = "";
                nameClear = false;
            }
        }

        private void txt_NewTopicID_GotFocus(object sender, RoutedEventArgs e)
        {
            if (idClear)
            {
                txt_NewTopicID.Text = "";
                idClear = false;
            }
        }
    }
    
}
