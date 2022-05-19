using EBNFParser.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBNFParser.EBNFOperators
{
    public class Terminal : Operator
    {
        public string Value = "";
        public Terminal() { }
        public Terminal(string str)
        {
            Value = str;
        }

        protected internal override bool Match(string str, int index, out int newIndex, out FailedMatch failedMatch)
        {
            newIndex = index;

            failedMatch = new FailedTerminalMatch(this, Value, string.Join("", str.Skip(index).Take(Value.Length)));
            if (!MatchString(str, index))
                return false;

            newIndex += Value.Length;
            failedMatch = null;
            return true;
        }

        private bool MatchString(string str, int strIndex)
        {
            for(int i = 0; i < Value.Length; i++)
            {
                if (Value[i] != str[strIndex + i])
                    return false;
            }
            return true;
        }
    }
}
