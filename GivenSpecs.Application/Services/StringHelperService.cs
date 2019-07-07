using GivenSpecs.Application.Interfaces;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GivenSpecs.Application.Services
{
    public class StringHelperService : IStringHelperService
    {
        private Task<string> _getMethodString(string input)
        {
            TextInfo ti = new CultureInfo("en-US", false).TextInfo;
            string rExp = @"[^\w\d]";
            string tmp = Regex.Replace(input, rExp, " ");
            tmp = ti.ToTitleCase(tmp);
            tmp = tmp.Replace(" ", "");
            return Task.FromResult(tmp);
        }

        public async Task<string> ToMethodString(string input)
        {
            return await _getMethodString(input);
        }

        public async Task<string> ToParamString(string input)
        {
            var tmp = await _getMethodString(input);
            return char.ToLower(tmp[0]) + tmp.Substring(1);
        }

        public Task<string> ToIdString(string input)
        {
            TextInfo ti = new CultureInfo("en-US", false).TextInfo;
            string rExp = @"[^\w\d]";
            string tmp = Regex.Replace(input, rExp, " ");
            tmp = tmp.Replace(" ", "-");
            tmp = tmp.Trim('-');
            return Task.FromResult(tmp.ToLower());
        }

        public async Task<string> GetEmbeddedFile(string filePath)
        {
            string file;
            var assembly = typeof(XunitGeneratorService).GetTypeInfo().Assembly;
            using (var stream = assembly.GetManifestResourceStream(filePath))
            {
                using (var reader = new StreamReader(stream))
                {
                    file = await reader.ReadToEndAsync();
                }
            }
            return file;
        }
    }
}
