using CodeParser.BlockParser;
using CodeParser.Enums;
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
        public async void Should_Extract_Abstract_And_Partial()
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
            res.Blocks.First().AccessModifier.ShouldBe(AccessModifierType.ClassDefault);
        }
        [Fact]
        public async void Should_Extract_Private()
        {
            var code =
@"namespace name
{
    private abstract partial class cls{}
}";
            var parser = new ClassBlockParser();

            var res = await parser.Parse(code);

            res.Blocks.Count().ShouldBe(1);
            res.Blocks.First().IsAbstract.ShouldBeTrue();
            res.Blocks.First().IsPartial.ShouldBeTrue();
            res.Blocks.First().IsSealed.ShouldBeFalse();
            res.Blocks.First().IsStatic.ShouldBeFalse();
            res.Blocks.First().AccessModifier.ShouldBe(AccessModifierType.Private);
        }
        [Fact]
        public async void Should_Extract_PrivateProtected()
        {
            var code =
@"namespace name
{
    private protected abstract partial class cls{}
}";
            var parser = new ClassBlockParser();

            var res = await parser.Parse(code);

            res.Blocks.Count().ShouldBe(1);
            res.Blocks.First().IsAbstract.ShouldBeTrue();
            res.Blocks.First().IsPartial.ShouldBeTrue();
            res.Blocks.First().IsSealed.ShouldBeFalse();
            res.Blocks.First().IsStatic.ShouldBeFalse();
            res.Blocks.First().AccessModifier.ShouldBe(AccessModifierType.PrivateProtected);
        }
        [Fact]
        public async void Should_Extract_Attribute()
        {
            var code =
@"namespace name
{
    [a] class cls{}
}";
            var parser = new ClassBlockParser();

            var res = await parser.Parse(code);

            res.Blocks.Count().ShouldBe(1);
            res.Blocks.First().AccessModifier.ShouldBe(AccessModifierType.ClassDefault);
            res.Blocks.First().Attributes.Count.ShouldBe(1);
            res.Blocks.First().Attributes.First().RawText.ShouldBe("[a]");
        }
        [Fact]
        public async void Should_Extract_Attribute_With_Parameter()
        {
            var code =
@"namespace name
{
    [a(123)] class cls{}
}";
            var parser = new ClassBlockParser();

            var res = await parser.Parse(code);

            res.Blocks.Count().ShouldBe(1);
            res.Blocks.First().AccessModifier.ShouldBe(AccessModifierType.ClassDefault);
            res.Blocks.First().Attributes.Count.ShouldBe(1);
            res.Blocks.First().Attributes.First().RawText.ShouldBe("[a(123)]");
        }
        [Fact]
        public async void Should_Extract_Attributes()
        {
            var code =
@"namespace name
{
    [b]
    [a] class cls{}
}";
            var parser = new ClassBlockParser();

            var res = await parser.Parse(code);

            res.Blocks.Count().ShouldBe(1);
            res.Blocks.First().AccessModifier.ShouldBe(AccessModifierType.ClassDefault);
            res.Blocks.First().Attributes.Count.ShouldBe(2);
            res.Blocks.First().Attributes.First().RawText.ShouldBe("[b]");
            res.Blocks.First().Attributes.Last().RawText.ShouldBe("[a]");
        }
    }
}
