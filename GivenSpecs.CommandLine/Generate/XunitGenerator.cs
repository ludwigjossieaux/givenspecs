using Gherkin.Ast;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Dynamic;
using System.Reflection;
using System.IO;
using System.Linq;
using RazorLight;
using System.Threading.Tasks;

namespace GivenSpecs.CommandLine.Generate
{
    public class XunitGenerator_Step
    {
        public string Random { get; set; }
        public string Keyword { get; set; }
        public string Text { get; set; }
        public DataTable Table { get; set; }
        public TableRow HeaderRow
        {
            get
            {
                return Table.Rows.ToList().First();
            }
        }
        public IEnumerable<TableRow> DataRows
        {
            get
            {
                return Table.Rows.ToList().Skip(1).ToList();
            }
        }
    }

    public class XunitGenerator_Scenario
    {
        public string DisplayName { get; set; }
        public List<string> Tags { get; set; }
        public string MethodName { get; set; }
        public List<XunitGenerator_Step> Steps { get; set; }

        public XunitGenerator_Scenario()
        {
            this.Tags = new List<string>();
            this.Steps = new List<XunitGenerator_Step>();
        }
    }

    public class XunitGenerator_Feature
    {
        public string Namespace { get; set;}
        public string Class { get; set; }
        public List<XunitGenerator_Step> BackgroundSteps { get; set; }
        public List<XunitGenerator_Scenario> Scenarios { get; set; }

        public XunitGenerator_Feature()
        {
            this.BackgroundSteps = new List<XunitGenerator_Step>();
            this.Scenarios = new List<XunitGenerator_Scenario>();
        }
    }

    public class XunitGenerator
    {
        private readonly GenerateOptions _opts;

        public XunitGenerator(GenerateOptions opts)
        {
            _opts = opts;
        }

        private string CleanUpString(string input)
        {
            TextInfo ti = new CultureInfo("en-US", false).TextInfo;
            string rExp = @"[^\w\d]";
            string tmp = Regex.Replace(input, rExp, " ");
            tmp = ti.ToTitleCase(tmp);
            tmp = tmp.Replace(" ", "");
            return tmp;
        }

        private string GetEmbeddedFile(string fileName)
        {
            string file;
            var assembly = typeof(XunitGenerator).GetTypeInfo().Assembly;
            using (var stream = assembly.GetManifestResourceStream($"GivenSpecs.CommandLine.Templates.{fileName}"))
            {
                using (var reader = new StreamReader(stream))
                {
                    file = reader.ReadToEnd();
                }
            }
            return file;
        }
        
        private XunitGenerator_Step ProcessStep(Step step)
        {
            var rand = new Random();
            var result = new XunitGenerator_Step()
            {
                Random = rand.Next(100000, 999999).ToString(),
                Keyword = step.Keyword.Trim(),
                Text = step.Text.Replace(@"""", @"""""")
            };
            if(step.Argument is DataTable dt)
            {
               // result.hasTable = true;
                result.Table = dt;
            }
            return result;
        }

        public string Generate(GherkinDocument doc)
        {
            if (doc.Feature == null) return "";

            // Templates
            var classRazorTpl = GetEmbeddedFile("xunit.class.cstpl");

            var sb = new StringBuilder();

            // Prepare data
            var model = new XunitGenerator_Feature();
            model.Namespace = _opts.Namespace;
            model.Class = CleanUpString(doc.Feature.Name);

            // Background
            foreach (var child in doc.Feature.Children)
            {
                if (child is Background background)
                {
                    var bgSteps = new List<XunitGenerator_Step>();
                    foreach (var step in background.Steps)
                    {
                        bgSteps.Add(this.ProcessStep(step));
                    }
                    model.BackgroundSteps = bgSteps;
                    break;
                }
            }

            // Scenarios
            var scenarios = new List<XunitGenerator_Scenario>();
            foreach (var child in doc.Feature.Children)
            {
                if (child is Scenario scenario)
                {
                    var s = new XunitGenerator_Scenario();
                    s.DisplayName = scenario.Name;
                    s.MethodName = CleanUpString(scenario.Name);

                    foreach (var tag in scenario.Tags)
                    {
                        s.Tags.Add(tag.Name.Replace("@", ""));
                    }

                    foreach(var step in scenario.Steps)
                    {
                        s.Steps.Add(this.ProcessStep(step));
                    }
                
                    scenarios.Add(s);
                    continue;
                }
            }
            model.Scenarios = scenarios;

            var engine = new RazorLightEngineBuilder()
              .UseMemoryCachingProvider()
              .Build();
            var result = (engine.CompileRenderAsync("xunitclass", classRazorTpl, model)).Result;

            return result;
        }
    }
}
