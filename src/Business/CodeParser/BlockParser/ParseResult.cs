using System.Collections.Generic;

namespace CodeParser.BlockParser
{
    public class ParseResult<T>
    {
        public int SartPosition { get; set; }
        public int FinishPosition { get; set; }
        public IEnumerable<T> Blocks { get; set; }
    }
}