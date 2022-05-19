using EBNFParser.Exceptions;
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
        protected internal override bool Match(string str, int index, out int newIndex, out FailedMatch failedMatch)
        {
            failedMatch = null;
            return InnerOperator.Match(str, index, out newIndex, out _);
        }
    }
}
