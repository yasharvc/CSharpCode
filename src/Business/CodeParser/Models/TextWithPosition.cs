namespace CodeParser.Models
{
    public class TextWithPosition
    {
        public int Start { get; set; }
        public string RawPhrase { get; set; } = "";
        public string WhiteSpaceAfter { get; set; } = "";
        public string CompiledPhrase { get; set; } = "";
        public int End => Start + RawPhrase.Length + WhiteSpaceAfter.Length;
        public int EndWithoutWhitespace => Start + RawPhrase.Length;
        public bool IsEmpty => string.IsNullOrEmpty(RawPhrase);
        public bool IsEmptyOrWhiteSpace => IsEmpty || string.IsNullOrWhiteSpace(RawPhrase);

        public static implicit operator string(TextWithPosition t) => t.RawPhrase;
    }
}