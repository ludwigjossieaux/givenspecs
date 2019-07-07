using Newtonsoft.Json;
using System.Collections.Generic;

namespace GivenSpecs.Application.Reporting
{
    public class ReportedArgument_Table : ReportedArgument
    {
        [JsonProperty("rows")]
        public List<ReportedArgument_TableRow> Rows { get; set; }
        public ReportedArgument_Table()
        {
            this.Rows = new List<ReportedArgument_TableRow>();
        }
    }
}
