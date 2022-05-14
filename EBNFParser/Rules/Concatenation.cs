using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBNFParser.Rules
{
    public class Concatenation : Rule
    {
        public List<Rule> Rules = new List<Rule>();
        public Concatenation() { }

        public Concatenation(List<Rule> rules)
        {
            Rules = rules;
        }
    }
}
