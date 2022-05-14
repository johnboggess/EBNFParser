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
            Rule.Buid("digit = \"0\" | [\"a\"], \"1\" | \"2\";");
            Rule.Buid("digit = \"0\", [\"a\"] | \"1\";");
            Rule.Buid("digit = \"0a\" | \"1\" | \"2\" | \"3\",\"b\",[\"&\"] | \"4\" | \"5\" | \"6\" | \"7\" | \"8\" | \"9\" ;");
            Rule.Buid("digit = [\"1\"];");
            Rule.Buid("digit = [];");
        }
    }
}