using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeParser.Patterns
{
    class ArrayPattern
    {
        public char SplitterChar { get; set; }
        public char ArrayEndChar { get; set; }
        public char ArrayStartChar { get; set; }
        public async Task<Tuple<List<string>, int>> Compile(string code) => await ArraySplit(code);

        private Task<Tuple<List<string>, int>> ArraySplit(string code)
        {
            var res = new List<string>();
            var pos = code.IndexOf(ArrayEndChar);
            if (pos >= 0)
            {
                if (pos == 0)
                    return Task.FromResult(new Tuple<List<string>, int>(res, 1));

                pos = code.IndexOf(ArrayStartChar) + 1;
                if (pos == 0)
                    throw new FormatException();
                var temp = -1;
                while (temp != pos || !(pos == -1 || pos == code.Length))
                {
                    var c = code[pos..];
                    temp = pos;
                    pos += GetNextElement(c, res);
                }
            }
            else
            {
                throw new FormatException();
            }
            return Task.FromResult(new Tuple<List<string>, int>(res, pos));
        }

        private int GetNextElement(string code, List<string> res)
        {
            var insideString = false;

            var temp = "";
            for (int i = 0; i < code.Length; i++)
            {
                var ch = code[i];
                temp += ch;
                if ((ch == '"' && i == 0) || (ch == '"' && i > 0 && code[i - 1] != '\\'))
                    insideString = !insideString;

                if (insideString)
                    continue;

                if (ch == SplitterChar)
                {
                    res.Add(temp[0..^1]);
                    return i + 1;
                }

                if (ch == ArrayEndChar)
                {
                    var value = temp[0..^1];
                    if (value.Length > 0)
                        res.Add(value);
                    return i + 1;
                }
            }
            return code.Length;
        }
    }
}