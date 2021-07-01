using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeParser.Patterns
{
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
                throw new FormatException();
                //res.Add(code);
                //pos = code.Length;
            }
            return Task.FromResult(new Tuple<List<string>, int>(res, pos));
        }
    }
}
