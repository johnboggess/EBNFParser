using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBNFParser.EBNFOperators
{
    public abstract class BinaryOperator : Operator
    {
        public Operator LeftOperator { get { return _operators[0]; } set { _operators[0] = value; } }
        public Operator RightOperator { get { return _operators[1]; } set { _operators[1] = value; } }

        public BinaryOperator()
        {
            _operators = new List<Operator>() { null, null };
        }
        public BinaryOperator(Operator left, Operator right)
        {
            _operators = new List<Operator>() { null, null };
            LeftOperator = left;
            RightOperator = right;
        }

        public T Left<T>() where T : Operator
        {
            return (T)LeftOperator;
        }
        public T Right<T>() where T : Operator
        {
            return (T)RightOperator;
        }
    }
}
