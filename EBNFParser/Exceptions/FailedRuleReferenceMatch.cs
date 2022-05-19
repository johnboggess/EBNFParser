using EBNFParser.EBNFOperators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBNFParser.Exceptions
{
    public class FailedRuleReferenceMatch : FailedMatch
    {
        public FailedMatch Inner;
        public Rule Rule;

        public FailedRuleReferenceMatch(RuleReference @operator, FailedMatch inner, Rule rule)
        {
            Operator = @operator;
            Inner = inner;
            Rule = rule;
        }
    }
}
