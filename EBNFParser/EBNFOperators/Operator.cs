using EBNFParser.EBNFOperators;
using EBNFParser.Exceptions;

namespace EBNFParser
{
    public abstract class Operator
    {
        protected List<Operator> _operators = new List<Operator>();
        public IReadOnlyCollection<Operator> Operators { get { return _operators.AsReadOnly(); } }
        public Operator() { }

        public bool Match(string str)
        {
            int newIndex;
            bool success = Match(str, 0, out newIndex, out FailedMatch failedMatch);
            if (!success)
                throw failedMatch;
            if(newIndex != str.Length)
                throw new EBNFException($"Expected end of line, found {str.Substring(newIndex, str.Length - newIndex)}");
            return success;
        }

        public override string ToString()
        {
            if (this is Alternation alt)
                return $"{alt.LeftOperator} | {alt.RightOperator}";
            else if (this is Concatenation concat)
                return $"{concat.LeftOperator} , {concat.RightOperator}";
            else if (this is EBNFOperators.Exception ex)
                return $"{ex.LeftOperator} - {ex.RightOperator}";
            else if (this is Grouping group)
                return $"({group.InnerOperator})";
            else if (this is Optional op)
                return $"[{op.InnerOperator}]";
            else if (this is Repetition rep)
                return $"{{{rep.InnerOperator}}}";
            else if (this is RuleReference rule)
                return $" {rule.ReferencedRule.Name} ";
            else if (this is Terminal term)
            {
                string terminal = term.Value;
                terminal = terminal.Replace("'", "\\'");
                terminal = terminal.Replace("\"", "\\\"");
                return $" '{ terminal }' ";
            }
            throw new System.Exception($"Uknown operator '{GetType().Name}'");
        }

        protected internal abstract bool Match(string str, int index, out int newIndex, out FailedMatch failedMatch);
    }
}
