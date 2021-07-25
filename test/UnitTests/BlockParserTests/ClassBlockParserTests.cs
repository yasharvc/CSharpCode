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
        [Fact]
        public async void Should_Extract_Type_Constarin_For_GenericType_With_GenericTypes_in_constraint()
        {
            var code =
@"namespace name
{
    class cls<T,U> where T: class,new(),abc<T,string> where U: struct{}
}";
            var parser = new ClassBlockParser();

            var res = await parser.Parse(code);

            res.Blocks.Count().ShouldBe(1);
            res.Blocks.First().AccessModifier.ShouldBe(AccessModifierType.ClassDefault);
            res.Blocks.First().Name.ShouldBe("cls");
            res.Blocks.First().GenericTypes.Count.ShouldBe(2);
            res.Blocks.First().GenericTypes.First().ShouldBe("T");
            res.Blocks.First().GenericTypes.Last().ShouldBe("U");
            res.Blocks.First().WhereClauses.Count.ShouldBe(2);

            var whereClause = res.Blocks.First().WhereClauses.First();
            whereClause.GenericTypeName.ShouldBe("T");
            whereClause.RawText.RawPhrase.ShouldBe("where T: class,new(),abc<T,string>");
            whereClause.Constraints.ShouldContain("class");
            whereClause.Constraints.ShouldContain("new()");
            whereClause.Constraints.ShouldContain("abc<T,string>");

            whereClause = res.Blocks.First().WhereClauses.ElementAt(1);
            whereClause.GenericTypeName.ShouldBe("U");
            whereClause.RawText.RawPhrase.ShouldBe("where U: struct");
            whereClause.Constraints.ShouldContain("struct");
        }
        [Fact]
        public async void Should_Extract_Class_Inheritance()
        {
            var code =
@"namespace name
{
    class cls : BaseClass,IIface,IDict<string, int>{}
}";
            var parser = new ClassBlockParser();

            var res = await parser.Parse(code);

            res.Blocks.Count().ShouldBe(1);
            res.Blocks.First().AccessModifier.ShouldBe(AccessModifierType.ClassDefault);
            res.Blocks.First().Name.ShouldBe("cls");
            res.Blocks.First().RawName.ShouldBe("cls");
            res.Blocks.First().GenericTypes.Count.ShouldBe(0);
            res.Blocks.First().InheritedClass.ClassName.ShouldBe("BaseClass");
            res.Blocks.First().InheritedClass.GenericTypes.Count.ShouldBe(0);

            var x = res.Blocks.First().InheritedInterfaces;

            x.Count.ShouldBe(2);
            x.First().InterfaceName.ShouldBe("IIface");
            x.Last().InterfaceName.ShouldBe("IDict");
            x.Last().GenericTypes.First().ShouldBe("string");
            x.Last().GenericTypes.Last().ShouldBe("int");
        }
        [Fact]
        public async void Should_Extract_Class_Inheritance_GenericBaseClass()
        {
            var code =
@"namespace name
{
    class cls : BaseClass<X>,IIface,IDict<string, int>{}
}";
            var parser = new ClassBlockParser();

            var res = await parser.Parse(code);

            res.Blocks.Count().ShouldBe(1);
            res.Blocks.First().AccessModifier.ShouldBe(AccessModifierType.ClassDefault);
            res.Blocks.First().Name.ShouldBe("cls");
            res.Blocks.First().RawName.ShouldBe("cls");
            res.Blocks.First().GenericTypes.Count.ShouldBe(0);
            res.Blocks.First().InheritedClass.ClassName.ShouldBe("BaseClass");
            res.Blocks.First().InheritedClass.GenericTypes.Count.ShouldBe(1);
            res.Blocks.First().InheritedClass.GenericTypes.First().ShouldBe("X");

            var x = res.Blocks.First().InheritedInterfaces;

            x.Count.ShouldBe(2);
            x.First().InterfaceName.ShouldBe("IIface");
            x.Last().InterfaceName.ShouldBe("IDict");
            x.Last().GenericTypes.First().ShouldBe("string");
            x.Last().GenericTypes.Last().ShouldBe("int");
        }
    }
}
