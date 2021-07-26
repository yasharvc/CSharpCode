using CodeParser.Enums;
using CodeParser.Models;
using CodeParser.Models.Blocks;
using CodeParser.Patterns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeParser.BlockParser
{
    //class dd<T> where T : class { }
    //class ddd<T> : Task where T: class { }
    public class ClassBlockParser : IBlockParser<ClassBlock>
    {
        readonly ParseWithWordSplit parseWithWordSplit = new();
        readonly BalancedOpenCloseCharactrers balancedOpenCloseCharactrers = new() { CloseChar = '}', OpenChar = '{' };
        public async Task<ParseResult<ClassBlock>> Parse(string code, int startPos = 0)
        {
            var classBlock = new ClassBlock
            {
                AccessModifier = AccessModifierType.ClassDefault,
                Attributes = new List<AttributeBlock>(),
                WhereClauses = new List<ConstraintWhereBlock>(),
                InheritedInterfaces = new List<InheritedInterface>()
            };
            var classPos = code.IndexOf("class");
            var finishPos = startPos;

            var classInCode = await balancedOpenCloseCharactrers.Compile(code[(classPos + "class".Length)..]);
            startPos = ExtractBeforeClass(code, startPos, classBlock, classPos);

            finishPos = ExtractAfterClass(code, classBlock, classPos + 5);
            await ParseClassBody(code, finishPos);

            return new ParseResult<ClassBlock>
            {
                Blocks = new List<ClassBlock> { classBlock },
                FinishPosition = finishPos,
                StartPosition = startPos
            };
        }

        private async Task ParseClassBody(string code, int startPos)
        {
            var accessModifiers = new List<string>
            {
                "public",
                "protected",
                "private",
                "internal"
            };
            //var body = (await balancedOpenCloseCharactrers.Compile(code, finishPos)).Item1.First();
            //var body = code[ParseWithWordSplit.SkipSpaces(code, startPos, extraChars: "{")..].Trim();
            var accessModifier = AccessModifierType.None;
            var isStatic = false;
            var isVirtual = false;
            var isAsync = false;
            var isOverride = false;
            var isAbstract = false;
            var isNew = false;
            var isSealed = false;

            var items = new List<string>();
            var parameters = new List<string>();

            var pos = ParseWithWordSplit.SkipSpaces(code, startPos);
            var text = parseWithWordSplit.NextWord(code, pos);

            var wordindexInDeclartion = 1;

            while (!text.IsEmptyOrWhiteSpace)
            {
                if (accessModifiers.Contains(text.RawPhrase))
                {
                    accessModifier = ExtractAccessModifier(text, accessModifier);
                    wordindexInDeclartion = 1;
                }
                else if (wordindexInDeclartion == 1)
                {
                    if (text.RawPhrase.Equals("static"))
                        isStatic = true;
                    else if (text.RawPhrase.Equals("virtual"))
                        isVirtual = true;
                    else if (text.RawPhrase.Equals("async"))
                        isAsync = true;
                    else if (text.RawPhrase.Equals("abstract"))
                        isAbstract = true;
                    else if (text.RawPhrase.Equals("override"))
                        isOverride = true;
                    else if (text.RawPhrase.Equals("new"))
                        isNew = true;
                    else if (text.RawPhrase.Equals("sealed"))
                        isSealed = true;
                    else
                    {
                        wordindexInDeclartion = 2;
                        continue;
                    }
                }
                else if(wordindexInDeclartion == 2)
                {
                    items.Add(text);
                    text = parseWithWordSplit.NextWord(code, ParseWithWordSplit.SkipSpaces(code, text.End), stopChars: "({=");
                    if (text.WhiteSpaceAfter == "(")
                    {
                        wordindexInDeclartion = 3;
                        text = parseWithWordSplit.NextWordWithStopWords(code, ",", text.End, ")",",");
                        continue;
                    }
                    else if(text.WhiteSpaceAfter == "{")
                    {
                        wordindexInDeclartion = 4;
                        continue;
                    }
                    else if(text.WhiteSpaceAfter == "=")
                    {
                        wordindexInDeclartion = 5;
                        continue;
                    }
                }
                else if(wordindexInDeclartion == 3)//Function of constructor
                {
                    while (!text.IsEmptyOrWhiteSpace && text.WhiteSpaceAfter != ")")
                    {
                        parameters.Add(text);

                        text = parseWithWordSplit.NextWordWithStopWords(code, ",)", ParseWithWordSplit.SkipSpaces(code, text.End, extraChars: ","), ",", ")");
                    }
                }
                else if(wordindexInDeclartion == 4)//Property
                {

                }
                else if (wordindexInDeclartion == 5)//Property with =>
                {

                }
                text = parseWithWordSplit.NextWordWithStopWords(code , "(", text.End , wordindexInDeclartion > 1 ? "(" : "");
            }
        }

        private int ExtractAfterClass(string code, ClassBlock classBlock, int starttPos)
        {
            var endPos = ParseWithWordSplit.SkipSpaces(code, starttPos);

            classBlock.InheritedClass = new InheritedClass { GenericTypes = new List<string>(), ClassName = "" };
            classBlock.InheritedInterfaces = new List<InheritedInterface>();

            classBlock.GenericTypes = new List<string>();

            var name = parseWithWordSplit.NextWord(code, endPos, stopChars: ">:{");
            classBlock.RawName = name;
            endPos = name.End;

            if (name.RawPhrase.Contains("<"))
            {
                GetNameWithGenericTypes(classBlock, name);
                var text = parseWithWordSplit.NextWord(code, endPos, stopChars: "{");
                endPos = text.End;
                while (!text.IsEmptyOrWhiteSpace)
                {
                    if (text.RawPhrase.StartsWith("where"))
                        endPos = ExtractWhereConstraint(code, classBlock, endPos, text.Start);
                    else if (text.RawPhrase.StartsWith(":"))
                        endPos = ExtractInheritedClasses(code, classBlock,endPos, text.Start);
                    else
                        return starttPos;
                    text = parseWithWordSplit.NextWord(code, ParseWithWordSplit.SkipSpaces(code, endPos), stopChars: "{");
                    if(!text.IsEmptyOrWhiteSpace)
                        endPos = text.End;
                }
            }
            else
            {
                classBlock.Name = name;
                var text = parseWithWordSplit.NextWord(code, endPos, stopChars: "{");
                endPos = text.End;
                if (text.RawPhrase.StartsWith(":"))
                    endPos = ExtractInheritedClasses(code, classBlock, endPos, text.Start);
            }

            return endPos;
        }

        private int ExtractInheritedClasses(string code, ClassBlock classBlock, int endPos, int start)
        {
            endPos = ParseWithWordSplit.SkipSpaces(code, endPos);
            var allInheritens = parseWithWordSplit.NextWordWithStopWords(code, "{", endPos, "where", "{", "{}");

            var text = parseWithWordSplit.NextWordWithStopWords(allInheritens, ",", 0, "{", "{}", "where", ",");

            while (!text.IsEmptyOrWhiteSpace)
            {
                if (text.RawPhrase.StartsWith("I") && char.IsUpper(text.RawPhrase[1]))
                    classBlock.InheritedInterfaces.Add(GetInterfaceInfo(text.RawPhrase));
                else
                    classBlock.InheritedClass = GetClassInfo(text.RawPhrase);
                if (text.End >= allInheritens.RawPhrase.Length)
                    break;
                text = parseWithWordSplit.NextWordWithStopWords(allInheritens, ",", ParseWithWordSplit.SkipSpaces(allInheritens, text.End, extraChars: ","), "{", "{}", "where", ",");
            }
            return allInheritens.End;
        }

        private InheritedInterface GetInterfaceInfo(string rawPhrase)
        {
            var pos = 0;
            var text = parseWithWordSplit.NextWordWithStopWords(rawPhrase, nextWordStopChars: "<{,", pos, ",", "{","<");
            pos = text.End;

            var interfaceInfo = new InheritedInterface
            {
                InterfaceName = text.RawPhrase,
                GenericTypes = new List<string>()
            };

            if (rawPhrase.Contains("<"))
            {
                var generics = parseWithWordSplit.NextWordWithStopWords(rawPhrase, ",", pos, ",",">").RawPhrase[1..^1];
                text = parseWithWordSplit.NextWordWithStopWords(generics, ",", 0, ",", ">");
                while (!text.IsEmptyOrWhiteSpace)
                {
                    interfaceInfo.GenericTypes.Add(text);
                    if (text.End >= generics.Length)
                        break;
                    text = parseWithWordSplit.NextWordWithStopWords(generics, ",", ParseWithWordSplit.SkipSpaces(generics, text.End, extraChars: ","), ",", ">");
                }
            }
            return interfaceInfo;
        }

        private InheritedClass GetClassInfo(string rawPhrase)
        {
            var res = GetInterfaceInfo(rawPhrase);
            return new InheritedClass
            {
                ClassName = res.InterfaceName,
                GenericTypes = res.GenericTypes
            };
        }

        private int ExtractWhereConstraint(string code, ClassBlock classBlock, int endPos, int startPos)
        {
            var whereConstraint = new ConstraintWhereBlock();
            endPos = ParseWithWordSplit.SkipSpaces(code, endPos);
            var genericName = parseWithWordSplit.NextWord(code, endPos, stopChars: ":");
            whereConstraint.GenericTypeName = genericName;
            endPos = ParseWithWordSplit.SkipSpaces(code, genericName.End, extraChars: ":");
            var allConstraint = parseWithWordSplit.NextWordWithStopWords(code, "{", endPos, "where", "{", "{}");
            endPos = allConstraint.End;

            whereConstraint.Constraints = GetAllConstraintOfWhere(allConstraint);
            whereConstraint.RawText = new TextWithPosition
            {
                RawPhrase = code[startPos..allConstraint.EndWithoutWhitespace],
                Start = startPos,
                WhiteSpaceAfter = allConstraint.WhiteSpaceAfter
            };
            //Direct where after class name

            classBlock.WhereClauses.Add(whereConstraint);
            return endPos;
        }

        private List<string> GetAllConstraintOfWhere(TextWithPosition allConstraint)
        {
            var res = new List<string>();
            var pos = 0;

            var word = parseWithWordSplit.NextWord(allConstraint, pos, stopChars: ",");
            while (!word.IsEmptyOrWhiteSpace && pos < allConstraint.RawPhrase.Length)
            {
                res.Add(word);
                pos = word.End;
                word = parseWithWordSplit.NextWord(allConstraint, pos, stopChars: ",");
            }

            return res;
        }

        private static void GetNameWithGenericTypes(ClassBlock classBlock, TextWithPosition name)
        {
            var temp = name.RawPhrase;
            classBlock.Name = temp[..temp.IndexOf("<")];
            temp = temp[temp.IndexOf("<")..].Replace(">", "").Replace("<", "");
            foreach (var item in temp.Split(','))
                classBlock.GenericTypes.Add(item.Trim());
        }

        private int ExtractBeforeClass(string code, int startPos, ClassBlock classBlock, int classPos)
        {
            var canContinue = true;

            var wordBefore = parseWithWordSplit.PreviousWord(code, classPos, stopChars: "(){};");
            canContinue = !wordBefore.IsEmpty;
            while (canContinue)
            {
                if (wordBefore.IsEmpty || wordBefore.RawPhrase.Trim() == "")
                    break;

                ExtractClassType(classBlock, wordBefore);
                classBlock.AccessModifier = ExtractAccessModifier(wordBefore, classBlock.AccessModifier);
                ExtractAttribute(classBlock, wordBefore);

                startPos = wordBefore.Start;
                wordBefore = parseWithWordSplit.PreviousWord(code, wordBefore.Start, stopChars: "(){};");
            }
            classBlock.Attributes.Reverse();
            return startPos;
        }

        private void ExtractAttribute(ClassBlock classBlock, TextWithPosition wordBefore)
        {
            if (wordBefore.RawPhrase.StartsWith('[') && wordBefore.RawPhrase.EndsWith(']'))
                classBlock.Attributes.Add(new AttributeBlock { RawText = wordBefore.RawPhrase });
        }

        private static AccessModifierType ExtractAccessModifier(TextWithPosition currentWord, AccessModifierType current = AccessModifierType.None)
        {
            if (currentWord.RawPhrase == "public")
                return AccessModifierType.Public;
            else if (currentWord.RawPhrase == "internal")
                return current == AccessModifierType.Protected ? AccessModifierType.ProtectedInternal : AccessModifierType.Internal;
            else if (currentWord.RawPhrase == "protected")
                return current == AccessModifierType.Internal ? AccessModifierType.ProtectedInternal : AccessModifierType.Protected;
            else if (currentWord.RawPhrase == "private")
                return current == AccessModifierType.Protected ? AccessModifierType.PrivateProtected : AccessModifierType.Private;
            return current;
        }

        private static void ExtractClassType(ClassBlock classBlock, TextWithPosition currentWord)
        {
            if (currentWord.RawPhrase == "partial")
                classBlock.IsPartial = true;
            else if (currentWord.RawPhrase == "abstract")
                classBlock.IsAbstract = true;
            else if (currentWord.RawPhrase == "sealed")
                classBlock.IsSealed = true;
            else if (currentWord.RawPhrase == "static")
                classBlock.IsStatic = true;
        }
    }
}
