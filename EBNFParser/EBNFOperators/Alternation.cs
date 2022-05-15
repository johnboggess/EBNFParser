using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBNFParser.EBNFOperators
{
    public class Alternation : BinaryOperator
    {
        public Alternation() { }
        public Alternation(Operator left, Operator right) : base(left, right) { }

    }
}
