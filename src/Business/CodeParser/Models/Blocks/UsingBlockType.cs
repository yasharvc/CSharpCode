namespace CodeParser.Models.Blocks
{
    public enum UsingBlockType
    {
        UseType,//using [lib];
        UseStaticType,//using static [lib];
        UseAliasType//using [var] = [lib];
    }
}