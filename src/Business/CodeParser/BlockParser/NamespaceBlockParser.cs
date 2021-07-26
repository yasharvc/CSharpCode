using CodeParser.Models;
using CodeParser.Models.Blocks;
using CodeParser.Patterns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeParser.BlockParser
{
    public class NamespaceBlockParser : IBlockParser<NamespaceBlock>
    {
        readonly ParseWithWordSplit parseWithWordSplit = new();
        readonly BalancedOpenCloseCharactrers balancedOpenCloseCharactrers = new() { CloseChar = '}', OpenChar = '{' };
        readonly IBlockParser<ClassBlock> classParser = new ClassBlockParser();
        public async Task<ParseResult<NamespaceBlock>> Parse(string code, int startPos = 0)
        {
            var res = new NamespaceBlock
            {
                Usings = new List<UsingBlock>(),
                Classes = new List<ClassBlock>()
            };

            var pos = startPos;
            await GetUsings(code, res, pos);

            pos = code.IndexOf("namespace", startPos);
            if (pos == -1)
                return ParseResult<NamespaceBlock>.Empty(startPos);

            pos += "namespace".Length;
            pos = ParseWithWordSplit.SkipSpaces(code, pos);
            pos = GetName(code, res, pos);
            pos = await GetBody(code, res, pos);
            pos = await GetClasses(code, res);

            return new ParseResult<NamespaceBlock>
            {
                Blocks = new List<NamespaceBlock> { res },
                FinishPosition = code.Length,
                StartPosition = startPos
            };
        }

        private async Task<int> GetClasses(string code, NamespaceBlock block)
        {
            if (!code.Contains("class", StringComparison.CurrentCulture))
                return 0;

            var res = await classParser.Parse(code, block.Body.Start);

            block.Classes.AddRange(res.Blocks);

            return res.FinishPosition;
        }

        private async Task<int> GetBody(string code, NamespaceBlock res, int pos)
        {
            
            var blocks = await balancedOpenCloseCharactrers.Compile(code);
            res.Body = new TextWithPosition
            {
                RawPhrase = blocks.Item1.First(),
                Start = pos,
                WhiteSpaceAfter = ""
            };
            return res.Body.End;
        }

        private int GetName(string code, NamespaceBlock res, int pos)
        {
            var name = parseWithWordSplit.NextWord(code, pos, stopChars: "{");
            while (name.IsEmpty)
                name = parseWithWordSplit.NextWord(code, name.End, stopChars: "{");
            res.Name = name;
            return name.End;
        }

        private static async Task<int> GetUsings(string code, NamespaceBlock res, int pos)
        {
            ParseResult<UsingBlock> usingsPart = await new UsingBlockParser().Parse(code, pos);
            res.Usings.AddRange(usingsPart.Blocks);
            pos = usingsPart.FinishPosition;
            return pos;
        }
    }
}
