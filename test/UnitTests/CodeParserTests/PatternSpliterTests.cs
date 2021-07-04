using CodeParser;
using CodeParser.Patterns;
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

        [Fact]
        public async void Should_Throw_Exception_For_Not_Matched_ArrayPattern()
        {
            var code = "var i = 100;";
            var spliter = new PatternSpliter("`@,|()`->{params}");

            await Assert.ThrowsAsync<FormatException>(async () => await spliter.Process(code, 0));
        }

        [Fact]
        public async void Should_Get_Items_for_Array_With_ArrayPattern()
        {
            var code = "[100,'233',03294]";
            var spliter = new PatternSpliter("`@,|[]`->{params}");

            var res = await spliter.Process(code, 0);

            res.Item2.ShouldBeGreaterThan(3);
            res.Item1.Count.ShouldBe(1);
            Assert.Contains(res.Item1["params"], x => x.Contains("100"));
            Assert.Contains(res.Item1["params"], x => x.Contains("'233'"));
            Assert.Contains(res.Item1["params"], x => x.Contains("03294"));
        }

        [Fact]
        public async void Should_Get_Items_for_Array_With_ArrayPattern_Ignore_NewLine()
        {
            var code = 
                @"[100,
'233',
03294,
""Yashar,"",190]";
            var spliter = new PatternSpliter("`@,|[]`->{params}");

            var res = await spliter.Process(code, 0);

            res.Item2.ShouldBeGreaterThan(3);
            res.Item1.Count.ShouldBe(1);
            Assert.Contains(res.Item1["params"], x => x.Contains("100"));
            Assert.Contains(res.Item1["params"], x => x.Contains("'233'"));
            Assert.Contains(res.Item1["params"], x => x.Contains("03294"));
            Assert.Contains(res.Item1["params"], x => x.Contains("\"Yashar,\""));
            Assert.Contains(res.Item1["params"], x => x.Contains("190"));
        }

        [Fact]
        public async void Get_First_Code_Inside_Namespace()
        {
            var code = await ReadResourceAsync("namespace1.txt");
            var spliter = new PatternSpliter("`|`namespace``->{using}`namespace`->{namespace}`|`{``->{name}`{_}`->{body}");

            var res = await spliter.Process(code, 0);

            var innerCode = res.Item1["body"].First();

            var x = await new BalancedOpenCloseCharactrers { OpenChar = '{', CloseChar = '}' }.Compile(innerCode[1..^1]);
            x = await new BalancedOpenCloseCharactrers { OpenChar = '{', CloseChar = '}' }.Compile(x.Item1.First()[1..^1]);
            var z = x.Item1.First();
        }

        [Fact]
        public async void Parse_With_Balanced_Parns()
        {
            var code = await ReadResourceAsync("namespace1.txt");
            var patternProceesor = new BalancedOpenCloseCharactrers { OpenChar = '{', CloseChar = '}' };

            var currentPos = 0;

            var parts = await ProcessCode(code, currentPos, patternProceesor);

            parts = await ProcessCode(code, parts.BlockStartPos + 1, patternProceesor);

            parts = await ProcessCode(code, parts.BlockStartPos + 1, patternProceesor);

            var ch = GetCharacterBeforePosition(code, parts.BlockStartPos, " \t\b\r\n{");

            var cc = code[ch];
            var fx = code[ch..];
        }

        private static async Task<CodeContext> ProcessCode(string code, int startPos, BalancedOpenCloseCharactrers patternProceesor)
        {
            var block = await patternProceesor.Compile(code[startPos..]);

            var charPos = GetCharacterBeforePosition(code, startPos + block.Item2 - block.Item1.First().Length);

            var before = charPos > 0 ? code[..charPos] : "";
            var after = code[(block.Item2 + startPos)..];

            var blockStr = block.Item1.First();

            return new CodeContext
            {
                After = after,
                Before = before,
                Block = blockStr,
                BlockStartPos = block.Item2 - block.Item1.First().Length + startPos
            };
        }

        private static int GetCharacterBeforePosition(string fullCode , int blockStartPos, string spaceChars = " \t\b\r\n")
        {
            for (; blockStartPos > 0; blockStartPos--)
            {
                if (spaceChars.Contains(fullCode[blockStartPos]))
                    continue;
                return blockStartPos;
            }
            return 0;
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

        class CodeContext
        {
            public string Before { get; set; }
            public string Block { get; set; }
            public string After { get; set; }
            public int BlockStartPos { get; set; }
        }
    }
}
