using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CodeParser.Models;
using Shouldly;
using Xunit;

namespace UnitTests.TextTransformTests
{
    public class TextTransformTests
    {
        [Fact]
        public void Exclude_String()
        {
            const string code = 
"this is test\r\n//Comment876 as6\r\nasd";
            var cls = CodeTransformer.GetCode(code);
            cls.OneLineComments.Count().ShouldBe(1);
            cls.OneLineComments.First().ShouldBe("//Comment876 as6\r\n");
        }

        private class CodeTransformer
        {
            private string OriginalCode { get; set; }
            public IEnumerable<TextWithPosition> CompiledCode { get; } = new List<TextWithPosition>();

            public IEnumerable<string> OneLineComments { get; } = new List<string>(); 

            public static CodeTransformer GetCode(string input)
            {
                var c = new CodeTransformer(input);
                c.CompiledCode = c.RemoveComments(c.OriginalCode);
                return c;
            }

            private CodeTransformer(string originalCode)
            {
                OriginalCode = originalCode;
            }

            private string RemoveComments(string input)
            {
                var pos = 0;
                while (input.IndexOf("//", StringComparison.Ordinal) != -1)
                {
                    pos = input.IndexOf("//", pos, StringComparison.Ordinal);
                    if (pos == -1) break;
                    
                    var eof = input.IndexOf("\n", pos, StringComparison.Ordinal);
                    eof = eof == -1 ? input.Length : eof + 1;
                    ((List<TextWithPosition>) OneLineComments).Add(new TextWithPosition
                    {
                        Start = pos,
                        RawPhrase = input[pos..eof],
                        WhiteSpaceAfter = ""
                    });
                }
                return input;
            }
        }
    }
}
