using EBNFParser;
using EBNFParser.EBNFOperators;
using EBNFParser.Exceptions;
using NUnit.Framework;
using System.Collections.Generic;

namespace Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            string letters = "letter = \"A\" | \"B\" | \"C\" | \"D\";";
            string number = "number = { [\"0\" | \"1\"] } , \".\" , { [\"0\" | \"1\"] };";
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
                Assert.Pass();
            }

            try
            {
                letters = "letter = \"o\" | \"p\"  [\"q\"] | \"r\";";
                rules = Grammar.Build(letters);
            }
            catch (EBNFException e)
            {
                Assert.Pass();
            }
        }
    }
}