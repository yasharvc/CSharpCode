using System.Threading.Tasks;

namespace CodeParser.BlockParser
{
    public interface IBlockParser<T>
    {
        Task<ParseResult<T>> Parse(string code, int startPos = 0);
    }
}