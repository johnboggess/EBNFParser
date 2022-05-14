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
                    throw new System.Exception($"Invalid rule at line {lineIndex}");//todo: show line number

                string name = split[0];//remove ws
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
            //check if its only terminal symbols
            bool hasTermials = !symbols.Any(x => x.Symbol == Symbol.Terminal);
            bool hasBinary = symbols.Any(x => IsBinarySymbol(x.Symbol));
            bool hasUnary = symbols.Any(x => IsUnarySymbol(x.Symbol));

            //The following converts symbols index pairs to grammar nodes.
            //Unary nodes do get there inner node assign here. Binary nodes do not, as unary have higher precedence.
            List<GrammarNode> nodes = new List<GrammarNode>();
            for(int i = 0; i < symbols.Count(); i++)
            {
                SymbolIndexPair symbol = symbols.ElementAt(i);
                if(symbol.Symbol == Symbol.Terminal)
                {
                    nodes.Add(new TerminalSymbol());
                    i += 1;
                }
                else if(symbol.Symbol == Symbol.Concatenation)
                {
                    nodes.Add(new BinarySymbol() { Symbol = Symbol.Concatenation });
                }
                else if (symbol.Symbol == Symbol.Alternation)
                {
                    nodes.Add(new BinarySymbol() { Symbol = Symbol.Alternation });
                }
                else if(IsUnarySymbol(symbol.Symbol))
                {
                    int end = FindEnd(symbols.Skip(i)) + i;
                    if(i+1 == end)
                        throw new System.Exception("empty optional, repetition or grouping sequence");
                    GrammarNode unaryNode = new UnarySymbol()
                    {
                        Symbol = symbol.Symbol,
                        Inner = ConstructGrammar(symbols.Skip(i+1).Take(end - i-1), rule)
                    };
                    nodes.Add(unaryNode);
                    i = end;
                }
            }


            if (nodes.Count == 1)
                return nodes[0];

            GenerateBinaryTreeForSpecificSymbol(nodes, Symbol.Concatenation);
            GenerateBinaryTreeForSpecificSymbol(nodes, Symbol.Alternation);

            if (nodes.Count != 1)
                throw new System.Exception("Failed to construct grammar tree");

            return nodes[0];
        }

        private static void GenerateBinaryTreeForSpecificSymbol(List<GrammarNode> nodes, Symbol symbol)
        {
            if (!IsBinarySymbol(symbol))
                throw new System.Exception("Must be a binary symbol");

            int binarySymbolStart = -1;
            int binarySymbolEnd = -1;
            for (int i = 1; i < nodes.Count; i += 2)
            {
                if (nodes[i].Symbol == symbol && binarySymbolStart < 0)
                {
                    binarySymbolStart = i;
                    binarySymbolEnd = binarySymbolStart;
                }
                else if (nodes[i].Symbol == symbol && binarySymbolStart >= 0)
                    binarySymbolEnd = i;
                
                if ((nodes[i].Symbol != symbol && binarySymbolStart >= 0) || binarySymbolEnd+2 >= nodes.Count)
                {
                    int rangeStart = binarySymbolStart - 1;
                    int rangeLength = (binarySymbolEnd + 1) - (binarySymbolStart - 1) + 1;
                    IEnumerable<GrammarNode> concatenationNodes = nodes.Skip(rangeStart).Take(rangeLength);
                    GrammarNode concate = GenerateBinaryTreeForSpecificSymbol(concatenationNodes);
                    nodes.RemoveRange(rangeStart, rangeLength);
                    nodes.Insert(rangeStart, concate);
                    binarySymbolStart = -1;
                    binarySymbolEnd = -1;

                    //minus one buase we add plus two to every iterations.
                    //Also replacing the concatenations moveed all binary symbols left such that they are nolonger aligned
                    i = rangeStart - 1;
                }

            }
        }

        /// <summary>
        /// Given a collection of grammar nodes formr a binary tree of the first binary symnol found in the collection.
        /// The first binary symbol in the collection must be the only binary symbol in the collection
        /// </summary>
        /// <param name="grammarNodes"></param>
        /// <returns></returns>
        private static GrammarNode GenerateBinaryTreeForSpecificSymbol(IEnumerable<GrammarNode> grammarNodes)
        {
            BinarySymbol leftSymbol = null;
            BinarySymbol topSymbol = null;
            for (int i = 1; i < grammarNodes.Count(); i+=2)
            {
                GrammarNode node = grammarNodes.ElementAt(i);
                if (leftSymbol == null)//shpuld only be null at first iteration
                {
                    leftSymbol = (BinarySymbol)grammarNodes.ElementAt(i);
                    leftSymbol.Left = grammarNodes.ElementAt(i - 1);
                    topSymbol = leftSymbol;
                }
                else
                {
                    BinarySymbol binarySymbol = (BinarySymbol)grammarNodes.ElementAt(i);
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

            for(int i = 0; i < symbols.Count(); i++)
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
            throw new System.Exception("Missing optional, repetition or grouping end");
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


        
        private class TerminalSymbol : GrammarNode
        {
            public string Value = "";
            public TerminalSymbol() { Symbol = Symbol.Terminal; }
        }
        private class UnarySymbol : GrammarNode
        {
            public GrammarNode Inner;
        }
        private class BinarySymbol : GrammarNode
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
