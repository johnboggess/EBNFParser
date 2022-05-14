using EBNFParser.Rules;

namespace EBNFParser
{
    public class Rule
    {
        public string Name { get; private set; }

        public Rule() { }

        public static Rule Buid(string ebnf)
        {
            string[] lines = ebnf.Split(";");
            for(int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                string line = lines[lineIndex];
                if (line == "")
                    continue;
                string[] split = line.Split("=");
                if (split.Length == 1)
                    throw new System.Exception($"Invalid rule at line {lineIndex}");

                string name = string.Join("", split[0].Where(x => !char.IsWhiteSpace(x)));
                string rule = string.Join("", split.Skip(1));

                //todo: strip out terminals first. Replace them with easy to identify references. e.g "return" become $1$
                //todo: when adding in regex crete special symbol to denote start/end (e.g. like ? for special sequence) and strip those out similar to strings
                IEnumerable<SymbolIndexPair> symbols = GetSymbols(rule);

                int optionalCount = symbols.Count(x => x.Symbol == Symbol.OptionalStart || x.Symbol == Symbol.OptionalEnd);
                int repetitionCount = symbols.Count(x => x.Symbol == Symbol.RepetitionStart || x.Symbol == Symbol.RepetitionEnd);
                int groupingCount = symbols.Count(x => x.Symbol == Symbol.GroupingStart || x.Symbol == Symbol.GroupingEnd);
                int terminalCount = symbols.Count(x => x.Symbol == Symbol.Terminal);

                if (!isEven(optionalCount))
                    throw new System.Exception("Unbalanced optional symbols");
                if (!isEven(repetitionCount))
                    throw new System.Exception("Unbalanced repetition symbols");
                if (!isEven(groupingCount))
                    throw new System.Exception("Unbalanced grouping symbols");
                if (!isEven(terminalCount))
                    throw new System.Exception("Unbalanced terminal symbols");

                GrammarNode node = ConstructGrammar(symbols, rule);
            }

            return null;
        }

        private static IEnumerable<SymbolIndexPair> GetSymbols(string str)
        {
            IEnumerable<(int index, (bool isSymbol, Symbol symbol) symbol)> chars = str.Select((c, i) =>
            {
                return (i, IsCharacterEBNFSymbol(c));
            });

            return chars.Where(x => x.symbol.isSymbol).Select(x=>new SymbolIndexPair() { Index = x.index, Symbol = x.symbol.symbol });
        }

        private static GrammarNode ConstructGrammar(IEnumerable<SymbolIndexPair> symbols, string rule)
        {

            //The following converts symbols index pairs to grammar nodes.
            //Unary nodes do get there inner node assign here. Binary nodes do not, as unary have higher precedence.
            List<GrammarNode> nodes = new List<GrammarNode>();
            for(int i = 0; i < symbols.Count(); i++)
            {
                SymbolIndexPair symbol = symbols.ElementAt(i);
                if(symbol.Symbol == Symbol.Terminal)
                {
                    SymbolIndexPair endTerminal = symbols.ElementAt(i+1);
                    if (endTerminal.Symbol != Symbol.Terminal)
                        throw new System.Exception("Non terminal end following terminal start");
                    nodes.Add(new TerminalOperator()
                    {
                        Value = rule.Substring(symbol.Index + 1, endTerminal.Index - symbol.Index - 1),
                    });
                    i +=1;
                }
                else if (IsUnarySymbol(symbol.Symbol))
                {
                    int end = FindEnd(symbols.Skip(i)) + i;
                    if (i + 1 == end)
                        throw new System.Exception("empty optional, repetition or grouping sequence");
                    GrammarNode unaryNode = new UnaryOperator()
                    {
                        Symbol = symbol.Symbol,
                        Inner = ConstructGrammar(symbols.Skip(i + 1).Take(end - i - 1), rule)
                    };
                    nodes.Add(unaryNode);
                    i = end;
                }
                else if(symbol.Symbol == Symbol.Concatenation)
                {
                    nodes.Add(new BinaryOperator() { Symbol = Symbol.Concatenation });
                }
                else if (symbol.Symbol == Symbol.Alternation)
                {
                    nodes.Add(new BinaryOperator() { Symbol = Symbol.Alternation });
                }
            }


            if (nodes.Count == 1)
                return nodes[0];

            
            (List<GrammarNode> sequence, int start, int length) sequence = GetFirstConsecutiveSequenceOfOperators(nodes, Symbol.Concatenation);
            while(sequence.start > -1)
            {
                GrammarNode tree = GenerateBinaryTreeFromSequence(sequence.sequence);
                nodes.RemoveRange(sequence.start, sequence.length);
                nodes.Insert(sequence.start, tree);
                sequence = GetFirstConsecutiveSequenceOfOperators(nodes, Symbol.Concatenation);
            }

            sequence = GetFirstConsecutiveSequenceOfOperators(nodes, Symbol.Alternation);
            while (sequence.start > -1)
            {
                GrammarNode tree = GenerateBinaryTreeFromSequence(sequence.sequence);
                nodes.RemoveRange(sequence.start, sequence.length);
                nodes.Insert(sequence.start, tree);
                sequence = GetFirstConsecutiveSequenceOfOperators(nodes, Symbol.Alternation);
            }

            if (nodes.Count != 1)
                throw new System.Exception("Failed to construct grammar tree");

            return nodes[0];
        }

        private static (List<GrammarNode> sequence, int start, int length) GetFirstConsecutiveSequenceOfOperators(List<GrammarNode> nodes, Symbol symbol)
        {
            int binarySymbolStart = -1;//beginning of the consecutive set of operators
            int binarySymbolEnd = -1;//end of the consecutive set of operators
            for (int i = 1; i < nodes.Count(); i += 2)//add two becuase at this point every other node should be a binary operator
            {
                GrammarNode node = nodes.ElementAt(i);
                if (node.Symbol == symbol && binarySymbolStart < 0)
                {
                    //We have found the first occurance of the operator
                    binarySymbolStart = i;
                    binarySymbolEnd = binarySymbolStart;
                }
                else if (node.Symbol == symbol && binarySymbolStart >= 0)
                {
                    //We have found a another node in the sequence. This is now the last known node
                    binarySymbolEnd = i;
                }

                if ((node.Symbol != symbol && binarySymbolStart >= 0) || binarySymbolEnd + 2 >= nodes.Count())
                {
                    //A operator has been found that isnt the target operator, ending the sequence
                    //Find the range over which the sequnce spans
                    int rangeStart = binarySymbolStart - 1;//Subtract one as the operator to the left of the first operator is the left child node of the first operator
                    int rangeEnd = binarySymbolEnd + 1;//Add one as the operator to the right of the last operator is the right child node of the last operator
                    int rangeLength = rangeEnd - rangeStart + 1;

                    IEnumerable<GrammarNode> concatenationNodes = nodes.Skip(rangeStart).Take(rangeLength);
                    return (concatenationNodes.ToList(), rangeStart, rangeLength);
                }
            }

            return new(new List<GrammarNode>(), -1, 0);
        }

        /// <summary>
        /// Given a collection of grammar nodes formr a binary tree of the first binary symnol found in the collection.
        /// The first binary symbol in the collection must be the only binary symbol in the collection
        /// </summary>
        /// <param name="grammarNodes"></param>
        /// <returns></returns>
        private static GrammarNode GenerateBinaryTreeFromSequence(IEnumerable<GrammarNode> grammarNodes)
        {
            BinaryOperator leftSymbol = null;
            BinaryOperator topSymbol = null;
            for (int i = 1; i < grammarNodes.Count(); i+=2)
            {
                GrammarNode node = grammarNodes.ElementAt(i);
                if (leftSymbol == null)//shpuld only be null at first iteration
                {
                    leftSymbol = (BinaryOperator)grammarNodes.ElementAt(i);
                    leftSymbol.Left = grammarNodes.ElementAt(i - 1);
                    topSymbol = leftSymbol;
                }
                else
                {
                    BinaryOperator binarySymbol = (BinaryOperator)grammarNodes.ElementAt(i);
                    binarySymbol.Left = grammarNodes.ElementAt(i - 1);
                    leftSymbol.Right = binarySymbol;
                    leftSymbol = binarySymbol;
                }
            }
            leftSymbol.Right = grammarNodes.ElementAt(grammarNodes.Count() - 1);
            return topSymbol;
        }

        private static int FindEnd(IEnumerable<SymbolIndexPair> symbols)
        {
            int optionalStart = 0;
            int repetitionStart = 0;
            int groupingStart = 0;

            for (int i = 0; i < symbols.Count(); i++)
            {
                if (symbols.ElementAt(i).Symbol == Symbol.OptionalStart)
                    optionalStart += 1;
                else if (symbols.ElementAt(i).Symbol == Symbol.RepetitionStart)
                    optionalStart += 1;
                else if (symbols.ElementAt(i).Symbol == Symbol.GroupingEnd)
                    optionalStart += 1;

                else if (symbols.ElementAt(i).Symbol == Symbol.OptionalEnd)
                    optionalStart -= 1;
                else if (symbols.ElementAt(i).Symbol == Symbol.RepetitionEnd)
                    optionalStart -= 1;
                else if (symbols.ElementAt(i).Symbol == Symbol.GroupingEnd)
                    optionalStart -= 1;

                if (optionalStart == 0 && repetitionStart == 0 && groupingStart == 0)
                    return i;
            }
            if (optionalStart > 0)
                throw new System.Exception("Missing optional end");
            if (repetitionStart > 0)
                throw new System.Exception("Missing repetition end");
            if (groupingStart > 0)
                throw new System.Exception("Missing grouping end");
            throw new System.Exception("Missing optional, repetition, grouping or terminal end");
        }

        /*
        private static GrammarNode ConstructGrammar(IEnumerable<SymbolIndexPair> symbols, string rule)
        {
            //check if its only terminal symbols
            bool onlyTermials = !symbols.Any(x => x.Symbol != Symbol.Terminal);
            bool hasBinary = symbols.Any(x => IsBinarySymbol(x.Symbol));
            bool hasUnary = symbols.Any(x => IsUnarySymbol(x.Symbol));
            if ()
            {
                return new TerminalSymbol()
                {
                    Symbol = Symbol.Terminal,
                    Value = rule.Substring(symbols.ElementAt(0).Index+1, symbols.ElementAt(1).Index - symbols.ElementAt(0).Index-1),
                };
            }

            (SymbolIndexPair symbol, int index) firstNonTerminal = symbols.Select((x, i) => (x, i)).Where((x => x.x.Symbol != Symbol.Terminal)).First();

            if(firstNonTerminal.symbol.Symbol == Symbol.Concatenation | firstNonTerminal.symbol.Symbol == Symbol.Alternation)
            {
                IEnumerable<SymbolIndexPair> left = symbols.Take(firstNonTerminal.index);
                IEnumerable<SymbolIndexPair> right = symbols.Skip(firstNonTerminal.index + 1);
                int i = 0;
                return new BinarySymbol() 
                { 
                    Symbol = firstNonTerminal.symbol.Symbol,
                    Left = ConstructGrammar(left, rule), 
                    Right = ConstructGrammar(right, rule)
                };

            }
            else if(firstNonTerminal.symbol.Symbol == Symbol.OptionalStart)
            {
                (SymbolIndexPair symbol, int index) endUnary = symbols.Select((x, i) => (x, i)).Where((x => x.x.Symbol == Symbol.OptionalEnd)).First();

                IEnumerable<SymbolIndexPair> start = symbols.Skip(firstNonTerminal.index + 1);
                IEnumerable<SymbolIndexPair> inner = start.Take(endUnary.index-1);
                return new UnarySymbol()
                {
                    Symbol = Symbol.OptionalStart,
                    Inner = ConstructGrammar(inner, rule),
                };
            }
            return null;
        }*/

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

        private static (bool, Symbol) IsCharacterEBNFSymbol(char c)
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
            else if (c == '"')
                return (true, Symbol.Terminal);
            else if(c==';')
                return (true, Symbol.Termination);
            return (false, Symbol.Terminal);
        }


        
        private class TerminalOperator : GrammarNode
        {
            public string Value = "";
            public TerminalOperator() { Symbol = Symbol.Terminal; }
        }
        private class UnaryOperator : GrammarNode
        {
            public GrammarNode Inner;
        }
        private class BinaryOperator : GrammarNode
        {
            public GrammarNode Left;
            public GrammarNode Right;
        }

        private class GrammarNode
        {
            public Symbol Symbol;
        }

        private class SymbolIndexPair
        {
            public Symbol Symbol;
            /// <summary>
            /// Where the symbol is in the string that forms the rule
            /// </summary>
            public int Index;
        }


        private enum Symbol
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
            Termination,
        }
    }
}
