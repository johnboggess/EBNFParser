using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBNFParser.EBNFOperators
{
    public class Repetition : UnaryOperator
    {
        public Repetition() { }
        public Repetition(Operator inner) : base(inner) { }
    }
}
