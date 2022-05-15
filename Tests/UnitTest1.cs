using EBNFParser;
using NUnit.Framework;

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
            Operator op = Grammar.Build("digit = \"0\" | [\"a\"], \"1\" | \"2\";");
            op = Grammar.Build("digit = \"0\", [\"a\"] | \"1\";");
            op = Grammar.Build("digit = \"0a\" | \"1\" | \"2\" | \"3\",\"b\",[\"&\"] | \"4\" | \"5\" | \"6\" | \"7\" | \"8\" | \"9\" ;");
            op = Grammar.Build("digit = [\"1\"];");
            op = Grammar.Build("digit = [];");
        }
    }
}