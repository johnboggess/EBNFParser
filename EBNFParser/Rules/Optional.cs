using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBNFParser.Rules
{
    public class Optional
    {
        public Rule Rule;
        public Optional() { }
        public Optional(Rule rule) 
        {
            Rule = rule;
        }
    }
}
