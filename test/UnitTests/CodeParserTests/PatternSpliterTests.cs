using CodeParser;
using Shouldly;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.CodeParserTests
{
    public class PatternSpliterTests
    {
        [Fact]
        public async void Should_Extract_Using_Part()
        {
            var code = await ReadResourceAsync("namespace1.txt");
            var spliter = new PatternSpliter("`|`namespace``->{using}`namespace`->{namespace}`{_}`->{body}");

            var res = await spliter.Process(code, 0);

            res.Item2.ShouldBeGreaterThan(2);
            res.Item1.Count.ShouldBeGreaterThan(1);
            Assert.Contains(res.Item1["using"], x => x.Contains("using System;"));
            Assert.Contains(res.Item1["namespace"], x => x.Contains("namespace"));
        }
        [Fact]
        public async void Should_Extract_namespace_Name_Part()
        {
            var code = await ReadResourceAsync("namespace1.txt");
            var spliter = new PatternSpliter("`|`namespace``->{using}`namespace`->{namespace}`|`{``->{name}`{_}`->{body}");

            var res = await spliter.Process(code, 0);

            res.Item2.ShouldBeGreaterThan(2);
            res.Item1.Count.ShouldBeGreaterThan(1);
            Assert.Contains(res.Item1["using"], x => x.Contains("using System;"));
            Assert.Contains(res.Item1["namespace"], x => x.Contains("namespace"));
            Assert.Contains(res.Item1.Keys, x => x.Equals("name"));
        }
        [Fact]
        public async void Should_Extract_All_Parameters()
        {
            var code = await ReadResourceAsync("UntilOr.txt");
            var spliter = new PatternSpliter("`function`->{function}`|`(``->{name}`@,|()`->{params}");

            var res = await spliter.Process(code, 0);

            res.Item2.ShouldBeGreaterThan(3);
            res.Item1.Count.ShouldBeGreaterThan(1);
            Assert.Contains(res.Item1["function"], x => x.Contains("function"));
            Assert.Contains(res.Item1["name"], x => x.Contains("fx"));
            Assert.Contains(res.Item1["params"], x => x.Equals("int a"));
            Assert.Contains(res.Item1["params"], x => x.Contains("string c"));
        }

        [Fact]
        public async void Should_Extract_All_Parameters_Ignore_String()
        {
            var code = "Yashar(\",\",123)";
            var spliter = new PatternSpliter("`|`(``->{name}`@,|()`->{params}");

            var res = await spliter.Process(code, 0);

            res.Item2.ShouldBeGreaterThan(3);
            res.Item1.Count.ShouldBeGreaterThan(1);
            Assert.Contains(res.Item1["name"], x => x.Contains("Yashar"));
            Assert.Contains(res.Item1["params"], x => x.Contains("\",\""));
            Assert.Contains(res.Item1["params"], x => x.Contains("123"));
        }

        [Fact]
        public async void Should_Throw_Exception_For_Not_Matched_UntilPattern()
        {
            var code = "function fx(int a,int b)";
            var spliter = new PatternSpliter("`|`namespace``->{using}`namespace`->{namespace}`|`{``->{name}`{_}`->{body}");

            await Assert.ThrowsAsync<FormatException>(async () => await spliter.Process(code, 0));
        }

        [Fact]
        public async void Should_Throw_Exception_For_Not_Matched_ExactWordPattern()
        {
            var code = "function fx(int a,int b)";
            var spliter = new PatternSpliter("`namespace`->{using}`{_}`->{body}");

            await Assert.ThrowsAsync<FormatException>(async () => await spliter.Process(code, 0));
        }

        private async Task<string> ReadResourceAsync(string fileName)
        {
            var asm = GetType().Assembly;
            var fullName = asm.GetManifestResourceNames().SingleOrDefault(m => m.EndsWith(fileName));
            if (string.IsNullOrEmpty(fullName))
                return "";
            using (var stream = asm.GetManifestResourceStream(fullName))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
                return await reader.ReadToEndAsync();
        }
    }
}
