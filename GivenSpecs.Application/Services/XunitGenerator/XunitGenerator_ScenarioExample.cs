using System.Collections.Generic;
using System.Linq;

namespace GivenSpecs.Application.Services.XunitGenerator
{
    public class XunitGenerator_ScenarioExample
    {
        public List<string> Values { get; set; }
        public string DataString
        {
            get
            {
                return string.Join(", ", Values.Select(x => $"@\"{x}\""));
            }
        }
        public XunitGenerator_ScenarioExample()
        {
            this.Values = new List<string>();
        }
    }
}
