using EBNFParser.Exceptions;
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

        protected internal override bool Match(string str, int index, out int newIndex, out FailedMatch failedMatch)
        {
            bool success = InnerOperator.Match(str, index, out newIndex, out FailedMatch failed);

            if (!success)
                failedMatch = new FailedUnaryMatch(this, failed);
            else
                failedMatch = null;

            return success;
        }
    }
}
