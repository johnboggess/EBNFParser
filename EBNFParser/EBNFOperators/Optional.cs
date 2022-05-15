using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBNFParser.EBNFOperators
{
    public class Optional : UnaryOperator
    {
        public Optional() { }
        public Optional(Operator inner) : base(inner) { }
    }
}
