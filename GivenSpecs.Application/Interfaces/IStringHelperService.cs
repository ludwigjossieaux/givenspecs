using System.Threading.Tasks;

namespace GivenSpecs.Application.Interfaces
{
    public interface IStringHelperService
    {
        Task<string> ToMethodString(string input);
        Task<string> ToParamString(string input);
        Task<string> ToIdString(string input);
        Task<string> GetEmbeddedFile(string filePath);
    }
}
