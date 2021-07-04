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
        public async void Should_Get_Next_Word()
        {
            var processor = new ParseWithWordSplit();
            var code = "fx(\"This is a big Test\",11)";

            var a = processor.NextWord(code);

            a.ShouldBe(fx)
        }
    }
}
