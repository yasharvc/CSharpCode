namespace UnitTests.RegExTests
{
    using PCRE;
    using Shouldly;
    using System.Linq;
    using System.Text.RegularExpressions;
    using UnitTests.CodeParserTests;
    using Xunit;
    public class PCRETests
    {

        [Fact]
        public async void Should_Parse_Using()
        {
            var code = await PatternSpliterTests.ReadResourceAsync("fullcode.txt");
            var matched = PcreRegex.Matches(code, @"(^\s*using\s+[\w|.]+;)*", PcreOptions.MultiLine)
                .Where(m => !(string.IsNullOrEmpty(m.Value) || string.IsNullOrWhiteSpace(m.Value)))
                .ToList();

            foreach (var match in matched)
            {
                var end = match.EndIndex;
                var start = end - match.Length;
                var y = code[start..end];
                var x = match.ToString();
            }
        }

        [Fact]
        public async void Should_Parse_Class()
        {
            var code = await PatternSpliterTests.ReadResourceAsync("fullcode.txt");
            var matched = PcreRegex.Matches(code, @"(\[\w+.*\])*\s*(public|private|protected|internal)?\s*class\s*\w+\s*", PcreOptions.MultiLine)
                .Where(m => !(string.IsNullOrEmpty(m.Value) || string.IsNullOrWhiteSpace(m.Value)))
                .ToList();

            foreach (var match in matched)
            {
                var end = match.EndIndex;
                var start = end - match.Length;
                var y = code[start..end];
                var x = match.ToString();
            }
        }

        [Fact]
        public async void Should_Parse_Class_RegEx()
        {
            var code = await PatternSpliterTests.ReadResourceAsync("fullcode.txt");
            var regex = new Regex(@"(\[\w+.*\])*\s*(?<modifier>(public|private|protected|internal)?)\s*class\s*\w+\s*", RegexOptions.Multiline);
                

            foreach (Match match in regex.Matches(code))
            {
                var x = match.ToString();
            }
        }

        [Fact]
        public async void Should_Parse_Balanced_Paranteses()
        {
            var code = "some text(text here(possible text)text(possible text(more text)))end text";
            var regex = new Regex(@"\((?>\((?<c>)|[^()]+|\)(?<-c>))*(?(c)(?!))\)");

            var res = regex.Matches(code);
            res.First().Value.ShouldBe("(text here(possible text)text(possible text(more text)))");
        }

        [Fact]
        public async void Should_Parse_Balanced_Braces()
        {
            var code = "some text{text here{possible text}text{possible text{more text}}}end text";
            var regex = new Regex(@"\{(?>\{(?<c>)|[^{}]+|\}(?<-c>))*(?(c)(?!))\}");

            var res = regex.Matches(code);
            res.First().Value.ShouldBe("{text here{possible text}text{possible text{more text}}}");
        }
    }
}
