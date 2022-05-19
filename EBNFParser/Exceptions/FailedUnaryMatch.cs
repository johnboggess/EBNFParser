using EBNFParser.EBNFOperators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBNFParser.Exceptions
{
    public class FailedUnaryMatch : FailedMatch
    {
        public FailedMatch Inner;

        public FailedUnaryMatch(UnaryOperator @operator, FailedMatch inner)
        {
            Operator = @operator;
            Inner = inner;
        }
    }
}
