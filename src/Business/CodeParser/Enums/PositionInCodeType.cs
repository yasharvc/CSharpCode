namespace CodeParser.Enums
{
    public enum PositionInCodeType
    {
        OutMost,
        InsideNamespace,
        InsideClass,
        InsideFunction,
        InsidePropertyGet,
        InsidePropertySet,
        InsideConstructor,
        InsideRegion,
    }

    public enum SubPositionInCodeType
    {
        For,
        ForEach,
        If,
        ElseIf,
        Else,
        Switch,
        Region,
        Block,
        Linq,
        LambdaExpression,
        NewingParameters,//new abc()
        NewingParameterSetPrans, //new fx{}
    }

    public enum CodeBlockType
    {
        UsingNamespace,
        UsingStatic,
        UsingGlobalAlias,
        Namespace
    }
}