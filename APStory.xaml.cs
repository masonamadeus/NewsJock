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
        public APChunk chunk {get;set;}
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
        private XmlNodeList m_location;
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
            m_headline = m_story.GetElementsByTagName("hedline");
            m_author = m_story.GetElementsByTagName("byline");
            m_paragraphs = m_story.GetElementsByTagName("p");
            m_location = m_story.GetElementsByTagName("location");
            ToggleModes();
        }

        public void PrepareReaderMode()
        {
            edit_Story.Visibility = Visibility.Collapsed;
            lbl_Edit.Visibility = Visibility.Collapsed;
            editorControls.Visibility = Visibility.Collapsed;
            
            txt_Headline.Text = m_headline[0] != null ? m_headline[0].InnerText : "Error Retrieving Headline";
            txt_Author.Text = m_author[0] != null ? m_author[0].InnerText : "By The Associated Press";

            if (m_location[0] == null)
            {
                txt_Location.Text = "";
                txt_Location.Visibility = Visibility.Collapsed;
            }
            else
            {
                txt_Location.Visibility = Visibility.Visible;
                txt_Location.Text = m_location[0].InnerText;
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
                        chunk_margin = new Thickness(0,0,0,3)
                    });
                }
                m_chunks.Add(new APChunk(m_author[0].InnerText, 11) 
                {
                    chunk_fontStyle = FontStyles.Italic,
                    chunk_margin = new Thickness(0,0,0,7),
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
                        chunk_margin = new Thickness(0,0,0,10)
                    });
                }
            }


            for (int e = 0; e < m_paragraphs.Count; e++)
            {
                m_chunks.Add(new APChunk(m_paragraphs[e].InnerText));
            }

            this.DataContext = this;

            edit_Story.Visibility = Visibility.Visible;
            editorControls.Visibility = Visibility.Visible;
            lbl_Edit.Visibility = Visibility.Visible;

        }

        public void ToggleModes()
        {
            if (!m_isEditMode)
            {
                PrepareReaderMode();
                m_isEditMode = true;
            }
            else
            {
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

        private void DeleteUnChecked(object sender, RoutedEventArgs e)
        {
            StringBuilder builder = new StringBuilder();
            List<APChunk> keepers = new List<APChunk>();
            foreach(TextBox box in checkedChunks)
            {
                APChunk chunk = box.Tag as APChunk;
                chunk.chunk_index = m_chunks.IndexOf(chunk);
                keepers.Add(chunk);
            }

            m_chunks.Clear();

            keepers = keepers.OrderBy(o => o.chunk_index).ToList();

            for (int l = 0; l<keepers.Count; l++)
            {
                APChunk obj = keepers[l];
                obj.chunk_index = l;
                m_chunks.Add(obj);
            }

            keepers.Clear();

            checkedChunks.Clear();
        }

        private void DeleteChecked(object sender, RoutedEventArgs e)
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
            if (m_chunks == null)
            {
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
            return sendlist;
        }
    }
}
