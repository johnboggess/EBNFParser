using EBNFParser.Exceptions;
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
        protected internal override bool Match(string str, int index, out int newIndex, out FailedMatch failedMatch)
        {
            failedMatch = null;
            while (InnerOperator.Match(str, index, out newIndex, out _))
            {
                index = newIndex;
            }
            return true;
        }
    }
}
