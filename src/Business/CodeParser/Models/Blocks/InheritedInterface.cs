using System.Collections.Generic;

namespace CodeParser.Models.Blocks
{
    public class InheritedInterface
    {
        public string InterfaceName { get; set; }
        public List<string> GenericTypes { get; set; }
    }
}