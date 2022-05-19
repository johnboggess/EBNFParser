using EBNFParser;
using EBNFParser.EBNFOperators;
using EBNFParser.Exceptions;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace Tests
{
    public class GrammarBuildTest
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
            string exception = "exception = {'1', '2', '3'} - '3' , '4';";
            List<Rule> rules = Grammar.Build(letters+number+exception);
            Assert.True(rules.Count == 3);
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


            Assert.True(((Concatenation)rules[2].Operator).Left<Exception>().Left<Repetition>().Inner<Concatenation>().Left<Terminal>().Value == "1");
            Assert.True(((Concatenation)rules[2].Operator).Left<Exception>().Left<Repetition>().Inner<Concatenation>().Right<Concatenation>().Left<Terminal>().Value == "2");
            Assert.True(((Concatenation)rules[2].Operator).Left<Exception>().Left<Repetition>().Inner<Concatenation>().Right<Concatenation>().Right<Terminal>().Value == "3");
            Assert.True(((Concatenation)rules[2].Operator).Left<Exception>().Right<Terminal>().Value == "3");
            Assert.True(((Concatenation)rules[2].Operator).Right<Terminal>().Value == "4");

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
            string s = "";
            foreach (Rule rule in rules)
                s += $"{rule.Name} = {rule.Operator.ToString()};\n";
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
                else if(rule.Name == "character")
                {
                    Assert.True(((Alternation)rule.Operator).Left<RuleReference>().ReferencedRule.Name == "letter");
                    Assert.True(((Alternation)rule.Operator).Right<Alternation>().Left<RuleReference>().ReferencedRule.Name == "digit");
                    Assert.True(((Alternation)rule.Operator).Right<Alternation>().Right<Alternation>().Left<RuleReference>().ReferencedRule.Name == "symbol");
                    Assert.True(((Alternation)rule.Operator).Right<Alternation>().Right<Alternation>().Right<Terminal>().Value == "_");
                }
                else if(rule.Name == "identifier")
                {
                    Assert.True(((Concatenation)rule.Operator).Left<RuleReference>().ReferencedRule.Name == "letter");
                    Assert.True(((Concatenation)rule.Operator).Right<Repetition>().Inner<Alternation>().Left<RuleReference>().ReferencedRule.Name == "letter");
                    Assert.True(((Concatenation)rule.Operator).Right<Repetition>().Inner<Alternation>().Right<Alternation>().Left<RuleReference>().ReferencedRule.Name == "digit");
                    Assert.True(((Concatenation)rule.Operator).Right<Repetition>().Inner<Alternation>().Right<Alternation>().Right<Terminal>().Value == "_");
                }
                else if(rule.Name == "lhs")
                {
                    Assert.True(((RuleReference)rule.Operator).ReferencedRule.Name == "identifier");
                }
                else if(rule.Name == "rhs")
                {
                    Assert.True(((Alternation)rule.Operator).Left<RuleReference>().ReferencedRule.Name == "identifier");
                    Assert.True(((Alternation)rule.Operator).Right<Alternation>().Left<RuleReference>().ReferencedRule.Name == "terminal");

                    Concatenation concate = ((Alternation)rule.Operator).Right<Alternation>().Right<Alternation>().Left<Concatenation>();
                    Assert.True(concate.Left<Terminal>().Value == "[");
                    Assert.True(concate.Right<Concatenation>().Left<RuleReference>().ReferencedRule.Name == "rhs");
                    Assert.True(concate.Right<Concatenation>().Right<Terminal>().Value == "]");
                    
                    concate = ((Alternation)rule.Operator).Right<Alternation>().Right<Alternation>().Right<Alternation>().Left<Concatenation>();
                    Assert.True(concate.Left<Terminal>().Value == "{");
                    Assert.True(concate.Right<Concatenation>().Left<RuleReference>().ReferencedRule.Name == "rhs");
                    Assert.True(concate.Right<Concatenation>().Right<Terminal>().Value == "}");

                    concate = ((Alternation)rule.Operator).Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Left<Concatenation>();
                    Assert.True(concate.Left<Terminal>().Value == "(");
                    Assert.True(concate.Right<Concatenation>().Left<RuleReference>().ReferencedRule.Name == "rhs");
                    Assert.True(concate.Right<Concatenation>().Right<Terminal>().Value == ")");

                    concate = ((Alternation)rule.Operator).Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Left<Concatenation>();
                    Assert.True(concate.Left<RuleReference>().ReferencedRule.Name == "rhs");
                    Assert.True(concate.Right<Concatenation>().Left<Terminal>().Value == "|");
                    Assert.True(concate.Right<Concatenation>().Right<RuleReference>().ReferencedRule.Name == "rhs");

                    concate = ((Alternation)rule.Operator).Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Alternation>().Right<Concatenation>();
                    Assert.True(concate.Left<RuleReference>().ReferencedRule.Name == "rhs");
                    Assert.True(concate.Right<Concatenation>().Left<Terminal>().Value == ",");
                    Assert.True(concate.Right<Concatenation>().Right<RuleReference>().ReferencedRule.Name == "rhs");
                }
                else if(rule.Name == "rule")
                {
                    Assert.True(((Concatenation)rule.Operator).Left<RuleReference>().ReferencedRule.Name == "lhs");
                    Assert.True(((Concatenation)rule.Operator).Right<Concatenation>().Left<Terminal>().Value == "=");
                    Assert.True(((Concatenation)rule.Operator).Right<Concatenation>().Right<Concatenation>().Left<RuleReference>().ReferencedRule.Name == "rhs");
                    Assert.True(((Concatenation)rule.Operator).Right<Concatenation>().Right<Concatenation>().Right<Terminal>().Value == ";");
                }
                else if(rule.Name == "grammar")
                {
                    Assert.True(((Repetition)rule.Operator).Inner<RuleReference>().ReferencedRule.Name == "rule");
                }
            }
        }
    }
}