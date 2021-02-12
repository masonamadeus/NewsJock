using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Text;

namespace NewsBuddy
{
    [Serializable]
    class NJSaveFile
    {
        public FlowDocument document { get; set; }
        public string uri { get; set; }

        public NJSaveFile(FlowDocument richTextBox, string path)
        {
            document = richTextBox;
            uri = path;
        }

    }
}
