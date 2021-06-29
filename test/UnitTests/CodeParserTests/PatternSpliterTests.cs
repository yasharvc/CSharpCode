using CodeParser;
using Shouldly;
using System;
using System.Collections.Generic;
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

            res.Item2.ShouldBeGreaterThan(0);
            res.Item1.Count.ShouldBeGreaterThan(1);
            Assert.Contains(res.Item1["using"], x => x.Contains("using System;"));
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
