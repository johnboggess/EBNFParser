using EBNFParser.EBNFOperators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBNFParser.Exceptions
{
    public class FailedBinaryMatch : FailedMatch
    {
        public FailedMatch Left;
        public FailedMatch Right;
        public FailedBinaryMatch(BinaryOperator @operator, FailedMatch left, FailedMatch right)
        {
            Operator = @operator;
            Left = left;
            Right = right;
        }
    }
}
