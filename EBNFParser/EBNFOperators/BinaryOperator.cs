using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBNFParser.EBNFOperators
{
    public abstract class BinaryOperator : Operator
    {
        public Operator Left;
        public Operator Right;

        public BinaryOperator() { }
        public BinaryOperator(Operator left, Operator right)
        {
            Left = left;
            Right = right;
        }
    }
}
