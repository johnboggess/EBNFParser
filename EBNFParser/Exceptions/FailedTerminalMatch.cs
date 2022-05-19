using EBNFParser.EBNFOperators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBNFParser.Exceptions
{
    public class FailedTerminalMatch : FailedMatch
    {
        public string ExpectedValue = "";
        public string ReceivedValue = "";

        public FailedTerminalMatch(Terminal @operator, string expectedValue, string receivedValue)
        {
            Operator = @operator;
            ExpectedValue = expectedValue;
            ReceivedValue = receivedValue;
        }
    }
}
