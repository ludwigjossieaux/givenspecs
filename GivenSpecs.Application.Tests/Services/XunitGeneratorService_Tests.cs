using FakeItEasy;
using FluentAssertions;
using Gherkin.Ast;
using GivenSpecs.Application.Interfaces;
using GivenSpecs.Application.Reporting;
using GivenSpecs.Application.Services;
using GivenSpecs.Application.Services.XunitGenerator;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace GivenSpecs.Application.Tests.Services
{
    public class XunitGeneratorService_Tests
    {
        private readonly IGivenSpecsAppConfiguration _config;
        private readonly IStringHelperService _string;
        private readonly XunitGeneratorService _service;

        public XunitGeneratorService_Tests()
        {
            _config = A.Fake<IGivenSpecsAppConfiguration>();
            _string = A.Fake<IStringHelperService>();
            _service = new XunitGeneratorService(_config, _string);
        }

        #region ProcessTags

        [Fact]
        public async Task ProcessTags_When_ItRuns_ItReturnsProcessedData()
        {
            // Arrange
            var tags = new List<Tag>()
            {
                new Tag(new Location(100, 1), "tag1"),
                new Tag(new Location(200, 2), "tag2"),
            };

            // Act
            var result = await _service.ProcessTags(tags);

            // Assert
            var reportedTags = result.Item1;
            var resultTags = result.Item2;
            reportedTags.Should().BeOfType<List<ReportedTag>>();
            resultTags.Should().BeOfType<List<string>>();

            reportedTags[0].Line.Should().Be(100);
            reportedTags[0].Name.Should().Be("tag1");
            reportedTags[1].Line.Should().Be(200);
            reportedTags[1].Name.Should().Be("tag2");

            resultTags[0].Should().Be("tag1");
            resultTags[1].Should().Be("tag2");
        }

        [Fact]
        public async Task ProcessTags_When_ItRuns_ItRemovesTagAtCharacter()
        {
            // Arrange
            var tags = new List<Tag>()
            {
                new Tag(new Location(100, 1), "@tag1"),
            };

            // Act
            var result = await _service.ProcessTags(tags);

            // Assert
            var resultTags = result.Item2;
            resultTags[0].Should().Be("tag1");
        }

        #endregion

        #region ProcessStep

        [Fact]
        public async Task ProcessStep_When_ItRuns_ItReturnsProcessedData()
        {
            // Arrange
            var step = new Step(
                    new Location(100, 1),
                    "when",
                    "this is the step text",
                    null
                );
            // Act
            var result = await _service.ProcessStep(step);

            // Assert
            result.Should().BeOfType<XunitGenerator_Step>();

            result.Random.Should().MatchRegex("^[1-9]{1}[0-9]{5}$");
            result.Keyword.Should().Be("when");
            result.Text.Should().Be("this is the step text");
        }

        [Fact]
        public async Task ProcessStep_When_ItRuns_ItTrimsTheKeyword()
        {
            // Arrange
            var step = new Step(
                    new Location(100, 1),
                    " when ",
                    "this is the step text",
                    null
                );
            // Act
            var result = await _service.ProcessStep(step);

            // Assert
            result.Keyword.Should().Be("when");
        }

        [Fact]
        public async Task ProcessStep_When_ItRuns_ItEscapesDoubleQuotes()
        {
            // Arrange
            var step = new Step(
                    new Location(100, 1),
                    "when",
                    @"this is the ""step"" text",
                    null
                );
            // Act
            var result = await _service.ProcessStep(step);

            // Assert
            result.Text.Should().Be(@"this is the """"step"""" text");
        }

        [Fact]
        public async Task ProcessStep_When_StepHasADataTableArgument_ItIsAdded()
        {
            // Arrange
            var dt = new DataTable(new List<TableRow>() {
                new TableRow(new Location(200, 1), new List<TableCell>()
                {
                    new TableCell(new Location(200, 2), "cellvalue")
                }.ToArray())
            }.ToArray());
            var step = new Step(
                    new Location(100, 1),
                    "when",
                    "text",
                    dt
                );
            // Act
            var result = await _service.ProcessStep(step);

            // Assert
            result.Table.Should().NotBeNull();
            result.Table.Should().Be(dt);
        }

        [Fact]
        public async Task ProcessStep_When_StepHasADocStringArgument_ItIsAdded()
        {
            // Arrange
            var ds = new DocString(
                    null, null, "this is the content"
                );
            var step = new Step(
                    new Location(100, 1),
                    "when",
                    "text",
                    ds
                );
            // Act
            var result = await _service.ProcessStep(step);

            // Assert
            result.HasMultilineText.Should().BeTrue();
            result.MultilineText.Should().Be("this is the content");
        }

        #endregion

        #region ProcessScenario

        [Fact]
        public async Task ProcessScenario_When_ItRuns_ItReturnsProcessedData()
        {
            // Arrange
            var sc = new Scenario(
                    new List<Tag>().ToArray(),
                    new Location(100, 1),
                    "scenario",
                    "Scenario name",
                    "Scenario description",
                    new List<Step>().ToArray(),
                    new List<Examples>().ToArray()
                );
            var featureId = "feat1234";
            var injectedService = A.Fake<IXunitGeneratorService>();
            A.CallTo(() => injectedService.ProcessTags(A<IEnumerable<Tag>>._))
                .Returns((new List<ReportedTag>(), new List<string>()));
            A.CallTo(() => _string.ToIdString(A<string>._))
                .ReturnsLazily((string x) => $"_string.ToIdString({x})");
            A.CallTo(() => _string.ToMethodString(A<string>._))
               .ReturnsLazily((string x) => $"_string.ToMethodString({x})");

            // Act
            var result = await _service.ProcessScenario(sc, featureId, injectedService);

            // Assert
            result.Should().BeOfType<List<XunitGenerator_Scenario>>();
            result.Count.Should().Be(1);
            var res = result[0];
            res.Reported.Id.Should().Be("feat1234;_string.ToIdString(Scenario name)");
            res.Reported.Name.Should().Be("Scenario name");
            res.Reported.Keyword.Should().Be("Scenario");
            res.Reported.Line.Should().Be(100);
            res.Reported.Type.Should().Be("scenario");
            res.DisplayName.Should().Be("Scenario name");
            res.MethodName.Should().Be("_string.ToMethodString(Scenario name)");
        }

        [Fact]
        public async Task ProcessScenario_When_ItHasTags_ItReturnsProcessedTags()
        {
            // Arrange
            var tags = new List<Tag>() {
                        new Tag(new Location(200, 1), "@tag1"),
                        new Tag(new Location(205, 1), "@tag2")
                    }.ToArray();
            var sc = new Scenario(
                    tags,
                    new Location(100, 1),
                    "scenario",
                    "Scenario name",
                    "Scenario description",
                    new List<Step>().ToArray(),
                    new List<Examples>().ToArray()
                );
            var featureId = "feat1234";
            var injectedService = A.Fake<IXunitGeneratorService>();
            A.CallTo(() => injectedService.ProcessTags(A<IEnumerable<Tag>>._))
                .Returns((
                    new List<ReportedTag>() {
                        new ReportedTag(), new ReportedTag()
                    },
                    new List<string>()
                    {
                        "", ""
                    }
                ));

            // Act
            var result = await _service.ProcessScenario(sc, featureId, injectedService);

            // Assert
            var res = result[0];
            res.Reported.Tags.Count.Should().Be(2);
            res.Tags.Count.Should().Be(2);

            A.CallTo(() => injectedService.ProcessTags(tags)).MustHaveHappened();
        }

        [Fact]
        public async Task ProcessScenario_When_ItHasSteps_ItReturnsProcessedTagsSteps()
        {
            // Arrange
            var steps = new List<Step>()
            {
                new Step(new Location(200, 1), "keyword", "text", null),
                new Step(new Location(205, 1), "keyword", "text", null),
            }.ToArray();
            var sc = new Scenario(
                    new List<Tag>().ToArray(),
                    new Location(100, 1),
                    "scenario",
                    "Scenario name",
                    "Scenario description",
                    steps,
                    new List<Examples>().ToArray()
                );
            var featureId = "feat1234";
            var injectedService = A.Fake<IXunitGeneratorService>();
            A.CallTo(() => injectedService.ProcessTags(A<IEnumerable<Tag>>._))
               .Returns((new List<ReportedTag>(), new List<string>()));
            A.CallTo(() => injectedService.ProcessStep(A<Step>._))
                .Returns(new XunitGenerator_Step());

            // Act
            var result = await _service.ProcessScenario(sc, featureId, injectedService);

            // Assert
            var res = result[0];
            res.Steps.Count.Should().Be(2);

            A.CallTo(() => injectedService.ProcessStep(steps[0])).MustHaveHappened();
            A.CallTo(() => injectedService.ProcessStep(steps[1])).MustHaveHappened();
        }


        [Fact]
        public async Task ProcessScenario_When_ItHasExampleTables_ItReturnsAsManyScenarios()
        {
            // Arrange
            var examples = new List<Examples>() {
                new Examples(
                    new List<Tag>().ToArray(),
                    new Location(200, 1),
                    null,
                    "Example table 1",
                    "Example table description 1",
                    null, null
                ),
                new Examples(
                    new List<Tag>().ToArray(),
                    new Location(205, 1),
                    null,
                    "Example table 2",
                    "Example table description 2",
                    null, null
                ),
            }.ToArray();
            var sc = new Scenario(
                    new List<Tag>().ToArray(),
                    new Location(100, 1),
                    "scenario",
                    "Scenario name",
                    "Scenario description",
                    new List<Step>().ToArray(),
                    examples
                );
            var featureId = "feat1234";
            var injectedService = A.Fake<IXunitGeneratorService>();
            A.CallTo(() => injectedService.ProcessTags(A<IEnumerable<Tag>>._))
               .Returns((new List<ReportedTag>(), new List<string>()));

            // Act
            var result = await _service.ProcessScenario(sc, featureId, injectedService);

            // Assert
            result.Count.Should().Be(2);
        }

        [Fact]
        public async Task ProcessScenario_When_ItHasExampleTables_ItEnrichedWithExampleSpecificData()
        {
            // Arrange
            var examples = new List<Examples>() {
                new Examples(
                    new List<Tag>().ToArray(),
                    new Location(200, 1),
                    null,
                    "Example table 1",
                    "Example table description 1",
                    null, null
                )
            }.ToArray();
            var sc = new Scenario(
                new List<Tag>().ToArray(),
                new Location(100, 1),
                "scenario",
                "Scenario name",
                "Scenario description",
                new List<Step>().ToArray(),
                examples
            );
            var featureId = "feat1234";
            var injectedService = A.Fake<IXunitGeneratorService>();
            A.CallTo(() => injectedService.ProcessTags(A<IEnumerable<Tag>>._))
               .Returns((new List<ReportedTag>(), new List<string>()));
            A.CallTo(() => _string.ToIdString(A<string>._))
                .ReturnsLazily((string x) => $"_string.ToIdString({x})");
            A.CallTo(() => _string.ToMethodString(A<string>._))
               .ReturnsLazily((string x) => $"_string.ToMethodString({x})");

            // Act
            var result = await _service.ProcessScenario(sc, featureId, injectedService);

            // Assert
            var res = result[0];

            res.Reported.Id.Should().Be("feat1234;_string.ToIdString(Scenario name - Example table 1)");
            res.Reported.Name.Should().Be("Scenario name - Example table 1");
            res.Reported.Keyword.Should().Be("Scenario");
            res.Reported.Line.Should().Be(100);
            res.Reported.Type.Should().Be("scenario");
            res.DisplayName.Should().Be("Scenario name - Example table 1");
            res.MethodName.Should().Be("_string.ToMethodString(Scenario name - Example table 1)");
        }

        [Fact]
        public async Task ProcessScenario_When_AnExampleTableHasTags_TheyAreProcessed()
        {
            // Arrange
            var tags = new List<Tag>() {
                new Tag(new Location(205, 1), "@tag1"),
                new Tag(new Location(210, 1), "@tag2"),
            }.ToArray();
            var examples = new List<Examples>() {
                new Examples(
                    tags,
                    new Location(200, 1),
                    null,
                    "Example table 1",
                    "Example table description 1",
                    null, null
                )
            }.ToArray();
            var sc = new Scenario(
                new List<Tag>().ToArray(),
                new Location(100, 1),
                "scenario",
                "Scenario name",
                "Scenario description",
                new List<Step>().ToArray(),
                examples
            );
            var featureId = "feat1234";
            var injectedService = A.Fake<IXunitGeneratorService>();
            A.CallTo(() => injectedService.ProcessTags(A<IEnumerable<Tag>>._))
                .Returns((
                    new List<ReportedTag>() { new ReportedTag(), new ReportedTag() },
                    new List<string>() { "", "" }));
            A.CallTo(() => _string.ToIdString(A<string>._))
                .ReturnsLazily((string x) => $"_string.ToIdString({x})");
            A.CallTo(() => _string.ToMethodString(A<string>._))
                .ReturnsLazily((string x) => $"_string.ToMethodString({x})");

            // Act
            var result = await _service.ProcessScenario(sc, featureId, injectedService);

            // Assert
            result[0].Reported.Tags.Count.Should().Be(2);
            result[0].Tags.Count.Should().Be(2);
            A.CallTo(() => injectedService.ProcessTags(tags))
                .MustHaveHappened();
        }

        [Fact]
        public async Task ProcessScenario_When_AnExampleTableHasHeaders_TheyAreAddedAsParameters()
        {
            // Arrange
            var examples = new List<Examples>() {
                new Examples(
                    new List<Tag>().ToArray(),
                    new Location(200, 1),
                    null,
                    "Example table 1",
                    "Example table description 1",
                    new TableRow(
                        new Location(205, 1),
                        new List<TableCell>()
                        {
                            new TableCell(new Location(210, 1), "header1"),
                            new TableCell(new Location(210, 2), "header2")
                        }.ToArray()
                    ),
                    null
                )
            }.ToArray();
            var sc = new Scenario(
                new List<Tag>().ToArray(),
                new Location(100, 1),
                "scenario",
                "Scenario name",
                "Scenario description",
                new List<Step>().ToArray(),
                examples
            );
            var featureId = "feat1234";
            var injectedService = A.Fake<IXunitGeneratorService>();
            A.CallTo(() => _string.ToParamString(A<string>._))
                .ReturnsLazily((string x) => $"_string.ToParamString({x})");

            // Act
            var result = await _service.ProcessScenario(sc, featureId, injectedService);

            // Assert
            var res = result[0];
            res.Parameters.Count.Should().Be(3);
            res.Parameters[0].Should().Be(("givenSpecsIdx", "givenSpecsIdx"));
            res.Parameters[1].Should().Be(("header1", "_string.ToParamString(header1)"));
            res.Parameters[2].Should().Be(("header2", "_string.ToParamString(header2)"));
        }

        [Fact]
        public async Task ProcessScenario_When_AnExampleTableHasRows_TheyAreAddedAsValues()
        {
            // Arrange
            var examples = new List<Examples>() {
                new Examples(
                    new List<Tag>().ToArray(),
                    new Location(200, 1),
                    null,
                    "Example table 1",
                    "Example table description 1",
                    new TableRow(
                        new Location(205, 1),
                        new List<TableCell>()
                        {
                            new TableCell(new Location(210, 1), "header1"),
                            new TableCell(new Location(210, 2), "header2")
                        }.ToArray()
                    ),
                    new List<TableRow>()
                    {
                        new TableRow(
                            new Location(215, 1),
                            new List<TableCell>()
                            {
                                new TableCell(new Location(220, 1), "value1.1"),
                                new TableCell(new Location(220, 2), "value1.2")
                            }.ToArray()
                        ),
                        new TableRow(
                            new Location(225, 1),
                            new List<TableCell>()
                            {
                                new TableCell(new Location(230, 1), "value2.1"),
                                new TableCell(new Location(230, 2), "value2.2")
                            }.ToArray()
                        )
                    }.ToArray()
                )
            }.ToArray();
            var sc = new Scenario(
                new List<Tag>().ToArray(),
                new Location(100, 1),
                "scenario",
                "Scenario name",
                "Scenario description",
                new List<Step>().ToArray(),
                examples
            );
            var featureId = "feat1234";
            var injectedService = A.Fake<IXunitGeneratorService>();

            // Act
            var result = await _service.ProcessScenario(sc, featureId, injectedService);

            // Assert
            var res = result[0];
            res.Examples.Count.Should().Be(2);

            res.Examples[0].Values[0].Should().Be("1");
            res.Examples[0].Values[1].Should().Be("value1.1");
            res.Examples[0].Values[2].Should().Be("value1.2");
            res.Examples[1].Values[0].Should().Be("2");
            res.Examples[1].Values[1].Should().Be("value2.1");
            res.Examples[1].Values[2].Should().Be("value2.2");
        }

        #endregion

        #region ProcessFeature

        [Fact]
        public async Task ProcessFeature_When_ItRuns_ItReturnsProcessedData()
        {
            // Arrange
            var feature = new Feature(
                new List<Tag>().ToArray(),
                new Location(100, 1),
                "english",
                "feature",
                "Feature name",
                "Feature description",
                new List<IHasLocation>().ToArray()
            );
            var fileLocation = "c:/file.feature";
            var injectedService = A.Fake<IXunitGeneratorService>();
            A.CallTo(() => _config.FeatureNamespace)
                .Returns("MyProject.Features");
            A.CallTo(() => _string.ToMethodString(A<string>._))
                .ReturnsLazily((string x) => $"_string.ToMethodString({x})");
            A.CallTo(() => _string.ToIdString(A<string>._))
               .ReturnsLazily((string x) => $"_string.ToIdString({x})");

            // Act
            var result = await _service.ProcessFeature(
                feature, fileLocation, true, injectedService
            );

            // Assert
            result.Should().BeOfType<XunitGenerator_Feature>();
            result.GenerateCollectionFixture.Should().Be(true);
            result.Namespace.Should().Be("MyProject.Features");
            result.Class.Should().Be("_string.ToMethodString(Feature name)");
            result.Reported.Id.Should().Be("_string.ToIdString(Feature name)");
            result.Reported.Uri.Should().Be("c:/file.feature");
            result.Reported.Keyword.Should().Be("Feature");
            result.Reported.Line.Should().Be(100);
            result.Reported.Name.Should().Be("Feature name");
        }

        [Fact]
        public async Task ProcessFeature_When_ItHasABackground_ItIsProcessed()
        {
            // Arrange
            var feature = new Feature(
                new List<Tag>().ToArray(),
                new Location(100, 1),
                "english",
                "feature",
                "Feature name",
                "Feature description",
                new List<IHasLocation>() {
                    new Background(
                        new Location(105, 1),
                        "Background",
                        "Background",
                        "Description",
                        new List<Step>()
                        {
                            new Step(
                                new Location(110, 1),
                                "Given",
                                "a step",
                                null
                            )
                        }.ToArray()
                    )
                }.ToArray()
            );
            var fileLocation = "c:/file.feature";
            var injectedService = A.Fake<IXunitGeneratorService>();
            A.CallTo(() => injectedService.ProcessStep(A<Step>._))
                .Returns(new XunitGenerator_Step());

            // Act
            var result = await _service.ProcessFeature(
                feature, fileLocation, true, injectedService
            );

            // Assert
            result.BackgroundSteps.Count.Should().Be(1);

            A.CallTo(() => injectedService.ProcessStep(A<Step>.That.Matches(x => x.Keyword == "Given")))
                .MustHaveHappened();
        }

        [Fact]
        public async Task ProcessFeature_When_ItHasScenarios_TheyAreProcessed()
        {
            // Arrange
            var feature = new Feature(
                new List<Tag>().ToArray(),
                new Location(100, 1),
                "english",
                "feature",
                "Feature name",
                "Feature description",
                new List<IHasLocation>() {
                    new Scenario(
                        new List<Tag>().ToArray(),
                        new Location(100, 1),
                        "scenario",
                        "Scenario name",
                        "Scenario description",
                        new List<Step>().ToArray(),
                        new List<Examples>().ToArray()
                    )
                }.ToArray()
            );
            var fileLocation = "c:/file.feature";
            var injectedService = A.Fake<IXunitGeneratorService>();
            A.CallTo(() => _string.ToIdString(A<string>._))
               .ReturnsLazily((string x) => $"_string.ToIdString({x})");
            A.CallTo(() => injectedService.ProcessScenario(A<Scenario>._, A<string>._, A<IXunitGeneratorService>._))
                .Returns(new List<XunitGenerator_Scenario>() { new XunitGenerator_Scenario() });

            // Act
            var result = await _service.ProcessFeature(
                feature, fileLocation, true, injectedService
            );

            // Assert
            result.Scenarios.Count.Should().Be(1);

            A.CallTo(() => injectedService.ProcessScenario(
                    A<Scenario>.That.Matches(x => x.Name == "Scenario name"),
                    "_string.ToIdString(Feature name)",
                    A<IXunitGeneratorService>._
                )).MustHaveHappened();
        }

        #endregion
    }
}
