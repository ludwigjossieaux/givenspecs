using ConsoleTables;
using GivenSpecs.Application.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GivenSpecs.Application.Tables
{
    public class TableRow
    {
        List<string> _headers;
        public List<TableCell> Cells { get; set; }

        public string[] GetValuesAsArray()
        {
            var list = Cells.Select(x => x.Value);
            return list.ToArray();
        }

        public List<string> GetHeaders()
        {
            return _headers;
        }

        public string Get(int idx)
        {
            if(idx < 0 || idx > Cells.Count - 1)
            {
                return null;
            }
            return Cells[idx].Value;
        }

        public string Get(string header)
        {
            var pos = _headers.IndexOf(header);
            if (pos < 0)
            {
                return null;
            }
            return Cells[pos].Value;
        }

        public TableRow(List<string> headers)
        {
            // if headers are null -> exception
            if(
                headers == null
                || !headers.Any()
                || headers.Any(h => string.IsNullOrWhiteSpace(h))
                || headers.Any(h => h.Trim() != h))
            {
                throw new TableRowInvalidHeadersException();
            }

            this._headers = headers;
            Cells = new List<TableCell>();
        }
    }
}
