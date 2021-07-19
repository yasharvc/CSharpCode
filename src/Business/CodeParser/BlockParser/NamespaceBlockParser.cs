using CodeParser.Models;
using CodeParser.Models.Blocks;
using CodeParser.Patterns;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeParser.BlockParser
{
    public class NamespaceBlockParser : IBlockParser<NamespaceBlock>
    {
        readonly ParseWithWordSplit parseWithWordSplit = new();
        public async Task<ParseResult<NamespaceBlock>> Parse(string code, int startPos = 0)
        {
            var res = new NamespaceBlock
            {
                Usings = new List<UsingBlock>()
            };

            var pos = startPos;
            await GetUsings(code, res, pos);

            pos = code.IndexOf("namespace", startPos);
            if (pos == -1)
                return ParseResult<NamespaceBlock>.Empty(startPos);

            pos += "namespace".Length;
            var name = parseWithWordSplit.NextWord(code, pos, stopChars: "{");
            while (name.IsEmpty)
                name = parseWithWordSplit.NextWord(code, name.End, stopChars: "{");
            res.Name = name;

            var blocks = await new BalancedOpenCloseCharactrers { CloseChar = '}', OpenChar = '{' }.Compile(code);
            res.Body = new TextWithPosition
            {
                RawPhrase = blocks.Item1.First(),
                Start = blocks.Item2,
                WhiteSpaceAfter = code[blocks.Item1.First().Length..]
            };

            return new ParseResult<NamespaceBlock>
            {
                Blocks = new List<NamespaceBlock> { res },
                FinishPosition = res.Body.End,
                SartPosition = startPos
            };
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
