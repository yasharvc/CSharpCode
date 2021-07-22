using CodeParser.Models;
using System;
using System.Linq;

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
        public const string BalanceableCharacters = "{}()[]<>";
        public const string ParameterSeparator = ",";

        public static int SkipSpaces(string code, int startPos = 0, string whiteSpaces = WhiteSpaces, string extraChars = "")
        {
            whiteSpaces += extraChars;
            while (whiteSpaces.Contains(code[startPos]) && startPos < code.Length)
                startPos++;
            return startPos;
        }

        public TextWithPosition NextWordWithStopWords(string code, string nextWordStopChars = "", int startPos = 0, params string[] stopWords)
        {
            if (stopWords.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(stopWords));

            var pos = startPos;
            TextWithPosition word = new();
            do
            {
                pos = SkipSpaces(code, pos);
                var spaceAfter = word.WhiteSpaceAfter;
                word = NextWord(code, pos, stopChars: nextWordStopChars);
                if (stopWords.Contains(word.RawPhrase))
                    return new TextWithPosition {
                        Start = startPos,
                        RawPhrase = code[startPos..(pos - spaceAfter.Length)],
                        WhiteSpaceAfter = spaceAfter
                    };
                else if (stopWords.Contains(word.WhiteSpaceAfter.Trim()))
                {
                    var res = new TextWithPosition
                    {
                        Start = startPos,
                        RawPhrase = code[startPos..(word.End - word.WhiteSpaceAfter.Length)]
                    };
                    var pp = SkipSpaces(code, word.EndWithoutWhitespace);
                    res.WhiteSpaceAfter = code[word.EndWithoutWhitespace..pp];
                    return res;
                }
                pos = word.End;
            } while (!word.IsEmptyOrWhiteSpace);

            return new TextWithPosition
            {
                Start = startPos,
                RawPhrase = code[startPos..]
            };
        }

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
            var wasInBalancing = false;

            for (int i = 0; i < balanceCheckArray.Length; i++)
            {
                balanceCheckArray[i] = 0;
            }

            for (int i = startPos; i < code.Length; i++)
            {
                var ch = code[i];
                temp += ch;

                if (!isInsideSring && stopChars.Contains(ch) && (!wasInBalancing || AllBalanced(balanceCheckArray)))
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
                    !BalancedChars(ch, balanceCheckArray, ref wasInBalancing))
                    continue;

                if (wasInBalancing)
                {
                    wasInBalancing = false;
                }

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
        public TextWithPosition PreviousWord(string code, int startPos = 0, string whtiteSpaces = WhiteSpaces, string stopChars = "")
        {
            startPos = startPos <= 0 ? code.Length : startPos > code.Length ? code.Length : startPos;

            var res = new TextWithPosition
            {
            };

            var temp = "";
            var isInsideSring = false;
            var stringStartedWith = '\0';
            var balanceCheckArray = new int[BalanceableCharacters.Length];
            var whitespacesPassed = false;
            var wasInBalancing = false;

            for (int i = 0; i < balanceCheckArray.Length; i++)
            {
                balanceCheckArray[i] = 0;
            }

            for (int i = startPos - 1; i >= 0; i--)
            {
                var ch = code[i];
                temp += ch;

                if (!isInsideSring && stopChars.Contains(ch) && !wasInBalancing)
                {
                    res.RawPhrase = temp[..^1];
                    res.RawPhrase = new string(res.RawPhrase.ToCharArray().Reverse().ToArray());
                    res.Start = startPos - res.RawPhrase.Length;
                    //Remove first spaces and get spaces after code to Whitespace after \t\r\n[a]\r\n
                    res.WhiteSpaceAfter = "";
                    return res;
                }

                if (StringCheck(code, ref isInsideSring, ref stringStartedWith, i, ch) ||
                    !BalancedChars(ch, balanceCheckArray, ref wasInBalancing))
                    continue;

                if (wasInBalancing)
                {
                    temp = new string(temp[..^1].ToCharArray().Reverse().ToArray());
                    res.RawPhrase = temp.TrimEnd();
                    res.WhiteSpaceAfter = temp[res.RawPhrase.Length..];
                    res.Start = startPos - res.RawPhrase.Length - res.WhiteSpaceAfter.Length;
                    return res;
                }

                if (stopChars.Contains(ch) || (IsWhiteSpace(ch) && whitespacesPassed))
                {
                    res.RawPhrase = temp[..^1];
                    res.RawPhrase = new string(res.RawPhrase.ToCharArray().Reverse().ToArray());
                    res.Start = startPos - res.RawPhrase.Length - res.WhiteSpaceAfter.Length;
                    return res;
                }

                if (!IsWhiteSpace(ch) && !whitespacesPassed)
                {
                    res.WhiteSpaceAfter = temp[..^1];
                    temp = temp[1..];
                    whitespacesPassed = true;
                }
            }
            res.RawPhrase = new string(temp.ToCharArray().Reverse().ToArray());
            return res;
        }

        private bool IsWhiteSpace(char ch) => WhiteSpaces.Contains(ch);

        private bool BalancedChars(char ch, int[] balanceCheckArray,ref bool wasInBalancing)
        {
            var index = BalanceableCharacters.IndexOf(ch);
            if (index != -1)
            {
                balanceCheckArray[index]++;
                wasInBalancing = true;
            }
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
