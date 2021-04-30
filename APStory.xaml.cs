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
using System.Xml;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;

namespace NewsBuddy
{
    public class APChunk
    {
        public string chunk_text { get; set; }
        public double chunk_fontSize { get; set; }
        public FontWeight chunk_fontWeight { get; set; }
        public Thickness chunk_margin { get; set; }
        public FontStyle chunk_fontStyle { get; set; }
        public TextAlignment chunk_textAlignment { get; set; }
        public TextBox chunk_textBox { get; set; }
        public APChunk chunk { get; set; }
        public int chunk_index { get; set; }

        // use multiple constructors you idiot

        public APChunk(string para, double font = 16)
        {
            this.chunk_text = para;
            this.chunk_fontSize = font;
            this.chunk_fontWeight = FontWeights.Normal;
            this.chunk_fontStyle = FontStyles.Normal;
            this.chunk_margin = new Thickness(0);
            this.chunk_textAlignment = TextAlignment.Left;
            chunk = this;
        }

    }
    /// <summary>
    /// Interaction logic for APStory.xaml
    /// </summary>
    public partial class APStory : Page
    {
        private XmlDocument m_story { get; set; }

        private XmlNodeList m_headline;
        private XmlNodeList m_author;
        private XmlNodeList m_paragraphs;
        private XmlNodeList m_updated;
        private XmlNodeList m_created;
        public ObservableCollection<APChunk> m_chunks { get; set; }
        public string Story { get; private set; }
        private bool m_isEditMode { get; set; }
        public bool isEditMode { get { return !m_isEditMode; } }

        private List<TextBox> checkedChunks;



        public APStory(XmlDocument _story, bool isEdit = false)
        {
            this.m_story = _story;
            this.m_isEditMode = isEdit;
            this.Story = String.Empty;
            InitializeComponent();
            ParseStory();


        }

        public void ParseStory()
        {
            // Remove the "Associated Press" from the author field. It's just cluttery.
            XmlElement el = (XmlElement)m_story.SelectSingleNode("/nitf/body/body.head/byline/byttl");
            if (el != null)
            {
                el.ParentNode.RemoveChild(el);
            }


            m_headline = m_story.GetElementsByTagName("hedline");
            m_author = m_story.GetElementsByTagName("byline");

            m_paragraphs = m_story.GetElementsByTagName("p");
            m_updated = m_story.GetElementsByTagName("date.issue");

            ToggleModes();
        }

        public void PrepareReaderMode()
        {
            edit_Story.Visibility = Visibility.Collapsed;

            if (m_chunks != null)
            {
                m_chunks.Clear();
            }
            if (checkedChunks != null)
            {
                checkedChunks.Clear();
            }

            txt_Headline.Text = m_headline[0] != null ? m_headline[0].InnerText : "Error Retrieving Headline";
            txt_Author.Text = m_author[0] != null ? m_author[0].InnerText : "By The Associated Press";

            if (m_updated[0] == null)
            {
                txt_Location.Text = "";
                txt_Location.Visibility = Visibility.Collapsed;
                sep2.Visibility = Visibility.Collapsed;
            }
            else
            {
                txt_Location.Visibility = Visibility.Visible;
                DateTime result = DateTime.ParseExact(m_updated[0].Attributes[0].Value, "yyyyMMddTHHmmssZ", null);
                int days = (int)DateTime.Now.Subtract(result).Days;
                int hours = (int)DateTime.Now.Subtract(result).Hours;
                int minutes = (int)DateTime.Now.Subtract(result).Minutes;
                string dayLabel = "day";
                string hourLabel = "hour";
                string minuteLabel = "minute";
                if (days > 1) { dayLabel = "days"; }
                if (hours > 1) { hourLabel = "hours"; }
                if (minutes > 1) { minuteLabel = "minutes"; }
                txt_Location.Text = "Updated" + (days > 0 ? (" " + days.ToString() + " " + dayLabel) : "")
                    + (hours > 0 ? (" " + hours.ToString() + " " + hourLabel) : "")
                    + (minutes > 0 ? (" " + minutes.ToString() + " " + minuteLabel) : "")
                    + (minutes > 0 || hours > 0 || days > 0 ? " ago." : " just now.");
                sep2.Visibility = Visibility.Visible;
            }

            if (Story == String.Empty && m_paragraphs != null)
            {
                StringBuilder builder = new StringBuilder();

                for (int i = 0; i < m_paragraphs.Count; i++)
                {


                    builder.AppendLine("\t" + m_paragraphs[i].InnerText.ToString());


                }
                Story = builder.ToString();
                txt_Story.Text = Story;
            }
            else if (Story != String.Empty)
            {
                txt_Story.Text = Story;
            }
            else
            {
                txt_Story.Text = "Error Retrieving Story";
            }

            sep1.Visibility = Visibility.Visible;
            txt_Headline.Visibility = Visibility.Visible;
            txt_Story.Visibility = Visibility.Visible;
            txt_Author.Visibility = Visibility.Visible;


        }

        public void PrepareEditMode()
        {
            txt_Story.Visibility = Visibility.Collapsed;
            txt_Headline.Visibility = Visibility.Collapsed;
            txt_Author.Visibility = Visibility.Collapsed;
            txt_Location.Visibility = Visibility.Collapsed;
            sep2.Visibility = Visibility.Collapsed;
            sep1.Visibility = Visibility.Collapsed;



            if (m_chunks != null)
            {
                m_chunks.Clear();
            }
            else
            {
                m_chunks = new ObservableCollection<APChunk>();
            }

            if (checkedChunks != null)
            {
                checkedChunks.Clear();
            }
            else
            {
                checkedChunks = new List<TextBox>();
            }


            if (m_author[0] != null)
            {
                if (m_headline[0] != null)
                {
                    m_chunks.Add(new APChunk(m_headline[0].InnerText, 22)
                    {
                        chunk_fontWeight = FontWeights.Bold,
                        chunk_textAlignment = TextAlignment.Center,
                        chunk_margin = new Thickness(0, 0, 0, 3)
                    });
                }
                m_chunks.Add(new APChunk(m_author[0].InnerText, 11)
                {
                    chunk_fontStyle = FontStyles.Italic,
                    chunk_margin = new Thickness(0, 0, 0, 7),
                    chunk_textAlignment = TextAlignment.Center
                });
            }
            else
            {
                if (m_headline[0] != null)
                {
                    m_chunks.Add(new APChunk(m_headline[0].InnerText, 22)
                    {
                        chunk_fontWeight = FontWeights.Bold,
                        chunk_textAlignment = TextAlignment.Center,
                        chunk_margin = new Thickness(0, 0, 0, 10)
                    });
                }
            }


            for (int e = 0; e < m_paragraphs.Count; e++)
            {
                m_chunks.Add(new APChunk(m_paragraphs[e].InnerText));

            }

            this.DataContext = this;

            edit_Story.Visibility = Visibility.Visible;

        }

        public void ToggleModes()
        {
            if (!m_isEditMode)
            {
                Trace.WriteLine("Story Preparing Reader Mode");
                PrepareReaderMode();
                m_isEditMode = true;
            }
            else
            {
                Trace.WriteLine("Story Preparing EDIT Mode");
                PrepareEditMode();
                m_isEditMode = false;
            }
        }

        private void edit_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = (CheckBox)sender;
            TextBox obj = chk.Tag as TextBox;
            checkedChunks.Add(obj);

        }

        private void edit_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = (CheckBox)sender;
            TextBox obj = chk.Tag as TextBox;
            checkedChunks.Remove(obj);
        }

        private void btn_ToggleModes_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                ToggleModes();
            });
        }

        private void HandlePreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }

        public void DeleteUnChecked()
        {
            List<APChunk> keepers = new List<APChunk>();
            foreach (TextBox box in checkedChunks)
            {
                APChunk chunk = box.Tag as APChunk;
                chunk.chunk_index = m_chunks.IndexOf(chunk);
                keepers.Add(chunk);
            }

            m_chunks.Clear();

            keepers = keepers.OrderBy(o => o.chunk_index).ToList();

            for (int l = 0; l < keepers.Count; l++)
            {
                APChunk obj = keepers[l];
                obj.chunk_index = l;
                m_chunks.Add(obj);
            }

            keepers.Clear();

            checkedChunks.Clear();
        }

        public void DeleteChecked()
        {
            foreach (TextBox box in checkedChunks)
            {
                m_chunks.Remove((APChunk)box.Tag);
            }
            checkedChunks.Clear();
        }


        public string GetStoryAsString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (APChunk chunk in m_chunks)
            {
                builder.AppendLine(chunk.chunk_text);
            }
            return builder.ToString();
        }

        public List<APChunk> GetChunks()
        {
            bool hadToSwapModes = false;
            bool autoDeleted = false;

            if (this.isEditMode && checkedChunks.Count != 0 && checkedChunks != null && m_chunks != null && m_chunks.Count > 0)
            {
                Trace.WriteLine("Deleting UNchecked from GetChunks method");
                DeleteUnChecked();
                autoDeleted = true;
            }

            if (m_chunks == null)
            {
                Trace.WriteLine("Asked for Story Chunks, Chunks were NULL");
                PrepareEditMode();
                hadToSwapModes = true;
            }
            if (m_chunks.Count <= 0)
            {
                Trace.WriteLine("Asked for Story Chunks, Chunks were ZERO");
                PrepareEditMode();
                hadToSwapModes = true;
            }

            List<APChunk> sendlist = new List<APChunk>();
            foreach (APChunk chunk in m_chunks)
            {
                chunk.chunk_index = m_chunks.IndexOf(chunk);
                sendlist.Add(chunk);
            }
            sendlist = sendlist.OrderBy(o => o.chunk_index).ToList();
            if (hadToSwapModes)
            {
                PrepareReaderMode();
            }
            if (autoDeleted)
            {
                PrepareReaderMode();
                PrepareEditMode();
            }
            return sendlist;
        }
    }
}
