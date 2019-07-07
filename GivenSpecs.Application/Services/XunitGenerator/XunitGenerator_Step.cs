using Gherkin.Ast;
using System.Collections.Generic;
using System.Linq;

namespace GivenSpecs.Application.Services.XunitGenerator
{
    public class XunitGenerator_Step
    {
        public string Random { get; set; }
        public string Keyword { get; set; }
        public string Text { get; set; }
        public DataTable Table { get; set; }
        public bool HasMultilineText { get; set; }
        public string MultilineText { get; set; }
        public Gherkin.Ast.TableRow HeaderRow
        {
            get
            {
                return Table.Rows.ToList().First();
            }
        }
        public IEnumerable<Gherkin.Ast.TableRow> DataRows
        {
            get
            {
                return Table.Rows.ToList().Skip(1).ToList();
            }
        }
    }
}
