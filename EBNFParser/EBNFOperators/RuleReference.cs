using EBNFParser.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBNFParser.EBNFOperators
{
    public class RuleReference : Operator
    {
        private Rule _rule;

        public Rule ReferencedRule
        {
            get { return _rule; }
            set
            {
                _rule = value;
                _operators[0] = value.Operator;
            }
        }
        public Operator Operator { get { return _operators[0]; } }

        public RuleReference()
        {
            _operators = new List<Operator>() { null };
        }

        public RuleReference(Rule rule)
        {
            _operators = new List<Operator>() { null };
            ReferencedRule = rule;
        }

        public T Inner<T>() where T : Operator
        {
            return (T)Operator;
        }

        protected internal override bool Match(string str, int index, out int newIndex, out FailedMatch failedMatch)
        {
            bool success = ReferencedRule.Operator.Match(str, index, out newIndex, out FailedMatch failed);

            if (!success)
                failedMatch = new FailedRuleReferenceMatch(this, failed, ReferencedRule);
            else
                failedMatch = null;

            return success;
        }

    }
}
