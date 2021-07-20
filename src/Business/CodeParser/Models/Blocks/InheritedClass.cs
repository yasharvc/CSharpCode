using System.Collections.Generic;

namespace CodeParser.Models.Blocks
{
    public class InheritedClass
    {
        public string ClassName { get; set; }
        public List<string> GenericTypes { get; set; }
    }
}