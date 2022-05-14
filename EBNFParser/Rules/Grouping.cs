using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBNFParser.Rules
{
    public class Grouping
    {
        public Rule Rule;
        public Grouping() { }
        public Grouping(Rule rule)
        {
            Rule = rule;
        }
    }
}
