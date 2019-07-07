using FastDeepCloner;
using Gherkin.Ast;
using GivenSpecs.Application.Interfaces;
using GivenSpecs.Application.Reporting;
using GivenSpecs.Application.Services.XunitGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Scriban;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GivenSpecs.Application.Services
{
    public class XunitGeneratorService : IXunitGeneratorService
    {
        private readonly IGivenSpecsAppConfiguration _config;
        private readonly IStringHelperService _stringService;

        public XunitGeneratorService(
            IGivenSpecsAppConfiguration config,
            IStringHelperService stringService
        )
        {
            _config = config;
            _stringService = stringService;
        }

        public Task<(List<ReportedTag>, List<string>)> ProcessTags(IEnumerable<Tag> tags)
        {
            var resultReportedTags = new List<ReportedTag>();
            var resultTags = new List<string>();
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    var reportedTag = new ReportedTag()
                    {
                        Line = tag.Location.Line,
                        Name = tag.Name
                    };
                    resultReportedTags.Add(reportedTag);
                    resultTags.Add(tag.Name.Replace("@", ""));
                }
            }
            return Task.FromResult((resultReportedTags, resultTags));
        }

        public Task<XunitGenerator_Step> ProcessStep(Step step)
        {
            var rand = new Random();
            var result = new XunitGenerator_Step()
            {
                Random = rand.Next(100000, 999999).ToString(),
                Keyword = step.Keyword.Trim(),
                Text = step.Text.Replace(@"""", @"""""")
            };
            if (step.Argument is DataTable dt)
            {
                result.Table = dt;
            }
            if (step.Argument is DocString ds)
            {
                result.HasMultilineText = true;
                result.MultilineText = ds.Content;
            }
            return Task.FromResult(result);
        }

        public async Task<List<XunitGenerator_Scenario>> ProcessScenario(Scenario scenario, string featureId, IXunitGeneratorService service)
        {
            var result = new List<XunitGenerator_Scenario>();

            // Single scenario

            var s = new XunitGenerator_Scenario();

            s.Reported = new ReportedScenario()
            {
                Keyword = "Scenario",
                Line = scenario.Location.Line,
                Type = "scenario"
            };

            if (scenario.Tags.Any())
            {
                var scenarioTags = await service.ProcessTags(scenario.Tags);
                s.Reported.Tags.AddRange(scenarioTags.Item1);
                s.Tags.AddRange(scenarioTags.Item2);
            }

            if (scenario.Steps.Any())
            {
                foreach (var step in scenario.Steps)
                {
                    s.Steps.Add(await service.ProcessStep(step));
                }
            }

            // Will the method returns multiple result because of an example table ?

            if (scenario.Examples == null || !scenario.Examples.Any())
            {
                var name = scenario.Name;
                s.Reported.Id = featureId + ";" + (await _stringService.ToIdString(name));
                s.Reported.Name = name;
                s.DisplayName = name;
                s.MethodName = await _stringService.ToMethodString(name);

                result.Add(s);
                return result;
            }

            foreach (var example in scenario.Examples)
            {
                var exScenario = DeepCloner.Clone(s);

                var name = $"{scenario.Name} - {example.Name}";
                exScenario.Reported.Id = featureId + ";" + (await _stringService.ToIdString(name));
                exScenario.Reported.Name = name;
                exScenario.DisplayName = name;
                exScenario.MethodName = await _stringService.ToMethodString(name);

                if (example.Tags.Any())
                {
                    var exTableTags = await service.ProcessTags(example.Tags);
                    exScenario.Reported.Tags.AddRange(exTableTags.Item1);
                    exScenario.Tags.AddRange(exTableTags.Item2);
                }

                if (example.TableHeader != null && example.TableHeader.Cells != null)
                {
                    foreach (var param in example.TableHeader.Cells)
                    {
                        var value = await _stringService.ToParamString(param.Value);
                        exScenario.Parameters.Add((param.Value, value));
                    }
                    if (exScenario.Parameters.Any())
                    {
                        exScenario.Parameters = exScenario.Parameters.Prepend(("givenSpecsIdx", "givenSpecsIdx")).ToList();
                    }
                }
                var givenSpecsIdx = 1;
                if (example.TableBody != null)
                {
                    foreach (var row in example.TableBody)
                    {
                        var dataExample = new XunitGenerator_ScenarioExample();
                        dataExample.Values.Add(givenSpecsIdx.ToString());
                        foreach (var cell in row.Cells)
                        {
                            dataExample.Values.Add(cell.Value);
                        }
                        exScenario.Examples.Add(dataExample);
                        givenSpecsIdx++;
                    }
                }

                result.Add(exScenario);
            }

            return result;
        }

        public async Task<XunitGenerator_Feature> ProcessFeature(Feature feature, string fileLocation, bool generateCollectionFixture, IXunitGeneratorService service)
        {
            var model = new XunitGenerator_Feature();

            // Whether this is the first feature generated, and it needs to add the [CollectionDefinition("GivenSpecsCollection")]
            model.GenerateCollectionFixture = generateCollectionFixture;

            model.Reported = new ReportedFeature()
            {
                Id = await _stringService.ToIdString(feature.Name),
                Uri = fileLocation,
                Keyword = "Feature",
                Line = feature.Location.Line,
                Name = feature.Name
            };

            model.Namespace = _config.FeatureNamespace;
            model.Class = await _stringService.ToMethodString(feature.Name);

            // Background
            if (feature.Children != null)
            {
                foreach (var child in feature.Children)
                {
                    if (child is Background background)
                    {
                        var bgSteps = new List<XunitGenerator_Step>();
                        foreach (var step in background.Steps)
                        {
                            bgSteps.Add(await service.ProcessStep(step));
                        }
                        model.BackgroundSteps = bgSteps;
                        break;
                    }
                }
            }

            // Scenarios
            var scenarios = new List<XunitGenerator_Scenario>();
            if (feature.Children != null)
            {
                foreach (var child in feature.Children)
                {
                    if (child is Scenario scenario)
                    {
                        var sResult = await service.ProcessScenario(scenario, await _stringService.ToIdString(feature.Name), service);
                        if (sResult.Any())
                        {
                            scenarios.AddRange(sResult);
                        }
                        continue;
                    }
                }
            }
            model.Scenarios = scenarios;
            return model;
        }

        public async Task<string> Generate(GherkinDocument doc, string fileLocation, bool generateCollectionFixture, IXunitGeneratorService service)
        {
            if (doc.Feature == null) return "";

            // Templates
            var filePath = $"GivenSpecs.Application.Services.XunitGenerator.xunit.class.tpl";
            var classTpl = await _stringService.GetEmbeddedFile(filePath);

            var sb = new StringBuilder();

            // Prepare data
            var model = await this.ProcessFeature(doc.Feature, fileLocation, generateCollectionFixture, service);

            var template = Template.Parse(classTpl);
            var result = template.Render(model);

            var tree = CSharpSyntaxTree.ParseText(result);
            var root = tree.GetRoot().NormalizeWhitespace();
            var ret = root.ToFullString();

            return ret;
        }
    }
}
