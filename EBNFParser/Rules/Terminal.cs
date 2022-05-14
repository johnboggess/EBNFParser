using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBNFParser.Rules
{
    public class Terminal : Rule
    {
        public string Value = "";
        public Terminal() { }
        public Terminal(string str)
        {
            Value = str;
        }
    }
}
