using Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeParser
{
    public class PatternSpliter
    {
        public string Pattern { get; }
        List<PatternPart> Parts { get; } = new List<PatternPart>();

        public PatternSpliter(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
            {
                throw new ArgumentException($"'{nameof(pattern)}' cannot be null or whitespace.", nameof(pattern));
            }

            Pattern = pattern;
            CheckPattern();
            ExtractPatternParts();
        }

        private void ExtractPatternParts()
        {
            var temp = Pattern;
            while (temp.Length > 0)
            {
                var old = temp;
                PatternPart patternPart = new PatternPart(temp);
                Parts.Add(patternPart);
                temp = temp[(temp.IndexOf($"{{{patternPart.Name}}}") + 2 + patternPart.Name.Length)..];
                if (old == temp)
                    throw new Exception("");
            }
        }

        private void CheckPattern()
        {
            if (!Pattern.Contains('`'))
                throw new Exception("Pattern is wrong");
        }

        public async Task<Tuple<Dictionary<string,List<string>>, int>> Process(string code, int pos)
        {
            var items = new Dictionary<string, List<string>>();

            foreach (var part in Parts)
            {
                pos = await WorkWithParts(code, pos, items, part);
            }

            return new Tuple<Dictionary<string, List<string>>, int>(items, pos);
        }

        private static async Task<int> WorkWithParts(string code, int pos, Dictionary<string, List<string>> items, PatternPart part)
        {
            if (part.Pattern.StartsWith("|"))
                pos = await UntilPatternAsync(code, pos, items, part);
            else if (part.Pattern.StartsWith('@'))
                pos = await ArrayPatternAsync(code, pos, items, part);
            else if (part.Pattern.Length == 3)
                pos = await BalancedOpenCloseCharactrersAsync(code, pos, items, part);
            else
                pos = await ExactWordPatternAsync(code, pos, items, part);

            return pos;
        }

        private static async Task<int> ArrayPatternAsync(string code, int pos, Dictionary<string, List<string>> items, PatternPart part)
        {
            var result = await new ArrayPattern { ArrayStartChar = part.Pattern[3], ArrayEndChar = part.Pattern[4], SplitterChar = part.Pattern[1] }.Compile(code[pos..]);
            items[part.Name] = result.Item1;
            pos += result.Item2;
            return pos;
        }

        private static async Task<int> BalancedOpenCloseCharactrersAsync(string code, int pos, Dictionary<string, List<string>> items, PatternPart part)
        {
            var result = await new BalancedOpenCloseCharactrers { CloseChar = part.Pattern[2], OpenChar = part.Pattern[0] }.Compile(code[pos..]);
            items[part.Name] = result.Item1;
            pos += result.Item2;
            return pos;
        }

        private static async Task<int> ExactWordPatternAsync(string code, int pos, Dictionary<string, List<string>> items, PatternPart part)
        {
            var result = await new ExactWordPattern { EndPhrase = part.Pattern }.Compile(code[pos..]);
            items[part.Name] = result.Item1;
            pos += result.Item2;
            return pos;
        }

        private static async Task<int> UntilPatternAsync(string code, int pos, Dictionary<string, List<string>> items, PatternPart part)
        {
            var result = await new UntilPattern { EndPhrase = part.Pattern[(part.Pattern.IndexOf('|') + 1)..] }.Compile(code[pos..]);
            items[part.Name] = result.Item1;
            pos += result.Item2;
            return pos;
        }

        class PatternPart
        {
            public string Name { get; set; }
            public string Pattern { get; set; }

            public PatternPart(string patternExpr)
            {
                Pattern = patternExpr.GetFromTo("`", "`->{")[1..];
                Name = patternExpr.GetFromTo("`->{", "}").Replace("`->{", "").Replace("}", "");
            }
        }

        class UntilPattern
        {
            public string EndPhrase { get; set; }
            const string NotAllowedInsideThisChars = "\"\'";

            public async Task<Tuple<List<string>, int>> Compile(string code)
            {
                if (EndPhrase.StartsWith('`'))
                    return await UntilLiteral(code, EndPhrase);
                return new Tuple<List<string>, int>(new List<string>(), 0);
            }

            private Task<Tuple<List<string>,int>> UntilLiteral(string code, string endPhrase)
            {
                var res = new List<string>();
                endPhrase = endPhrase.Replace("`", "");
                var pos = code.IndexOf(endPhrase);
                if (pos > 0)
                    res.Add(code.Substring(0, pos));
                else
                {
                    res.Add(code);
                    pos = code.Length;
                }
                return Task.FromResult(new Tuple<List<string>, int>(res,pos));
            }
        }

        class ExactWordPattern
        {
            public string EndPhrase { get; set; }
            const string NotAllowedInsideThisChars = "\"\'";

            public async Task<Tuple<List<string>, int>> Compile(string code) => await ExactWord(code, EndPhrase);

            private Task<Tuple<List<string>, int>> ExactWord(string code, string endPhrase)
            {
                var res = new List<string>();
                endPhrase = endPhrase.Replace("`", "");
                var pos = code.IndexOf(endPhrase);
                if (pos == 0)
                {
                    res.Add(code.Substring(0, endPhrase.Length));
                    pos = endPhrase.Length;
                }
                else
                {
                    res.Add(code);
                    pos = code.Length;
                }
                return Task.FromResult(new Tuple<List<string>, int>(res, pos));
            }
        }

        class BalancedOpenCloseCharactrers
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
                        throw new Exception("Input is in Wrong format");
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
                    pos = code.Length;
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

                    if(ch == ArrayEndChar)
                    {
                        var value = temp[0..^1];
                        if(value.Length > 0)
                            res.Add(value);
                        return i + 1;
                    }
                }
                return code.Length;
            }
        }
    }
}
