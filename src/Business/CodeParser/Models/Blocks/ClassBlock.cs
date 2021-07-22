using CodeParser.Enums;
using System.Collections.Generic;

namespace CodeParser.Models.Blocks
{
    public class ClassBlock
    {
        public List<AttributeBlock> Attributes { get; set; }
        public AccessModifierType AccessModifier { get; set; }
        public bool IsStatic { get; set; }
        public bool IsAbstract { get; set; }
        public bool IsSealed { get; set; }
        public bool IsPartial { get; set; }
        public string Name { get; set; }
        public string RawName { get; set; }
        public List<ConstraintWhereBlock> WhereClauses { get; set; }
        public InheritedClass InheritedClass { get; set; }
        public List<InheritedInterface> InheritedInterfaces { get; set; }
        public List<string> GenericTypes { get; set; } //class FX<T,R>...
    }
}