using EBNFParser;
using EBNFParser.EBNFOperators;
using EBNFParser.Exceptions;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void StructureTesting()
        {
            string letters = "letter = 'A' | 'B' | 'C' | 'D';";
            string number = "number = { ['0' | '1'] } , '.' , { ['0' | '1'] };";
            List<Rule> rules = Grammar.Build(letters+number);
            Assert.True(rules.Count == 2);
            Assert.True(rules[0].Name == "letter");
            Assert.True(rules[1].Name == "number");

            Alternation op = (Alternation)rules[0].Operator;
            (op.Left<Terminal>()).Value = "A";

            op = (Alternation)op.RightOperator;
            (op.Left<Terminal>()).Value = "B";

            op = (Alternation)op.RightOperator;
            (op.Left<Terminal>()).Value = "C";
            (op.Right<Terminal>()).Value = "D";


            Assert.True(((Concatenation)rules[1].Operator).Left<Repetition>().Inner<Optional>().Inner<Alternation>().Left<Terminal>().Value == "0");
            Assert.True(((Concatenation)rules[1].Operator).Left<Repetition>().Inner<Optional>().Inner<Alternation>().Right<Terminal>().Value == "1");

            Assert.True(((Concatenation)rules[1].Operator).Right<Concatenation>().Left<Terminal>().Value == ".");

            Assert.True(((Concatenation)rules[1].Operator).Right<Concatenation>().Right<Repetition>().Inner<Optional>().Inner<Alternation>().Left<Terminal>().Value == "0");
            Assert.True(((Concatenation)rules[1].Operator).Right<Concatenation>().Right<Repetition>().Inner<Optional>().Inner<Alternation>().Right<Terminal>().Value == "1");


            try
            {
                letters = "letter = \"o\" | \"p\"  \"q\" | \"r\";";
                rules = Grammar.Build(letters);
            }
            catch(EBNFException e)
            {
                Assert.True(true);
            }

            try
            {
                letters = "letter = \"o\" | \"p\"  [\"q\"] | \"r\";";
                rules = Grammar.Build(letters);
            }
            catch (EBNFException e)
            {
                Assert.True(true);
            }

            try
            {
                letters = "letter = \"o\" | \"p\" | \"q\" | \"r\" |;";
                rules = Grammar.Build(letters);
            }
            catch (EBNFException e)
            {
                Assert.True(true);
            }
        }

        [Test]
        public void QuotesInStrings()
        {

            string letters = @"letter = 'A', 'A\'', 'A\"+"\""+@"', 'A\\'";
            List<Rule> rules = Grammar.Build(letters);
            Rule rule = rules[0];

            Assert.True(((Concatenation)rule.Operator).Left<Terminal>().Value == "A");
            Assert.True(((Concatenation)rule.Operator).Right<Concatenation>().Left<Terminal>().Value == "A'");
            Assert.True(((Concatenation)rule.Operator).Right<Concatenation>().Right<Concatenation>().Left<Terminal>().Value == "A\"");
            Assert.True(((Concatenation)rule.Operator).Right<Concatenation>().Right<Concatenation>().Right<Terminal>().Value == "A\\");


            letters = @"letter = '{\'a\',\'b}' | {'A' | 'b'}";
            rules = Grammar.Build(letters);
            rule = rules[0];

            Assert.True(((Alternation)rule.Operator).Left<Terminal>().Value == "{'a','b}");
            Assert.True(((Alternation)rule.Operator).Right<Repetition>().Inner<Alternation>().Left<Terminal>().Value == "A");
            Assert.True(((Alternation)rule.Operator).Right<Repetition>().Inner<Alternation>().Right<Terminal>().Value == "b");

            try
            {
                letters = @"letter = 'A', 'B', 'C''";
                rules = Grammar.Build(letters);
            }
            catch (EBNFException e)
            {
                Assert.True(true);
            }

            try
            {
                letters = @"letter = 'A'', 'B'', 'C'";
                rules = Grammar.Build(letters);
            }
            catch (EBNFException e)
            {
                Assert.True(true);
            }

        }

        [Test]
        public void RuleReferenceTest()
        {

            string letterA = "letter = \"A\", otherLetter;";
            string letterB = "otherLetter = \"B\";";

            List<Rule> rules = Grammar.Build(letterA + letterB);

            Assert.True(rules[0].Name == "letter");
            Assert.True(((Concatenation)rules[0].Operator).Left<Terminal>().Value == "A");
            Assert.True(((Concatenation)rules[0].Operator).Right<RuleReference>().ReferencedRule.Name == "otherLetter");
            Assert.True(((Concatenation)rules[0].Operator).Right<RuleReference>().Inner<Terminal>().Value == "B");

            Assert.True(rules[1].Name == "otherLetter");
            Assert.True(((Terminal)rules[1].Operator).Value == "B");
        }

        [Test]
        public void SplitOnTerminalRuleEndSymbol()
        {
            //There was an issue where the grammar builder though ; inside terminals were the end of the rule
            string ebnf = "test = '[' | ';';";
            List<Rule> rules = Grammar.Build(ebnf);

            ebnf = "test = '[' | 'a;';";
            rules = Grammar.Build(ebnf);

            ebnf = "test = '[' | 'a;a';";
            rules = Grammar.Build(ebnf);
        }

        [Test]
        public void SplitOnRuleStartSymbol()
        {
            //There was an issue where the grammar builder though = inside terminals were to spliut the rule between its name and value
            string ebnf = "test = '[' | '=';";
            List<Rule> rules = Grammar.Build(ebnf);

            ebnf = "test = '[' | 'a=';";
            rules = Grammar.Build(ebnf);

            ebnf = "test = '[' | 'a=a';";
            rules = Grammar.Build(ebnf);
        }

        [Test]
        public void FullEBNFGrammarTest()
        {
            string grammar = File.ReadAllText("EBNFGrammar.txt");
            List<Rule> rules = Grammar.Build(grammar);

            foreach(Rule rule in rules)
            {
                if(rule.Name == "letter")
                {
                    int A = 'A';
                    int a = 'a';
                    Alternation op = (Alternation)rule.Operator;
                    for(int i = 0; i < 26; i++)
                    {
                        Assert.True(op.Left<Terminal>().Value == ""+((char)(A + i)));
                        op = op.Right<Alternation>();
                    }
                    for (int i = 0; i < 24; i++)
                    {
                        Assert.True(op.Left<Terminal>().Value == "" + ((char)(a + i)));
                        op = op.Right<Alternation>();
                    }
                    Assert.True(op.Left<Terminal>().Value == "y");
                    Assert.True(op.Right<Terminal>().Value == "z");
                }
                else if(rule.Name == "digit")
                {
                    Alternation op = (Alternation)rule.Operator;
                    for (int i = 0; i < 8; i++)
                    {
                        Assert.True(op.Left<Terminal>().Value == i.ToString());
                        op = op.Right<Alternation>();
                    }
                    Assert.True(op.Left<Terminal>().Value == "8");
                    Assert.True(op.Right<Terminal>().Value == "9");
                }
                else if(rule.Name == "symbol")
                {
                    Assert.True(((Alternation)rule.Operator).Left<Terminal>().Value == "[");
                    Assert.True(((Alternation)rule.Operator).Right<Alternation>().Left<Terminal>().Value == "]");
                    Assert.True(((Alternation)rule.Operator).Right<Alternation>().Right<Alternation>().Left<Terminal>().Value == "{");
                    Assert.True(((Alternation)rule.Operator).Right<Alternation>().Right<Alternation>().Right<Alternation>().Left<Terminal>().Value == "}");
                    Assert.True(((Alternation)rule.Operator).Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Left<Terminal>().Value == "(");
                    Assert.True(((Alternation)rule.Operator).Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Left<Terminal>().Value == ")");
                    Assert.True(((Alternation)rule.Operator).Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Left<Terminal>().Value == "<");
                    Assert.True(((Alternation)rule.Operator).Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Left<Terminal>().Value == ">");
                    Assert.True(((Alternation)rule.Operator).Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Left<Terminal>().Value == "'");
                    Assert.True(((Alternation)rule.Operator).Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Left<Terminal>().Value == "\"");
                    Assert.True(((Alternation)rule.Operator).Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Left<Terminal>().Value == "=");
                    Assert.True(((Alternation)rule.Operator).Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Left<Terminal>().Value == "|");
                    Assert.True(((Alternation)rule.Operator).Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Left<Terminal>().Value == ".");
                    Assert.True(((Alternation)rule.Operator).Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Left<Terminal>().Value == ",");
                    Assert.True(((Alternation)rule.Operator).Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Terminal>().Value == ";");
                }
            }
        }
    }
}