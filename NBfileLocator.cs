using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Documents;
using System.Windows;

namespace NewsBuddy
{
    [Serializable]
    public class NBfileLocator
    {
        public int offset;
        public NBfile file;
        //public string repName;
        public TextPointer textPointer;

        public string GetIDs(NBfile nb, int index)
        {
            string repName;


            repName = String.Format("%${0}", index) + nb.NBPath + String.Format("$%{0}", index);


            return repName;
        }

        public string GetIDc(NBfile nb, int index)
        {
            string repName;

            repName = String.Format("%#{0}", index) + nb.NBPath + String.Format("#%{0}", index);

            return repName;
        }



    }
}
