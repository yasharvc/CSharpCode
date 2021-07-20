using CodeParser.Enums;
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
            var classBlock = new ClassBlock
            {
                AccessModifier = AccessModifierType.ClassDefault,
                Attributes = new List<AttributeBlock>()
            };
            var classPos = code.IndexOf("class");

            var classInCode = await balancedOpenCloseCharactrers.Compile(code[(classPos + "class".Length)..]);

            var canContinue = true;

            var wordBefore = parseWithWordSplit.PreviousWord(code, classPos, stopChars: "(){};");
            canContinue = !wordBefore.IsEmpty;
            while (canContinue)
            {
                if (wordBefore.IsEmpty || wordBefore.RawPhrase.Trim() == "")
                    break;

                ExtractClassType(classBlock, wordBefore);
                ExtractAccessModifier(classBlock, wordBefore);
                ExtractAttribute(classBlock, wordBefore);

                wordBefore = parseWithWordSplit.PreviousWord(code, wordBefore.Start, stopChars: "(){};");
            }
            classBlock.Attributes.Reverse();
            return new ParseResult<ClassBlock>
            {
                Blocks = new List<ClassBlock> { classBlock },
                FinishPosition = startPos,
                StartPosition = 0
            };
        }

        private void ExtractAttribute(ClassBlock classBlock, TextWithPosition wordBefore)
        {
            if (wordBefore.RawPhrase.StartsWith('[') && wordBefore.RawPhrase.EndsWith(']'))
                classBlock.Attributes.Add(new AttributeBlock { RawText = wordBefore.RawPhrase });
        }

        private static void ExtractAccessModifier(ClassBlock classBlock, TextWithPosition currentWord)
        {
            if (currentWord.RawPhrase == "public")
                classBlock.AccessModifier = AccessModifierType.Public;
            else if (currentWord.RawPhrase == "internal")
                classBlock.AccessModifier = AccessModifierType.Internal;
            else if (currentWord.RawPhrase == "protected")
                classBlock.AccessModifier = classBlock.AccessModifier == AccessModifierType.Internal ? AccessModifierType.ProtectedInternal : AccessModifierType.Protected;
            else if (currentWord.RawPhrase == "private")
                classBlock.AccessModifier = classBlock.AccessModifier == AccessModifierType.Protected ? AccessModifierType.PrivateProtected : AccessModifierType.Private;
        }

        private static void ExtractClassType(ClassBlock classBlock, TextWithPosition currentWord)
        {
            if (currentWord.RawPhrase == "partial")
                classBlock.IsPartial = true;
            else if (currentWord.RawPhrase == "abstract")
                classBlock.IsAbstract = true;
            else if (currentWord.RawPhrase == "sealed")
                classBlock.IsSealed = true;
            else if (currentWord.RawPhrase == "static")
                classBlock.IsStatic = true;
        }
    }
}
