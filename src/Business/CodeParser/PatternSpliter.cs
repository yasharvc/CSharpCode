using CodeParser.Patterns;
using Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeParser
{
    public partial class PatternSpliter
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
    }
}
