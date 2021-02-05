using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Documents;

namespace NewsBuddy
{
    [Serializable]
    class NBfileLocator
    {
        public int offset;
        public NBfile file;
        public string repName;


        public string GetID(NBfile nb, int index)
        {
            NBfileLocator locator = new NBfileLocator();

            if (nb.NBisSounder)
            {
                locator.repName = String.Format("%${0}", index) + nb.NBPath + String.Format("$%{0}", index);
            } else
            {
                locator.repName = @"%#C%" + nb.NBPath + @"%C#%";
            }

            return locator.repName;
        }




    }
}
