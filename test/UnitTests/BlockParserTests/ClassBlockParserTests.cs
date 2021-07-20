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
    abstract partial class dd{

    }
    public class ClassBlockParserTests
    {
        [Fact]
        public async void Test()
        {
            var code =
@"namespace name
{
    abstract partial class cls{}
}";
            var parser = new ClassBlockParser();

            var res = await parser.Parse(code);

            res.Blocks.Count().ShouldBe(1);
            res.Blocks.First().IsAbstract.ShouldBeTrue();
            res.Blocks.First().IsPartial.ShouldBeTrue();
            res.Blocks.First().IsSealed.ShouldBeFalse();
            res.Blocks.First().IsStatic.ShouldBeFalse();
        }
    }
}
