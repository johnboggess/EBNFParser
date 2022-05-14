using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBNFParser.Rules
{
    public class Repetition : Rule
    {
        public Rule Rule;
        public Repetition() { }
        public Repetition(Rule rule)
        {
            Rule = rule;
        }
    }
}
