using EBNFParser.Exceptions;
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

        protected internal override bool Match(string str, int index, out int newIndex, out FailedMatch failedMatch)
        {
            FailedMatch failedLeft;
            FailedMatch failedRight;
            failedMatch = null;

            bool successLeft = LeftOperator.Match(str, index, out newIndex, out failedLeft);
            if (successLeft)
                return true;

            bool successRight = RightOperator.Match(str, index, out newIndex, out failedRight);
            if (successRight)
                return true;

            failedMatch = new FailedBinaryMatch(this, failedLeft, failedRight);
            return false;
        }

    }
}
