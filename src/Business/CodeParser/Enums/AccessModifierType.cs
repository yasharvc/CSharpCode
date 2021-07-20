namespace CodeParser.Enums
{
    //https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/accessibility-levels
    public enum AccessModifierType
    {
        Public,
        Internal,
        Protected,
        ProtectedInternal,
        Private,
        PrivateProtected,
        ClassDefault = Private,
        EnumDefault = Public,
        InterfaceDefault = Public,
        StructDefault = Private
    }
}