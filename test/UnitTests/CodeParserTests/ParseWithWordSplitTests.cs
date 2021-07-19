using CodeParser;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.CodeParserTests
{
    public class ParseWithWordSplitTests
    {
        [Fact]
        public void Should_Get_Next_Word()
        {
            var processor = new ParseWithWordSplit();
            var code = "My  name";

            var firstWord = processor.NextWord(code);
            var secondWord = processor.NextWord(code, firstWord.End);

            firstWord.RawPhrase.ShouldBe("My");
            firstWord.Start.ShouldBe(0);
            firstWord.WhiteSpaceAfter.ShouldBe("  ");
            firstWord.IsEmpty.ShouldBeFalse();
            firstWord.End.ShouldBe(4);
        }

        [Fact]
        public void Should_Get_Function_Name_And_Parameters()
        {
            var processor = new ParseWithWordSplit();
            var code = "func(1,\r\n\"ab,c)d,sjfh,gfdg f,dgd\")";

            var firstWord = processor.NextWord(code, stopChars: "({");
            var secondWord = processor.NextWord(code, firstWord.End, stopChars: ",)");
            var thirdWord = processor.NextWord(code, secondWord.End, stopChars: ",)");

            firstWord.RawPhrase.ShouldBe("func");
            firstWord.Start.ShouldBe(0);
            firstWord.WhiteSpaceAfter.ShouldBe("(");
            firstWord.IsEmpty.ShouldBeFalse();

            secondWord.RawPhrase.ShouldBe("1");
            thirdWord.RawPhrase.ShouldBe("\"ab,c)d,sjfh,gfdg f,dgd\"");
        }


        [Fact]
        public void Should_Get_Lambda()
        {
            var processor = new ParseWithWordSplit();
            var code = "func((X)=>{X++;})";

            var firstWord = processor.NextWord(code, stopChars: "({");
            var secondWord = processor.NextWord(code, firstWord.End, stopChars: ",)");

            firstWord.RawPhrase.ShouldBe("func");
            firstWord.Start.ShouldBe(0);
            firstWord.WhiteSpaceAfter.ShouldBe("(");
            firstWord.IsEmpty.ShouldBeFalse();

            secondWord.RawPhrase.ShouldBe("1");
        }

        [Fact]
        public async void Should_Get_Using()
        {
            var processor = new ParseWithWordSplit();
            var code = await PatternSpliterTests.ReadResourceAsync("namespace1.txt");

            var firstWord = processor.NextWord(code);
            var secondWord = processor.NextWord(code, firstWord.End, stopChars: ";");

            firstWord.RawPhrase.ShouldBe("using");
            firstWord.Start.ShouldBe(0);
            firstWord.WhiteSpaceAfter.ShouldBe(" ");
            firstWord.IsEmpty.ShouldBeFalse();

            secondWord.RawPhrase.ShouldBe("System");
        }
    }
}
