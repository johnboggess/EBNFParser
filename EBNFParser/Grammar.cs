using EBNFParser.EBNFOperators;
using EBNFParser.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EBNFParser
{
    public class Grammar
    {
        private static Regex ruleReference = new Regex("[a-zA-Z][a-zA-Z0-9]*(?!&)");
        private static Regex ruleEnd = new Regex(";(?!\"|')");
        public static List<Rule> Build(string ebnf)
        {
            //Rmove all terminal string values and replace them with an identifier. This identifer of the form &x& where x which terminal in the rule
            //Terminal operators will be assigned their actual values when created
            //This is to prevent issues detecting characters inside strings as EBNF symbols
            Dictionary<int, string> terminalValues = ExtractStrings(ebnf, out ebnf);
            if (ebnf.Contains("\"") || ebnf.Contains("'"))
                throw new EBNFException("Unbalanced terminal symbols");//todo: give line number

            List<Rule> rules = new List<Rule>();

            string[] lines = ruleEnd.Split(ebnf);

            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                string line = lines[lineIndex];
                if (line.All(x=>char.IsWhiteSpace(x)))
                    continue;
                string[] split = line.Split("=");
                if (split.Length == 1)
                    throw new EBNFException($"Undefined/Unnamed rule at line {lineIndex}");
                if (split.Length > 2)
                    throw new EBNFException($"Extra '=' at line {lineIndex}");

                string name = string.Join("", split[0].Where(x => !char.IsWhiteSpace(x)));
                string rule = string.Join("", split.Skip(1));

                if (rules.Any(x => x.Name == name))
                    throw new EBNFException($"Rule '{name}' defined more than once");

                rule = ruleReference.Replace(rule, "%$&%");

                //todo: when adding in regex crete special symbol to denote start/end (e.g. like ? for special sequence) and strip those out similar to strings
                
                //Get only the characters representing EBNF operators
                IEnumerable<OperatorCharacter> symbols = GetOperators(rule);

                int optionalCount = symbols.Count(x => x.Symbol == Symbol.OptionalStart || x.Symbol == Symbol.OptionalEnd);
                int repetitionCount = symbols.Count(x => x.Symbol == Symbol.RepetitionStart || x.Symbol == Symbol.RepetitionEnd);
                int groupingCount = symbols.Count(x => x.Symbol == Symbol.GroupingStart || x.Symbol == Symbol.GroupingEnd);

                if (!isEven(optionalCount))
                    throw new EBNFException($"Unbalanced optional symbols at line {lineIndex}");
                if (!isEven(repetitionCount))
                    throw new EBNFException($"Unbalanced repetition symbols {lineIndex}");
                if (!isEven(groupingCount))
                    throw new EBNFException($"Unbalanced grouping symbols {lineIndex}");

                Operator topLevelOperator = ConstructGrammar(symbols, rule, terminalValues);



                rules.Add(new Rule() { Name = name, Operator = topLevelOperator });
            }

            foreach(Rule rule in rules)
                FillRuleReferences(rule.Operator, rules);

            return rules;
        }
        private static Operator ConstructGrammar(IEnumerable<OperatorCharacter> operators, string rule, Dictionary<int, string> terminalValues)
        {
            List<Operator> operatorSequence = new List<Operator>();

            ///First create a list of the operators in the rule.
            ///Binary operators child operators are not set here.
            ///Each unary and terminal operator does have their inner operator set.
            ///Defining the unary operators first allows the tree to be orangized by optional/repition/group operators. I splits each into there own level of the tree.
            for (int i = 0; i < operators.Count(); i++)
            {
                OperatorCharacter op = operators.ElementAt(i);

                if (op.Symbol == Symbol.Terminal)
                {
                    //define the terminal operator
                    //The first & of a terminal will always be followed by the ending & of the terminal (see are only looking at characters representing EBNF operators).
                    OperatorCharacter endTerminal = operators.ElementAt(i + 1);

                    //even though the previous comment should be true, check just in case
                    if (endTerminal.Symbol != Symbol.Terminal)
                        throw new EBNFException("Non terminal end following terminal start");

                    string terminalValue = rule.Substring(op.Index + 1, endTerminal.Index - op.Index - 1);
                    terminalValue = terminalValue.Replace("&", "");
                    operatorSequence.Add(new Terminal(terminalValues[int.Parse(terminalValue)]));
                    i += 1;
                }
                else if (op.Symbol == Symbol.RuleReference)
                {
                    //we are referencing another rule
                    //Like terminal symbols, the symbol after the first in a pair of rule reference symbols is always another rule reference
                    OperatorCharacter endTerminal = operators.ElementAt(i + 1);

                    //even though the previous comment should be true, check just in case
                    if (endTerminal.Symbol != Symbol.RuleReference)
                        throw new EBNFException("Invalid name for rule being referenced");

                    string referencedRule = rule.Substring(op.Index + 1, endTerminal.Index - op.Index - 1);
                    referencedRule = referencedRule.Replace("%", "");
                    //put dummy name with real rules name.
                    operatorSequence.Add(new RuleReference(new Rule() { Name = referencedRule }));
                    i += 1;
                }
                else if (IsUnarySymbol(op.Symbol))
                {
                    //We define unary operators now so the tree is first divided by optional/repition/group operators
                    int end = FindUnaryEnd(operators.Skip(i)) + i;
                    if (i + 1 == end)
                        throw new EBNFException("empty optional, repetition or grouping sequence");

                    UnaryOperator unaryOperator;
                    IEnumerable<OperatorCharacter> innerOperators = operators.Skip(i + 1).Take(end - i - 1);
                    Operator inner = ConstructGrammar(innerOperators, rule, terminalValues);

                    if (op.Symbol == Symbol.OptionalStart)
                        unaryOperator = new Optional(inner);
                    else if (op.Symbol == Symbol.RepetitionStart)
                        unaryOperator = new Repetition(inner);
                    else if (op.Symbol == Symbol.GroupingStart)
                        unaryOperator = new Grouping(inner);
                    else
                        throw new EBNFException($"Unknown unary operator {op.Symbol.ToString()}");

                    operatorSequence.Add(unaryOperator);
                    i = end;
                }
                else if (op.Symbol == Symbol.Concatenation)
                    operatorSequence.Add(new Concatenation());
                else if (op.Symbol == Symbol.Alternation)
                    operatorSequence.Add(new Alternation());
                else if(op.Symbol == Symbol.Exception)
                    operatorSequence.Add(new EBNFOperators.Exception());
            }

            if (operatorSequence.Count == 1)
                return operatorSequence[0];


            for(int i = 1; i < operatorSequence.Count; i+=2)
            {
                if(!(operatorSequence[i] is BinaryOperator))
                {
                    if (operatorSequence[i] is Terminal)
                        throw new EBNFException($"Terminal {((Terminal)(operatorSequence[i])).Value} not properly seperated by a binary operator.");
                    else
                        throw new EBNFException($"{operatorSequence[i].GetType().Name} not properly seperated by a binary operator.");
                }
            }

            if (operatorSequence.First() is BinaryOperator || operatorSequence.Last() is BinaryOperator)
                throw new EBNFException("Rule cannot start or end with binary operator");

            (List<Operator> sequence, int start, int length) sequence = GetFirstConsecutiveSequenceOfOperators(operatorSequence, typeof(EBNFOperators.Exception));
            while (sequence.start > -1)
            {
                Operator tree = GenerateBinaryTreeFromSequence(sequence.sequence);
                operatorSequence.RemoveRange(sequence.start, sequence.length);
                operatorSequence.Insert(sequence.start, tree);
                sequence = GetFirstConsecutiveSequenceOfOperators(operatorSequence, typeof(EBNFOperators.Exception));
            }

            sequence = GetFirstConsecutiveSequenceOfOperators(operatorSequence, typeof(Concatenation));
            while (sequence.start > -1)
            {
                Operator tree = GenerateBinaryTreeFromSequence(sequence.sequence);
                operatorSequence.RemoveRange(sequence.start, sequence.length);
                operatorSequence.Insert(sequence.start, tree);
                sequence = GetFirstConsecutiveSequenceOfOperators(operatorSequence, typeof(Concatenation));
            }

            sequence = GetFirstConsecutiveSequenceOfOperators(operatorSequence, typeof(Alternation));
            while (sequence.start > -1)
            {
                Operator tree = GenerateBinaryTreeFromSequence(sequence.sequence);
                operatorSequence.RemoveRange(sequence.start, sequence.length);
                operatorSequence.Insert(sequence.start, tree);
                sequence = GetFirstConsecutiveSequenceOfOperators(operatorSequence, typeof(Alternation));
            }

            if (operatorSequence.Count != 1)
                throw new EBNFException("Failed to construct grammar tree");

            return operatorSequence[0];
        }

        private static void FillRuleReferences(Operator parent, List<Rule> rules)
        {
            if(parent is RuleReference ruleReference)
            {
                string ruleName = ruleReference.ReferencedRule.Name;
                Rule r = rules.Where(x => x.Name == ruleName).FirstOrDefault();
                if (r == null)
                    throw new EBNFException($"Referencing undefined rule '{ruleName}'");
                else
                    ruleReference.ReferencedRule = r;
                return;
            }
            foreach(Operator op in parent.Operators)
            {
                FillRuleReferences(op, rules);
            }
        }
        private static (List<Operator> sequence, int start, int length) GetFirstConsecutiveSequenceOfOperators(List<Operator> nodes, Type operatorType)
        {
            int binarySymbolStart = -1;//beginning of the consecutive set of operators
            int binarySymbolEnd = -1;//end of the consecutive set of operators
            for (int i = 1; i < nodes.Count(); i += 2)//add two becuase at this point every other node should be a binary operator
            {
                Operator node = nodes.ElementAt(i);
                Type nodeType = node.GetType();
                if (nodeType == operatorType && binarySymbolStart < 0)
                {
                    //We have found the first occurance of the operator
                    binarySymbolStart = i;
                    binarySymbolEnd = binarySymbolStart;
                }
                else if (nodeType == operatorType && binarySymbolStart >= 0)
                {
                    //We have found a another node in the sequence. This is now the last known node
                    binarySymbolEnd = i;
                }

                if ((nodeType != operatorType && binarySymbolStart >= 0) || binarySymbolEnd + 2 >= nodes.Count())
                {
                    //A operator has been found that isnt the target operator, ending the sequence
                    //Find the range over which the sequnce spans
                    int rangeStart = binarySymbolStart - 1;//Subtract one as the operator to the left of the first operator is the left child node of the first operator
                    int rangeEnd = binarySymbolEnd + 1;//Add one as the operator to the right of the last operator is the right child node of the last operator
                    int rangeLength = rangeEnd - rangeStart + 1;

                    IEnumerable<Operator> concatenationNodes = nodes.Skip(rangeStart).Take(rangeLength);
                    return (concatenationNodes.ToList(), rangeStart, rangeLength);
                }
            }

            return new(new List<Operator>(), -1, 0);
        }

        /// <summary>
        /// Given a collection of grammar nodes formr a binary tree of the first binary symnol found in the collection.
        /// The first binary symbol in the collection must be the only binary symbol in the collection
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        private static Operator GenerateBinaryTreeFromSequence(IEnumerable<Operator> nodes)
        {
            BinaryOperator leftSymbol = null;
            BinaryOperator topSymbol = null;
            for (int i = 1; i < nodes.Count(); i += 2)
            {
                Operator node = nodes.ElementAt(i);
                if (leftSymbol == null)//should only be null at first iteration
                {
                    leftSymbol = (BinaryOperator)nodes.ElementAt(i);
                    leftSymbol.LeftOperator = nodes.ElementAt(i - 1);
                    topSymbol = leftSymbol;
                }
                else
                {
                    BinaryOperator binarySymbol = (BinaryOperator)nodes.ElementAt(i);
                    binarySymbol.LeftOperator = nodes.ElementAt(i - 1);
                    leftSymbol.RightOperator = binarySymbol;
                    leftSymbol = binarySymbol;
                }
            }
            leftSymbol.RightOperator = nodes.ElementAt(nodes.Count() - 1);
            return topSymbol;
        }

        /// <summary>
        /// Find the ending character for the first optional/repetition/group start character of the given operator list.
        /// The list must start with a optional/repetition/group start character
        /// </summary>
        /// <param name="operators"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        private static int FindUnaryEnd(IEnumerable<OperatorCharacter> operators)
        {
            int optionalStart = 0;
            int repetitionStart = 0;
            int groupingStart = 0;

            for (int i = 0; i < operators.Count(); i++)
            {
                if (operators.ElementAt(i).Symbol == Symbol.OptionalStart)
                    optionalStart += 1;
                else if (operators.ElementAt(i).Symbol == Symbol.RepetitionStart)
                    optionalStart += 1;
                else if (operators.ElementAt(i).Symbol == Symbol.GroupingEnd)
                    optionalStart += 1;

                else if (operators.ElementAt(i).Symbol == Symbol.OptionalEnd)
                    optionalStart -= 1;
                else if (operators.ElementAt(i).Symbol == Symbol.RepetitionEnd)
                    optionalStart -= 1;
                else if (operators.ElementAt(i).Symbol == Symbol.GroupingEnd)
                    optionalStart -= 1;

                if (optionalStart == 0 && repetitionStart == 0 && groupingStart == 0)
                    return i;
            }
            if (optionalStart > 0)
                throw new EBNFException("Missing optional end");
            if (repetitionStart > 0)
                throw new EBNFException("Missing repetition end");
            if (groupingStart > 0)
                throw new EBNFException("Missing grouping end");
            throw new EBNFException("Missing optional, repetition or grouping end");
        }
        private static IEnumerable<OperatorCharacter> GetOperators(string str)
        {
            IEnumerable<(int index, (bool isSymbol, Symbol symbol) symbol)> chars = str.Select((c, i) =>
            {
                return (i, IsCharacterEBNFOperator(c));
            });

            return chars.Where(x => x.symbol.isSymbol).Select(x => new OperatorCharacter() { Index = x.index, Symbol = x.symbol.symbol });
        }

        private static (bool, Symbol) IsCharacterEBNFOperator(char c)
        {
            if (c == ',')
                return (true, Symbol.Concatenation);
            else if (c == '|')
                return (true, Symbol.Alternation);
            else if (c == '[')
                return (true, Symbol.OptionalStart);
            else if (c == ']')
                return (true, Symbol.OptionalEnd);
            else if (c == '{')
                return (true, Symbol.RepetitionStart);
            else if (c == '}')
                return (true, Symbol.RepetitionEnd);
            else if (c == '(')
                return (true, Symbol.GroupingStart);
            else if (c == ')')
                return (true, Symbol.GroupingEnd);
            else if (c == '-')
                return (true, Symbol.Exception);
            else if (c == '&')
                return (true, Symbol.Terminal);
            else if (c == '%')
                return (true, Symbol.RuleReference);
            return (false, Symbol.Terminal);
        }
        private static bool isEven(int i)
        {
            return (i % 1) == 0;
        }

        private static bool IsBinarySymbol(Symbol symbol)
        {
            return symbol == Symbol.Concatenation || symbol == Symbol.Alternation;
        }

        private static bool IsUnarySymbol(Symbol symbol)
        {
            bool result = symbol == Symbol.OptionalStart || symbol == Symbol.OptionalEnd;
            result = result || symbol == Symbol.RepetitionStart || symbol == Symbol.RepetitionEnd;
            result = result || symbol == Symbol.GroupingStart || symbol == Symbol.GroupingEnd;
            result = result || symbol == Symbol.Terminal;
            return result;
        }

        private static Dictionary<int, string> ExtractStrings(string str, out string result)
        {
            Dictionary<int, string> strings = new Dictionary<int, string>();
            int stringsFound = 0;
            int start = -1;
            for (int i = 0; i < str.Length; i++)
            {
                if (!(str[i] == '\'' || str[i] == '\"'))
                    continue;

                //ignore \' and dont ignore \\'
                if (i > 0 && str[i - 1] == '\\')
                    if (!(i > 1 && str[i - 2] == '\\'))
                        continue;
                if (start == -1)
                {
                    start = i;
                    continue;
                }
                string strValue = str.Substring(start + 1, i - start - 1);
                string insertValue = $"&{stringsFound}&";
                str = str.Remove(start, i - start+1);
                str = str.Insert(start, insertValue);

                strValue = strValue.Replace(@"\'", @"'");
                strValue = strValue.Replace("\\\"", "\"");
                strValue = strValue.Replace(@"\\", @"\");
                strings.Add(stringsFound, strValue);

                i = start;
                start = -1;
                stringsFound += 1;
            }
            result = str;
            return strings;
        }

        private class OperatorCharacter
        {
            /// <summary>
            /// The operators symbol
            /// </summary>
            public Symbol Symbol;
            /// <summary>
            /// Where the symbol is in the string that forms the rule
            /// </summary>
            public int Index;
        }

        internal enum Symbol
        {
            Concatenation,
            Alternation,
            OptionalStart,
            OptionalEnd,
            RepetitionStart,
            RepetitionEnd,
            GroupingStart,
            GroupingEnd,
            Exception,
            Terminal,
            RuleReference,
        }
    }
}
