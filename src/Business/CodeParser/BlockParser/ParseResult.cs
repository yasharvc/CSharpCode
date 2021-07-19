using System.Collections.Generic;

namespace CodeParser.BlockParser
{
    public class ParseResult<T>
    {
        public int SartPosition { get; set; }
        public int FinishPosition { get; set; }
        public IEnumerable<T> Blocks { get; set; }

        public static ParseResult<T> Empty(int position) => new()
        {
            Blocks = new List<T>(),
            FinishPosition = position,
            SartPosition = position
        };
    }
}