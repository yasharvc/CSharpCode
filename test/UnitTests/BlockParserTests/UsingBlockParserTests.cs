using CodeParser.BlockParser;
using CodeParser.Models.Blocks;
using Shouldly;
using System.Linq;
using Xunit;

namespace UnitTests.BlockParserTests
{
    public class UsingBlockParserTests
    {
        [Fact]
        public async void Should_Parse_StaticTypeUsing()
        {
            var code = "using static \r\nSystem.Console;";
            var parser = new UsingBlockParser();

            var x = await parser.Parse(code);

            x.Blocks.Count().ShouldBe(1);
            x.Blocks.First().ToString().ShouldBe(code);
        }

        [Fact]
        public void Should_Generate_StaticTypeUsing_With_Given_Parameters()
        {
            var code = "using static System.Console;";
            var usingBlock = new UsingBlock
            {
                Type = UsingBlockType.UseStaticType,
                Library = "System.Console"
            };

            usingBlock.ToString().ShouldBe(code);
        }
        [Fact]
        public async void Should_Parse_TypeUsing()
        {
            var code = "using System.Console;";
            var parser = new UsingBlockParser();

            var x = await parser.Parse(code);

            x.Blocks.Count().ShouldBe(1);
            x.Blocks.First().ToString().ShouldBe(code);
        }
        [Fact]
        public async void Should_Parse_2_Using()
        {
            var code = "using a;using b;using static   system.Data.DataTable  ;";
            var parser = new UsingBlockParser();

            var x = await parser.Parse(code);

            x.Blocks.Count().ShouldBe(3);
            x.Blocks.First().ToString().ShouldBe(code.Split(';')[0] + ";");
            x.Blocks.Last().Type.ShouldBe(UsingBlockType.UseStaticType);
            x.Blocks.Last().Library.ShouldBe("system.Data.DataTable");
        }
        [Fact]
        public async void Should_Parse_AliasUsing()
        {
            var code = "using dt= System.Data.DateTable;";
            var parser = new UsingBlockParser();

            var x = await parser.Parse(code);

            x.Blocks.Count().ShouldBe(1);
            x.Blocks.First().ToString().ShouldBe(code);
            x.Blocks.First().Type.ShouldBe(UsingBlockType.UseAliasType);
            x.Blocks.First().Library.ShouldBe("System.Data.DateTable");
            x.Blocks.First().VariableName.ShouldBe("dt");
        }
        [Fact]
        public async void Should_Parse_AllUsings()
        {
            var code = 
@"
using System;
using dt= System.Data.DateTable;
using static System.Console;

namespace fx{
    public class cls
    {
    }
}
";
            var parser = new UsingBlockParser();

            var x = await parser.Parse(code);

            x.Blocks.Count().ShouldBe(3);
            x.Blocks.First().Type.ShouldBe(UsingBlockType.UseType);
            x.Blocks.ElementAt(1).Type.ShouldBe(UsingBlockType.UseAliasType);
            x.Blocks.ElementAt(2).Type.ShouldBe(UsingBlockType.UseStaticType);
        }
    }
}