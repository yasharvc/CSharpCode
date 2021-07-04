using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public string NextWord(string code, string whtiteSpaces = WhiteSpaces)
        {
            var temp = "";
            var isInsideSring = false;
            var stringStartedWith = '\0';
            var balanceCheckArray = new int[BalanceableCharacters.Length];

            for (int i = 0; i < balanceCheckArray.Length; i++)
            {
                balanceCheckArray[i] = 0;
            }

            for (int i = 0; i < code.Length; i++)
            {
                var ch = code[i];
                temp += ch;

                if (StringCheck(code, ref isInsideSring, ref stringStartedWith, i, ch) ||
                    !BalancedChars(ch, balanceCheckArray))
                    continue;

                if (IsWhiteSpace(ch))
                    return temp[..^1];
            }
            return temp;
        }

        private bool IsWhiteSpace(char ch) => WhiteSpaces.Contains(ch);

        private bool BalancedChars(char ch, int[] balanceCheckArray)
        {
            var index = BalanceableCharacters.IndexOf(ch);
            if (index == -1 && AllBalanced(balanceCheckArray))
                return true;
            balanceCheckArray[index]++;
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

            return true;
        }

        private static bool IsGoingIntoString(bool isInsideSring, char ch, char? charBefore) 
            => !isInsideSring && StringIndicators.Contains(ch) && (!charBefore.HasValue || charBefore != '\'');
    }
}
