using System;

namespace CodeParser.Models.Blocks
{
    public class UsingBlock
    {
        public UsingBlockType Type { get; set; }
        public TextWithPosition Text { get; set; }
        public string VariableName { get; set; }
        public string Library { get; set; }

        public override string ToString()
        {
            if (Text == null || string.IsNullOrEmpty(Text)) {
                switch (Type)
                {
                    case UsingBlockType.UseType:
                        return $"using {Library};";
                    case UsingBlockType.UseStaticType:
                        return $"using static {Library};";
                    case UsingBlockType.UseAliasType:
                        return $"using {VariableName} = {Library};";
                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                return Text;
            }
        }
    }
}