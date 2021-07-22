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
    public class ClassBlockParserTests
    {
        class Cls<T,U> :FormatException where T:IComparable where U:ICollection<T>
        {

        }
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
        public async void Should_Extract_Attribute_With_String_Parameter()
        {
            var code =
@"namespace name
{
    [a(""ab[(c"",123)] class cls{}
}";
            var parser = new ClassBlockParser();

            var res = await parser.Parse(code);

            res.Blocks.Count().ShouldBe(1);
            res.Blocks.First().AccessModifier.ShouldBe(AccessModifierType.ClassDefault);
            res.Blocks.First().Attributes.Count.ShouldBe(1);
            res.Blocks.First().Attributes.First().RawText.ShouldBe("[a(\"ab[(c\",123)]");
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
        [Fact]
        public async void Should_Extract_Class_Name()
        {
            var code =
@"namespace name
{
    class cls{}
}";
            var parser = new ClassBlockParser();

            var res = await parser.Parse(code);

            res.Blocks.Count().ShouldBe(1);
            res.Blocks.First().AccessModifier.ShouldBe(AccessModifierType.ClassDefault);
            res.Blocks.First().Name.ShouldBe("cls");
        }
        [Fact]
        public async void Should_Extract_Class_Name_With_GenericType()
        {
            var code =
@"namespace name
{
    class cls<T>{}
}";
            var parser = new ClassBlockParser();

            var res = await parser.Parse(code);

            res.Blocks.Count().ShouldBe(1);
            res.Blocks.First().AccessModifier.ShouldBe(AccessModifierType.ClassDefault);
            res.Blocks.First().Name.ShouldBe("cls");
            res.Blocks.First().GenericTypes.Count.ShouldBe(1);
            res.Blocks.First().GenericTypes.First().ShouldBe("T");
        }
        [Fact]
        public async void Should_Extract_Class_Name_With_GenericTypes()
        {
            var code =
@"namespace name
{
    class cls<T,U,R>{}
}";
            var parser = new ClassBlockParser();

            var res = await parser.Parse(code);

            res.Blocks.Count().ShouldBe(1);
            res.Blocks.First().AccessModifier.ShouldBe(AccessModifierType.ClassDefault);
            res.Blocks.First().Name.ShouldBe("cls");
            res.Blocks.First().RawName.ShouldBe("cls<T,U,R>");
            res.Blocks.First().GenericTypes.Count.ShouldBe(3);
            res.Blocks.First().GenericTypes.First().ShouldBe("T");
            res.Blocks.First().GenericTypes.ElementAt(1).ShouldBe("U");
            res.Blocks.First().GenericTypes.Last().ShouldBe("R");
        }
        [Fact]
        public async void Should_Extract_Type_Constarin_For_GenericType()
        {
            var code =
@"namespace name
{
    internal class cls<T> where T: class,new(){}
}";
            var parser = new ClassBlockParser();

            var res = await parser.Parse(code);

            res.Blocks.Count().ShouldBe(1);
            res.Blocks.First().AccessModifier.ShouldBe(AccessModifierType.Internal);
            res.Blocks.First().Name.ShouldBe("cls");
            res.Blocks.First().GenericTypes.Count.ShouldBe(1);
            res.Blocks.First().GenericTypes.First().ShouldBe("T");
            res.Blocks.First().WhereClauses.Count.ShouldBe(1);
            var whereClause = res.Blocks.First().WhereClauses.First();
            whereClause.GenericTypeName.ShouldBe("T");
            whereClause.RawText.RawPhrase.ShouldBe("where T: class,new()");
            whereClause.Constraints.ShouldContain("class");
            whereClause.Constraints.ShouldContain("new()");
        }
        [Fact]
        public async void Should_Extract_Type_Constarin_For_GenericType_With_Complex_GenericType_in_constraint()
        {
            var code =
@"namespace name
{
    internal class cls<T> where T: class,new(),abc<T,string>{}
}";
            var parser = new ClassBlockParser();

            var res = await parser.Parse(code);

            res.Blocks.Count().ShouldBe(1);
            res.Blocks.First().AccessModifier.ShouldBe(AccessModifierType.Internal);
            res.Blocks.First().Name.ShouldBe("cls");
            res.Blocks.First().GenericTypes.Count.ShouldBe(1);
            res.Blocks.First().GenericTypes.First().ShouldBe("T");
            res.Blocks.First().WhereClauses.Count.ShouldBe(1);

            var whereClause = res.Blocks.First().WhereClauses.First();
            whereClause.GenericTypeName.ShouldBe("T");
            whereClause.RawText.RawPhrase.ShouldBe("where T: class,new(),abc<T,string>");
            whereClause.Constraints.ShouldContain("class");
            whereClause.Constraints.ShouldContain("new()");
            whereClause.Constraints.ShouldContain("abc<T,string>");
        }
    }
}
