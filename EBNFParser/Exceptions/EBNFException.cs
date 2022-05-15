using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBNFParser.Exceptions
{
    public class EBNFException : Exception
    {
        public EBNFException(string msg) : base(msg) { }
        public EBNFException(string msg, Exception innerException) : base(msg, innerException) { }
    }
}
