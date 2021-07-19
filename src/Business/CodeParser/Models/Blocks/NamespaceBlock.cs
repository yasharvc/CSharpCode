using System.Collections.Generic;

namespace CodeParser.Models.Blocks
{
    public class NamespaceBlock
    {
        public TextWithPosition Name { get; set; }
        public TextWithPosition Body { get; set; }
        public List<UsingBlock> Usings { get; set; }
        public List<ClassBlock> Classes { get; set; }
        public List<DelegateBlock> Delegates { get; set; }
        public List<StructBlock> Structs { get; set; }
        public List<InterfaceBlock> Interfaces { get; set; }
        public List<EnumBlock> Enums { get; set; }
        public List<NamespaceBlock> NestedNamespaces { get; set; }
    }
}