using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Documents;

namespace NewsBuddy
{
    public class NButton : Button
    {
        public NBfileLocator locator;
        public NBfile file;
        public TextPointer textPointer;
    }
}
