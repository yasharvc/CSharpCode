using CodeParser.Models;
using CodeParser.Models.Blocks;
using CodeParser.Patterns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeParser.BlockParser
{
    public class ClassBlockParser : IBlockParser<ClassBlock>
    {
        readonly ParseWithWordSplit parseWithWordSplit = new();
        readonly BalancedOpenCloseCharactrers balancedOpenCloseCharactrers = new() { CloseChar = '}', OpenChar = '{' };
        public async Task<ParseResult<ClassBlock>> Parse(string code, int startPos = 0)
        {
            var classBlock = new ClassBlock();
            var classPos = code.IndexOf("class");

            var classInCode = await balancedOpenCloseCharactrers.Compile(code[(classPos + "class".Length)..]);

            var canContinue = true;

            var wordBefore = parseWithWordSplit.PreviousWord(code, classPos, stopChars: "(){};");
            canContinue = !wordBefore.IsEmpty;
            while (canContinue)
            {
                ExtractClassType(classBlock, canContinue, wordBefore);
                if (wordBefore.IsEmpty || wordBefore.RawPhrase.Trim() == "")
                    canContinue = false;
                wordBefore = parseWithWordSplit.PreviousWord(code, wordBefore.Start, stopChars: "(){};");
            }

            return new ParseResult<ClassBlock>
            {
                Blocks = new List<ClassBlock> { classBlock },
                FinishPosition = startPos,
                StartPosition = 0
            };
        }

        private static void ExtractClassType(ClassBlock classBlock, bool canContinue, TextWithPosition wordBefore)
        {
            if (wordBefore.RawPhrase == "partial")
                classBlock.IsPartial = true;
            else if (wordBefore.RawPhrase == "abstract")
                classBlock.IsAbstract = true;
            else if (wordBefore.RawPhrase == "sealed")
                classBlock.IsSealed = true;
            else if (wordBefore.RawPhrase == "static")
                classBlock.IsStatic = true;
        }
    }
}
