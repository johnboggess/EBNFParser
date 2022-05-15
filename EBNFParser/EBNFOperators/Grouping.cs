using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBNFParser.EBNFOperators
{
    public class Grouping : UnaryOperator
    {
        public Grouping() { }
        public Grouping(Operator inner) : base(inner) { }
    }
}
