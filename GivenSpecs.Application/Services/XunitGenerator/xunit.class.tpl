using Xunit;
using Xunit.Abstractions;
using System.Collections.Generic;

namespace {{ namespace }} {

{{ if generate_collection_fixture }}
[CollectionDefinition("GivenSpecsCollection")]
public class GivenSpecsCollectionFixture : ICollectionFixture<GivenSpecs.FixtureClass> { }
{{ end }}

[Collection("GivenSpecsCollection")]
public class {{ class }} {

private readonly GivenSpecs.StepResolver _steps;
private readonly ITestOutputHelper _output;
private readonly GivenSpecs.FixtureClass _fixture;
private readonly GivenSpecs.Application.Reporting.ReportedFeature _feature;

public {{ class }}(ITestOutputHelper output, GivenSpecs.FixtureClass fixture)
{
_fixture = fixture;
_output = output;
_steps = new GivenSpecs.StepResolver(System.Reflection.Assembly.GetExecutingAssembly(), _output);

// Report feature
_feature = new GivenSpecs.Application.Reporting.ReportedFeature()
{
Uri = @"{{ reported.uri }}",
Id = "{{ reported.id }}",
Keyword = "Feature",
Line = {{ reported.line }},
Name = "{{ reported.name }}"
};
_fixture.ReportFeature(_feature);
}

// Background
private void _Background()
{
{{ for step in background_steps }}
    {{ if step.has_multiline_text }}
    string txt{{ step.random }} = @"{{ step.multiline_text }}";
    {{ else }}
    string txt{{ step.random }} = null;
    {{ end }}
    {{ if step.table }}
        var table{{ step.random }} = new  GivenSpecs.Application.Tables.Table(new string[] {
            {{ for header in step.header_row.cells }}
                "{{ header.value }}",
            {{ end }}
        });
        {{ for table_row in step.data_rows }}
            table{{ step.random }}.AddRow(new string[] {
            {{ for cell in table_row.cells }}
                "{{ cell.value }}",
            {{ end }}
            });
        {{ end }}
    {{ else }}
         GivenSpecs.Application.Tables.Table table{{ step.random }} = null;
    {{ end }}

    var stepMessage{{ step.random }} = @"{{ step.text }}";
    _steps.BeforeStep(stepMessage{{ step.random }});
    _steps.{{ step.keyword }}(stepMessage{{ step.random }}, txt{{ step.random }}, table{{ step.random }});
    _steps.AfterStep(stepMessage{{ step.random }});
{{ end }}
}

// Scenarios
{{ for sc in scenarios }}
{{ if sc.examples.empty? }}
[Fact(DisplayName="{{ sc.display_name }}")]
{{ else }}
[Theory(DisplayName="{{ sc.display_name }}")]
{{ for ex in sc.examples }}
[InlineData({{ ex.data_string }})]
{{ end }}
{{ end }}
{{ for tag in sc.tags }}
[Trait("Category", "{{ tag }}")]
{{ end }}
public void {{ sc.method_name }}({{ sc.parameters_string}}) 
{

{{ if sc.examples.empty? }}
string givenSpecsIdx = "0";
{{ end }}

var reportedScenario = new GivenSpecs.Application.Reporting.ReportedScenario() {
Id = $"{{ sc.reported.id }}-{givenSpecsIdx}",
Keyword = "Scenario",
Line = {{ sc.reported.line }},
Name = $"{{ sc.reported.name }} [{givenSpecsIdx}]",
Type = "{{ sc.reported.type }}"
};
{{ for tag in sc.reported.tags }}
reportedScenario.Tags.Add(new GivenSpecs.Application.Reporting.ReportedTag() { Line = {{ tag.line }}, Name = "{{ tag.name }}" });
{{ end }}
_fixture.ReportScenario(_feature, reportedScenario);
var replacements = new List<(string, string)>() { {{ sc.parameters_map }} };
_steps.ScenarioReset(_fixture, _feature, replacements, reportedScenario);
_steps.BeforeScenario();
this._Background();
{{ for step in sc.steps }}

    {{ if step.has_multiline_text }}
    string txt{{ step.random }} = @"{{ step.multiline_text }}";
    {{ else }}
    string txt{{ step.random }} = null;
    {{ end }}

    {{ if step.table }}
        var table{{ step.random }} = new GivenSpecs.Application.Tables.Table(new string[] {
        {{ for header in step.header_row.cells }}
            "{{ header.value }}",
        {{ end }}
        });
        {{ for table_row in step.data_rows }}
            table{{ step.random }}.AddRow(new string[] {
                {{ for cell in table_row.cells }}
                    "{{ cell.value }}",
                {{ end }}
            });
        {{ end }}
    {{ else }}
        GivenSpecs.Application.Tables.Table table{{ step.random }} = null;
    {{ end }}

    var stepMessage{{ step.random }} = @"{{ step.text }}";
    _steps.BeforeStep(stepMessage{{ step.random }});
    _steps.{{ step.keyword }}(stepMessage{{ step.random }}, txt{{ step.random }}, table{{ step.random }});
    _steps.AfterStep(stepMessage{{ step.random }});
{{ end }}
_steps.AfterScenario();
}
{{ end }}

}

}
