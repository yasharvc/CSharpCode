using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeParser.Patterns
{
    public class BalancedOpenCloseCharactrers
    {
        public char OpenChar { get; set; }
        public char CloseChar { get; set; }

        public async Task<Tuple<List<string>, int>> Compile(string code) => await BalancedOpenClose(code);

        private Task<Tuple<List<string>, int>> BalancedOpenClose(string code)
        {
            var res = new List<string>();
            var pos = code.IndexOf(OpenChar);
            if (pos >= 0)
            {
                var c = code.Substring(pos, GetBalancedEndIndex(code[pos..]));
                res.Add(c);
                pos += c.Length;
            }
            else
            {
                res.Add(code);
                pos = code.Length;
            }
            return Task.FromResult(new Tuple<List<string>, int>(res, pos));
        }

        private int GetBalancedEndIndex(string code)
        {
            var cnt = 0;
            var findedFirstChar = false;
            var insideString = false;
            var nextCloseCharIndex = code.IndexOf(CloseChar);
            if (nextCloseCharIndex == -1)
                return code.Length;
            for (int i = 0; i < code.Length; i++)
            {
                var ch = code[i];
                if ((ch == '"' && i == 0) || (ch == '"' && i > 0 && code[i - 1] != '\\'))
                    insideString = !insideString;

                if (insideString)
                    continue;

                if (code[i] == OpenChar)
                {
                    findedFirstChar = true;
                    cnt++;
                }
                else if (code[i] == CloseChar)
                    cnt--;

                if (cnt == 0 && findedFirstChar)
                    return i + 1;
            }
            return code.Length;
        }
    }
}