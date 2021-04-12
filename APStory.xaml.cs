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
using System.Diagnostics;

namespace NewsBuddy
{
    /// <summary>
    /// Interaction logic for APStory.xaml
    /// </summary>
    public partial class APStory : Page
    {
        public XmlDocument story { get; set; }

        private XmlNodeList m_headline;
        private XmlNodeList m_author;
        private XmlNodeList m_paragraphs;
        private XmlNodeList m_location;



        public APStory(XmlDocument _story)
        {
            this.story = _story;
            InitializeComponent();
            ParseStory();

        }

        public void ParseStory()
        {
            m_headline = story.GetElementsByTagName("hedline");
            m_author = story.GetElementsByTagName("byline");
            m_paragraphs = story.GetElementsByTagName("p");
            m_location = story.GetElementsByTagName("location");

            txt_Headline.Text = m_headline[0] != null ? m_headline[0].InnerText : "Error Retrieving Headline";

            txt_Location.Text = m_location[0] != null ? m_location[0].InnerText : "";

            txt_Author.Text = m_author[0] != null ? m_author[0].InnerText : "By The Associated Press";

            
            if (m_paragraphs != null)
            {
                StringBuilder builder = new StringBuilder();

                for (int i = 0; i < m_paragraphs.Count; i++)
                {
                    builder.AppendLine("\t" + m_paragraphs[i].InnerText.ToString());
                }
                txt_Story.Text = builder.ToString();
            }
            else
            {
                txt_Story.Text = "Error Retrieving Story";
            }
            
        }
    }
}
