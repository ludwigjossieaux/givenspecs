using ConsoleTables;
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

        public string Get(int idx)
        {
            return Cells[idx].Value;
        }

        public string Get(string header)
        {
            var pos = _headers.IndexOf(header);
            return Cells[pos].Value;
        }

        public TableRow(List<string> headers)
        {
            this._headers = headers;
            Cells = new List<TableCell>();
        }
    }

    public class TableCell
    {
        public string Value { get; set; }
    }

    public class Table
    {
        List<string> _headers;
        Dictionary<string, string> _entries;
        int rowCount = 0;

        public Table(string[] headers)
        {
            _headers = new List<string>(headers);
            _entries = new Dictionary<string, string>();
        }

        public void AddRow(string[] values)
        {
            foreach (var idx in Enumerable.Range(0, values.Length))
            {
                _entries.Add($"{rowCount}--{_headers[idx]}", values[idx]);
            }
            rowCount++;
        }

        public List<string> GetHeaders()
        {
            return _headers;
        }

        public IEnumerable<TableRow> GetRows()
        {
            foreach (var idx in Enumerable.Range(0, rowCount))
            {
                var values = _entries.Where(x => x.Key.StartsWith($"{idx}--"));
                var row = new TableRow(_headers);
                foreach (var h in _headers)
                {
                    var cellValue = values.FirstOrDefault(x => x.Key.EndsWith($"--{h}")).Value;
                    row.Cells.Add(new TableCell() { Value = cellValue });
                }
                yield return row;
            }
        }

        public List<TableRow> Rows
        {
            get
            {
                return this.GetRows().ToList();
            }
        }

        public void ApplyReplacements(Func<string, string> applyReplacements)
        {
            var newEntries = new Dictionary<string, string>();
            foreach (var kv in _entries)
            {
                newEntries[kv.Key] = applyReplacements(kv.Value);
            }
            _entries = newEntries;
        }

        public override string ToString()
        {
            var table = new ConsoleTable(_headers.ToArray());
            foreach (var r in this.GetRows())
            {
                table.AddRow(r.GetValuesAsArray());
            }
            return table.ToString();
        }
    }


}
