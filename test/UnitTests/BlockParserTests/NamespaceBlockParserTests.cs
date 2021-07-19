using CodeParser.BlockParser;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.BlockParserTests
{
    public class NamespaceBlockParserTests
    {
        [Fact]
        public async Task TestAsync()
        {
            var code =
@"using System;
using dt = Syste.DateTime;

namespace a.b.c
{
    internal class cls
    {
        int add(int a, int b) => a+b; 
    }
}";
            var parse = await new NamespaceBlockParser().Parse(code);
            var namespaceBlock = parse.Blocks.First();

            namespaceBlock.Usings.Count.ShouldBe(2);
            namespaceBlock.Name.RawPhrase.ShouldBe("a.b.c");
            namespaceBlock.Body.RawPhrase.ShouldContain("int add(int a, int b) => a+b;");
        }
    }
}
