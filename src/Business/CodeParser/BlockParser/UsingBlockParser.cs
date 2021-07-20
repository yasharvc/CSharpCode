using CodeParser.Models;
using CodeParser.Models.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeParser.BlockParser
{
    public class UsingBlockParser : IBlockParser<UsingBlock>
    {
        readonly ParseWithWordSplit parseWithWordSplit = new();
        public Task<ParseResult<UsingBlock>> Parse(string code, int startPos = 0)
        {
            var list = new List<UsingBlock>();
            int pos = startPos;
            try
            {
                if (!code.Contains("using"))
                    throw new FormatException();

                while(pos >= 0)
                    pos = ParseNextUsing(code, list, pos);

                return Task.FromResult(new ParseResult<UsingBlock> { Blocks = list, FinishPosition = list.Last().Text.End, StartPosition = startPos });
            }
            catch (FormatException)
            {
                return Task.FromResult(new ParseResult<UsingBlock> { Blocks = list, FinishPosition = pos, StartPosition = startPos });
            }
        }

        private int ParseNextUsing(string code, List<UsingBlock> list, int pos)
        {
            var usingKeyword = GoUntilUsingKeyword(code, pos);
            if (usingKeyword.IsEmpty || usingKeyword.RawPhrase.Trim() != "using")
                return -1;
            var wordAfterUsing = parseWithWordSplit.NextWord(code, usingKeyword.End, stopChars: ";=");
            UsingBlock item = null;
            if (wordAfterUsing == "static")
            {
                item = GetStaticType(code, usingKeyword, wordAfterUsing);
            }
            else if(wordAfterUsing.WhiteSpaceAfter.Trim() == ";")
            {
                item = GetUsingType(code, usingKeyword, wordAfterUsing);
            }
            else
            {
                wordAfterUsing = parseWithWordSplit.NextWord(code, usingKeyword.End, stopChars: "=");
                item = GetAliasType(code, usingKeyword, wordAfterUsing);
            }
            list.Add(item);
            pos = item.Text.End;
            return pos;
        }

        private UsingBlock GetAliasType(string code, TextWithPosition usingKeyword, TextWithPosition wordAfterUsing)
        {
            var lib = parseWithWordSplit.NextWord(code, code.IndexOf("=", wordAfterUsing.Start) + 1, stopChars: ";");
            while (lib.IsEmpty)
                lib = parseWithWordSplit.NextWord(code, lib.End, stopChars: ";");
            return new UsingBlock
            {
                Type = UsingBlockType.UseAliasType,
                Library = lib.RawPhrase,
                VariableName = wordAfterUsing.RawPhrase,
                Text = new TextWithPosition
                {
                    RawPhrase = code[usingKeyword.Start..lib.End],
                    Start =usingKeyword.Start,
                    WhiteSpaceAfter=""
                }
            };
        }

        private UsingBlock GetUsingType(string code, TextWithPosition usingKeyword, TextWithPosition wordAfterUsing)
        {
            return new UsingBlock
            {
                Type = UsingBlockType.UseType,
                Library = wordAfterUsing.RawPhrase,
                VariableName = "",
                Text = new TextWithPosition
                {
                    RawPhrase = code[usingKeyword.Start..wordAfterUsing.End],
                    Start = usingKeyword.Start,
                    WhiteSpaceAfter = ""
                },
            };
        }

        private UsingBlock GetStaticType(string code, TextWithPosition usingKeyword, TextWithPosition staticKeyword)
        {
            var lib = parseWithWordSplit.NextWord(code, staticKeyword.End, stopChars: ";");
            return new UsingBlock
            {
                Text = new TextWithPosition
                {
                    RawPhrase = code[usingKeyword.Start..lib.End],
                    Start = usingKeyword.Start,
                    WhiteSpaceAfter = ""
                },
                Type = UsingBlockType.UseStaticType,
                Library = lib,
                VariableName = ""
            };
        }

        private TextWithPosition GoUntilUsingKeyword(string code, int startPos)
        {
            var res = parseWithWordSplit.NextWord(code, startPos, stopChars: ";");
            while (res.RawPhrase != "using" && res.End < code.Length && res.End > 0)
                res = parseWithWordSplit.NextWord(code, res.End, stopChars: ";");
            return res;
        }
    }
}