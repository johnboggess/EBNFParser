using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBNFParser.Rules
{
    public class Alternation : Rule
    {
        public List<Rule> Rules;

        public Alternation() { }

        public Alternation(List<Rule> rules)
        {
            Rules = rules;
        }

    }
}
