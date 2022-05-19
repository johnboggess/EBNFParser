using EBNFParser.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBNFParser.EBNFOperators
{
    public class Exception : BinaryOperator
    {
        public Exception() { }
        public Exception(Operator left, Operator right) : base(left, right) { }
        protected internal override bool Match(string str, int index, out int newIndex, out FailedMatch failedMatch)
        {
            bool successLeft = LeftOperator.Match(str, index, out newIndex, out FailedMatch failedLeft);
            bool successRight = RightOperator.Match(str, newIndex, out newIndex, out FailedMatch failedRight);

            if (!successLeft || successRight)
                failedMatch = new FailedBinaryMatch(this, failedLeft, failedRight);
            else
                failedMatch = null;

            return successLeft && !successRight;
        }
    }
}
