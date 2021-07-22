using System.Collections.Generic;

namespace CodeParser.Models.Blocks
{
    public class ConstraintWhereBlock
    {
        public TextWithPosition RawText { get; set; }
        public string GenericTypeName { get; set; }
        public List<string> Constraints { get; set; }
    }
}