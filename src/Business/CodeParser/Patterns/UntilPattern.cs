using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeParser.Patterns
{
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

        private Task<Tuple<List<string>, int>> UntilLiteral(string code, string endPhrase)
        {
            var res = new List<string>();
            endPhrase = endPhrase.Replace("`", "");
            var pos = code.IndexOf(endPhrase);
            if (pos > 0)
                res.Add(code.Substring(0, pos));
            else
            {
                throw new FormatException();
                //res.Add(code);
                //pos = code.Length;
            }
            return Task.FromResult(new Tuple<List<string>, int>(res, pos));
        }
    }
}
