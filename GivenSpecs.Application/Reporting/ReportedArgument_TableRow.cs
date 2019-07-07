using Newtonsoft.Json;
using System.Collections.Generic;

namespace GivenSpecs.Application.Reporting
{
    public class ReportedArgument_TableRow
    {
        [JsonProperty("cells")]
        public List<string> Cells { get; set; }
        public ReportedArgument_TableRow()
        {
            this.Cells = new List<string>();
        }
    }
}
