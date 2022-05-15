using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBNFParser.EBNFOperators
{
    public abstract class BinaryOperator : Operator
    {
        private Operator _leftOperator;
        private Operator _rightOperator;

        public Operator LeftOperator { get { return _leftOperator; } set { _leftOperator = value; } }
        public Operator RightOperator { get { return _rightOperator; } set { _rightOperator = value; } }

        public BinaryOperator() { }
        public BinaryOperator(Operator left, Operator right)
        {
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
