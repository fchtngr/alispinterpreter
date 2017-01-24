using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfApplication1
{
    class GridItem
    {
        string key;
        string value;
        string documentation;

        public string Key
        {
            get { return key; }
            set { this.key = value; }
        }

        public string Value
        {
            get { return value; }
            set { this.value = value; }
        }

        public string Documentation
        {
            get { return documentation; }
            set { this.documentation = value; }
        }

        public GridItem(string key, string value, string documentation)
        {
            this.key = key;
            this.value = value;
            this.documentation = documentation;
        }
    }
}
