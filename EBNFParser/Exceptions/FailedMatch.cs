using EBNFParser.EBNFOperators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBNFParser.Exceptions
{
    public abstract class FailedMatch : System.Exception
    {
        public Operator Operator;

        public override string Message
        {
            get
            {
                return $"Match failed, expected '{Operator}'";
            }
        }
    }
}
