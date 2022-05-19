using EBNFParser;
using EBNFParser.Exceptions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class Match
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Basic()
        {
            string ebnf = "test = 'a' | 'b' | 'c';";
            List<Rule> rules = Grammar.Build(ebnf);

            Assert.True(rules[0].Operator.Match("a"));
            Assert.True(rules[0].Operator.Match("b"));
            Assert.True(rules[0].Operator.Match("c"));

            try
            {
                rules[0].Operator.Match("d");
            }
            catch(FailedMatch failedMatch)
            {
                Assert.True(true);
            }

            try
            {
                Assert.True(!rules[0].Operator.Match("ab"));
            }
            catch (EBNFException failedMatch)
            {
                Assert.True(true);
            }

        }

        [Test]
        public void RecursiveRuleReferences()
        {
            string ebnf = "test = test | 't' ;\n";
            List<Rule> rules = Grammar.Build(ebnf);

            Assert.True(rules[0].Operator.Match("t"));

        }

        [Test]
        public void FullEBNFGrammar()
        {
            string grammar = File.ReadAllText("EBNFGrammar.txt");
            List<Rule> rules = Grammar.Build(grammar);

            Rule r = rules.Where(x => x.Name == "grammar").First();

            Assert.True(r.Operator.Match(grammar));
        }
    }
}
