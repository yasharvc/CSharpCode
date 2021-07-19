using CodeParser.Models;

namespace CodeParser
{
    public class ParseWithWordSplit
    {
        public const string WhiteSpaces = " \t\r\n";
        public const string StringIndicators = "\"'";
        public const string ArrayStartIndicator = "[";
        public const string ArrayEndIndicator = "]";
        public const string BlockStartIndicator = "{";
        public const string BlockEndIndicator = "}";
        public const string EndOfCodeIndicator = ";";
        public const string BalanceableCharacters = "{}()[]";
        public const string ParameterSeparator = ",";

        public TextWithPosition NextWord(string code, int startPos = 0, string whtiteSpaces = WhiteSpaces, string stopChars = "")
        {
            startPos = startPos < 0 ? 0 : startPos > code.Length ? code.Length : startPos;

            var res = new TextWithPosition
            {
                Start = startPos
            };

            var temp = "";
            var isInsideSring = false;
            var stringStartedWith = '\0';
            var balanceCheckArray = new int[BalanceableCharacters.Length];

            for (int i = 0; i < balanceCheckArray.Length; i++)
            {
                balanceCheckArray[i] = 0;
            }

            for (int i = startPos; i < code.Length; i++)
            {
                var ch = code[i];
                temp += ch;

                if(!isInsideSring && stopChars.Contains(ch))
                {
                    res.RawPhrase = temp[..^1];
                    temp = $"{ch}";
                    for (int j = i + 1; j < code.Length; j++)
                    {
                        var c = code[j];
                        if (IsWhiteSpace(c))
                            temp += c;
                        else
                            break;
                    }
                    res.WhiteSpaceAfter = temp;
                    return res;
                }

                if (StringCheck(code, ref isInsideSring, ref stringStartedWith, i, ch) ||
                    !BalancedChars(ch, balanceCheckArray))
                    continue;

                if (IsWhiteSpace(ch) || stopChars.Contains(ch))
                {
                    res.RawPhrase = temp[..^1];
                    temp = $"{ch}";
                    for (int j = i + 1; j < code.Length; j++)
                    {
                        var c = code[j];
                        if (IsWhiteSpace(c))
                            temp += c;
                        else
                            break;
                    }
                    res.WhiteSpaceAfter = temp;
                    return res;
                }
            }
            res.RawPhrase = temp;
            return res;
        }

        private bool IsWhiteSpace(char ch) => WhiteSpaces.Contains(ch);

        private bool BalancedChars(char ch, int[] balanceCheckArray)
        {
            var index = BalanceableCharacters.IndexOf(ch);
            if(index != -1)
                balanceCheckArray[index]++;
            else if (AllBalanced(balanceCheckArray))
                return true;
            return false;
        }

        private static bool AllBalanced(int[] balanceCheckArray)
        {
            for (int i = 0; i < balanceCheckArray.Length / 2; i++)
            {
                var openCount = balanceCheckArray[i * 2];
                var closeCount = balanceCheckArray[i * 2 + 1];
                if (openCount - closeCount != 0)
                    return false;
            }
            return true;
        }

        private static bool StringCheck(string code, ref bool isInsideSring, ref char stringStartedWith, int i, char ch)
        {
            if (IsGoingIntoString(isInsideSring, ch, i > 0 ? code[i - 1] : null))
            {
                isInsideSring = true;
                stringStartedWith = ch;
                return true;
            }


            if (isInsideSring && ch == stringStartedWith && (i == 0 || code[i - 1] != '\''))
            {
                isInsideSring = false;
                return false;
            }

            return isInsideSring;
        }

        private static bool IsGoingIntoString(bool isInsideSring, char ch, char? charBefore) 
            => !isInsideSring && StringIndicators.Contains(ch) && (!charBefore.HasValue || charBefore != '\'');
    }
}
