using GivenSpecs.Application.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GivenSpecs.Helpers
{
    public static class TableHelpers
    {
        private static void AssignValue<T>(
            T obj, 
            List<(string property, Action<T, string> action)> transforms,
            string h,
            string v
        )
        {
            var objClass = typeof(T);
            if (transforms != null)
            {
                var transform = transforms.FirstOrDefault(x => x.property == h);
                if (transform != default)
                {
                    transform.action(obj, v);
                    return;
                }
            }
            objClass.GetProperty(h).SetValue(obj, v);
        }

        private static (bool Result, string Actual) CompareValue<T>(
            T obj,
            List<(string property, Func<T, string, (bool, string)> func)> comparators,
            string h,
            string expected
        )
        {
            var objClass = typeof(T);
            if (comparators != null)
            {
                var transform = comparators.FirstOrDefault(x => x.property == h);
                if (transform != default)
                {
                    return transform.func(obj, expected);
                }
            }
            var actual = (string) objClass.GetProperty(h).GetValue(obj);
            return (actual == expected, actual);
        }

        private static T GetObjectFromRowData<T>(List<string> headers, TableRow row, List<(string property, Action<T, string> action)> transforms) where T : new()
        {
            var obj = new T();
            foreach (var h in headers)
            {
                var v = row.Get(h);
                AssignValue<T>(obj, transforms, h, v);
            }
            return obj;
        }

        private static T GetObjectFromColumnData<T>(Table table, List<(string property, Action<T, string> action)> transforms) where T : new()
        {
            var obj = new T();
            foreach (var row in table.GetRows())
            {
                var h = row.Get(0);
                var v = row.Get(1);
                AssignValue<T>(obj, transforms, h, v);
            }
            return obj;
        }

        public static T CreateInstance<T>(this Table table, List<(string property, Action<T, string> action)> transforms = null, bool columnData = false) where T: new()
        {
            if(!table.GetRows().Any())
            {
                return default;
            }
            if(columnData)
            {
                return GetObjectFromColumnData<T>(table, transforms);
            }
            var row = table.GetRows().First();
            return GetObjectFromRowData<T>(row.GetHeaders(), row, transforms);
        }

        public static IList<T> CreateSet<T>(this Table table, List<(string property, Action<T, string> action)> transforms = null) where T: new()
        {
            var list = new List<T>();
            foreach(var row in table.GetRows())
            {
                var obj = GetObjectFromRowData<T>(row.GetHeaders(), row, transforms);
                list.Add(obj);
            }
            return list;
        }

        public static (bool Result, string Message) CompareToInstance<T>(
            this Table table, 
            T instance, 
            List<(string property, Func<T, string, (bool, string)> action)> comparators = null, 
            bool columnData = false) where T: new()
        {
            if (!table.GetRows().Any())
            {
                return (false, "Empty table");
            }

            var result = true;
            var message = string.Empty;

            if (columnData)
            {
                foreach(var row in table.GetRows())
                {
                    var h = row.Get(0);
                    var expected = row.Get(1);
                    var hRes = CompareValue<T>(instance, comparators, h, expected);
                    if (!hRes.Result)
                    {
                        result = false;
                        message = $"Property: {h}, Is: {hRes.Actual}, Expected: {expected}";
                        break;
                    }
                }
            }
            else
            {
                var row = table.GetRows().First();
                foreach (var h in table.GetHeaders())
                {
                    var expected = row.Get(h);
                    var hRes = CompareValue<T>(instance, comparators, h, expected);
                    if(!hRes.Result)
                    {
                        result = false;
                        message = $"Property: {h}, Is: {hRes.Actual}, Expected: {expected}";
                        break;
                    }
                } 
            }

            return (result, message);
        }

        public static (bool Result, string Message) CompareToSet<T>(this Table table, IEnumerable<T> set, List<(string property, Func<T, string, (bool, string)> action)> comparators = null) where T : new()
        {
            var result = true;
            var message = string.Empty;

            if(!table.GetRows().Any())
            {
                return (false, "Empty table");
            }

            if(table.GetRows().Count() != set.Count())
            {
                return (false, "Number of items mismatch");
            }

            var idx = 0;
            foreach(var row in table.GetRows())
            {
                var inst = set.ElementAt(idx);
                var testTable = new Table(table.GetHeaders().ToArray());
                testTable.AddRow(row.GetValuesAsArray());
                var res = testTable.CompareToInstance(inst, comparators, false);
                if(!res.Result)
                {
                    result = false;
                    message = $"Index: {idx}, {res.Message}";
                    break;
                }
                idx++;
            }

            return (result, message);
        }
    }
}
