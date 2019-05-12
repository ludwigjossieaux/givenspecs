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
using System.Threading.Tasks;
using Scriban;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

namespace GivenSpecs.CommandLine.Generate
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

    public class XunitGenerator_Scenario
    {
        public string DisplayName { get; set; }
        public List<string> Tags { get; set; }
        public string MethodName { get; set; }
        public List<XunitGenerator_Step> Steps { get; set; }
        public List<(string, string)> Parameters { get; set; }
        public List<XunitGenerator_ScenarioExample> Examples { get; set; }
        public ReportedScenario Reported { get; set; }
        public string ParametersString
        {
            get
            {
                return string.Join(", ", Parameters.Select(x => $"string {x.Item2}"));
            }
        }
        public string ParametersMap
        {
            get
            {
                return string.Join(", ", Parameters.Select(x => $"(@\"{x.Item1}\", {x.Item2})"));
            }
        }

        public XunitGenerator_Scenario()
        {
            this.Tags = new List<string>();
            this.Steps = new List<XunitGenerator_Step>();
            this.Parameters = new List<(string, string)>();
            this.Examples = new List<XunitGenerator_ScenarioExample>();
        }
    }

    public class XunitGenerator_Feature
    {
        public string Namespace { get; set;}
        public string Class { get; set; }
        public List<XunitGenerator_Step> BackgroundSteps { get; set; }
        public List<XunitGenerator_Scenario> Scenarios { get; set; }
        public bool GenerateCollectionFixture { get; set; }

        public ReportedFeature Reported { get; set; }

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

        private string ToMethodString(string input)
        {
            TextInfo ti = new CultureInfo("en-US", false).TextInfo;
            string rExp = @"[^\w\d]";
            string tmp = Regex.Replace(input, rExp, " ");
            tmp = ti.ToTitleCase(tmp);
            tmp = tmp.Replace(" ", "");
            return tmp;
        }

        private string ToParamString(string input)
        {
            var tmp = ToMethodString(input);
            return char.ToLower(tmp[0]) + tmp.Substring(1);
        }

        private string GetId(string input)
        {
            TextInfo ti = new CultureInfo("en-US", false).TextInfo;
            string rExp = @"[^\w\d]";
            string tmp = Regex.Replace(input, rExp, " ");
            tmp = tmp.Replace(" ", "-");
            return tmp.ToLower();
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
        
        private void ProcessTags(IEnumerable<Tag> tags, XunitGenerator_Scenario s)
        {
            foreach (var tag in tags)
            {
                var reportedTag = new ReportedTag()
                {
                    Line = tag.Location.Line,
                    Name = tag.Name
                };
                s.Reported.Tags.Add(reportedTag);
                s.Tags.Add(tag.Name.Replace("@", ""));
            }
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
            if (step.Argument is DocString ds)
            {
                result.HasMultilineText = true;
                result.MultilineText = ds.Content;
            }
            return result;
        }

        private XunitGenerator_Scenario GenerateScenario(Scenario scenario, string featureId, Examples exampleTable)
        {
            var s = new XunitGenerator_Scenario();

            var name = scenario.Name + ((exampleTable != null && !string.IsNullOrWhiteSpace(exampleTable.Name)) ? $" - {exampleTable.Name}" : "");
            var id = featureId + ";" + GetId(name);

            s.Reported = new ReportedScenario()
            {
                Id = id,
                Keyword = "Scenario",
                Line = scenario.Location.Line,
                Name = name,
                Type = "scenario"
            };

            s.DisplayName = name;
            s.MethodName = ToMethodString(name);

            this.ProcessTags(scenario.Tags, s);

            if(exampleTable != null)
            {
                this.ProcessTags(exampleTable.Tags, s);
                foreach (var param in exampleTable.TableHeader.Cells)
                {
                    var value = ToParamString(param.Value);
                    s.Parameters.Add((param.Value, value));
                }
                if(s.Parameters.Any())
                {
                    s.Parameters = s.Parameters.Prepend(("givenSpecsIdx", "givenSpecsIdx")).ToList();
                }
                var givenSpecsIdx = 1;
                foreach(var row in exampleTable.TableBody)
                {
                    var dataExample = new XunitGenerator_ScenarioExample();
                    dataExample.Values.Add(givenSpecsIdx.ToString());
                    foreach(var cell in row.Cells)
                    {
                        dataExample.Values.Add(cell.Value);
                    }
                    s.Examples.Add(dataExample);
                    givenSpecsIdx++;
                }
            }

            foreach (var step in scenario.Steps)
            {
                s.Steps.Add(this.ProcessStep(step));
            }

            return s;
        }

        public string Generate(GherkinDocument doc, string fileLocation, bool generateCollectionFixture = false)
        {
            if (doc.Feature == null) return "";
            
            // Templates
            var classTpl = GetEmbeddedFile("xunit.class.tpl");

            var sb = new StringBuilder();

            // Prepare data
            var model = new XunitGenerator_Feature();
            model.GenerateCollectionFixture = generateCollectionFixture;

            model.Reported = new ReportedFeature()
            {
                Id = GetId(doc.Feature.Name),
                Uri = fileLocation,
                Keyword = "Feature",
                Line = doc.Feature.Location.Line,
                Name = doc.Feature.Name
            };

            model.Namespace = _opts.Namespace;
            model.Class = ToMethodString(doc.Feature.Name);

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
                    if (null != scenario.Examples && scenario.Examples.Count() > 0)
                    {
                        foreach (var exampleTable in scenario.Examples)
                        {
                            var s = GenerateScenario(scenario, GetId(doc.Feature.Name), exampleTable);
                            scenarios.Add(s);
                        }
                    }
                    else
                    {
                        var s = GenerateScenario(scenario, GetId(doc.Feature.Name), null);
                        scenarios.Add(s);
                    }
                    continue;
                }
            }
            model.Scenarios = scenarios;

            var template = Template.Parse(classTpl);
            var result = template.Render(model);

            var tree = CSharpSyntaxTree.ParseText(result);
            var root = tree.GetRoot().NormalizeWhitespace();
            var ret = root.ToFullString();

            return ret;
        }
    }
}
