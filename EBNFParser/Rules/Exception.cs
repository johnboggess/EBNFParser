using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBNFParser.Rules
{
    public class Exception : Rule
    {
        public Rule Rule;
        public Exception() { }
        public Exception(Rule rule)
        {
            Rule = rule;
        }
    }
}
