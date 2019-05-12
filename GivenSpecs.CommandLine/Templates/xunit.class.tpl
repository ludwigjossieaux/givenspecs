using Xunit;
using Xunit.Abstractions;
using GivenSpecs;
using System.Collections.Generic;

namespace {{ namespace }} {

{{ if generate_collection_fixture }}
[CollectionDefinition("GivenSpecsCollection")]
public class GivenSpecsCollectionFixture : ICollectionFixture<FixtureClass> { }
{{ end }}

[Collection("GivenSpecsCollection")]
public class {{ class }} {

private readonly GivenSpecs.StepResolver _steps;
private readonly ITestOutputHelper _output;
private readonly GivenSpecs.FixtureClass _fixture;
private readonly ReportedFeature _feature;

public {{ class }}(ITestOutputHelper output, GivenSpecs.FixtureClass fixture)
{
_fixture = fixture;
_output = output;
_steps = new GivenSpecs.StepResolver(System.Reflection.Assembly.GetExecutingAssembly(), _output);

// Report feature
_feature = new ReportedFeature()
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

{{ if step.table != null }}
var table{{ step.random }} = new GivenSpecs.Table(new string[] {
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
GivenSpecs.Table table{{ step.random }} = null;
{{ end }}
_steps.{{ step.keyword }}(@"{{ step.text }}", txt{{ step.random }}, table{{ step.random }});
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
string givenSpecsIdx = "";
{{ end }}

var reportedScenario = new ReportedScenario() {
Id = $"{{ sc.reported.id }}-{givenSpecsIdx}",
Keyword = "Scenario",
Line = {{ sc.reported.line }},
Name = $"{{ sc.reported.name }} [{givenSpecsIdx}]",
Type = "{{ sc.reported.type }}"
};
{{ for tag in sc.reported.tags }}
reportedScenario.Tags.Add(new ReportedTag() { Line = {{ tag.line }}, Name = "{{ tag.name }}" });
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

{{ if step.table != null }}
var table{{ step.random }} = new GivenSpecs.Table(new string[] {
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
GivenSpecs.Table table{{ step.random }} = null;
{{ end }}

_steps.{{ step.keyword }}(@"{{ step.text }}", txt{{ step.random }}, table{{ step.random }});
{{ end }}
_steps.AfterScenario();
}
{{ end }}

}

}
