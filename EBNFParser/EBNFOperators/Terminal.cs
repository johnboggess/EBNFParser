using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBNFParser.EBNFOperators
{
    public class Terminal : Operator
    {
        public string Value = "";
        public Terminal() { }
        public Terminal(string str)
        {
            Value = str;
        }
    }
}
